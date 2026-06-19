#if UNITY_EDITOR
using System;
using Task;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{

    public class MapEditorTriggerView : MapEditorBaseView
    {
        public MapEditorTriggerView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Triggers != null)
                {
                    foreach (var trigger in jsonData.Triggers)
                    {
                        CreateTrigger(trigger.Value);
                        if (trigger.Value.ID > MaxIndex)
                        {
                            MaxIndex = trigger.Value.ID;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private void CreateTrigger(TriggerJsonData jsonData)
        {
            var go= MapEditorUtils.CreatePort(jsonData.Desc,Root);
            if (jsonData.Position != null)
            {
                go.position = jsonData.Position.Convert();
            }
            else
            {
                go.position = Vector3.zero;
            }

            go.localRotation=Quaternion.identity;
           
            TrrigerBase  trriger = go.gameObject.AddComponent<TrrigerBase>();
            trriger.ID = jsonData.ID;
            trriger.TrrigerType = (TrrigerType)jsonData.TrrigerType;
            trriger.Count = jsonData.Count;
            
             switch (trriger.TrrigerType)
        {
            case Task.TrrigerType.PositionTrriger:
                {
          
                    trriger.Radius = jsonData.PositionJsonData.Radius;
                    trriger.IsEnterTrigger = jsonData.PositionJsonData.IsEnterTrigger;
                    trriger.shapType = (AreaShape)jsonData.PositionJsonData.shapType;
                    trriger.Length = jsonData.PositionJsonData.Length;
                    trriger.Width = jsonData.PositionJsonData.Width;
                    
                    trriger.Polygons.Clear();

                    var list = jsonData.PositionJsonData.Polygons;
                    if (list != null && list.Count > 0)
                    {
                        int index = 0;
                        foreach (var item in list)
                        {
                           var t= MapEditorUtils.CreatePort((index++).ToString(),Root);
                           t.position = item.Convert();
                            trriger.Polygons.Add(t);
                        }
                    }

                }
                break;
            case Task.TrrigerType.TimerTrriger:
                {
                    trriger.Delay = jsonData.TimerJsonData.Delay;
                    trriger.Interval = jsonData.TimerJsonData.Interval;
                }
                break;
            case Task.TrrigerType.PropertyTrriger:
                {
                    trriger.IsSelf = jsonData.PropertyJsonData.IsSelf;
                    trriger.PropName = jsonData.PropertyJsonData.PropName;
                    trriger.PropValue = jsonData.PropertyJsonData.PropValue;
                    trriger.Compare = (CompareEnum)jsonData.PropertyJsonData.Compare;
                }
                break;
        }
             
 
             go.gameObject.layer = LayerMask.NameToLayer("Entity");
        }

    public override void OnGUI()
        {
            if (GUILayout.Button("创建触发器"))
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
                MapEditor.TriggerEditorData editorData = new MapEditor.TriggerEditorData(position, true);
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

            MapEditor.TriggerEditorData data = userData as MapEditor.TriggerEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            TrrigerBase trriger = go.AddComponent<TrrigerBase>();
            MaxIndex++;
            trriger.ID = MaxIndex;
            go.name = "Trigger_" + trriger.ID;
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Triggers.Clear();
            TrrigerBase[] trrigers = Root.GetComponentsInChildren<TrrigerBase>();
            if (trrigers != null && trrigers.Length > 0)
            {
                foreach (var item in trrigers)
                {
                    if (sceneJson.Triggers.ContainsKey(item.ID))
                    {
                        Debug.LogError($"触发器ID重复  {item.ID}");
                        continue;
                    }

                    sceneJson.Triggers.Add(item.ID, new TriggerJsonData(item));
                }
            }
        }
    }
}
#endif