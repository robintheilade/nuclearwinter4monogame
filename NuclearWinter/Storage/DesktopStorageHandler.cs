using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NuclearWinter.Storage
{
    class DesktopStorageHandler: StorageHandler
    {
        //----------------------------------------------------------------------
        public readonly string      RootPath;
        public readonly string      AppIdentifier;

        //----------------------------------------------------------------------
        public DesktopStorageHandler( string _strAppIdentifier )
        {
            AppIdentifier = _strAppIdentifier;
            RootPath = Path.Combine( NuclearGame.ApplicationDataFolderPath, AppIdentifier );
            System.IO.Directory.CreateDirectory( RootPath );
        }

        //----------------------------------------------------------------------
        public override BinaryReader OpenRead( string _strFilename )
        {
            var reader = new BinaryReader( File.OpenRead( Path.Combine( RootPath, _strFilename ) ) );
            return reader;
        }

        //----------------------------------------------------------------------
        public override BinaryWriter OpenWrite( string _strFilename )
        {
            var writer = new BinaryWriter( File.OpenWrite( Path.Combine( RootPath, _strFilename ) ) );
            return writer;
        }
    }
}
