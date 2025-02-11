using System;
using UnityEditor;
using UnityEngine;

namespace Editor.CFrameWork.AddressableModule
{
    public class CAddressableWindow : EditorWindow
    {
        // 当前选中的菜单项索引
        private int selectedMenuItemIndex = 0;
        // 菜单项数组
        private string[] menuItems =
        {
            "显示标记的可寻址文件",
            "生成可寻址类",
        };

        private AddressSaveData data;
        private Action[] actions;
        
        [MenuItem("CFrameWork/资源系统")]
        public static void ShowWindow()
        {
            // 获取或创建自定义编辑器窗口实例
            CAddressableWindow window = GetWindow<CAddressableWindow>("CFrameWork资源管理系统");
            window.data = AddressableUtility.GetAddressSaveData();
            window.Show();
        }

        private void OnEnable()
        {
            actions = new Action[]
            {
                Click1,
                Click2
            };
        }

        private void Click1()
        {
            selectedMenuItemIndex = 0;
        }

        private void Click2()
        {
            PathToStaticClass.GenerateAddressableStaticClass();
        }

        private void OnGUI()
        {
            // 定义左右部分的布局
            EditorGUILayout.BeginHorizontal();
            // 左边菜单栏部分
            EditorGUILayout.BeginVertical(GUILayout.Width(200), GUILayout.Height(500));
            // 绘制菜单栏按钮
            for(int i = 0; i < menuItems.Length; i++)
            {
                if(GUILayout.Button(menuItems[i]))
                {
                    // 点击按钮时更新选中的菜单项索引
                    actions[i]?.Invoke();
                }
            }
            EditorGUILayout.EndVertical();
            if(selectedMenuItemIndex == 0)
            {
                ShowAddress();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAddress()
        {
            EditorGUILayout.BeginVertical();
            foreach (string pFolder in data.addressableFolders)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(pFolder);
                if(GUILayout.Button("取消标记"))
                {
                    MarkAddressableFolder.UnMark(pFolder);
                    data = AddressableUtility.GetAddressSaveData();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}