using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class CFrameWorkConfig
    {
        private const string AssetsPath = "Assets";
        private const string DataFileName = "CFrameWorkData";
        private const string ScriptsFolder = "Scripts";
        private const string AutoGenerateCodeFolder = "AutoGenerate";

        private static readonly string AssetsFilePath = $"{AssetsPath}/{DataFileName}";
        private static readonly string CodeFilePath = $"{AssetsPath}/{ScriptsFolder}/{AutoGenerateCodeFolder}";
        
        public static string GetCodeFilePath()
        {
            if(!AssetDatabase.IsValidFolder(CodeFilePath))
            {
                AssetDatabase.CreateFolder($"{AssetsPath}/{ScriptsFolder}", AutoGenerateCodeFolder);
            }
            return $"{Application.dataPath}/{ScriptsFolder}/{AutoGenerateCodeFolder}";
        }

        public static string GetAssetsFilePath()
        {
            if(!AssetDatabase.IsValidFolder(AssetsFilePath))
            {
                AssetDatabase.CreateFolder(AssetsPath, DataFileName);
            }
            return AssetsFilePath;
        }
    }
}