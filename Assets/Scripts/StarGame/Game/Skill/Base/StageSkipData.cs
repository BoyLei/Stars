using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 阶段 跳转 的数据, 后面 要将这种技能 里的 基础数据对象 采用共同的 池基类
    /// </summary>
    public class StageSkipData
    {
        public int StageID = 0;
        public int LoopIdx = 0;
        public E_SkipToStageType SkipToStageType = E_SkipToStageType.None;

        public StageSkipData(int skipStageID, int loopIdx, E_SkipToStageType skipToStageType)
        {
            StageID = skipStageID;
            LoopIdx = loopIdx;
            SkipToStageType = skipToStageType;
        }
    }
}
