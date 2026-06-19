using System.Collections;
using System.Collections.Generic;
using System.IO;
using SGF.UI.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasEditor : Editor
{
    [MenuItem("Assets/AtlasEditor")]
    public static void AtlasEditorUpdate()
    {
        string[] paths =  Directory.GetFiles("Assets/Res/UI/Common/Textures/Role","*.png");
        foreach (var s in paths)
        {
            if (s.EndsWith(".meta"))
            {
                continue;
            }
            Object[] o = AssetDatabase.LoadAllAssetsAtPath(s);
            SpriteAtlas atlas = new UnityEngine.U2D.SpriteAtlas();
            atlas.Add(o);
            string p ="Assets/Res/UI/Common/Atlas/" + Path.GetFileNameWithoutExtension(s)+ ".spriteatlas" ;
            AssetDatabase.CreateAsset(atlas,p);
            AssetDatabase.ImportAsset(p);
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/UpdatePrefab")]
    public static void UpdatePrefab()
    {
        /*
  string[] paths =  Directory.GetFiles("Assets/Res/UI","*.asset",SearchOption.AllDirectories);
  int count= paths.Length;
  int index = 0;
  UnityEditor.EditorUtility.ClearProgressBar();
  foreach (var asset in paths)
  {
      var atlas = AssetDatabase.LoadAssetAtPath<Atlas>(asset);
      if (atlas != null)
      {
          Debug.LogError(asset);
         string path=asset.Replace(".asset",".txt");
         path = path.Replace("\\", "/");
         Debug.LogError(path);
          var  json=AssetDatabase.LoadAssetAtPath<TextAsset>(path);
          if (json != null)
          {
              AtlasGenera.ProcessToSprite(json);
          }
      }
      UnityEditor.EditorUtility.DisplayProgressBar("UpdatePrefab", "正在更新" + index + "/" + count, (float)index / count);
      index++;
  }
  UnityEditor.EditorUtility.ClearProgressBar();
string[] paths =  Directory.GetFiles("Assets/Res/UI","*.prefab",SearchOption.AllDirectories);
  foreach (var asset in paths)
  {
      var prefabobj = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
      if (prefabobj != null)
      {
          var prefab = (GameObject)PrefabUtility.InstantiateAttachedAsset(prefabobj);

          if (prefab != null)
          {
              UIPanel panel = prefab.GetComponent<UIPanel>();
              if (panel != null && panel.Atlas!=null)
              {
                  panel.OnSetAtlas();
                  Debug.LogError(asset);
              }
              PrefabUtility.SaveAsPrefabAsset(prefab, asset);
          }
          GameObject.DestroyImmediate(prefab);
      }

      AssetDatabase.SaveAssets();
        
      
  }*/
        
    }
    
}
