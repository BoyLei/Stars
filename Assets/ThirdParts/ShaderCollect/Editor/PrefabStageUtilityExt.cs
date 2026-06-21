using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;

namespace GameExtensions
{
    public class PrefabStageUtilityExt
    {
        public static UnityEditor.SceneManagement.PrefabStage OpenStage(GameObject root)
        {
            return UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(AssetDatabase.GetAssetPath(root));
        }
        public static UnityEditor.SceneManagement.PrefabStage OpenStage(string prefabAssetPath)
        {
            return UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(prefabAssetPath);
        }
    }
}
