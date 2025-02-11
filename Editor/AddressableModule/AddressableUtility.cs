using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Editor.CFrameWork.AddressableModule
{
    public static class AddressableUtility
    {
        private const string JsonName = "AddressableSystemData.json";
        private static string Path => System.IO.Path.Combine(CFrameWorkConfig.DataFilePath, JsonName);

        private static AddressSaveData saveData = null;

        public static AddressSaveData GetAddressSaveData()
        {
            if(saveData != null)
            {
                return saveData;
            }
            CommonUtility.TrySetDataPath();
            if(!File.Exists(Path))
            {
                return new AddressSaveData();
            }
            string jsonStr = File.ReadAllText(Path);
            AddressSaveData data = JsonUtility.FromJson<AddressSaveData>(jsonStr);
            return data;
        }

        public static void SaveData(AddressSaveData data)
        {
            saveData = data;
            string jsonStr = JsonUtility.ToJson(data, true);
            string p = Path;
            File.WriteAllText(p, jsonStr);
            AssetDatabase.Refresh();
        }

        #region 可寻址文件夹内容标记为可寻址

        /// <summary>
        /// 将标记的文件夹内所有内容标记为可寻址
        /// </summary>
        public static void ToAddressableFiles()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if(!settings)
            {
                Debug.LogError("Addressable Asset Settings not found.");
                return;
            }
            // 获取默认的组
            AddressableAssetGroup defaultGroup = settings.DefaultGroup;
            if(!defaultGroup)
            {
                Debug.LogError("Default Addressable Asset Group not found.");
                return;
            }
            foreach (string path in GetAddressSaveData().addressableFolders)
            {
                ToAddressableFile(path, settings, defaultGroup);
            }
        }

        private static void ToAddressableFile(string file, AddressableAssetSettings settings, AddressableAssetGroup defaultGroup)
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { file });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if(Directory.Exists(assetPath))
                {
                    continue;
                }
                string parentPath = System.IO.Path.GetDirectoryName(assetPath);
                string label = GetLabel(parentPath);
                settings.AddLabel(label);
                AddressableAssetEntry existingEntry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
                if(existingEntry != null)
                {
                    existingEntry.address = assetPath;
                    existingEntry.labels.Clear();
                    existingEntry.labels.Add(label);
                    continue;
                }
                AddressableAssetEntry newEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), defaultGroup);
                if(newEntry != null)
                {
                    // 设置资源的地址
                    newEntry.address = assetPath;

                    // 为资源添加标签
                    newEntry.labels.Add(label);
                    Debug.Log($"Asset at path {assetPath} has been marked as addressable with label {newEntry.labels}.");
                }
                else
                {
                    Debug.LogError($"Failed to mark asset at path {assetPath} as addressable.");
                }
            }
            // 保存设置更改
            settings.SetDirty();
            AssetDatabase.SaveAssets();
        }

        private static string GetLabel(string filePath)
        {
            Debug.Log(System.IO.Path.GetFileName(filePath));
            return System.IO.Path.GetFileName(filePath);
        }
        #endregion

    }

    [Serializable]
    public class AddressSaveData
    {
        public List<string> addressableFolders = new List<string>();
    }
}