using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
namespace MapEditor
{
    public class MainPlayer : MonoBehaviour
    {
        private int RoleID = 99999999;
        
        [FoldoutGroup("基础信息", 0)]
        [LabelText("触发器")]
        [ShowInInspector]
        [OnValueChanged("OnCollectionChanged", true)]
        public List<TriggerGroup> TriggerGroups;

        [HideInInspector]
        private List<int> Triggers = new List<int>();


        public void OnCollectionChanged()
        {
            List<int> remove = new List<int>();
            List<int> add = new List<int>();
            if (Triggers != null && Triggers.Count > 0)
            {
                for (int i = 0; i < Triggers.Count; i++)
                {
                    if (!ContainsTrigger(Triggers[i]))
                    {
                        remove.Add(Triggers[i]);
                    }
                }
            }

            if (TriggerGroups != null)
            {
                foreach (var item in TriggerGroups)
                {
                    if (item != null && item.trriger != null && !Triggers.Contains(item.trriger.ID))
                    {
                        add.Add(item.trriger.ID);
                    }
                }
            }

            TrrigerBase[] trrigers = GameObject.FindObjectsOfType<TrrigerBase>();

            if (trrigers != null)
            {
                //构建字典
                Dictionary<int, TrrigerBase> TrrigerDictionary = new Dictionary<int, TrrigerBase>();
                foreach (var item in trrigers)
                {
                    if (!TrrigerDictionary.ContainsKey(item.ID))
                    {
                        TrrigerDictionary.Add(item.ID, item);
                    }
                }

                //删除
                foreach (var item in remove)
                {
                    if (TrrigerDictionary.ContainsKey(item))
                    {
                        TrrigerDictionary[item].DeleteNpc(RoleID);
                    }
                    Triggers.Remove(item);
                }

                //添加
                foreach (var item in add)
                {
                    if (TrrigerDictionary.ContainsKey(item))
                    {
                        TrrigerDictionary[item].AddNpc(RoleID);
                    }
                    Triggers.Add(item);
                }
            }
        }

        private bool ContainsTrigger(int id)
        {
            if (TriggerGroups != null)
            {
                foreach (var item in TriggerGroups)
                {
                    if (item != null && item.trriger != null && item.trriger.ID == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
#endif
