#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace MapEditor
{
    using UnityEngine;

    public class MapEditorNpcView : MapEditorBaseView
    {
        public MapEditorNpcView(string viewName, System.Action<string> action) : base(viewName, action)
        {
        }

        private long NpcID;
        private int NpcGroupID;

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            NpcID = EditorGUILayout.LongField("NpcID", NpcID);
            NpcGroupID = EditorGUILayout.IntField("Npc组ID", NpcGroupID);
            if (GUILayout.Button("创建NPC"))
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
                if (jsonData!=null && jsonData.Npcs != null && jsonData.Npcs.Count > 0)
                {
                    foreach (var npcJson in jsonData.Npcs)
                    {
                        CreateByNpcJson(npcJson.Value);
                    }
                }

                NPC[] npcs = Root.GetComponentsInChildren<NPC>();
                if (npcs != null && npcs.Length > 0)
                {
                    foreach (var item in npcs)
                    {
                        if (item.Index > MaxIndex)
                        {
                            MaxIndex = item.Index;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private void CreateByNpcJson(NPCJsonData jsonData)
        {
            Transform parent = Root.transform;
            if (jsonData.GroupID != 0)
            {
                parent = Root.transform.Find($"{jsonData.GroupID}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{jsonData.GroupID}", Root.transform);
                }
            }

            GameObject go = new GameObject(jsonData.Desc);
            go.transform.SetParent(parent);
            if (jsonData.ClientRot != null)
            {
                go.transform.localRotation = Quaternion.Euler(jsonData.ClientRot.Convert());
            }
            else
            {
                go.transform.localRotation = Quaternion.Euler(0,jsonData.Rotation,0);
            }
            go.transform.position = jsonData.Position.Convert();
            NPC npc = go.AddComponent<NPC>();
            npc.Index = jsonData.Index;
            npc.NpcID = jsonData.NpcID;
            npc.npcType = (NPCType)jsonData.NpcType;
            npc.IsMain = jsonData.IsMain;
            npc.DefaultVisible = jsonData.DefaultVisible;
            npc.GroupID = jsonData.GroupID;
            npc.mWalkType = (WalkableType)jsonData.WalkType;
            npc.PathID = jsonData.PathID;
            npc.WalkRange = jsonData.WalkRange;
            npc.Num = jsonData.Num;
            npc.IgnoreGravity = jsonData.IgnoreGravity;
            npc.Range = jsonData.Range;
            npc.Count = jsonData.Count;
            npc.freshType = (FreshType)jsonData.FreshType;
            npc.DeadFreshInterval = jsonData.DeadFreshInterval; //  死亡间隔刷新时间
            npc.OpenServerInterval = jsonData.OpenServerInterval; //   开服间隔刷新时间 
            npc.ResolveFixFreshTimes(jsonData.FixTime); // 固定时刻刷新时间
            npc.WaveID = jsonData.WaveID; // 波次ID
            npc.ControlID = jsonData.ControlID; //    控制器ID
            npc.RandomMin = jsonData.RandomMin;
            npc.RandomMax = jsonData.RandomMax;
            npc.CanCreateUnreachableArea = jsonData.CanCreateUnreachableArea;
            if (npc.TriggerGroups == null)
            {
                npc.TriggerGroups = new List<TriggerGroup>();
            }

            npc.TriggerGroups.Clear();
            if (jsonData.TriggerGroups != null && jsonData.TriggerGroups.Count > 0)
            {
                foreach (var item in jsonData.TriggerGroups)
                {
                    npc.TriggerGroups.Add(CreateTriggerGroup(item));
                }
            }

            npc.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
        }

        public TrrigerBase GetTrrigerBase(int TriggerID)
        {
            TrrigerBase[] trrigerBases = Parent.GetComponentsInChildren<TrrigerBase>();
            if (trrigerBases != null && trrigerBases.Length > 0)
            {
                for (int i = 0; i < trrigerBases.Length; i++)
                {
                    if (trrigerBases[i].ID == TriggerID)
                    {
                        return trrigerBases[i];
                    }
                }
            }

            return null;
        }

        private TriggerGroup CreateTriggerGroup(TriggerGroupJsonData jsonData)
        {
            TriggerGroup group = new TriggerGroup();
            group.TriggerID = jsonData.TriggerID;
            group.trriger = GetTrrigerBase(jsonData.TriggerID);
            if (group.TrrigerEffects == null)
            {
                group.TrrigerEffects = new List<TrrigerEffect>();
            }

            group.TrrigerEffects.Clear();

            if (jsonData.Effects != null && jsonData.Effects.Count > 0)
            {
                foreach (var item in jsonData.Effects)
                {
                    group.TrrigerEffects.Add(item.GetTrrigerEffect());
                }
            }

            return group;
        }

        public override void CreateChild()
        {
            Vector3 position = Vector3.zero;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc && EditorConfigUtils.NpcCfgs.Npcs.ContainsKey(NpcID))
            {
                MapEditor.NPCEditorData editorData =
                    new MapEditor.NPCEditorData(position, EditorConfigUtils.NpcCfgs.Npcs[NpcID], NpcGroupID, suc);
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

            Transform parent = Root.transform;
            MapEditor.NPCEditorData data = userData as MapEditor.NPCEditorData;
            if (data.Group != 0)
            {
                parent = Root.transform.Find($"{data.Group}");
                if (parent == null)
                {
                    parent = MapEditorUtils.CreatePort($"{data.Group}", Root.transform);
                }
            }

            GameObject go = new GameObject(data.Npc.GetNpcName());
            go.transform.SetParent(parent);
            go.transform.position = data.Position;
            NPC npc = go.AddComponent<NPC>();
            MaxIndex++;
            npc.Index = MaxIndex;
            npc.NpcID = data.Npc.ID;
            npc.ReshModel();
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Npcs.Clear();
            NPC[] npcs = Root.GetComponentsInChildren<NPC>();
            if (npcs != null && npcs.Length > 0)
            {
                foreach (var item in npcs)
                {
                    sceneJson.Npcs.Add(item.Index, new NPCJsonData(item.Index, item));
                    sceneJson.AddSpawner(item.SpawnerID, 2, item.NpcID);
                }
            }
        }
    }
}
#endif