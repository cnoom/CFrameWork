using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Editor.CFrameWork.AddressableModule
{
    public static class PathToStaticClass
    {
        private const string AddressableAddressesClassName = "CFrameWorkAddresses";

        public static void GenerateAddressableStaticClass()
        {
            AddressableUtility.ToAddressableFiles();
            // 获取所有可寻址资源的地址
            List<string> allAddresses = GetAllAddressableAssetPaths();

            // 构建目录树
            Dictionary<string, object> directoryTree = BuildDirectoryTree(allAddresses);

            // 生成代码
            StringBuilder sb = new StringBuilder();
            GenerateClassCode(sb, AddressableAddressesClassName, directoryTree, 0);

            CommonUtility.TrySetDataPath();

            CommonUtility.SaveFile($"{AddressableAddressesClassName}.cs", sb.ToString());
        }

        private static List<string> GetAllAddressableAssetPaths()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            List<string> addresses = new List<string>();
            if(settings != null)
            {
                foreach (var group in settings.groups)
                {
                    foreach (var entry in group.entries)
                    {
                        addresses.Add(entry.address);
                    }
                }
            }
            return addresses;
        }

        private static Dictionary<string, object> BuildDirectoryTree(List<string> addresses)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            foreach (string address in addresses)
            {
                string[] parts = address.Split('/');
                Dictionary<string, object> current = root;
                for(int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if(!current.ContainsKey(part))
                    {
                        current[part] = new Dictionary<string, object>();
                    }
                    current = (Dictionary<string, object>)current[part];
                }
                string assetName = Path.GetFileNameWithoutExtension(parts[^1]);
                current[assetName] = address;
            }
            return root;
        }

        private static void GenerateClassCode(StringBuilder sb, string className, Dictionary<string, object> directoryTree, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            sb.AppendLine($"{indent}public static class {className}");
            sb.AppendLine($"{indent}{{");

            foreach (var kvp in directoryTree)
            {
                string validKey = MakeValidVariableName(kvp.Key);
                // 检查生成的名称是否与类名相同，如果相同则添加后缀
                if(validKey == className)
                {
                    validKey += "_Asset";
                }

                if(kvp.Value is Dictionary<string, object> value)
                {
                    // 如果是子目录，递归生成类
                    GenerateClassCode(sb, validKey, value, indentLevel + 1);
                }
                else
                {
                    // 如果是资源，生成常量
                    string variableName = validKey;
                    string address = kvp.Value.ToString();
                    sb.AppendLine($"{indent}    public const string {variableName} = \"{address}\";");
                }
            }

            sb.AppendLine($"{indent}}}");
        }
        
        private static string MakeValidVariableName(string input)
        {
            // 去除非法字符，将空格替换为下划线
            string validName = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
            // 确保变量名以字母或下划线开头
            if(!char.IsLetter(validName[0]) && validName[0] != '_')
            {
                validName = "_" + validName;
            }
            return validName;
        }
    }
}