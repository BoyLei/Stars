#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    public class MapEditorMonsterView : MapEditorBaseView
    {
        public MapEditorMonsterView(string viewName, System.Action<string> action) : base(viewName, action)
        {
        }

        private long MonsterID;
        private int MonsterGroupID;

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                if (jsonData!=null && jsonData.Monsters != null)
                {
                    foreach (var monster in jsonData.Monsters)
                    {
                        CreateMonster(monster.Value);
                    }
                }

                return true;
            }

            return false;
        }

        private void CreateMonster(MonsterJsonData jsonData)
        {
            Transform parent = Root;
            if (jsonData.GroupID != 0)
            {
                parent = Root.transform.Find($"{jsonData.GroupID}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{jsonData.GroupID}", Root);
                    ;
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

            var monster = go.gameObject.AddComponent<Monster>();
            monster.MonsterID = jsonData.MonsterID;
            monster.ChaseAreaID = jsonData.ChaseAreaId;
            monster.ChaseArea = GetAreaById(jsonData.ChaseAreaId);
            monster.MonsterLevel = jsonData.MonsterLevel;
            monster.Alias = jsonData.Alias;
            monster.Title = jsonData.Title;
            monster.monsterType = (MonsterType)jsonData.MonsterType;
            monster.IsMain = jsonData.IsMain;
            monster.GroupID = jsonData.GroupID;
            monster.mWalkType = (WalkableType)jsonData.WalkType;
            monster.PathID = jsonData.PathID;
            monster.WalkRange = jsonData.WalkRange;
            monster.IgnoreGravity = jsonData.IgnoreGravity;
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

            monster.ReshModel();
            go.gameObject.layer = LayerMask.NameToLayer("Entity");
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            MonsterID = EditorGUILayout.LongField("怪物ID", MonsterID);
            MonsterGroupID = EditorGUILayout.IntField("怪物组ID", MonsterGroupID);
            if (GUILayout.Button("创建怪物"))
            {
                CreateChild();
            }

            GUILayout.EndHorizontal();
        }

        public override void CreateChild()
        {
            Vector3 position = Vector3.zero;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc && EditorConfigUtils.MonsterCfgs.Monsters.ContainsKey(MonsterID))
            {
                MapEditor.MonsterEditorData editorData = new MapEditor.MonsterEditorData(position,
                    EditorConfigUtils.MonsterCfgs.Monsters[MonsterID], MonsterGroupID, suc);
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

            Transform parent = Root;
            MapEditor.MonsterEditorData data = userData as MapEditor.MonsterEditorData;
            if (data.Group != 0)
            {
                parent = Root.transform.Find($"{data.Group}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{data.Group}", Root);
                    ;
                }
            }

            GameObject go = new GameObject(data.Monster.GetMonsterName());
            go.transform.SetParent(parent);
            go.transform.position = data.Position;
            Monster monster = go.AddComponent<Monster>();
            monster.MonsterID = data.Monster.ID;
            monster.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Monsters.Clear();
            Monster[] monsters = Root.GetComponentsInChildren<Monster>();
            if (monsters != null && monsters.Length > 0)
            {
                int index = 1;
                foreach (var item in monsters)
                {
                    sceneJson.Monsters.Add(index++, new MonsterJsonData(index, item));
                    sceneJson.AddSpawner(item.SpawnerID, 1, item.MonsterID);
                }
            }
        }
    }
}
#endif