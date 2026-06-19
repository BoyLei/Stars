using ProtoMsg;
using SGF.Network;
using SGF.Unity;
using StarProject.Game.Data;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.WithOutLife.Summoner;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Player
{
    /// <summary>
    /// 子弹实体的控制层，控制的是单个子弹实体
    /// </summary>
    public class BulletEntityCtrl : EntityCtrlBase
    {
        protected new string LOG_TAG = "BulletEntityCtrl";


        /// <summary>
        /// bulletCtr的数据层
        /// </summary>
        new NoneVitalSignData m_data;

        BulletEntity bullet;


        public override AOIEntityObject M_Curr => bullet;


        public override void Create(EntityBaseData data, UnityEngine.Vector3 pos)
        {
            base.Create(data, pos);
            m_data = (NoneVitalSignData)data;

            //创建bullet 实体
            bullet = EntityFactory.InstanceEntity<BulletEntity>();
            m_data.myOwnerNtt = bullet;
            m_data.myOwnerNttGroup = this;

            bullet.ActionOnSyncBorthPos += SyncBornPos;


            // 子弹已经改为召唤物了,
            bullet.Create(m_data, m_container.transform);

            // SGF.Debuger.Log($"{LOG_TAG} Create  id = {data.M_EntityID} ");

        }

        public override void Release()
        {
            base.Release();
            ViewFactory.ReleaseView(bullet);
            bullet = null;


            GameObject.Destroy(m_container);
            m_container = null;
        }

        public override void EnterFrame(int frameIndex)
        {
            bullet.EnterFrame();
        }

        protected override void CreateMContainer(EntityBaseData data)
        {
            m_container = new GameObject("BulletCtr" + data.M_EntityID);
            m_container.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
        }


        #region  BulletCtr收到的rpcMsg
        /// <summary>
        /// 按服务器的意思，子弹的创建由AOI同步来 告知
        /// 而BulletCreateRet 通知的是子弹 运行时的创建
        /// </summary>
        /// <param name="data"></param>
        public override void OnBulletCreateRet(MessageHandleData data)
        {

            BulletCreateRet bulletCreateRet = (BulletCreateRet)data.data;
            SGF.Debuger.Log($"{LOG_TAG} OnBulletCreateRet  id = {bulletCreateRet.RuntimeID} ");

            bullet.CreateRuntime(bulletCreateRet);
        }

        public override void OnBulletEndRet(MessageHandleData data)
        {
            BulletEndRet bulletEndRet = (BulletEndRet)data.data;

            {
                //OwnerEntityID 就是子弹的entityID
                ulong enityId = bulletEndRet.OwnerEntityID;


                GameCommand gameCommand = GameManager.Instance.gameCommand.Init(E_Command.Destroy, enityId, null);
                gameCommand.isServerAOI = false;
                DelayInvoker.DelayInvokerOnEndOfFrame(delegate
                {
                    GameManager.Instance.EntityDataCommand(gameCommand);
                }, null);


                //SnapShotUtils.StopEffects(bulletEndRet.RuntimeID);
                SGF.Debuger.Log($"{LOG_TAG}: OnBulletEndRet M_EntityID {enityId} ,StopEffects RuntimeID {bulletEndRet.RuntimeID}  ");
            }
        }


        protected override void SyncBornPos(UnityEngine.Vector3 pos)
        {
            M_Curr.MoveByServerNew(pos, false, true);
            // 子弹 这块暂时 不会使用
        }


        #endregion
    }
}
