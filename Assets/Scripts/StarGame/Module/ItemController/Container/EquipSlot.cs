using Google.Protobuf;
using ProtoMsg;
using StarProject.Game.Data;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;


namespace StarProject.Module
{

    [XLua.LuaCallCSharp]

    //装备格子类
    public class EquipSlot:BaseSlot
    {
        //是否已经装备
        public bool IsEquiped;

       
        public void Equip(ulong entityID)
        {
            IsEquiped = true;
            EntityID = entityID;
            TipsType = TipsType.Equip;
        }

        public void UnEquip()
        {
            IsEquiped = false;
            EntityID = 0;
        }

    }
}
