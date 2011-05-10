using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VectorLevel
{
    public class Camera
    {
        //----------------------------------------------------------------------
        public Camera( Vector2 _vViewportSize )
        {
            mvViewportSize = _vViewportSize;

            Projection = Matrix.CreateOrthographicOffCenter(
				0,
                mvViewportSize.X,
				mvViewportSize.Y,
				0,
				0, 1f );

            Scroll              = Vector2.Zero;
            Zoom                = 1f;
        }

        //----------------------------------------------------------------------
        internal void Update( float _fElapsedTime )
        {
            Behavior.Update( _fElapsedTime );

            SetupViewMatrix();
        }

        //----------------------------------------------------------------------
        void SetupViewMatrix()
        {
            View =  Matrix.CreateTranslation( new Vector3( -Scroll + mvViewportSize / 2f, 0f ) ) * Matrix.CreateScale( Zoom, Zoom, 1f );
        }

        //----------------------------------------------------------------------
        Vector2                 mvViewportSize;

        public ICameraBehavior  Behavior;

        public Matrix           Projection;
        public Matrix           View;

        public Vector2          Scroll;
        public float            Zoom;
    }
}
