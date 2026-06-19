using SGF.Network;
using System.Collections.Generic;

namespace StarProject.Game.SnapShot
{
    /// <summary>
    /// 1帧内并行的所有RPC协议
    /// </summary>
    public class SerRPCOneFrameSnapALLData : ServiceSnapshotData
    {
        #region 1帧内的并行数据

        public List<MessageHandleData> _rPCMsgList = new List<MessageHandleData>();

        #endregion

        protected override void Release()
        {
            base.Release();
            _rPCMsgList.Clear();
        }
    }
}
