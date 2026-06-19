#if UNITY_EDITOR
using System.Collections.Generic;
using Task;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    public class MapEditorAreaView : MapEditorBaseView
    {
        public MapEditorAreaView(string viewName, System.Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Areas != null && jsonData.Areas.Count > 0)
                {
                    foreach (var area in jsonData.Areas)
                    {
                        CreateArea(area.Value);
                        if (area.Value.AreaID > MaxIndex)
                        {
                            MaxIndex = area.Value.AreaID;
                        }
                    }
                }

                /*Area[] areas = Root.GetComponentsInChildren<Area>();
                if (areas != null && areas.Length > 0)
                {
                    foreach (var item in areas)
                    {
                        if (item.AreaID > MaxIndex)
                        {
                            MaxIndex = item.AreaID;
                        }
                    }
                }*/
                return true;
            }

            return false;
        }

        private void CreateArea(AreaJsonData jsonData)
        {
            var go = MapEditorUtils.CreatePort(jsonData.Desc, Root);
            go.position = jsonData.Position.Convert();
            go.localRotation = Quaternion.Euler(0, jsonData.Rotation, 0);

            var area = go.gameObject.AddComponent<Area>();

            area.AreaID = jsonData.AreaID;
            area.Faction = jsonData.Faction;
            area.TriggerCount = jsonData.TriggerCount;

            area.areaType = (AreaType)jsonData.areaType;
            area.shapType = (AreaShape)jsonData.shapType;

            area.Length = jsonData.Length;
            area.Width = jsonData.Width;
            area.Radius = jsonData.Radius;

            area.Polygons.Clear();
            var list = jsonData.Polygons;

            int index = 0;
            foreach (var item in list)
            {
                var t = MapEditorUtils.CreatePort((index++).ToString(), Root);
                t.position = item.Convert();
                area.Polygons.Add(t);
            }

            area.AreaName = jsonData.AreaName;
            area.AreaName_Key = jsonData.AreaName_Key;
            //colliderTypes = (int)area.colliderTypes;
            area.blockEffect = jsonData.blockEffect;
            area.showType = (BlockShowType)jsonData.shapType;
            area.EffectNums = jsonData.EffectNums;
            area.DeadValid = jsonData.DeadValid;
            area.EnterEventEffect = jsonData.EnterEventEffect;
            area.ExitEventEffect = jsonData.ExitEventEffect;
            area.WaveID = jsonData.WaveID; // 波次ID
            area.ControlID = jsonData.ControlID; //    控制器ID
            if (jsonData.SpawnPointActiveConditions != null)
            {
                area.SpawnPointActiveConditions = jsonData.SpawnPointActiveConditions;
            }
            area.ReshModel();
            go.gameObject.layer = LayerMask.NameToLayer("Entity");
        }

        public override void UnLoad()
        {
            base.UnLoad();
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建区域"))
            {
                CreateChild();
            }

            if (GUILayout.Button("检测出生点唯一"))
            {
                CheckBronPoint();
            }
        }

        public override void CreateChild()
        {
            Vector3 position;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc)
            {
                MapEditor.AreaEditorData editorData = new MapEditor.AreaEditorData(position, AreaShape.Circle, true);
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

            MapEditor.AreaEditorData data = userData as MapEditor.AreaEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.name = data.areaShape.ToString();
            go.transform.SetParent(Root);
            go.transform.position = data.Position;
            Area area = go.AddComponent<Area>();
            MaxIndex++;
            area.AreaID = MaxIndex;
            area.shapType = data.areaShape;
            area.OnAreaShapeChange();
            go.name = "Area_" + area.AreaID;
            area.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Areas.Clear();
            Area[] areas = Root.GetComponentsInChildren<Area>();
            if (areas != null && areas.Length > 0)
            {
                int index = 1;
                foreach (var item in areas)
                {
                    sceneJson.Areas.Add(item.AreaID, new AreaJsonData(index++, item));
                }
            }
        }

        public void CheckBronPoint()
        {
            if (Root == null)
            {
                return;
            }

            int count = 0;
            Area[] areas = Root.GetComponentsInChildren<Area>();
            if (areas != null && areas.Length > 0)
            {
                foreach (var item in areas)
                {
                    if (item.areaType == AreaType.BronPoint)
                    {
                        count++;
                    }
                }
            }

            string message = count > 1 ? "出生点不唯一" : "出生点唯一";
            Debug.Log(message);
            OnShowMessage?.Invoke(message);
        }
    }
}
#endif