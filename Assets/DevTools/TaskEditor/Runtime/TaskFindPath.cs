///--------------------------------------------------------------------
/// 文件名   :   TaskFindPath.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 14:34:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using MessagePack;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace Task
{
    [XLua.LuaCallCSharp]
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    public class TaskFindPath
    {
        [IgnoreMember]
        [JsonIgnore]
        [HideInInspector]
        public TaskTypeSerialize TaskTypeInfo = new ();
        public IEnumerable GetFindPathTypes()
        {
            return TaskEnumUtils._e_findpath;
        }

        public IEnumerable GetFindTypes()
        {
            return TaskEnumUtils._e_findtype;
        }

#if UNITY_EDITOR
        [XLua.BlackList]
        public IEnumerable GetFindPathID()
        {
            if (TaskEnumUtils._SceneList.TryGetValue(this.MapID, out var scfg))
            {
                switch (this.FindPathType)
                {
                    case E_FindPath.None:
                        break;
                    case E_FindPath.Spawer:
                        return scfg._spawner;
                    case E_FindPath.NPC:
                        return scfg._npclist;
                    case E_FindPath.InterAction:
                        return scfg._interlist;
                    case E_FindPath.Area:
                        return scfg._arealist;
                    default:
                        break;
                }
            }

            return null;
        }
#endif

        [HideInInspector][Key(0)] public int MapID;

        [LabelText("寻路类型")]
        [ValueDropdown("GetFindPathTypes")]
        [OnValueChanged("OnFindPathType")]
        [Key(1)]
        public E_FindPath FindPathType = E_FindPath.NPC;

        [LabelText("到达半径")]
        [ShowIf("ShouldRange")]
        [SuffixLabel("单位厘米")]
        [Key(2)]
        public int Range;


        [LabelText("参数")]
        [Key(3)]
        [ShowIf("ShouldRange")]

#if UNITY_EDITOR
        [ValueDropdown("GetFindPathID", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle =
"目标")]
#endif
        public long ID;


        [LabelText("寻路方式")]
        [ValueDropdown("GetFindTypes")]
        [ShowIf("ShouldRange")]
        [HideInInspector]
        [Key(4)]
        public E_FindType FindType;

        public bool ShouldRange()
        {
            if (this.FindPathType != E_FindPath.None && this.FindPathType != E_FindPath.JumpTo &&
                   this.FindPathType != E_FindPath.DoGuide &&
                   this.FindPathType != E_FindPath.NPC &&
                   this.FindPathType != E_FindPath.InterAction) 
            {
                return true;
            }
            else
            {
                ID = 0;
                return false;
            }

            
        }

        [LabelText("寻路结束是否开启自动战斗")]
        [ShowIf("IsNotJump")]
        [Key(5)]
        public bool FinishAutoBattle;


        [LabelText("跳转ID")]
        [ShowIf("IsJump")]
        [Key(6)]
#if UNITY_EDITOR
        [ValueDropdown("GetSystemID", NumberOfItemsBeforeEnablingSearch = 3, SortDropdownItems = true, DropdownTitle =
"跳转", DropdownHeight = 200)]
#endif
        public int JumpID;

        //是否是跳转
        public bool IsJump()
        {
            return this.FindPathType == E_FindPath.JumpTo;
        }

        public bool IsNotJump()
        {
            return this.FindPathType != E_FindPath.JumpTo && this.FindPathType != E_FindPath.DoGuide && this.FindPathType != E_FindPath.None;
        }

        [LabelText("道具ID")]
        [ShowIf("IsJump")]
        [Key(7)]
        public int ItemID;

        public bool IsGuide()
        {
            return this.FindPathType == E_FindPath.DoGuide;
        }

        [LabelText("引导ID")]
        [ShowIf("IsGuide")]
        [Key(8)]
        public int GuideID;


        private void OnFindPathType() 
        {
            ID = 0;
            FinishAutoBattle = FindPathType == E_FindPath.Spawer && (TaskTypeInfo.TaskType == TaskType.KillMonster || TaskTypeInfo.TaskType == TaskType.SkillMonsterSource || TaskTypeInfo.TaskType == TaskType.KillMonsterGetItem);
        }

#if UNITY_EDITOR
        [XLua.BlackList]
        public IEnumerable GetSystemID()
        {
            if (this.FindPathType == E_FindPath.JumpTo)
            {
                return TaskEnumUtils.SystemJumps;
            }

            return null;
        }
#endif
    }
}