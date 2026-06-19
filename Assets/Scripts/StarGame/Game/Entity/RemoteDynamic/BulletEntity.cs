using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.WithOutLife.Summoner
{
    /// <summary>
    /// 具体的子弹，1不带属性（因为目前设计就是碰撞后，直接取得玩家身上的数据）
    /// 类似卡尔
    /// 2,一般都具有表现层
    /// </summary>
    public class BulletEntity : AOIEntityObject
    {
        private string LOG_TAG = "BulletEntity";
        public ulong runtimeID;

        public int bulletID;

        /// <summary>
        /// bullet 对应的逻辑层数据
        /// </summary>
        private NoneVitalSignData m_data;

        public ProtoMsg.BulletCreateRet bulletCreateRet;

        protected bool m_IsDriveByServerStop = false;


        /// <summary>
        /// Command 指令创建实体的接口，此时只创建这个实体，并没有子弹的详细信息
        /// </summary>
        /// <param name="data"></param>
        public void Create(NoneVitalSignData data, Transform container)
        {
            //base.Init(data.M_EntityID, E_EntityType.BulletEntity);

            m_data = data;

            InitRegisterAttribute();

            //走的是AOI创建的子弹,子弹数据都在 Attrs 中,所以先UpdateWithAttr
            //然后再 创建对应的子弹view
            InitWithAttr();
            SGF.Debuger.Log($"{LOG_TAG}: Create  bulletID {bulletID} entityID {data.M_EntityID}");

            //base.Create(E_WithOuLifeResType.Bullet, bulletID, container);
        }
        protected override void InitRegisterAttribute()
        {
            base.InitRegisterAttribute();
        }

        /// <summary> AOI [ID] 变化 </summary>
        private void OnAOIIndexChange(string key)
        {
            bulletID = (int)m_data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, key);
        }
        /// <summary>
        /// AOI 同步的子弹，AOI创建的时候，会带一些朝向、还有配置表id等数据过来，要跟服务器确认
        /// </summary>
        private void InitWithAttr()
        {
            // 此处 如果 子弹 配置了 客户端模拟路径移动, 那这个子弹就不跟随属性路点而移动

        }

        /// <summary>
        /// 收到BulletCreateRet 信息后，创建Bullet运行时。
        /// note:
        /// 由于服务器夏哥告诉我
        /// 现在不需要客户端去 tick(因此没有给子弹的CrateTime)，所有子弹tick产生的效果，
        /// 都会经由 RunBlackRet 通知给客户端。
        /// 客户端收到 RunBlackRet 后，根据RunBlackRet 的 RuntimeID ，可以找到对应的 子弹运行时(客户端只需要根据bullet.runtimeID)，
        /// 然后处理 RunBlackRet 的效果就可以了。
        /// </summary>
        /// <param name="_bulletCreateRet"></param>
        /// <param name="contaienr"></param>
        public void CreateRuntime(ProtoMsg.BulletCreateRet _bulletCreateRet)
        {
            //运行时创建时的消息
            // message BulletCreateRet {
            //     // 拥有者id
            //     optional uint64 OwnerEntityID = 1;
            //     //运行时ID
            //     optional uint64 RuntimeID = 2;
            //     // 子弹ID
            //     optional int32 BulletID = 3;
            //     //开始的黑板数据
            //     repeated BlackBoardNode BlackList = 15;
            // }

            runtimeID = _bulletCreateRet.RuntimeID;
            bulletID = _bulletCreateRet.BulletID;
            bulletCreateRet = _bulletCreateRet;

            SGF.Debuger.Log($"{LOG_TAG}: CreateRuntime runtimeID {runtimeID} , BulletID {bulletID} , OwnerEntityID {_bulletCreateRet.OwnerEntityID} ");
        }

        /// <summary>
        /// 夏哥说 
        ///     不需要客户端本地维护一个计时器去tick，所有的tick由服务器 运行时黑板(RunBlackRet) 推送
        /// </summary>
        internal override void EnterFrame()
        {
            base.EnterFrame();
        }

        protected override void Release()
        {
            SGF.Debuger.Log($"{LOG_TAG}: Release Bullet_{m_data.M_EntityID}");
            runtimeID = 0;
            bulletCreateRet = null;
            m_data = null;
            //GlobalEvent.onAttrSync.RemoveListener(OnAttrSync);
            ViewFactory.ReleaseView(this);
            base.Release();
        }

    }
}
