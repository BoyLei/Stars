#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapEditorWantedTaskView : MapEditorBaseView
    {
        public MapEditorWantedTaskView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.WantedTasks != null && jsonData.WantedTasks.Count > 0)
                {
                    foreach (var wantedTask in jsonData.WantedTasks)
                    {
                        CreateWantedTask(wantedTask.Value);
                        if (wantedTask.Value.Index > MaxIndex)
                        {
                            MaxIndex = wantedTask.Value.Index;
                        }
                    }
                }
                /*WantedTask[] wantedTasks = Root.GetComponentsInChildren<WantedTask>();
                if (wantedTasks != null && wantedTasks.Length > 0)
                {
                    foreach (var item in wantedTasks)
                    {
                        if (item.Index > MaxIndex)
                        {
                            MaxIndex = item.Index;
                        }
                    }
                }*/

                return true;
            }

            return false;
        }

        private void CreateWantedTask(WantedTaskData jsonData)
        {
            var go = MapEditorUtils.CreatePort(jsonData.Desc, Root);
            go.position = jsonData.Position.Convert();
            go.localRotation = Quaternion.Euler(0, jsonData.Rotation, 0);
            go.localScale = Vector3.one;

            WantedTask task = go.gameObject.AddComponent<WantedTask>();
            task.Index = jsonData.Index;
            task.Type = jsonData.Type;
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建通缉任务"))
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
            var wanted = go.AddComponent<WantedTask>();
            MaxIndex++;
            wanted.Index = MaxIndex;
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.WantedTasks.Clear();
            if (Root != null)
            {
                WantedTask[] wantedTasks = Root.GetComponentsInChildren<WantedTask>();
                foreach (var task in wantedTasks)
                {
                    sceneJson.WantedTasks.Add(task.Index, new WantedTaskData(task.Index, task));
                }
            }
        }
    }
}
#endif