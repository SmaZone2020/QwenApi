using System.Text;

namespace QwenApi.Models
{
    public class ConfigManager
    {
        private readonly string _configFilePath;
        private readonly object _lock = new();

        public string BxUa { get; set; } = "";
        public string Cookie { get; set; } = "";
        public string BxUmidtoken { get; set; } = "";

        public ConfigManager(string filename = "config.txt")
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("配置文件名不能为空", nameof(filename));

            string configDir = Path.Combine(Environment.CurrentDirectory, "config");
            Directory.CreateDirectory(configDir);
            _configFilePath = Path.Combine(configDir, filename);

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

                string[] lines = File.ReadAllLines(_configFilePath, Encoding.UTF8);
                lock (_lock)
                {
                    BxUa = lines.Length > 0 ? lines[0].Trim() : "";
                    Cookie = lines.Length > 1 ? lines[1].Trim() : "";
                    BxUmidtoken = lines.Length > 2 ? lines[2].Trim() : "";
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 加载配置失败: {ex.Message}");
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                string content = $"{BxUa}{Environment.NewLine}{Cookie}{Environment.NewLine}{BxUmidtoken}";
                lock (_lock)
                {
                    File.WriteAllText(_configFilePath, content, Encoding.UTF8);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Config] 保存配置失败: {ex.Message}");
                return false;
            }
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(BxUa)
                                 && !string.IsNullOrWhiteSpace(Cookie)
                                 && !string.IsNullOrWhiteSpace(BxUmidtoken);


        public void InitializeEmpty()
        {
            BxUa = "";
            Cookie = "";
            BxUmidtoken = "";
            Save();
        }
    }
}