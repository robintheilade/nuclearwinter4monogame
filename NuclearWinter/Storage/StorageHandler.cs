using System.IO;

namespace NuclearWinter.Storage
{
    public abstract class StorageHandler
    {
        public abstract BinaryReader OpenRead(string filename);
        public abstract BinaryWriter OpenWrite(string filename);
    }
}
