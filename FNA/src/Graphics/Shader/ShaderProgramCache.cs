#if OPENGL

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{

    internal class ShaderProgram
    {
        public readonly uint Program;

        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        public ShaderProgram(uint program)
        {
            Program = program;
        }

        public int GetUniformLocation(string name)
        {
            if (_uniformLocations.ContainsKey(name))
                return _uniformLocations[name];

            var location = Game.Instance.GraphicsDevice.GLDevice.glGetUniformLocation(Program, name);
            _uniformLocations[name] = location;
            return location;
        }
    }

    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache : IDisposable
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();
        bool disposed;

        ~ShaderProgramCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clear the program cache releasing all shader programs.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _programCache)
            {
                if (Game.Instance.GraphicsDevice.GLDevice.glIsProgram(pair.Value.Program))
                {
                    Game.Instance.GraphicsDevice.GLDevice.glDeleteProgram(pair.Value.Program);
                }
            }
            _programCache.Clear();
        }

        public ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader)
        {
            // TODO: We should be hashing in the mix of constant 
            // buffers here as well.  This would allow us to optimize
            // setting uniforms to only when a constant buffer changes.

            var key = vertexShader.HashKey | pixelShader.HashKey;
            if (!_programCache.ContainsKey(key))
            {
                // the key does not exist so we need to link the programs
                Link(vertexShader, pixelShader);
            }

            return _programCache[key];
        }        

        private void Link(Shader vertexShader, Shader pixelShader)
        {
            // NOTE: No need to worry about background threads here
            // as this is only called at draw time when we're in the
            // main drawing thread.
            var program = vertexShader.GraphicsDevice.GLDevice.glCreateProgram();

            vertexShader.GraphicsDevice.GLDevice.glAttachShader(program, vertexShader.GetShaderHandle());

            vertexShader.GraphicsDevice.GLDevice.glAttachShader(program, pixelShader.GetShaderHandle());

            vertexShader.GraphicsDevice.GLDevice.glLinkProgram(program);

            vertexShader.GraphicsDevice.GLDevice.glUseProgram(program);

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            var linked = 0;
            vertexShader.GraphicsDevice.GLDevice.glGetProgramiv(
                program,
                OpenGLDevice.GLenum.GL_LINK_STATUS,
                out linked
            );
            if (linked == 0)
            {
                Console.WriteLine(
                    vertexShader.GraphicsDevice.GLDevice.glGetProgramInfoLog(program)
                );

                vertexShader.GraphicsDevice.GLDevice.glDetachShader(program, vertexShader.GetShaderHandle());
                vertexShader.GraphicsDevice.GLDevice.glDetachShader(program, pixelShader.GetShaderHandle());

                vertexShader.GraphicsDevice.GLDevice.glDeleteProgram(program);
                throw new InvalidOperationException("Unable to link effect program");
            }

            ShaderProgram shaderProgram = new ShaderProgram(program);

            _programCache.Add(vertexShader.HashKey | pixelShader.HashKey, shaderProgram);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Clear();
                disposed = true;
            }
        }
    }
}

#endif // OPENGL
