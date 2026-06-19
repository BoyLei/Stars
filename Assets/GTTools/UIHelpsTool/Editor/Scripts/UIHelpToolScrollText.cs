using UnityEditor;
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    internal class UIHelpToolScrollText
    {
        [MenuItem("Tools/UI辅助工具 (v1.0.0)/UIScrollText转换", priority = 203)]
        static public void Doit()
        {
            var selectedgos =  Selection.gameObjects;
            var uiroot = GameObject.Find("Canvas").transform;
            if (selectedgos.Length > 0)
            {
                for(int i = 0;i < selectedgos.Length;i++)
                {
                    //var go = selectedgos[i];
                    //
                    var gopaths = AssetDatabase.GetAssetPath(selectedgos[i]);
                    var usego = AssetDatabase.LoadAssetAtPath<GameObject>(gopaths);
                    usego = GameObject.Instantiate(usego);
                    usego.transform.SetParent(uiroot, false);
                    ScrollText[] texts = usego.GetComponentsInChildren<ScrollText>(true);
                    for (int j = 0; j < texts.Length; j++)
                    {
                        texts[j].ReplaceScrollView();
                    }
                    PrefabUtility.SaveAsPrefabAsset(usego, gopaths);
                    GameObject.DestroyImmediate(usego);
                }
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "请选择需要处理的预制", "是");
            }
        }
    }
}