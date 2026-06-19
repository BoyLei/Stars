#if UNITY_EDITOR
using System;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    public class MapEditorPathView : MapEditorBaseView
    {
        public MapEditorPathView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData,Transform Parent)
        {
            if (base.Load(jsonData,Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Paths != null && jsonData.Paths.Count > 0)
                {
                    foreach (var path in jsonData.Paths)
                    {
                        CreatePath(path.Value);
                        if (path.Value.PathID > MaxIndex)
                        {
                            MaxIndex = path.Value.PathID;
                        }
                    }
                }
                /*Path[] paths = Root.GetComponentsInChildren<Path>();
                if (paths != null && paths.Length > 0)
                {
                    foreach (var item in paths)
                    {
                        if (item.PathID > MaxIndex)
                        {
                            MaxIndex = item.PathID;
                        }
                    }
                }*/
                return true;
            }

            return false;
        }

        private void CreatePath(PathJsonData jsonData)
        {
            
            var go = MapEditorUtils.CreatePort(jsonData.Desc, Root);
            if (jsonData.Position != null)
            {
                go.position = jsonData.Position.Convert();
            }
            else
            {
                go.position = Vector3.zero;
            }

            if (jsonData.Rotation != null)
            {


                go.localRotation = Quaternion.Euler(jsonData.Rotation.Convert());
            }
            else
            {
                go.localRotation = Quaternion.identity;
            }
                

            var path = go.gameObject.AddComponent<Path>();
              path.PathID=jsonData.PathID;
              path.IsLoop=jsonData.IsLoop;
              path.path.m_Resolution = jsonData.Resolution;
            //Position = path.transform.position;
            // Rotation = path.transform.rotation.eulerAngles;
            var list = jsonData.KeyPoints;
            if (list != null && list.Count > 0)
            {
                int index = 0;
                path.path.m_Waypoints = new CinemachinePath.Waypoint[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    path.path.m_Waypoints[i] = new CinemachinePath.Waypoint()
                    {
                        position = list[i].position.Convert(),
                        tangent = list[i].tangent.Convert(),
                        roll = list[i].roll
                    };
                }
            }
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建路径"))
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
                MapEditor.PathEditorData editorData = new MapEditor.PathEditorData(position, true);
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

            MapEditor.PathEditorData data = userData as MapEditor.PathEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            Path path = go.AddComponent<Path>();
            MaxIndex++;
            path.PathID = MaxIndex;
            go.name = "Path_" + path.PathID;
            path.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            Path[] paths = Root.GetComponentsInChildren<Path>();
            if (paths != null && paths.Length > 0)
            {
                int index = 1;
                foreach (var item in paths)
                {
                    sceneJson.Paths.Add(item.PathID, new PathJsonData(index++, item));
                }
            }
        }
    }
}
#endif
