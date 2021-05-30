using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTEModem
{
    class ConfigManager
    {
        private Configuration config = null;
        private static readonly string KEY_APPSETTINGS = "appSettings";
        public static readonly string KEY_ROUTER = "Router";
        public static readonly string KEY_USERNAME = "Username";
        public static readonly string KEY_PASSWORD = "Password";

        public ConfigManager()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public bool SetString(string key, string value)
        {
            try
            {
                config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection(KEY_APPSETTINGS);
                return true;
            }
            catch (Exception)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine(e.Message);
                //Console.ResetColor();
                return false;
            }
        }

        public string GetString(string key)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception($"Config: key \"{key}\" is not found!");
                else
                    return value;
            }
            catch (Exception)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine(e.Message);
                //Console.ResetColor();
                return string.Empty;
            }
        }
    }
}
