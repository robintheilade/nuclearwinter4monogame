using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Xml;

// TODO: replace this with the type you want to import.
using TImport = System.String;

namespace VectorLevelProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, XMLFile.
    /// </summary>
    [ContentImporter(".svg", DisplayName = "SVG Level Importer", DefaultProcessor = "VectorLevelProcessor")]
    public class SVGLevelImporter : ContentImporter<XMLFile>
    {
        public override XMLFile Import(string _strFilename, ContentImporterContext context)
        {
            // Read file content and return it
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create( _strFilename );

            return new XMLFile( System.IO.Path.GetFullPath( _strFilename ), reader );
        }
    }
}
