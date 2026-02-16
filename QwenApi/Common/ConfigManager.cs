using System.Text;
using System.Text.Json;
using System.IO;

namespace QwenApi.Common
{
    /// <summary>
    /// 配置管理器，负责加载和保存Qwen API的认证配置
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configFilePath;
        private readonly object _lock = new();

        /// <summary>
        /// bx-ua 认证头
        /// </summary>
        public string BxUa { get; set; } = "";

        /// <summary>
        /// cookie 认证头
        /// </summary>
        public string Cookie { get; set; } = "";

        /// <summary>
        /// bx-umidtoken 认证头
        /// </summary>
        public string BxUmidtoken { get; set; } = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filename">配置文件名，默认为config.json</param>
        public ConfigManager(string filename = "config.json")
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("配置文件名不能为空", nameof(filename));

            string configDir = Path.Combine(Environment.CurrentDirectory, "config");
            Directory.CreateDirectory(configDir);
            _configFilePath = Path.Combine(configDir, filename);

            Load();
        }

        /// <summary>
        /// 从字符串加载配置
        /// </summary>
        /// <param name="bxua">bx-ua值</param>
        /// <param name="cookie">cookie值</param>
        /// <param name="bxUmidtoken">bx-umidtoken值</param>
        public void LoadString(string bxua, string cookie, string bxUmidtoken)
        {
            BxUa = bxua.Trim();
            Cookie = cookie.Trim();
            BxUmidtoken = bxUmidtoken.Trim();
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        /// <returns>加载是否成功</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Save();
                    return true;
                }

                string jsonContent = File.ReadAllText(_configFilePath, Encoding.UTF8);
                var config = JsonSerializer.Deserialize<ConfigData>(jsonContent);
                
                lock (_lock)
                {
                    BxUa = config?.BxUa?.Trim() ?? "";
                    Cookie = config?.Cookie?.Trim() ?? "";
                    BxUmidtoken = config?.BxUmidtoken?.Trim() ?? "";
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 加载配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        /// <returns>保存是否成功</returns>
        public bool Save()
        {
            try
            {
                var config = new ConfigData
                {
                    BxUa = BxUa,
                    Cookie = Cookie,
                    BxUmidtoken = BxUmidtoken
                };
                
                string jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                
                lock (_lock)
                {
                    File.WriteAllText(_configFilePath, jsonContent, Encoding.UTF8);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 保存配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 配置是否完整
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(BxUa)
                                 && !string.IsNullOrWhiteSpace(Cookie)
                                 && !string.IsNullOrWhiteSpace(BxUmidtoken);

        /// <summary>
        /// 初始化空配置
        /// </summary>
        public void InitializeEmpty()
        {
            BxUa = "";
            Cookie = "";
            BxUmidtoken = "";
            Save();
        }

        /// <summary>
        /// 配置数据结构
        /// </summary>
        private class ConfigData
        {
            public string BxUa { get; set; } = "";
            public string Cookie { get; set; } = "";
            public string BxUmidtoken { get; set; } = "";
        }
    }
}
