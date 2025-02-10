using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Editor.AddressableModel
{
    public static class PathToStaticClass
    {
        private const string AddressableAddressesClassName = "CFrameWorkAddresses";
        [MenuItem("CFrameWork/资源系统/生成所有可寻址资源地址静态类")]
        private static void GenerateAddressableStaticClass()
        {
            // 获取所有可寻址资源的地址
            List<string> allAddresses = GetAllAddressableAssetPaths();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public static class {AddressableAddressesClassName}");
            sb.AppendLine("{");
            foreach (string address in allAddresses)
            {
                string variableName = MakeValidVariableName(address);
                sb.AppendLine($"    public const string {variableName} = \"{address}\";");
            }
            sb.AppendLine("}");
            if (!Directory.Exists(CFrameWorkConfig.DataFilePath))
            {
                Directory.CreateDirectory(CFrameWorkConfig.DataFilePath);
            }
            string filePath = CFrameWorkConfig.DataFilePath + $"/{AddressableAddressesClassName}.cs";
            File.WriteAllText(filePath, sb.ToString());
            AssetDatabase.Refresh();
        }
        
        private static List<string> GetAllAddressableAssetPaths()
        {
            // 获取 Addressable 系统的设置对象
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable settings not found.");
                throw new Exception("Addressable settings not found.");
            }
            // 用于存储所有可寻址资源的地址
            List<string> allAddresses = new List<string>();
            // 遍历所有 Addressable 资源组
            foreach (AddressableAssetGroup group in settings.groups)
            {
                // 遍历每个组中的条目
                foreach (AddressableAssetEntry entry in group.entries)
                {
                    // 将条目的地址添加到列表中
                    allAddresses.Add(entry.address);
                }
            }
            return allAddresses;
        }
        
        private static string MakeValidVariableName(string input)
        {
            // 移除非法字符
            string validName = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
            // 如果以数字开头，添加一个前缀
            if (char.IsDigit(validName[0]))
            {
                validName = "_" + validName;
            }
            return validName;
        }
    }
}