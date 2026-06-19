using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Task
{
    /// <summary>
    /// 修改道具
    /// </summary>
    [System.Serializable]
    public class EffectUpItem : BaseEffect
    {
        [LabelText("添加道具")]
        //[SerializeField]
        public List<ItemField> AddItems = new List<ItemField>();

        [LabelText("扣除道具")]
        //[SerializeField]
        public List<ItemField> DelItems = new List<ItemField>();



        public override void OnSerialized(EffectJsonData effectJson)
        {
            base.OnSerialized(effectJson);
            effectJson.Args1 = string.Join(';', AddItems);
            effectJson.Args2 = string.Join(';', DelItems);


        }

        public override void OnDeSerialized(EffectJsonData effectJson)
        {
            base.OnDeSerialized(effectJson);
            var s = effectJson.Args1.Split(';');
            for (int i = 0; i+1 < s.Length; i+=2)
            {
                AddItems.Add(new ItemField()
                {
                    ItemID = Convert.ToInt64(s[i]),
                    ItemNum = Convert.ToInt64(s[i + 1]),
                });
            }
             s = effectJson.Args2.Split(';');
            for (int i = 0; i+1 < s.Length; i += 2)
            {
                DelItems.Add(new ItemField()
                {
                    ItemID = Convert.ToInt64(s[i]),
                    ItemNum = Convert.ToInt64(s[i + 1]),
                });
            }
        }
    }

    [System.Serializable]
    public class ItemField
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        [LabelText("道具ID")]
        public long ItemID;

        /// <summary>
        /// 道具数量
        /// </summary>
        [LabelText("道具数量")]
        public long ItemNum;

        public override string ToString()
        {
            return ItemID + ";" + ItemNum;
        }
    }
}
