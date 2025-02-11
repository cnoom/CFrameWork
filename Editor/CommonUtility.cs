using System.IO;
using UnityEditor;

namespace Editor.CFrameWork
{
    public static class CommonUtility
    {
        public static void TrySetDataPath()
        {
            if(!Directory.Exists(CFrameWorkConfig.DataFilePath))
            {
                Directory.CreateDirectory(CFrameWorkConfig.DataFilePath);
            }
            AssetDatabase.Refresh();
        }

        public static void SaveFile(string fileName, string str)
        {
            File.WriteAllText(Path.Combine(CFrameWorkConfig.DataFilePath, fileName), str);
            AssetDatabase.Refresh();
        }
    }
}
