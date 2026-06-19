/*
 * @Description: 实时预览功能，遇到些问题
 */
// using UnityEngine;
// using System.Collections;
// using UnityEditor;
// using System.IO;
// using System.Reflection;
// using System.Linq;
// using System.Collections.Generic;
// using GameTechTools.UIHelpsTool;
// using UnityEngine.UIElements;

// [CustomEditor(typeof(GameObject))]
// public class UIHelpsPrefabPreviewEditor : Editor
// {
//     private Editor m_GameObjectInspector;
//     private MethodInfo m_OnHeaderGUI;
//     private MethodInfo m_ShouldHideOpenButton;

//     Editor reflectorGameObjectEditor
//     {
//         get
//         {
//             return m_GameObjectInspector;
//         }
//     }


//     bool ValidObject()
//     {
//         GameObject targetGameObject = target as GameObject;
//         //后面加下开关
//         return true;
//     }

//     public override bool HasPreviewGUI()
//     {
//         if (!ValidObject())
//             return reflectorGameObjectEditor.HasPreviewGUI();

//         return true;
//     }

//     public override void OnPreviewGUI(Rect r, GUIStyle background)
//     {

//         if (!ValidObject())
//         {
//             reflectorGameObjectEditor.OnPreviewGUI(r, background);
//             return;
//         }

//         var targetGameObject = target as GameObject;
//         string guid = UIHelpsToolUtils.ObjectToGUID(targetGameObject);

//         string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + guid + ".png";
//         Texture2D previewTex = null;
//         if (File.Exists(preview_path))
//         {
//             previewTex = AssetDatabase.LoadAssetAtPath<Texture2D>(preview_path);
//         }
//         else
//         {
//             previewTex = UIHelpsToolUtils.ExportBasicCompImg(targetGameObject) as Texture2D;
//             if (previewTex != null)
//             {
//                 AssetDatabase.ImportAsset(preview_path, ImportAssetOptions.ForceUpdate);
//             }
//         }

//         if (previewTex == null)
//         {
//             return;
//         }

//         GUI.DrawTexture(r, previewTex);
//     }

//     public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
//     {
//         if (!ValidObject())
//             return reflectorGameObjectEditor.RenderStaticPreview(assetPath, subAssets, width, height);
 
//         GameObject targetGameObject = target as GameObject;
//         string guid = UIHelpsToolUtils.ObjectToGUID(targetGameObject);

//         string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + guid + ".png";
//         Texture2D previewTex = null;
//         if (File.Exists(preview_path))
//         {
//             previewTex = AssetDatabase.LoadAssetAtPath<Texture2D>(preview_path);
//         }
//         else
//         {
//             previewTex = UIHelpsToolUtils.ExportBasicCompImg(targetGameObject) as Texture2D;
//             if (previewTex != null)
//             {
//                 AssetDatabase.ImportAsset(preview_path, ImportAssetOptions.ForceUpdate);
//             }
//         }

//         if (previewTex == null)
//         {
//             return null;
//         }
 
//         //example.PreviewIcon must be a supported format: ARGB32, RGBA32, RGB24,
//         // Alpha8 or one of float formats
//         Texture2D tex = new Texture2D(width, height);
//         EditorUtility.CopySerialized(previewTex, tex);
 
//         return tex;
//     }

//     public override void DrawPreview(Rect previewArea)
//     {
//         reflectorGameObjectEditor.DrawPreview(previewArea);
//     }

//     public override void OnInspectorGUI()
//     {
//         reflectorGameObjectEditor.OnInspectorGUI();
//     }

//     public override string GetInfoString()
//     {
//         return reflectorGameObjectEditor.GetInfoString();
//     }

//     public override GUIContent GetPreviewTitle()
//     {
//         return reflectorGameObjectEditor.GetPreviewTitle();
//     }

//     public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
//     {
//         reflectorGameObjectEditor.OnInteractivePreviewGUI(r, background);
//     }


//     public override void OnPreviewSettings()
//     {
//         reflectorGameObjectEditor.OnPreviewSettings();
//     }

//     public override void ReloadPreviewInstances()
//     {
//         reflectorGameObjectEditor.ReloadPreviewInstances();
//     }

//     void OnEnable()
//     {
//         System.Type gameObjectorInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
//         m_OnHeaderGUI = gameObjectorInspectorType.GetMethod("OnHeaderGUI",
//             BindingFlags.NonPublic | BindingFlags.Instance);
//         m_GameObjectInspector = Editor.CreateEditor(target, gameObjectorInspectorType);
//     }

//     void OnDisable()
//     {
//         if (m_GameObjectInspector)
//             DestroyImmediate(m_GameObjectInspector);
//         m_GameObjectInspector = null;
//     }

//     protected override void OnHeaderGUI()
//     {
//         if (m_OnHeaderGUI != null)
//         {
//             m_OnHeaderGUI.Invoke(m_GameObjectInspector, null);
//         }
//     }

//     public override bool RequiresConstantRepaint()
//     {
//         return reflectorGameObjectEditor.RequiresConstantRepaint();
//     }

//     public override bool UseDefaultMargins()
//     {
//         return reflectorGameObjectEditor.UseDefaultMargins();
//     }

//     protected override bool ShouldHideOpenButton()
//     {
//         return (bool)m_ShouldHideOpenButton.Invoke(m_GameObjectInspector, null);
//     }

// }
