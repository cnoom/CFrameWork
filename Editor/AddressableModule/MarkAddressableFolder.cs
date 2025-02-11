using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.CFrameWork.AddressableModule
{
    public static class MarkAddressableFolder
    {

        [MenuItem("Assets/标记为可寻址文件夹")]
        private static void MarkFolderAsAddressable()
        {
            // 获取当前选择的文件夹路径
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("请选择一个文件夹");
            }
            AddressSaveData saveData = AddressableUtility.GetAddressSaveData();
            saveData.addressableFolders.Add(folderPath);
            AddressableUtility.SaveData(saveData);
            Debug.Log("成功标记!");
        }

        [MenuItem("Assets/取消标记为可寻址文件夹")]
        private static void UnMarkFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("请选择一个文件夹");
            }
            UnMark(folderPath);
        }

        public static void UnMark(string path)
        {
            AddressSaveData saveData = AddressableUtility.GetAddressSaveData();
            saveData.addressableFolders.Remove(path);
            AddressableUtility.SaveData(saveData);
            Debug.Log("成功取消标记!");
        }

        [MenuItem("Assets/标记为可寻址文件夹", true)]
        private static bool ShowMark()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(!IsExistsFolder(path)) return false;
            AddressSaveData saveData = AddressableUtility.GetAddressSaveData();
            return !saveData.addressableFolders.Contains(path);
        }

        [MenuItem("Assets/取消标记为可寻址文件夹", true)]
        public static bool ShowUnMark()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(!IsExistsFolder(path)) return false;
            AddressSaveData saveData = AddressableUtility.GetAddressSaveData();
            return saveData.addressableFolders.Contains(path);
        }

        private static bool IsExistsFolder(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return false;
            }
            if(!Directory.Exists(path))
            {
                return false;
            }
            return true;
        }
    }
}