#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    
    public class MapEditorGVEBossView : MapEditorBaseView
    {
        public MapEditorGVEBossView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform parent)
        {
            if (base.Load(jsonData, parent))
            {
                if (jsonData!=null && jsonData.GVE_Points != null && jsonData.GVE_Points.Count> 0)
                {
                    for (int i = 0; i < jsonData.GVE_Points.Count; i++)
                    {
                        var  go = MapEditorUtils.CreatePort($"Point_{i}",Root);
                        go.position=jsonData.GVE_Points[i].Convert();
                    }
                }
                return true;
            }

            return false;
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建GVEBoss点"))
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
                MapEditor.PositionData editorData = new MapEditor.PositionData(position, true);
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

            MapEditor.PositionData data = userData as MapEditor.PositionData;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.GVE_Points.Clear();
            if (Root != null)
            {
                int count = Root.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    sceneJson.GVE_Points.Add(i, new CustomVector3(Root.transform.GetChild(i).position));
                }
            }
        }
    }
}
#endif
