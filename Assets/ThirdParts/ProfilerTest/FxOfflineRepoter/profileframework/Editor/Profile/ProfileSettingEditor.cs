// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/23 17:33:5)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yoka.Galaxy.Configure;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;


namespace Yoka.Galaxy.Profile
{
    public class ProfileSettingWindow : OdinMenuEditorWindow
    {
        private const string ITEM_NAME_PROFILESETTING = "[ Yoka ]/���ܷ���... &F10";

        [MenuItem(ITEM_NAME_PROFILESETTING)]
        public static void OpenWindow()
        {
            var window = GetWindow<ProfileSettingWindow>("���Թ���");
            window.MenuWidth = 150;
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 800);
        }

        [InitializeOnLoadMethod]
        private static void InitializeUseCaseDropdown()
        {
            Debug.Log("DEBUG!!! InitializeUseCaseDropdown");
            UseCaseSetting.InitializeFromEditor(OnCaseAdded, OnCaseRemoved, OnCaseSynced);
        }

        private static void OnCaseAdded(ScriptableObject addedCase)
        {
            if (addedCase != null)
            {
                var path = UseCaseSetting.AssetPath;
                AssetDatabase.AddObjectToAsset(addedCase, path);
                AssetDatabase.Refresh();
            }
        }

        private static void OnCaseRemoved(ScriptableObject removedCase)
        {
            if (removedCase != null)
            {
                AssetDatabase.RemoveObjectFromAsset(removedCase);
                AssetDatabase.Refresh();
            }
        }

        private static void OnCaseSynced(HashSet<ProfilerUseCase> usingCases)
        {
            var path = UseCaseSetting.AssetPath;
            var allSub = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach(var sub in allSub)
            {
                if(sub is ProfilerUseCase)
                {
                    ProfilerUseCase subCase = sub as ProfilerUseCase;
                    if(!usingCases.Contains(subCase))
                    {
                        AssetDatabase.RemoveObjectFromAsset(subCase);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();

            tree.Add(UseCaseSetting.Name, UseCaseSetting.Fetch());

            return tree;
        }

    }
}

#endif