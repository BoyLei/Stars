using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProtoMsg
{
    public partial class ItemMD
    {

        public ulong EntityID
        {
            get
            {
                return ID;
            }
        }

        public int SpaceId
        {
            get
            {
                return SpaceID;
            }
        }

        public ulong Heroid
        {
            get
            {
                return HeroID;
            }
        }

        public bool IsNew;//是否是新增(客户端计算，数量变多的时候改变)
        public bool IsNeedCheckQuickEquip;//是否需要检查了快捷穿戴


        public long OldNum;//老的数量


        public long CD;//道具CD（下次可以使用的时间戳毫秒）客户端自己计算


        public int UpdatePoint;//强化点数(道具表里没有)

    }
}


