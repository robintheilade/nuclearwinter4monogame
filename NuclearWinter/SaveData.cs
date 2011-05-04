using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System;

namespace NuclearWinter
{
    public abstract class SaveData
    {
        public string                           FileName        = "SaveData.bin";
        public readonly UInt32                  MagicNumber     = 0xffffffff;

        Dictionary<UInt32,Action<BinaryReader>> mdLoadData;

        //--------------------------------------------------------------------------
        protected abstract void WriteData( BinaryWriter _output );

        //--------------------------------------------------------------------------
        public SaveData( UInt32 _uiMagicNumber, Dictionary<UInt32,Action<BinaryReader>> _dLoadData )
        {
            Debug.Assert( _uiMagicNumber != 0xffffffff );

            mdLoadData      = _dLoadData;
            MagicNumber     = _uiMagicNumber;
        }

        //--------------------------------------------------------------------------
        public void Save()
        {

            try
            {
#if XBOX360 || WINDOWS_PHONE
                using( var store = IsolatedStorageFile.GetUserStoreForApplication() )
#else
                using( var store = IsolatedStorageFile.GetUserStoreForDomain() )
#endif
                {
                    var stream = store.OpenFile( FileName, FileMode.Create );

                    if( stream != null )
                    {
                        var output = new BinaryWriter( stream );
                        output.Write( MagicNumber );
                        WriteData( output );
                        stream.Close();
                    }
                }
            }
            catch ( IsolatedStorageException ex )
            {
                Debug.Assert( false, "SaveData.Save() has failed: {0}", "Unhandled IsolatedStorageException", ex );
            }
        }

        //--------------------------------------------------------------------------
        public void Load()
        {
            try
            {
#if XBOX360 || WINDOWS_PHONE
                using( var store = IsolatedStorageFile.GetUserStoreForApplication() )
#else
                using( var store = IsolatedStorageFile.GetUserStoreForDomain() )
#endif
                {
                    if( store.FileExists( FileName ) )
                    {
                        var stream = store.OpenFile( FileName, FileMode.Open );

                        if( stream != null )
                        {
                            using ( var input = new BinaryReader( stream ) )
                            {
                                uint magicNumber = 0xffffffff;
                                try
                                {
                                    magicNumber = input.ReadUInt32();
                                }
                                catch
                                {
                                    Debug.Assert( false );
                                }

                                if( mdLoadData.ContainsKey( magicNumber ) )
                                {
                                    mdLoadData[ magicNumber ]( input );
                                }
                            }

                            stream.Close();
                        }
                    }
                }
            }
            catch( IsolatedStorageException ex )
            {
                Debug.Assert( false, "SaveData.Load() has failed: {0}", "Unhandled IsolatedStorageException", ex );
            }
        }
    }
}
