///--------------------------------------------------------------------
/// 文件名   :   InputKeySerialize.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/16 14:51:25
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace SkillEditor
{
    [System.Serializable]
    public class InputKeySerialize
    {
        [LabelText("输入KEY")]
        [TableList(CellPadding = 3)]
#if UNITY_EDITOR
        [OnCollectionChanged("After")]
#endif
        public List<SaveInput> Savas = new List<SaveInput>();

#if UNITY_EDITOR
        public void After(CollectionChangeInfo info, object value)
        {
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                if (info.Index > -1 && info.Index < Guids.Count)
                {
                    Guids.RemoveAt(info.Index);
                }
            }

            else if (info.ChangeType == CollectionChangeType.Add)
            {
                SaveInput saveInput = info.Value as SaveInput;
                saveInput.Guid = SkillEditorUtils.GeneraGUID();
                Guids.Add(saveInput.Guid);
            }
        }
#endif
        [HideInInspector]
        public List<string> Guids = new List<string>();

        private bool CanInput()
        {
            return Savas.Count == Guids.Count;
        }

        public void OnInput(string guid,string key)
        {
            if (!CanInput())
            {
                Debug.LogError("Savas.Count != Guids.Count");
                return;
            }
            bool removed = string.IsNullOrEmpty(key);
            int index = Guids.IndexOf(guid);

            if (index > -1)
            {
                //存在
                if (removed)
                {
                    Savas.RemoveAt(index);
                    Guids.RemoveAt(index);
                }
                else
                {
                    Savas[index].Key = key;
                }
            }
            else
            {
                //不存在
                if (!removed)
                {
                    Guids.Add(guid);
                    Savas.Add(new SaveInput(guid, key));
                }
            }

        }

        public List<string> GetOutputKey()
        {
            List<string> list = new List<string>();
            foreach (var item in Savas)
            {
                    list.Add(item.Key);
            }
            return list;
        }
    }

    [System.Serializable]
    public class SaveInput
    {
        [LabelText("Guid")]
        [ReadOnly]
        public string Guid;

        [LabelText("输入值")]
        public string Key;

        public SaveInput(string guid, string key)
        {
            this.Guid = guid;
            this.Key = key;
        }
    }
}


