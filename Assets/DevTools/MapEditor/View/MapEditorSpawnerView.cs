#if UNITY_EDITOR
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapEditorSpawnerView : MapEditorBaseView
    {
        public MapEditorSpawnerView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData,Transform Parent)
        {
            if (base.Load(jsonData,Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Spawners != null)
                {
                    foreach (var spawner in jsonData.Spawners)
                    {
                        CreateSpawner(spawner.Value);
                        if (spawner.Value.SpawnerID > MaxIndex)
                        {
                            MaxIndex = spawner.Value.SpawnerID;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private void CreateSpawner(SpawnerJsonData jsonData)
        {
           var go= MapEditorUtils.CreatePort(jsonData.Desc,Root);
           go.position = jsonData.Position.Convert();
           go.localRotation=Quaternion.identity;
           
          Spawner  spawner = go.gameObject.AddComponent<Spawner>();
          spawner.SpawnerID = jsonData.SpawnerID;
          spawner.Range = jsonData.Range;
          go.gameObject.layer = LayerMask.NameToLayer("Entity");
        }
        public override void OnGUI()
        {
            if (GUILayout.Button("创建Spawner"))
            {
                CreateChild();
            }
        }

        public override void CreateChild()
        {
            Vector3 position;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc)
            {
                MapEditor.SpawnerEditorData editorData = new MapEditor.SpawnerEditorData(position, true);
                CreateChild(editorData);
            }
        }

        public override void CreateChild(object userData)
        {
            if (Root == null)
            {
                EditorUtility.DisplayDialog("提示", "编辑环境已经破坏，请重新打开", "确定");
                return;
            }

            MapEditor.SpawnerEditorData data = userData as MapEditor.SpawnerEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            Spawner spawner = go.AddComponent<Spawner>();
            MaxIndex++;
            spawner.SpawnerID = MaxIndex;
            go.name = "Spawner_" + spawner.SpawnerID;
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Spawners.Clear();
            Spawner[] spawners = Root.GetComponentsInChildren<Spawner>();
            if (spawners != null && spawners.Length > 0)
            {
                int index = 1;
                foreach (var item in spawners)
                {
                    sceneJson.Spawners.Add(item.SpawnerID, new SpawnerJsonData(index++, item));
                }
            }
        }
    }
}
#endif