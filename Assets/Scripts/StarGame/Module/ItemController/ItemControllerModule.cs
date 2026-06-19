///--------------------------------------------------------------------
/// �ļ���   :   ItemControllerModule
/// ��  ��   :   ���߹���ģ��
/// ˵  ��   :   ����������
/// �������� :   2022/10/27 18:26:38
/// ������   :   ������
/// ��Ȩ���� :   �ο�����Ƽ��������޹�˾ 
///--------------------------------------------------------------------

using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.DisplayProcess;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using static SGF.Network.FixMessageManager;

namespace StarProject.Module
{
    public class ItemControllerModule : BusinessModule
    {

        ItemMDMgr itemMDMgr;


        public override void Create(object args = null)
        {
            base.Create(args);

            itemMDMgr = (ItemMDMgr)FixMessageManager.Instance.GetMDMgr(FixUpdateDef.Items);

            BindItemControllerMsg();
        }

        protected override void Show(object arg)
        {

        }

        private void BindItemControllerMsg()
        {
            //获取道具刷新
            FixMessageManager.Instance.OnMessage(FixUpdateDef.Items, OnItemRefesh);

            //刷新道具CD
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.UseItemCDInfoNtfID, OnUseItemCDInfoRefesh, this);

            //道具初始化完毕回调
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.SpaceLoadEndNtfID, OnSpaceLoadEndNtf, this);


            GlobalEvent.OnItemChangeToRefeshBag.AddListener(OnItemChangeToRefeshBagAction);

        }


        private void OnItemChangeToRefeshBagAction(object arg0)
        {
            //刷新背包
            ModuleManager.Instance.SendMessage(ModuleDef.Name.BagModule, "OnRefeshBag", new object[] { });

            //刷新改变背包
            ModuleManager.Instance.SendMessage(ModuleDef.Name.MedicineModule, "OnBagChanged", new object[] { });

            //刷新资源bar
            GlobalEvent.OnRefeshCurrencyBar?.Invoke(true);
        }

        private void OnSpaceLoadEndNtf(MessageHandleData data)
        {

            //Debug.LogError("道具加载完毕");
            BusinessManager.Instance.IsInitAllItem = true;
        }

        private void OnUseItemCDInfoRefesh(MessageHandleData data)
        {
            itemMDMgr.OnUseItemCDInfoRefesh(data);
        }

        /// <summary>
        /// ����ˢ�»ص�
        /// </summary>
        /// <param name="data"></param>
        private void OnItemRefesh(FixMessageNotifyData data)
        {


            //修改的
            for (int i = 0; i < data.Changes.Count; i++)
            {
                var item = (ItemMD)data.Changes[i];

                if (item.EquipProps != null)
                {
                    //临时背包不加入
                    if (item.SpaceID != 22)
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.QuickEquipModule, "OnAddQuickEquip", new object[] { item.BaseID, item.EntityID });
                }

                /*
                //New
                if (it.SyncType == 2 || it.SyncType == 5)
                {
                    item.IsNew = true;


                }
                else if (it.SyncType == 3)
                {
                    item.IsNew = false;//登录下发的全关闭new
                }
                */



            }

            //删除的
            for (int i = 0; i < data.Delets.Count; i++)
            {

            }



        }



    }
}
