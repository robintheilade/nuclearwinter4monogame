using System.IO;

namespace NuclearWinter.Storage
{
    class DesktopStorageHandler : StorageHandler
    {
        //----------------------------------------------------------------------
        public readonly string RootPath;
        public readonly string AppIdentifier;

        //----------------------------------------------------------------------
        public DesktopStorageHandler(string appIdentifier)
        {
            AppIdentifier = appIdentifier;
            RootPath = Path.Combine(NuclearGame.ApplicationDataFolderPath, AppIdentifier);
            System.IO.Directory.CreateDirectory(RootPath);
        }

        //----------------------------------------------------------------------
        public override BinaryReader OpenRead(string filename)
        {
            var reader = new BinaryReader(File.OpenRead(Path.Combine(RootPath, filename)));
            return reader;
        }

        //----------------------------------------------------------------------
        public override BinaryWriter OpenWrite(string filename)
        {
            var writer = new BinaryWriter(File.OpenWrite(Path.Combine(RootPath, filename)));
            return writer;
        }
    }
}
