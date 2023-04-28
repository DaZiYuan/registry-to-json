using Microsoft.Win32;
using System.Text;
using System.Text.Json;

namespace RegistryToJson
{
    class Program
    {
        static RegistryKey? GetRegistryKey(string keyPath)
        {
            RegistryKey? registryKey = null;

            var index = keyPath.IndexOf("HKEY_");
            if (index < 0)
                return null;

            keyPath = keyPath[index..];

            string rootKeyName = keyPath.Split('\\')[0];

            switch (rootKeyName.ToUpper())
            {
                case "HKEY_CLASSES_ROOT":
                    registryKey = Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_USER":
                    registryKey = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    registryKey = Registry.LocalMachine;
                    break;
                case "HKEY_USERS":
                    registryKey = Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    registryKey = Registry.CurrentConfig;
                    break;
            }

            string subKeyPath = keyPath[(rootKeyName.Length + 1)..];
            registryKey = registryKey?.OpenSubKey(subKeyPath);

            return registryKey;
        }
        static void Main(string[] args)
        {
            // 获取命令行参数
            string registryPath = "";
            string outputFilePath = "";
            bool watch = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-r" && i + 1 < args.Length)
                {
                    registryPath = args[i + 1];
                }
                else if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputFilePath = args[i + 1];
                }
                else if (args[i] == "-watch")
                {
                    watch = true;
                }
            }

            // 检查参数是否正确
            if (string.IsNullOrEmpty(registryPath))
            {
                Console.WriteLine("Please specify the registry path using the -r option.");
                return;
            }
            if (string.IsNullOrEmpty(outputFilePath))
            {
                Console.WriteLine("Please specify the output file path using the -o option.");
                return;
            }

            if (watch)
            {
                while (true)
                {
                    Export(registryPath, outputFilePath);
                    Thread.Sleep(1000);
                }
            }
            else
            {
                Export(registryPath, outputFilePath);
                Console.WriteLine($"Successfully exported the registry to {outputFilePath}");
            }
        }

        private static void Export(string registryPath, string outputFilePath)
        {
            // 将注册表导出到 JSON 文件
            try
            {
                RegistryKey? registryKey = GetRegistryKey(registryPath);
                if (registryKey != null)
                {
                    Dictionary<string, object> registryDict = ParseRegistryKey(registryKey);
                    string json = JsonSerializer.Serialize(registryDict, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(outputFilePath, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while exporting the registry: {ex.Message}");
            }
        }

        // 递归遍历注册表并保存键值对
        static Dictionary<string, object> ParseRegistryKey(RegistryKey registryKey)
        {
            Dictionary<string, object> dict = new();

            foreach (string valueName in registryKey.GetValueNames())
            {
                if (registryKey == null)
                    continue;
                object? value = registryKey.GetValue(valueName);
                value ??= "";
                if (value is byte[] bytes)
                {
                    value = Encoding.UTF8.GetString(bytes);
                }

                dict.Add(valueName, value);
            }

            foreach (string subKeyName in registryKey.GetSubKeyNames())
            {
                RegistryKey? subKey = registryKey.OpenSubKey(subKeyName);
                if (subKey != null)
                    dict.Add(subKeyName, ParseRegistryKey(subKey));
            }

            return dict;
        }
    }
}
