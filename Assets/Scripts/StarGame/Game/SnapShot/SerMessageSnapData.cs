using System.Collections;
using System.Collections.Generic;
using SGF.Network;
using UnityEngine;

namespace StarProject.Game.SnapShot
{
    /// <summary>
    /// 客户端 收到的消息 快照数据
    /// </summary>
    public class SerMessageSnapData : ServiceSnapshotData
    {
        public MessageHandleData SnapMessageData = null;

        protected override void Release()
        {
            base.Release();
            SnapMessageData = null;
        }

    }
}
