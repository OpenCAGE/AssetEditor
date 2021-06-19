using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienPAK
{
    class ToolOptionsHandler
    {
        private string SettingsFile = AppDomain.CurrentDomain.BaseDirectory + "/AlienPAK.conf";
        public enum Settings { EXPERIMENTAL_TEXTURE_IMPORT, SETTINGS_LENGTH };

        /* Create settings file on launch if it doesn't exist */
        public ToolOptionsHandler()
        {
            if (!File.Exists(SettingsFile))
            {
                BinaryWriter SettingsWriter = new BinaryWriter(File.Create(SettingsFile));
                for (int i = 0; i < (int)Settings.SETTINGS_LENGTH; i++)
                {
                    SettingsWriter.Write(false);
                }
                SettingsWriter.Close();
            }
        }

        /* Update a setting */
        public void UpdateSetting(bool Value, Settings Setting)
        {
            int SettingIndex = (int)Setting;
            BinaryWriter SettingWriter = new BinaryWriter(File.OpenWrite(SettingsFile));
            SettingWriter.BaseStream.Position = SettingIndex;
            SettingWriter.Write(Value);
            SettingWriter.Close();
        }

        /* Get a setting */
        public bool GetSetting(Settings Setting)
        {
            int SettingIndex = (int)Setting;
            BinaryReader SettingReader = new BinaryReader(File.OpenRead(SettingsFile));
            SettingReader.BaseStream.Position = SettingIndex;
            bool SettingValue = SettingReader.ReadBoolean();
            SettingReader.Close();
            return SettingValue;
        }
    }
}
