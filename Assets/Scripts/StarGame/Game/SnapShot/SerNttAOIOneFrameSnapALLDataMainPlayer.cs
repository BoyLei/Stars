using ProtoMsg;
namespace StarProject.Game.SnapShot
{
    /// //一帧，全部人的全部数据（主角），一条
    /// 一帧一帧的存储，一帧一帧的取，是一个组
    /// 原来有主角就是有主角只是结构变化，原来没有主角就是主角放进来处理
    /// </summary>
    public class SerNttAOIOneFrameSnapALLDataMainPlayer : ServiceSnapshotData
    {
        #region 1帧内的并行数据

        //主角协议2
        public PropSyncList _propSyncList;
        #endregion

        protected override void Release()
        {
            base.Release();
            _propSyncList = null;
        }
    }
}