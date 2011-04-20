using System;
using System.Collections.Generic;

using System.Text;
using System.Xml;

namespace VectorLevelProcessor
{
    public class XMLFile
    {
        public XMLFile( string _strFilepath, XmlReader _reader )
        {
            Filepath = _strFilepath;
            XmlReader = _reader;
        }
    
        public string       Filepath    { get; private set; }
        public XmlReader    XmlReader   { get; private set; }
    }
}
