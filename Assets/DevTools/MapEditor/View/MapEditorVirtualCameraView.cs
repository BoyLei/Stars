#if UNITY_EDITOR
using System;
using Cinemachine;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapEditorVirtualCameraView : MapEditorBaseView
    {
        public MapEditorVirtualCameraView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public void OnLoad(GameObject root)
        {
            Root = root.transform;
            var cams = Root.GetComponentsInChildren<VirtualCamera>();
            if (cams != null && cams.Length > 0)
            {
                foreach (var c in cams)
                {
                    if (c.Index > MaxIndex)
                    {
                        MaxIndex = c.Index;
                    }
                }
            }
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建虚拟相机"))
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
                MapEditor.RandomMonsterEditorData editorData = new MapEditor.RandomMonsterEditorData(position, true);
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

            MaxIndex++;
            MapEditor.RandomMonsterEditorData data = userData as MapEditor.RandomMonsterEditorData;
            GameObject go = new GameObject("VirtualCamera");
            var c = go.AddComponent<VirtualCamera>();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.AddComponent<CinemachineVirtualCamera>();
            c.Index = MaxIndex;
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
        }
    }
}
#endif