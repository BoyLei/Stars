///--------------------------------------------------------------------
/// 文件名   :   EffectNpcMove.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/05 17:10:52
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Task
{
    [System.Serializable]
    public class EffectNpcMove : BaseEffect
    {
        [LabelText("地图ID")]
        public int MapID;
        
        [LabelText("NpcIndex")]
        public int NpcIndex;
        
        [OnValueChanged("GetMoveTypes")]
        [LabelText("移动类型")]
        public  MoveType  MoveType;

        [ShowIf("MoveByPath")]
        [LabelText("路径ID")]
        public int PathID;

        [ShowIf("MoveBySpawner")]
        [LabelText("SpawnerID")]
        public int SpawnerID;

        public bool MoveBySpawner()
        {
            return MoveType == MoveType.Spawner;
        }

        public bool MoveByPath()
        {
            return MoveType == MoveType.Path;
        }

        public IEnumerable GetMoveTypes()
        {
            return TaskEnumUtils._movetypes;
        }
        
        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = MapID.ToString();
            effectJson.Args2 = NpcIndex.ToString();
            effectJson.Args3 = ((int)MoveType).ToString();
            if (MoveType == MoveType.Spawner)
            {
                effectJson.Args4 = SpawnerID.ToString();
            }
            else  if (MoveType == MoveType.Path)
            {
                effectJson.Args4 = PathID.ToString();
            }
        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            MapID = ToInt(effectJson.Args1);
            NpcIndex = ToInt(effectJson.Args2);
            MoveType = (MoveType)(ToInt(effectJson.Args3));
            if (MoveType == MoveType.Spawner)
            {
                SpawnerID = ToInt(effectJson.Args4);
            }
            else  if (MoveType == MoveType.Path)
            {
                PathID = ToInt(effectJson.Args4);
            }
        }
    }
}