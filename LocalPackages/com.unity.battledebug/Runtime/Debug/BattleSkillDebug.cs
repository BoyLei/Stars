///--------------------------------------------------------------------
/// 文件名   :   BattleSkillDebug.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/02 09:49:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
namespace BattleDebug
{
    public class BattleSkillDebug
    {
        public int SkillID { get; private set; }
        public bool CanModify { get; private set; }
        public Dictionary<string, StageDebug> Stages { get; private set; }


        public System.DateTime SkillTime;


        public BattleSkillDebug()
        {
            Stages = new Dictionary<string, StageDebug>();
            CanModify = true;
            SkillTime = System.DateTime.Now;
        }


        public BattleSkillDebug(int skillID)
        {
            SkillID = skillID;
            Stages = new Dictionary<string, StageDebug>();
            CanModify = true;
            SkillTime = System.DateTime.Now;
        }

        public StageDebug GetStageDebug(string UniqueID)
        {
            if (!ContainsStage(UniqueID))
            {
                return null;
            }
            return Stages[UniqueID];
        }

        public void OnEnd()
        {
            CanModify = false;
        }

        private bool ContainsStage(string UniqueID)
        {
            return Stages.ContainsKey(UniqueID);
        }

        public void OnModify(StageEnum stage, DebugData debugData)
        {
            if (!CanModify)
            {
                Debug.LogError($"技能{SkillID}已经结束不能修改");
                return;
            }

            ///开始
            if (debugData.IsStart)
            {
                //
                if (!ContainsStage(debugData.UniqueID))
                {
                    StageDebug debug = new StageDebug(debugData.UniqueID, stage);
                    Stages.Add(debugData.UniqueID, debug);
                }
                Stages[debugData.UniqueID].EndTime = Stages[debugData.UniqueID].StarTime + 0.5f;
                if (debugData.Args != null && debugData.Args.Count > 0)
                {
                    for (int i = 0; i < debugData.Args.Count; i++)
                    {
                        Stages[debugData.UniqueID].Modify(debugData.Args[i]);
                    }
                }
            }
            else
            {
                if (ContainsStage(debugData.UniqueID))
                {
                    if (debugData.Args != null && debugData.Args.Count > 0)
                    {
                        for (int i = 0; i < debugData.Args.Count; i++)
                        {
                            Stages[debugData.UniqueID].Modify(debugData.Args[i]);
                        }
                    }
                    Stages[debugData.UniqueID].OnEnd();
                }
            }
        }
    }
}