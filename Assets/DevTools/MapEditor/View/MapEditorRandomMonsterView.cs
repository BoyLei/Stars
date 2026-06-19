#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace MapEditor
{
    public class MapEditorRandomMonsterView : MapEditorBaseView
    {
        public MapEditorRandomMonsterView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.RandomMonsters != null && jsonData.RandomMonsters.Count > 0)
                {
                    foreach (var monster in jsonData.RandomMonsters)
                    {
                        CreateRandomMonster(monster.Value);
                        if (monster.Value.IndexID > MaxIndex)
                        {
                            MaxIndex = monster.Value.IndexID;
                        }
                    }
                }

                /*RandomMonster[] randomMonsters = Root.GetComponentsInChildren<RandomMonster>();
                if (randomMonsters != null && randomMonsters.Length > 0)
                {
                    foreach (var item in randomMonsters)
                    {
                        if (item.IndexID > MaxIndex)
                        {
                            MaxIndex = item.IndexID;
                        }
                    }
                }*/
                return true;
            }

            return false;
        }

        private void CreateRandomMonster(RandomMonsterJsonData jsonData)
        {
            var go = MapEditorUtils.CreatePort(jsonData.Desc, Root);
            go.position = jsonData.Position.Convert();
            go.localRotation = Quaternion.Euler(0, jsonData.Rotation, 0);
            go.localScale = Vector3.one;

            RandomMonster monster = go.gameObject.AddComponent<RandomMonster>();
            monster.IndexID = jsonData.Index;
            monster.monsterType = (MonsterType)jsonData.MonsterType;
            monster.IsMain = jsonData.IsMain;
            monster.GroupID = jsonData.GroupID;
            monster.mWalkType = (WalkableType)jsonData.WalkType;
            monster.PathID = jsonData.PathID;
            monster.IgnoreGravity = jsonData.IgnoreGravity;
            monster.WalkRange = jsonData.WalkRange;
            monster.Num = jsonData.Num;
            monster.Range = jsonData.Range;
            monster.Count = jsonData.Count;
            monster.freshType = (FreshType)jsonData.FreshType;
            monster.DeadFreshInterval = jsonData.DeadFreshInterval; //  死亡间隔刷新时间
            monster.OpenServerInterval = jsonData.OpenServerInterval; //   开服间隔刷新时间 
            monster.ResolveFixFreshTimes(jsonData.FixTime); // 固定时刻刷新时间
            monster.WaveID = jsonData.WaveID; // 波次ID
            monster.ControlID = jsonData.ControlID; //    控制器ID
            monster.RandomMin = jsonData.RandomMin;
            monster.RandomMax = jsonData.RandomMax;
            monster.CanCreateUnreachableArea = jsonData.CanCreateUnreachableArea;
            monster.ChaseAreaID = jsonData.ChaseAreaId;
            monster.ChaseArea = GetAreaById(jsonData.ChaseAreaId);
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("创建随机怪"))
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

            MapEditor.RandomMonsterEditorData data = userData as MapEditor.RandomMonsterEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            RandomMonster monster = go.AddComponent<RandomMonster>();
            MaxIndex++;
            monster.IndexID = MaxIndex;
            go.name = "RandomMonster_" + monster.IndexID;
            monster.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.RandomMonsters.Clear();
            RandomMonster[] randomMonsters = Root.GetComponentsInChildren<RandomMonster>();
            if (randomMonsters != null && randomMonsters.Length > 0)
            {
                int index = 1;
                foreach (var item in randomMonsters)
                {
                    sceneJson.RandomMonsters.Add(item.IndexID, new RandomMonsterJsonData(index++, item));
                }
            }
        }
    }
}
#endif