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
    public class RandomMonster : FreshRule
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
        [LabelText("索引ID")]
        [ReadOnly]
        public int IndexID;

        [FoldoutGroup("基础信息", 0)]
        [LabelText("怪物类型")]
        [ValueDropdown("_monsterTypes")]
        public MonsterType monsterType=MonsterType.Normal;

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
            var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            model.layer = LayerMask.NameToLayer("Entity");
            model.hideFlags = HideFlags.HideInHierarchy;
        }

        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            excel[row, index++].Value = IndexID;
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