using Google.Protobuf;
using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;


namespace StarProject.Module
{

    [XLua.LuaCallCSharp]

    //护符格子类
    public class AmuletSlot : BaseSlot
    {
        public int AmuletSlotType;//护符种类(0为都可以，1为武器，2为防具)


        public bool IsSelected;//是否被选中


        //是否已经装备
        public bool IsEquiped;//是否已经装备

        /// <summary>
        /// 初始化护符格子
        /// </summary>
        public override void Init(int id,int size, int state = 1, int maxStack = 0, int posX = 0, int posY = 0)
        {
            base.Init(id,size, state, maxStack, posX, posY);
            IsSelected = false;
            IsEquiped = false;
            TipsType = TipsType.Amulet;
        }

        /// <summary>
        /// 设置护符的槽位种类
        /// </summary>
        /// <param name="amuletSlotType"></param>
        public void SetAmuletSlotType(int amuletSlotType)
        {
            AmuletSlotType = amuletSlotType;
        }

        /// <summary>
        /// 装备护符
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="baseID"></param>
        public void Equip(long baseID,ulong entityID)
        {
            PutIn(baseID, entityID,1);
            IsEquiped = true;
        }

        /// <summary>
        /// 卸载护符
        /// </summary>
        public void UnEquip()
        {
            CancelBind();
            IsEquiped = false;
        }

    }
}
