using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;

namespace NuclearWinter.Storage
{
    public abstract class SaveHandler
    {
        protected NuclearGame Game;

        public string FileName = "SaveData.bin";

        public readonly UInt32 SettingsMagicNumber = 0xffffffff;
        protected Dictionary<UInt32, Action<BinaryReader>> ReadSettingsActions;

        public readonly UInt32 DataMagicNumber = 0xffffffff;
        protected Dictionary<UInt32, Action<BinaryReader>> ReadDataActions;

        protected string ContainerName;

        //----------------------------------------------------------------------
        protected abstract void WriteSettings(BinaryWriter output);
        protected abstract void WriteData(BinaryWriter output);

        //----------------------------------------------------------------------
        public SaveHandler(NuclearGame game, UInt32 uiSettingsMagicNumber, string containerName, UInt32 uiDataMagicNumber)
        {
            Debug.Assert(uiDataMagicNumber != 0xffffffff);
            DataMagicNumber = uiDataMagicNumber;

            ContainerName = containerName;

            Debug.Assert(uiSettingsMagicNumber != 0xffffffff);
            SettingsMagicNumber = uiSettingsMagicNumber;

            Game = game;
        }

        //----------------------------------------------------------------------
        // Save settings
        public void SaveGameSettings()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForDomain())
                {
                    try
                    {
                        var stream = store.OpenFile(FileName, FileMode.Create);

                        if (stream != null)
                        {
                            var output = new BinaryWriter(stream);
                            output.Write(SettingsMagicNumber);
                            WriteSettings(output);

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
                Debug.Assert(false, "Saving settings has failed");
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
                using (var store = IsolatedStorageFile.GetUserStoreForDomain())
                {
                    if (store.FileExists(FileName))
                    {
                        try
                        {
                            var stream = store.OpenFile(FileName, FileMode.Open);

                            if (stream != null)
                            {
                                using (var input = new BinaryReader(stream))
                                {
                                    uint magicNumber = 0xffffffff;

                                    try
                                    {
                                        magicNumber = input.ReadUInt32();

                                        if (ReadSettingsActions.ContainsKey(magicNumber))
                                        {
                                            ReadSettingsActions[magicNumber](input);
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
                Debug.Assert(false, "Loading settings has failed");
            }
        }
    }
}
