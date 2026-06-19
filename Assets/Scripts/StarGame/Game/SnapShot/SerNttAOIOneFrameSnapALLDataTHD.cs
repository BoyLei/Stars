using ProtoMsg;
using System.Collections.Generic;

namespace StarProject.Game.SnapShot
{
    /// <summary>
    /// 过程化还原：1技能，2路点角度
    /// 实际装配在角色数据中
    /// 【移动过程中弱能发生唯一，或不可能】
    /// 【都需要抉择那个为优先级，不然要等两个，暂移动为唯一，时间最长的结束点】【结束complete必须结束】
    /// 一帧的全部数据
    /// 
    /// //一帧，全部人的全部数据（非主角），一条
    /// 一帧一帧的存储，一帧一帧的取，是一个组
    /// 原来有主角就是有主角只是结构变化，原来没有主角就是主角放进来处理
    /// </summary>
    public class SerNttAOIOneFrameSnapALLDataTHD : ServiceSnapshotData
    {
        #region 1帧内的并行数据
        //请给我一帧，所有人的，所有数据
        //除了主角
        public List<AOIMsg> _aOIMsgList = new List<AOIMsg>();
        public ulong _frameIndex = 0;

      

        ////主角协议1
        //public UserMainDataNotify _userMainDataNotify;
        //主角协议2
        //public PropSyncList _propSyncList;
        #endregion


        protected override void Release()
        {
            base.Release();
            _aOIMsgList.Clear();
            _frameIndex = 0;

        }
    }
}