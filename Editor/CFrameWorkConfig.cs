using System.IO;
using UnityEngine;

namespace Editor.CFrameWork
{
    public static class CFrameWorkConfig
    {
        public static readonly string DataFilePath = Path.Combine(Application.dataPath, "CFrameWorkData");
    }
}