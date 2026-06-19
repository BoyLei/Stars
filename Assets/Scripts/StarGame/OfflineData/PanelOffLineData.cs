using SGF.UI.Framework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

//6文件

namespace StarProject.OffLine//1
{
    [XLua.CSharpCallLua]
    [ShowOdinSerializedPropertiesInInspector]
    public class PanelOffLineData/*2 */: MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization

    {
        //3
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }
        //4
        public Dictionary<string, Transform> BindButtonPos = new();

        public void GenerateNodesData()
        {
            BindButtonPos.Clear();
            UpdateNodesData(gameObject.transform, "");
        }
        public string SPECIALNAME = "";


        //两个问题，开始找对应人维护【不应该】，后续找维护【不必关心别的反而不好维护就是历史的层次结构就好，有key是给别的功能用的，但是具备唯一性它不是节点而是配置和查找key同步不影响修改】。动态被动注册【容易重复多脚本！】，是否第一个的问题【配置解决【1】】
        //思路：1要人维护给dlkey，和策划配置key路径同步dl写死比较麻烦，但是动态修改item这个要配置解决【1】，节省性能，写死代码写死配置要改代码不同步结构不同步策划配置要常年维护
        //2查找性能，要注册按钮比较麻烦，需要改代码，查找损耗新能
        //3第一次用策划知道的结构，结构变策划需要变化，用transform查找也不消耗性能，第二次也能要到，也直接知道结果不用修改代码，要的话第一次比较便宜，第二次更好，使用角度出发这个更合适（这种内存缓存不会持久化缓存本身需求也是内存缓存）

        public Transform GetNewRectAndCacheInMemory(string path)
        {
            Transform tran;
            if (BindButtonPos.TryGetValue(path, out tran))
            {
                return tran;
            }

            tran = transform.Find(path);
            if (tran != null)
            {
                BindButtonPos.Add(path, tran);
                return tran;

            }
            return null;//缓存没有找也找不到啊
            //不缓存了，不是一个类型的，也不消耗性能
            //动态游离再管理的内存之外
            //无所谓
        }

        public void DynamicNewButton(Transform newButtonTransform)
        {
            Stack<string> names = new();

            Transform current = newButtonTransform;

            // check if current object contains PanelOffLineData component
            while (current != null && current.GetComponent<PanelOffLineData>() == null)
            {
                names.Push(current.name); // save current parent's name
                current = current.parent;
            }

            // check if we have reached the root node with PanelOffLineData
            if (current != null && current.GetComponent<PanelOffLineData>() != null)
            {
                // constructing path
                string path = "";
                while (names.Count > 1)  //修改了循环条件
                {
                    path += names.Pop() + "/"; //修改了拼接字符串的位置
                }
                path += names.Pop();  // 在循环结束时添加最后一个节点名

                // at this point, we have path to our new button with all its data
                Debug.Log("Button path: " + path);
                BindButtonPos.Add(path, newButtonTransform);
            }
        }

        /// <summary>
        /// panelofflineData这个节点不算，就比如popWindow/aaa/bbb/ccc/aaaBtn
        /// “aaa/bbb/ccc/aaaBtn”作为Path给我
        /// <param name="path"></param>
        /// <param name="newButtonTransform"></param>
        public void DynamicNewButton(string path, Transform newButtonTransform)
        {
            if (!string.IsNullOrEmpty(path) && !BindButtonPos.ContainsKey(path))
            {
                //尔东自己会找
                //if (newButtonTransform.GetComponent<Button>() != null || newButtonTransform.GetComponent<JButton>() != null)
                {
                    BindButtonPos.Add(path, newButtonTransform);
                }

            }

        }

        //5pub动态
        public void UpdateNodesData(Transform parentTransform, string path)
        {
            int childCount = parentTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                /*     // 对节点名称做全字匹配处理
                             if (child.name == "AssemblyRatio2")
                             {
                                 child.name = "AR2";
                             }

                             if (child.name == "Cut4Cam90")
                             {
                                 child.name = "C90";
                             }*/

                string newPath = path == "" ? child.name : path + "/" + child.name;
                // 检查此子项是否有Button或JButton组件
                //if (child.GetComponent<Button>() != null || child.GetComponent<JButton>() != null)
                {
                    BindButtonPos[newPath] = child;
                }

                UpdateNodesData(child, newPath);
            }
        }

        public Transform GetTransformByKey(string key)
        {
            if (BindButtonPos.ContainsKey(key))
            {
                return BindButtonPos[key];
            }
            return null;
        }


        public void OnEnable()
        {
            var realName = name;
            var t = name.Split("/");
            if (t != null && t.Length > 0)
            {
                realName = t[t.Length - 1];
            }

            if (SPECIALNAME == "")
            {
                GlobalEvent.OnOpenUI.Invoke(realName);
            }
            else
            {
                GlobalEvent.OnOpenUI.Invoke(SPECIALNAME); 
            }
        }

        //加载成功后注册，UIMGR能找到的都是有的
        private void Awake()
        {
            var realName = name;
            var t = name.Split("/");
            if (t != null && t.Length > 0)
            {
                realName = t[t.Length - 1];
            }
            if (SPECIALNAME == "")
            {
                realName = realName.Replace("(Clone)", string.Empty);
                if (UIManager.Instance.PlaneSingleNameTrans.ContainsKey(realName))
                {
                    UIManager.Instance.PlaneSingleNameTrans[realName] = transform;
                }
                else
                {
                    UIManager.Instance.PlaneSingleNameTrans.Add(realName, transform);
                }
            }
            else
            {
                if (UIManager.Instance.PlaneSingleNameTrans.ContainsKey(SPECIALNAME))
                {
                    UIManager.Instance.PlaneSingleNameTrans[SPECIALNAME] = transform;
                }
                else
                {
                    UIManager.Instance.PlaneSingleNameTrans.Add(SPECIALNAME, transform);
                }
            }

            //if (SPECIALNAME == "")
            //{
            //    GlobalEvent.OnOpenUI.Invoke(realName);
            //}
            //else
            //{
            //    GlobalEvent.OnOpenUI.Invoke(SPECIALNAME);
            //}
        }
        //节省开发时间：通过这个脚本，可以避免在代码中重复查找节点。这可以显著减少开发时间，并使代码更加整洁。

        /*  提高代码重用性：这个脚本可以作为一个通用组件在其他项目或场景中使用。只需将这个脚本添加到需要记录离线数据的Prefab上，即可在新的项目中实现相同的功能。

  层级管理：该脚本可以帮助您更好地管理场景中对象的层级结构。您可以根据需要轻松地将Prefab实例添加到场景中的不同层级，确保对象结构正确无误。

  事件和动画：通过一个脚本记录离线数据，开发者可以更容易地对Prefab中的节点添加或删除事件、动画、触发器等组件。这将有助于提高Prefab的功能性，使其更易于进行后期处理和扩展。

  快速调试与性能优化：这个脚本可以帮助您更容易地定位和修复Prefab中的问题，从而加快迭代速度。减少场景内不必要的查询和遍历操作也有助于提高运行时性能。

  维护与升级：如果未来需要修改、升级Prefab或实现新功能，有了这个记录准确离线数据的脚本，可以轻松地找到依赖关系并进行相应修改，降低维护难度。*/
    }
}