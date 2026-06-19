///--------------------------------------------------------------------
/// 文件名   :   Monster
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 11:35:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using OfficeOpenXml;
using UnityEditor;

namespace MapEditor
{
    [HideMonoScript]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class Monster : FreshRule
    {
        private IEnumerable _monsterTypes = new ValueDropdownList<MonsterType>()
        {
            { "普通", MonsterType.Normal },
            { "精英", MonsterType.Elite },
            { "BOSS", MonsterType.Boss },
            { "NPC", MonsterType.NPC },
            { "近战", MonsterType.Melee },
            { "远程", MonsterType.Ranged },
            { "奖励怪", MonsterType.Reward },
        };

        private IEnumerable _walkTypes = new ValueDropdownList<WalkableType>()
        {
            {"闲置",WalkableType.None },
            {"站立",WalkableType.Stand },
            {"自由徘徊" ,WalkableType.WalkInage },
            {"沿路径行走",WalkableType.WalkByPath }
        };

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物ID")]
        [OnValueChanged("OnMonsterIDChange")]
        public long MonsterID;


        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物等级")]
        public int MonsterLevel;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物别名")]
        public string Alias;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物称号")]
        public string Title;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物类型")]
        [ValueDropdown("_monsterTypes")]
        [ReadOnly]
        public MonsterType monsterType;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("是否为主怪")]
        public bool IsMain;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物组ID")]
        public int GroupID;

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
        [Button("刷新模型")]
        public void FreshModel()
        {
            ReshModel();
        }


        public bool ShowPath()
        {
            return mWalkType == WalkableType.WalkByPath;
        }

        public bool ShowRange()
        {
            return mWalkType == WalkableType.WalkInage;
        }

        private GameObject mModel;
        protected override void Awake()
        {
            ReshModel();
        }

        private void OnMonsterIDChange()
        {
            if (EditorConfigUtils.MonsterCfgs.Monsters.ContainsKey(MonsterID))
            {
                monsterType = (MonsterType)EditorConfigUtils.MonsterCfgs.Monsters[MonsterID].Type;
            }
            else
            {
                monsterType = MonsterType.None;
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
        public void ReshModel()
        {
            ClearChild();
            OnMonsterIDChange();
            int modelID = 0;
            if (EditorConfigUtils.MonsterCfgs.Monsters.TryGetValue(MonsterID, out var monsterData))
            {
                modelID = monsterData.ModelID;
                string path = string.Empty;
                if (EditorConfigUtils.ModelCfgs.Models.TryGetValue(modelID, out path))
                {
                    // string path = "Models/VitalSign/" + MonsterID + "/N";
                    if (mModel != null)
                    {
                        GameObject.DestroyImmediate(mModel);
                    }
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset == null)
                    {
                        StarDebug.Log(StarDebug.Purple, $"加载模型失败:{path}");
                        return;
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
                    mModel.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                }
            }


        }

        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = MonsterID;
            excel[row, index++].Value = (int)monsterType;
            excel[row, index++].Value = IsMain;
            excel[row, index++].Value = GroupID;
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.position);
            excel[row, index++].Value = EditorConfigUtils.Vector3ToString(transform.rotation.eulerAngles);
            base.ExportExcel(row, excel, index);
        }
    }
}
#endif
