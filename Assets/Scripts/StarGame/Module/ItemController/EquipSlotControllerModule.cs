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
using StarProject.Service.DisplayProcess;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class EquipSlotControllerModule : BusinessModule
    {

        Dictionary<int, ProtoMsg.EquipSlotData> equipSlots = new Dictionary<int, ProtoMsg.EquipSlotData>();




        public override void Create(object args = null)
        {
            base.Create(args);
            BindEquipSlotControllerMsg();
        }

        protected override void Show(object arg)
        {

        }

        private void BindEquipSlotControllerMsg()
        {

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.EquipSlotAllNoticeID, OnEquipSlotAllNotice, this);

            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.EquipSlotDifferNoticeID, OnEquipSlotDifferNotice, this);

        }


        private void OnEquipSlotDifferNotice(MessageHandleData data)
        {
            EquipSlotDifferNotice EquipSlotDifferNoticeMsg = (EquipSlotDifferNotice)data.data;

            for (int i = 0; i < EquipSlotDifferNoticeMsg.Differ.Count; i++)
            {

                var equipSlot = EquipSlotDifferNoticeMsg.Differ[i];

                equipSlots[equipSlot.SlotID] = equipSlot;

                //refesh UI 
                ModuleManager.Instance.SendMessage(ModuleDef.Name.EquipUpgradeModule, "OnRefeshAllEquipUpgrade", new object[] { });

                GlobalEvent.OnRefeshCurrencyBar?.Invoke(true);
            }


        }


        private void OnEquipSlotAllNotice(MessageHandleData data)
        {
            EquipSlotAllNotice EquipSlotAllNoticeMsg = (EquipSlotAllNotice)data.data;


            for (int i = 0; i < EquipSlotAllNoticeMsg.All.Count; i++)
            {

                var equipSlot = EquipSlotAllNoticeMsg.All[i];


                equipSlots[equipSlot.SlotID] = equipSlot;


                //refesh UI 
                ModuleManager.Instance.SendMessage(ModuleDef.Name.EquipUpgradeModule, "OnRefeshAllEquipUpgrade", new object[] { });



                GlobalEvent.OnRefeshCurrencyBar?.Invoke(true);
            }


        }

        public void ClearEquipSlotDatas()
        {
            equipSlots.Clear();
        }

        public ProtoMsg.EquipSlotData GetEquipSlotData(int slot_id)
        {
            if (!equipSlots.ContainsKey(slot_id))
            {
                return null;
            }
            return equipSlots[slot_id];
        }

        public int GetEquipSlotResonateLv()
        {

            int ResonateLv = 10000;

            //暂时就8个槽位
            for (int i = 1; i <= 8; i++)
            {
                if (equipSlots.ContainsKey(i))
                {
                    if (ResonateLv > equipSlots[i].UpLv)
                    {
                        ResonateLv = equipSlots[i].UpLv;
                    }
                }
                else
                {
                    ResonateLv = 0;
                }
            }

            if (ResonateLv == 10000)
            {
                ResonateLv = 0;
            }

            return ResonateLv;
        }

    }
}
