using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json.Serialization;

namespace OnceMi.AspNetCore.Config
{
    public class ConfigManager
    {
        private static ConfigManager _manager = null;

        [JsonIgnore]
        public IConfiguration Configuration { get; private set; }

        public static ConfigManager Instance
        {
            get
            {
                if (_manager == null)
                {
                    throw new Exception("Please load config at first.");
                }
                return _manager;
            }
        }

        public ConfigManager(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(IConfiguration));
            this.Configuration = configuration;
        }

        internal void Load()
        {
            try
            {
                _manager = this;
            }
            catch (Exception ex)
            {
                throw new Exception($"Load config failed. {ex.Message}");
            }
        }

        private T GetSection<T>(string sectionName = null)
        {
            string nameofT = typeof(T).Name;
            if (string.IsNullOrEmpty(nameofT) && string.IsNullOrEmpty(sectionName))
            {
                return default;
            }
            if (string.IsNullOrEmpty(sectionName))
            {
                int index = nameofT.LastIndexOf("Node");
                if (index > 0 && index == nameofT.Length - 4)
                {
                    nameofT = nameofT.Substring(0, index);
                }
            }
            else
            {
                nameofT = sectionName;
            }
            IConfigurationSection section = this.Configuration.GetSection(nameofT);
            if(section == null || !section.Exists())
            {
                return default;
            }
            return section.Get<T>();
        }

        public AppSettingsNode AppSettings
        {
            get
            {
                return GetSection<AppSettingsNode>();
            }
        }

        public ConnectionStringsNode ConnectionStrings
        {
            get
            {
                return GetSection<ConnectionStringsNode>();
            }
        }
    }
}
