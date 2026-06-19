///--------------------------------------------------------------------
/// 文件名   :   NPC
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/27 10:02:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using OfficeOpenXml;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace MapEditor
{
    [HideMonoScript]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class NPC : FreshRule
    {
        private IEnumerable _npcTypes = new ValueDropdownList<NPCType>()
        {
            { "普通NPC", NPCType.NORMALNPC },
            { "任务NPC", NPCType.TASKNPC },
            { "护送NPC", NPCType.ESCORTNPC },
            { "对话NPC", NPCType.DIALOGUENPC },
            { "传送NPC", NPCType.TRANSFERNPC },
        };

        private IEnumerable _walkTypes = new ValueDropdownList<WalkableType>()
        {
            {"站立",WalkableType.Stand },
            { "自由徘徊" ,WalkableType.WalkInage },
            { "沿路径行走",WalkableType.WalkByPath }
        };

        [ReadOnly]
        [FoldoutGroup("基础信息", 0)]
        [LabelText("唯一ID")]
        public int Index;


        [FoldoutGroup("基础信息", 0)]
        [LabelText("NPCID")]
        [OnValueChanged("OnMonsterIDChange")]
        public long NpcID;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("NPC类型")]
        [ValueDropdown("_npcTypes")]
        [ReadOnly]
        public NPCType npcType;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("是否为主NPC")]
        public bool IsMain;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("NPC组ID")]
        public int GroupID;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("默认显隐")][SuffixLabel("true 显示  false 隐藏")]
        public bool DefaultVisible = true;
        
        [FoldoutGroup("基础信息", 0)]
        [LabelText("巡逻类型")]
        [ValueDropdown("_walkTypes")]
        public WalkableType mWalkType;


        [FoldoutGroup("基础信息", 0)]
        [LabelText("关联路径")]
        [ShowIf("ShowPath")]
        public int PathID;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("行走范围")]
        [ShowIf("ShowRange")]
        public int WalkRange;


        [FoldoutGroup("基础信息", 0)]
        [LabelText("随机间隔_最小值")]
        public int RandomMin;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("随机间隔_最大值")]
        public int RandomMax;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("是否显示模型")]
        [OnValueChanged("ChangeHide")]
        public bool ShowModle;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("触发器")]
        [ShowInInspector]
        [OnValueChanged("OnCollectionChanged", true)]
        public List<TriggerGroup> TriggerGroups;

        [HideInInspector]
        private List<int> Triggers = new List<int>();


        public void OnCollectionChanged()
        {
            List<int> remove = new List<int>();
            List<int> add = new List<int>();
            if (Triggers != null && Triggers.Count > 0)
            {
                for (int i = 0; i < Triggers.Count; i++)
                {
                    if (!ContainsTrigger(Triggers[i]))
                    {
                        remove.Add(Triggers[i]);
                    }
                }
            }

            if (TriggerGroups != null)
            {
                foreach (var item in TriggerGroups)
                {
                    if (item != null && item.trriger != null && !Triggers.Contains(item.trriger.ID))
                    {
                        add.Add(item.trriger.ID);
                    }
                }
            }

            TrrigerBase[] trrigers = GameObject.FindObjectsOfType<TrrigerBase>();

            if (trrigers != null)
            {
                //构建字典
                Dictionary<long, TrrigerBase> TrrigerDictionary = new Dictionary<long, TrrigerBase>();
                foreach (var item in trrigers)
                {
                    if (!TrrigerDictionary.ContainsKey(item.ID))
                    {
                        TrrigerDictionary.Add(item.ID, item);
                    }
                }

                //删除
                foreach (var item in remove)
                {
                    if (TrrigerDictionary.ContainsKey(item))
                    {
                        TrrigerDictionary[item].DeleteNpc(NpcID);
                    }
                    Triggers.Remove(item);
                }

                //添加
                foreach (var item in add)
                {
                    if (TrrigerDictionary.ContainsKey(item))
                    {
                        TrrigerDictionary[item].AddNpc(NpcID);
                    }
                    Triggers.Add(item);
                }
            }
        }

        private bool ContainsTrigger(int id)
        {
            if (TriggerGroups != null)
            {
                foreach (var item in TriggerGroups)
                {
                    if (item != null && item.trriger != null && item.trriger.ID == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ShowPath()
        {
            return mWalkType == WalkableType.WalkByPath;
        }

        public bool ShowRange()
        {
            return mWalkType == WalkableType.WalkInage;
        }

        [FoldoutGroup("基础信息", 0)]
        [Button("刷新模型")]
        public void FreshModel()
        {
            ReshModel();
        }

        
        
        private GameObject mModel;
        protected override void Awake()
        {
            ReshModel();
        }
        private void OnMonsterIDChange()
        {
            if (EditorConfigUtils.NpcCfgs.Npcs.ContainsKey(NpcID))
            {
                npcType = (NPCType)EditorConfigUtils.NpcCfgs.Npcs[NpcID].Type;
            }
            else
            {
                npcType = NPCType.NORMALNPC;
            }
        }

        private void ClearChild()
        {
            int count = transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);

            }
        }

        public void ReBuildIndex()
        {
            if (MapEditorUtils.SceneConfig!=null)
            {
                int isRepeat = 0;
                NPC[] npcs = MapEditorUtils.SceneConfig.gameObject.GetComponentsInChildren<NPC>();
                if (npcs != null && npcs.Length > 0)
                {
                    foreach (var item in npcs)
                    {
                        if (item.Index == Index)
                        {
                            isRepeat++;
                        }
                    }
                }
                if (isRepeat > 1)
                {
                    Index = MapEditorUtils.SceneConfig.GetNpcIndex();
                }
            }

        }

        public void ReshModel()
        {
            ClearChild();
            OnMonsterIDChange();
            ReBuildIndex();
            int modelID = 0;
            if (EditorConfigUtils.NpcCfgs.Npcs.TryGetValue(NpcID, out var nPCData))
            {
                modelID = nPCData.ModelID;
                string path = string.Empty;
                if (EditorConfigUtils.ModelCfgs.Models.TryGetValue(modelID, out path))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (asset != null)
                        {
                            if (mModel != null)
                            {
                                GameObject.DestroyImmediate(mModel);
                            }

                            mModel = GameObject.Instantiate(asset) as GameObject;
                            mModel.transform.SetParent(transform);
                            mModel.transform.localPosition = Vector3.zero;
                            mModel.transform.localRotation = Quaternion.identity;
                            mModel.transform.localScale = Vector3.one;
                            var offset = mModel.transform.Find("ModelOffset");
                            if (offset != null)
                            {
                                offset.transform.localPosition = Vector3.zero;
                                offset.transform.localRotation = Quaternion.identity;
                                offset.transform.localScale = Vector3.one;
                            }

                            if (ShowModle)
                            {
                                mModel.hideFlags = HideFlags.DontSave;
                            }
                            else
                            {
                                mModel.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                            }
                           
                        }
                    }
                }
            }
        }

        private void ChangeHide()
        {
            if (mModel != null)
            {
                if (ShowModle)
                {
                    mModel.hideFlags = HideFlags.DontSave;
                }
                else
                {
                    mModel.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                }
            }
        }
        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = NpcID;
            excel[row, index++].Value = (int)npcType;
            excel[row, index++].Value = IsMain;
            excel[row, index++].Value = GroupID;
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.position);
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.rotation.eulerAngles);
            base.ExportExcel(row, excel, index);
        }
    }

    [System.Serializable]
    public class TriggerGroup
    {

        [LabelText("触发器ID")]
        [ReadOnly]
        public int TriggerID;

        [LabelText("TrrigerTransform")]
        [Newtonsoft.Json.JsonIgnore]
        [OnValueChanged("OnValueChanged")]
        public TrrigerBase trriger;

        [LabelText("可执行效果")]
        [HideInTables]
        public List<TrrigerEffect> TrrigerEffects;


        private void OnValueChanged()
        {
            if (trriger == null)
            {
                TriggerID = 0;
            }
            else
            {
                TriggerID = trriger.ID;
            }
        }
    }
}


#endif