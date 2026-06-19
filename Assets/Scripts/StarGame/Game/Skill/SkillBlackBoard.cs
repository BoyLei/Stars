using Google.Protobuf.Collections;
using ProtoMsg;
using SGF;
using SGF.Network;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StarProject.Game.Skill
{
    /// <summary>
    /// 黑板数据的标签枚举
    /// </summary>
    public enum E_BlackBoardTag
    {
        Client,
        Server,
        Reg,
        /// <summary>
        /// 黑板数据的标签枚举
        /// </summary>
        ExecuteResult,
    }

    /// <summary>
    /// 黑板数据的节点的状态
    /// </summary>
    [Flags]
    public enum E_BlackBoardNodeState
    {
        /// <summary>
        /// 正常开启
        /// </summary>
        Open,
        /// <summary>
        /// 服务器已经执行了这个效果
        /// </summary>
        ServerUsed,
        /// <summary>
        /// 客户端已经执行了这个效果
        /// </summary>
        ClientUsed,
        /// <summary>
        /// 效果节点已经被客户端关闭
        /// </summary>
        ClientClosed,
        /// <summary>
        /// 效果节点已经被服务器关闭
        /// </summary>
        ServerClosed,
    }

    /// <summary>
    /// 封装的 存放服务器 黑板节点数据的一个结构,用来保持客户端和服务器对于黑板节点数据结构的统一
    /// </summary>
    public class CustomBlackBoardNode
    {
        public string Key;

        public object Value;

        public bool IsOpen => state == E_BlackBoardNodeState.Open;

        /// <summary>
        /// 效果状态. 默认每个效果都是 Open状态.
        /// note:
        ///     目前 state的使用 存在一些限制,目前主要是 用来处理 客户端/服务器共同执行的 延时效果的状态标识.
        /// 
        ///     对于任何的一个状态 state, 开启后默认都是 open状态. 对于预输入这种, 如果 客户端主动触发/ 服务器主动触发,
        ///     state 会分别记录 为 clientUse/ ServerUse. 
        ///
        ///     对于普通的效果,目前不需要单独设置这个状态。
        /// </summary>
        public E_BlackBoardNodeState state;

        /// <summary>
        /// 效果执行 输出的结果,目前暂定用 一个bool值来表示True/False两种结果.
        /// 如果一个效果,客户端 提前预演 执行了,当收到服务器效果数据后,
        /// 检查客户端执行的结果是否与服务器的一致.
        /// note:
        ///     如果不一致,就需要做效果的恢复逻辑！！！
        ///     如果不一致,就需要做效果的恢复逻辑！！！
        ///     如果不一致,就需要做效果的恢复逻辑！！！
        /// </summary>
        public bool OutputResult;

        /// <summary>
        /// 这个黑板数据的 builderID
        /// </summary>
        public ulong BuilderID;

        /// <summary>
        /// 这个黑板数据的 Owner
        /// </summary>
        public ulong OwnerEntityID;

        public CustomBlackBoardNode(string key, object value)
        {
            Key = key;
            Value = value;
            state = E_BlackBoardNodeState.Open;
        }

    }

    /// <summary>
    /// 客户端自定义的黑板基类
    /// </summary>
    public class BaseBlackBoard
    {
        private static StringBuilder sb = new StringBuilder(64);
        private static string _clientKey = "client_";
        private static string _serverKey = "server_";

        private static string _regKey = "reg_";
        private static string _executeResult = "executeResult_";

        /// <summary>
        /// 配置表ID 的key. 
        /// 目前主要是 为了存储 技能/被动/buff 的 配置表ID.
        /// </summary>
        public static string KEY_CFG_ID = "key_cfg_id";

        /// <summary>
        /// KEY: 跳转到 对应阶段ID的 key
        /// </summary>
        public static string KEY_SKIP_TO_STAGEID = "key_skip_to_stageid";

        /// <summary>
        /// KEY: 用户输入的 key
        /// </summary>
        public static string KEY_USER_INPUT = "key_user_input";
        /// <summary>
        /// KEY: 用户输入效果 缓存cache的key(比如在存在活跃技能的时候,
        ///      按了一个有输入轴的技能,此时需要将此技能缓存,等待技能变为非活跃时,执行缓存逻辑)
        /// </summary>
        public static string KEY_USER_INPUT_CACHE = "key_user_input_cache";
        /// <summary>
        /// KEY: 蓄力的标签
        /// </summary>
        public static string KEY_ENERGY = "key_energy";

        /// <summary>
        /// 格式化 存入客户端/服务器黑板的key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="tag">存入黑板的标签</param>
        private static string FormatBlackKey(string key, E_BlackBoardTag tag)
        {
            string realyKey = GetCacheFromateBlackKey(tag, key);
            return realyKey;
        }

        private static string tmpStr = "";
        public static Dictionary<E_BlackBoardTag, Dictionary<string, string>> BlackKeyCache = new();
        private static string GetCacheFromateBlackKey(E_BlackBoardTag tag, string key)
        {
            if (!BlackKeyCache.ContainsKey(tag))
            {
                BlackKeyCache.Add(tag, new());
            }

            if (!BlackKeyCache[tag].ContainsKey(key))
            {
                tmpStr = "";
                switch (tag)
                {
                    case E_BlackBoardTag.Client:
                        {
                            tmpStr = _clientKey;
                        }
                        break;
                    case E_BlackBoardTag.Server:
                        {
                            tmpStr = _serverKey;
                        }
                        break;
                    case E_BlackBoardTag.Reg:
                        {
                            tmpStr = _regKey;
                        }
                        break;
                    case E_BlackBoardTag.ExecuteResult:
                        {
                            tmpStr = _executeResult;
                        }
                        break;
                }
                // 目前 对黑板 key 的 要求是 <= 40位字符, 尽量不超过 32 位字符. 
                // extraKey + key 的 字符串长度 <= 64
                BlackKeyCache[tag].Add(key, sb.Clear().Append(tmpStr).Append(key).ToString());
            }

            return BlackKeyCache[tag][key];
        }

        public static CustomBlackBoardNode InitServerCustomBlackBoardNode(string key, BlackBoardNode value, ulong builderID, ulong ownerEntityID)
        {
            try
            {
                // 解析黑板的时候发现 服务器可能发 空类型的 黑板过来， 此时跟夏哥的约定是服务器过滤一遍, 客户端自己也过滤
                object message = ProtoUtils.DeserializeBlackBoardCommon<object>(value);
                CustomBlackBoardNode customBlackBoardNode = new CustomBlackBoardNode(key, message);
                customBlackBoardNode.BuilderID = builderID;
                customBlackBoardNode.OwnerEntityID = ownerEntityID;
                return customBlackBoardNode;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static CustomBlackBoardNode InitClientCustomBlackBoardNode(string key, object value, ulong builderID, ulong ownerEntityID)
        {
            CustomBlackBoardNode customBlackBoardNode = new CustomBlackBoardNode(key, value);
            customBlackBoardNode.BuilderID = builderID;
            customBlackBoardNode.OwnerEntityID = ownerEntityID;
            return customBlackBoardNode;
        }

        protected virtual string TagFlag { get; }
        public E_ULayerSubState state;



        public BaseBlackBoard parent;

        /// <summary>
        /// 一个阶段 的黑板 效果数量 不会特别多, 所以 设置初始化容量为 16 .如果容量不过, dic 会自动扩容
        /// </summary>
        public Dictionary<string, object> clientBackBord = new Dictionary<string, object>(16);
        public Dictionary<string, object> serverBlackBord = new Dictionary<string, object>(16);
        public Dictionary<string, object> regBlackBord = new Dictionary<string, object>(16);

        /// <summary>
        /// 服务器效果线 注册效果的 index, 目前 服务器效果线的 reg数据 是一个map结构，没有顺序．
        /// 而　服务器　黑板　同步过来的　blackList 也是一个map,没有顺序. 
        /// 比如 服务器 可能发来一个 [Move2,Move1], 但它的顺序应为 Move1-->Move2.
        /// 为了 处理服务器 数据无序 的问题, 本地 在 每次reg 服务器效果线的时候,都去生成 一个它 regIndex,
        /// 这样, 在 触发 服务器 效果线之前, 就可以根据 regIndex 对黑板 做一次排序, 从而客户端依次执行。
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        public Dictionary<string, int> serverRegIndexDic = new Dictionary<string, int>(16);

        private int serverRegIndex = 0;

        public void Clear()
        {
            clientBackBord.Clear();
            serverBlackBord.Clear();
            regBlackBord.Clear();

            serverRegIndexDic.Clear();
            serverRegIndex = 0;

            parent = null;
        }



        public Dictionary<string, object> GetBlackBoard(E_BlackBoardTag tag)
        {
            switch (tag)
            {
                case E_BlackBoardTag.Client:
                    {
                        return clientBackBord;
                    }
                case E_BlackBoardTag.Server:
                    {
                        return serverBlackBord;
                    }
                case E_BlackBoardTag.Reg:
                    {
                        return regBlackBord;
                    }
                default:
                    {
                        return clientBackBord;
                    }
            }
        }

        private void set(string key, object value, E_BlackBoardTag tag)
        {
            if (string.IsNullOrEmpty(key))
            {
                SGF.Debuger.LogWarning($"BaseBlackBoard tag={tag},key=null");
                return;
            }
            GetBlackBoard(tag)[key] = value;
        }


        private object get(string key, E_BlackBoardTag tag)
        {
            object value = null;
            if (GetBlackBoard(tag).TryGetValue(key, out value))
            {
                return value;
            }

            // 如果找不到的时候,继续向上找
            if (parent != null)
            {
                return parent.get(key, tag);
            }

            return null;
        }


        private void remove(string key, E_BlackBoardTag tag)
        {
            Dictionary<string, object> blackBoard = GetBlackBoard(tag);
            if (blackBoard.ContainsKey(key))
            {
                blackBoard.Remove(key);
                return;
            }

            // 如果当前 blackBoard 不存在要删除的key, 就往 父级黑板里查找
            if (parent != null)
            {
                parent.remove(key, tag);
            }
        }

        private bool contain(string key, E_BlackBoardTag tag)
        {
            bool isContain = GetBlackBoard(tag).ContainsKey(key);
            if (isContain)
            {
                return true;
            }
            // 如果找不到,且 父级 黑板存在,那就往父级黑板查找
            if (parent != null)
            {
                return parent.contain(key, tag);
            }
            return false;
        }

        private void RegServerIndex(string key)
        {
            if (serverRegIndexDic.ContainsKey(key))
            {
                //DB_Close   SGF.Debuger.LogError($"RegServerIndex key : {key} repeat error!!!");
                // #if DEBUG
                //                 Debug.Break();
                // #endif
                return;
            }
            serverRegIndexDic.Add(key, ++serverRegIndex);
        }

        /// <summary>
        /// 获取 服务器效果线 效果 注册的 regIndex, 如果没有注册,那就返回 0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetRegServerIndex(string key)
        {
            if (serverRegIndexDic.ContainsKey(key))
            {
                return serverRegIndexDic[key];
            }
            return 0;
        }

        public void Set(string key, object value, E_BlackBoardTag tag, bool saveInSkill = false, bool formateKey = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (saveInSkill && parent != null)
            {
                parent.Set(key, value, tag, saveInSkill, formateKey);
                return;
            }
            if (formateKey)
            {
                string realyKey = FormatBlackKey(key, tag);
                set(realyKey, value, tag);
            }
            else
            {
                set(key, value, tag);
            }


            if (tag == E_BlackBoardTag.Reg)
            {
                RegServerIndex(key);
            }
        }

        public object Get(string key, E_BlackBoardTag tag)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            string realyKey = FormatBlackKey(key, tag);
            return get(realyKey, tag);
        }


        public void Remove(string key, E_BlackBoardTag tag)
        {
            string realyKey = FormatBlackKey(key, tag);
            remove(realyKey, tag);
        }

        public void Remove(List<string> keys, E_BlackBoardTag tag)
        {
            keys.ForEach((string key) =>
            {
                string realyKey = FormatBlackKey(key, tag);
                remove(realyKey, tag);
            });
        }
        public bool Contain(string key, E_BlackBoardTag tag)
        {
            string realyKey = FormatBlackKey(key, tag);
            return contain(realyKey, tag);
        }

        public bool ContainKey(List<string> keys, bool containRegTag = false)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                if (!ContainKey(key, containRegTag))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainKey(string key, bool containRegTag = false)
        {
            if (Contain(key, E_BlackBoardTag.Server))
            {
                return true;
            }
            // 如果没有服务器黑板数据,那就只能自己去算
            string clientCenterKey = FormatBlackKey(key, E_BlackBoardTag.Client);

            // 如果客户端黑板有的话, 直接采用客端都黑板的 数据
            if (Contain(key, E_BlackBoardTag.Client))
            {
                return true;
            }

            // 如果get 不包含 reg黑板
            if (!containRegTag)
            {
                // 如果客户端黑板中也没有了,那其实就不需要处理了
                return false;
            }

            // 如果 注册黑板中 有这个数据,就采用注册黑板数据
            if (contain(key, E_BlackBoardTag.Reg))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 通过 key 获取 黑板中的 效果数据,先找 服务器数据,找不到的时候，找客户端数据
        /// note:
        ///     如果key 传入的value是 效果中 配置的key (原始数据),外面不需要 额外封装 FormatBlackKey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="containRegTag">查找的时候,是否包含 注册黑板</param>
        /// <typeparam name="T">如果存入黑板的是 blackNode的value,它是proto的Imessage结构,那客户端黑板相同key创建的数据应该是相应的proto结构</typeparam>
        public T GetKey<T>(string key, bool containRegTag = false)
        {
            T value = default(T);

            // 如果 服务器黑板中存在 中心目标key,那直接取 服务器黑板中的数据
            if (Contain(key, E_BlackBoardTag.Server))
            {
                value = (T)Get(key, E_BlackBoardTag.Server);

                return value;
            }
            // 如果没有服务器黑板数据,那就只能自己去算

            // 如果客户端黑板有的话, 直接采用客端都黑板的 数据
            if (Contain(key, E_BlackBoardTag.Client))
            {
                value = (T)Get(key, E_BlackBoardTag.Client);
                return value;
            }

            // 如果get 不包含 reg黑板
            if (!containRegTag)
            {
                // 如果客户端黑板中也没有了,那其实就不需要处理了
                return value;
            }

            // 如果 注册黑板中 有这个数据,就采用注册黑板数据
            if (contain(key, E_BlackBoardTag.Reg))
            {
                value = (T)Get(key, E_BlackBoardTag.Reg);
            }
            return value;
        }

        /// <summary>
        /// 删除 client和 server中 key的数据(containRegTag 是否包含reg黑板)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="containRegTag">是否包含Reg黑板</param>
        public void RemoveKey(string key, bool containRegTag = false)
        {
            //首先检查是否存在 服务器黑板中的 坐标Key  

            // 如果 服务器黑板中存在 中心目标key,那直接取 服务器黑板中的数据
            if (Contain(key, E_BlackBoardTag.Server))
            {
                Remove(key, E_BlackBoardTag.Server);
            }
            // 如果没有服务器黑板数据,那就只能自己去算

            // 如果客户端黑板有的话, 直接采用客端都黑板的 数据
            if (Contain(key, E_BlackBoardTag.Client))
            {
                Remove(key, E_BlackBoardTag.Client);
            }

            // 如果get 不包含 reg黑板
            if (!containRegTag)
            {
                // 如果客户端黑板中也没有了,那其实就不需要处理了
                return;
            }

            // 如果 注册黑板中 有这个数据,就采用注册黑板数据
            if (contain(key, E_BlackBoardTag.Reg))
            {
                Remove(key, E_BlackBoardTag.Reg);
            }

            return;
        }


        ~BaseBlackBoard()
        {
            //SGF.Debuger.Log($"{TagFlag} {this.GetHashCode()} finalizer");
        }
    }

    /// <summary>
    /// 技能黑板
    /// </summary>
    public class SkillBlackBoard : BaseBlackBoard
    {

    }

    public class StageBlackBoard : BaseBlackBoard
    {
        private RepeatedField<RunLineData> _stageLinesCache;

        public void ReadStageLinesCache(Action<RepeatedField<RunLineData>> action)
        {
            if (_stageLinesCache == null)
            {
                return;
            }

            action.Invoke(_stageLinesCache);
            _stageLinesCache = null;
        }
    }
}
