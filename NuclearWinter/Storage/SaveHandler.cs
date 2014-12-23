using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Storage;

namespace NuclearWinter.Storage
{
    public abstract class SaveHandler
    {
        protected NuclearGame                                   Game;

        public string                                           FileName                = "SaveData.bin";

        public readonly UInt32                                  SettingsMagicNumber     = 0xffffffff;
        protected Dictionary<UInt32,Action<BinaryReader>>       ReadSettingsActions;

        public readonly UInt32                                  DataMagicNumber         = 0xffffffff;
        protected Dictionary<UInt32,Action<BinaryReader>>       ReadDataActions;

        protected string                                        ContainerName;

        //----------------------------------------------------------------------
        protected abstract void WriteSettings( BinaryWriter _output );
        protected abstract void WriteData( BinaryWriter _output );

        //----------------------------------------------------------------------
        public SaveHandler( NuclearGame _game, UInt32 _uiSettingsMagicNumber, string _strContainerName, UInt32 _uiDataMagicNumber )
        {
            Debug.Assert( _uiDataMagicNumber != 0xffffffff );
            DataMagicNumber         = _uiDataMagicNumber;

            ContainerName   = _strContainerName;

            Debug.Assert( _uiSettingsMagicNumber != 0xffffffff );
            SettingsMagicNumber     = _uiSettingsMagicNumber;

            Game            = _game;
        }

        //----------------------------------------------------------------------
        // Save settings
        public void SaveGameSettings()
        {
            try
            {
                using( var store = IsolatedStorageFile.GetUserStoreForDomain() )
                {
                    try {
                        var stream = store.OpenFile( FileName, FileMode.Create );

                        if( stream != null )
                        {
                            var output = new BinaryWriter( stream );
                            output.Write( SettingsMagicNumber );
                            WriteSettings( output );

                            stream.Close();
                        }
                    }
                    catch
                    {
                    }

                    store.Dispose();
                }
            }
            catch
            {
                Debug.Assert( false, "Saving settings has failed" );
            }
        }

        protected abstract void ResetSettings();

        //----------------------------------------------------------------------
        // Load game settings
        public void LoadGameSettings()
        {
            ResetSettings();

            try
            {
                using( var store = IsolatedStorageFile.GetUserStoreForDomain() )
                {
                    if( store.FileExists( FileName ) )
                    {
                        try
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

                                        if( ReadSettingsActions.ContainsKey( magicNumber ) )
                                        {
                                            ReadSettingsActions[ magicNumber ]( input );
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }

                                stream.Close();
                            }
                        }
                        catch
                        {
                        }

                        store.Dispose();
                    }
                }
            }
            catch
            {
                Debug.Assert( false, "Loading settings has failed" );
            }
        }
    }
}
