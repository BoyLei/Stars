#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapEditorMineView : MapEditorBaseView
    {
        public MapEditorMineView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public long MineID;
        public int MineGroupID;

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            MineID = EditorGUILayout.LongField("矿物ID", MineID);
            MineGroupID = EditorGUILayout.IntField("矿物组ID", MineGroupID);
            if (GUILayout.Button("创建矿物"))
            {
                CreateChild();
            }

            GUILayout.EndHorizontal();
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Mines != null)
                {
                    foreach (var mine in jsonData.Mines)
                    {
                        CreateMine(mine.Value);
                        if (mine.Value.Index > MaxIndex)
                        {
                            MaxIndex = mine.Value.Index;
                        }
                    }
                }
                /*Mine[] mines = Root.GetComponentsInChildren<Mine>();
                if (mines != null && mines.Length > 0)
                {
                    foreach (var item in mines)
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

        private void CreateMine(MineJsonData jsonData)
        {
            var parent = Root.transform;
            if (jsonData.MineGroup != 0)
            {
                parent = Root.transform.Find($"{jsonData.MineGroup}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{jsonData.MineGroup}", Root.transform);
                }
            }
            
            var go = MapEditorUtils.CreatePort(jsonData.Desc, parent);
            go.position = jsonData.Position.Convert();
            if (jsonData.ClientRot != null)
            {
                go.localRotation = Quaternion.Euler(jsonData.ClientRot.Convert());
            }
            else
            {
                go.localRotation = Quaternion.Euler(0,jsonData.Rotation,0);
            }
            var mine = go.gameObject.AddComponent<Mine>();
            mine.Index = jsonData.Index;
            mine.MineID = jsonData.MineID;
            mine.MineGroup = jsonData.MineGroup;
            mine.mineType = (MineType)jsonData.MineType;
            mine.IsShowRange = jsonData.IsShowRange;
            mine.Num = jsonData.Num;
            mine.IgnoreGravity = jsonData.IgnoreGravity;
            mine.Range = jsonData.Range;
            mine.Count = jsonData.Count;
            mine.DefaultVisible = jsonData.DefaultVisible;
            mine.freshType = (FreshType)jsonData.FreshType;
            mine.DeadFreshInterval = jsonData.DeadFreshInterval; //  死亡间隔刷新时间
            mine.OpenServerInterval = jsonData.OpenServerInterval; //   开服间隔刷新时间 
            mine.ResolveFixFreshTimes(jsonData.FixTime); // 固定时刻刷新时间
            mine.WaveID = jsonData.WaveID; // 波次ID
            mine.ControlID = jsonData.ControlID; //    控制器ID
            mine.CanCreateUnreachableArea = jsonData.CanCreateUnreachableArea;

            mine.ReshModel();
            go.gameObject.layer = LayerMask.NameToLayer("Entity");
        }

        public override void CreateChild()
        {
            Vector3 position;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc && EditorConfigUtils.MineCfgs.Mines.ContainsKey(MineID))
            {
                MapEditor.MineEditorData editorData = new MapEditor.MineEditorData(position,
                    EditorConfigUtils.MineCfgs.Mines[MineID],
                    MineGroupID, true);
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

            MapEditor.MineEditorData data = userData as MapEditor.MineEditorData;
            var parent = Root.transform;
            if (data.Group != 0)
            {
                parent = Root.transform.Find($"{data.Group}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{data.Group}", Root.transform);
                }
            }

            GameObject go = new GameObject(data.Mine.GetMineName());
            go.transform.SetParent(parent);
            go.transform.position = data.Position;
            Mine monster = go.AddComponent<Mine>();
            MaxIndex++;
            monster.Index = MaxIndex;
            monster.MineID = data.Mine.ID;
            monster.ReshModel();
            // monster.MineGroup = data.Group;
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Mines.Clear();
            Mine[] mines = Root.GetComponentsInChildren<Mine>();
            if (mines != null && mines.Length > 0)
            {
                foreach (var item in mines)
                {
                    sceneJson.Mines.Add(item.Index, new MineJsonData(item.Index, item));
                    sceneJson.AddSpawner(item.SpawnerID, 3, item.MineID);
                }
            }
        }
    }
}
#endif