using System.IO;

namespace QwenWebAPI
{
    public class Runtimes
    {
        public static string AuthToken { get; set; } = string.Empty;
        public static ConfigManager ConfigManager { get; } = new ConfigManager();
    }

    public class ConfigManager
    {
        private readonly string _configFilePath;
        private readonly object _lock = new();

        public string AuthToken { get; set; } = string.Empty;

        public ConfigManager()
        {
            string configDir = Path.Combine(Environment.CurrentDirectory, "config");
            Directory.CreateDirectory(configDir);
            _configFilePath = Path.Combine(configDir, "webapi-config.json");

            Load();
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Save();
                    return true;
                }

                string jsonContent = File.ReadAllText(_configFilePath);
                var config = System.Text.Json.JsonSerializer.Deserialize<WebApiConfig>(jsonContent);
                
                lock (_lock)
                {
                    AuthToken = config?.AuthToken ?? string.Empty;
                }
                
                // 更新Runtimes中的AuthToken
                Runtimes.AuthToken = AuthToken;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebAPI Config] 加载配置失败: {ex.Message}");
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                var config = new WebApiConfig
                {
                    AuthToken = AuthToken
                };
                
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                
                lock (_lock)
                {
                    File.WriteAllText(_configFilePath, jsonContent);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebAPI Config] 保存配置失败: {ex.Message}");
                return false;
            }
        }

        private class WebApiConfig
        {
            public string AuthToken { get; set; } = string.Empty;
        }
    }
}
