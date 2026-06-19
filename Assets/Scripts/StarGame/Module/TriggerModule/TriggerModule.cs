///--------------------------------------------------------------------
/// 文件名   :   TriggerModule
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   #CREATETIME#
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Player;
using System.Collections.Generic;
using UnityEngine;
namespace StarProject.Module
{
    public class TriggerModule : BusinessModule
    {
        //主角
        private PlayerCtrlGroup MainPlayer;
        //触发器状态
        private Dictionary<string, bool> TriggersState = null;
        //触发器数据
        private Dictionary<string, TriggerData> TriggersData = null;
        //等待移除的数据
        private List<string> WaitRemoveTrigger = null;
        //等待添加的数据
        private Dictionary<string, TriggerData> WaitAddTrigger = null;

        private Dictionary<string, System.Action<object>> InternalMessages = null;

        public override void Create(object args = null)
        {
            base.Create(args);
            TriggersState = new Dictionary<string, bool>();
            TriggersData = new Dictionary<string, TriggerData>();
            WaitRemoveTrigger = new List<string>();
            WaitAddTrigger = new Dictionary<string, TriggerData>();
            InternalMessages = new Dictionary<string, System.Action<object>>();
            InternalMessages.Add("Register", Register);
            InternalMessages.Add("UnRegister", UnRegister);
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
            MonoHelper.AddFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
            GlobalEvent.TaskStageChange.AddListener(OnTaskChangeHandler);
        }

        private void OnTaskChangeHandler(int arg0, int arg1)
        {
            if (TriggersData != null)
            {
                foreach (var item in TriggersData)
                {
                    if (item.Value.TriggerEvent != null)
                    {
                        item.Value.TriggerEvent.Invoke(TriggersState[item.Key], item.Value.Position, item.Value.Radius);
                    }
                }
            }
        }

        private void OnRoleCreateComplete(object obj)
        {
            MainPlayerCreateSuccess();
        }

        public override void Release()
        {
            GlobalEvent.OnRoleCreateComplete.RemoveListener(OnRoleCreateComplete);
            MonoHelper.RemoveFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
            if (WaitRemoveTrigger != null)
            {
                WaitRemoveTrigger.Clear();
            }
            if (WaitAddTrigger != null)
            {
                WaitAddTrigger.Clear();
            }
            if (TriggersState != null)
            {
                TriggersState.Clear();
            }

            if (TriggersData != null)
            {
                TriggersData.Clear();
            }
            base.Release();
        }

        private void FixedUpdate()
        {
            if (MainPlayer == null || MainPlayer.M_Curr == null)
            {
                return;
            }

            if (WaitRemoveTrigger != null && WaitRemoveTrigger.Count > 0)
            {
                foreach (var item in WaitRemoveTrigger)
                {
                    Remove(item);
                }
                WaitRemoveTrigger.Clear();
            }

            if (WaitAddTrigger != null && WaitAddTrigger.Count > 0)
            {
                foreach (var item in WaitAddTrigger)
                {
                    Add(item.Key, item.Value);
                }

                WaitAddTrigger.Clear();
            }

            if (TriggersData != null && TriggersData.Count > 0)
            {
                Vector3 mainPlayerPos = MainPlayer.M_Curr.Position();
                foreach (var item in TriggersData)
                {
                    bool inner = false;
                    Vector3 pos = item.Value.Position;
                    if (item.Value.EntityID != 0)
                    {
                        pos = GameManager.Instance.GetEntityPosById(item.Value.EntityID);
                    }
                    //其他的
                    if (item.Value.Data==null)
                    {
                        float dis = Vector3.Distance(mainPlayerPos, pos);
                        inner = dis <= item.Value.Radius;
                    }
                    else
                    {
                        //圆形
                        if (item.Value.Data.shapType == 0)
                        {
                            float dis = Vector3.Distance(mainPlayerPos, pos);
                            inner = dis <= item.Value.Radius;
                        }
                        //方形
                        else if (item.Value.Data.shapType == 1)
                        {

                        }
                        //多边形
                        else if (item.Value.Data.shapType == 2)
                        {
                            inner = IsPointInPolygon(mainPlayerPos, item.Value.Data.Polygons);
                        }
                    }

                    if (TriggersState.ContainsKey(item.Key))
                    {
                        bool record = TriggersState[item.Key];
                        if (record != inner)
                        {
                            item.Value.TriggerEvent.Invoke(inner, item.Value.Position, item.Value.Radius);
                            TriggersState[item.Key] = inner;
                        }
                    }
                    else
                    {
                        item.Value.TriggerEvent.Invoke(inner, item.Value.Position, item.Value.Radius);
                        TriggersState.Add(item.Key, inner);
                    }
                }
            }
        }

        /// <summary>
        /// 点是否在多边形范围内
        /// </summary>
        /// <param name="p">点</param>
        /// <param name="vertexs">多边形顶点列表</param>
        /// <returns></returns>
        public bool IsPointInPolygon(Vector3 p, List<CustomVector3> vertexs)
        {
            int crossNum = 0;
            int vertexCount = vertexs.Count;

            for (int i = 0; i<vertexCount; i++)
            {
                CustomVector3 v1 = vertexs[i];
                CustomVector3 v2 = vertexs[(i + 1) % vertexCount];

                if (((v1.z <= p.z) && (v2.z > p.z))
                    || ((v1.z > p.z) && (v2.z <= p.z)))
                {
                    if (p.x<v1.x + ((p.z - v1.z) / (v2.z - v1.z) * (v2.x - v1.x)))
                    {
                        crossNum += 1;
                    }
                }
            }

            if (crossNum % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #region 事件处理
        protected override void OnModuleMessage(string msg, object[] args)
        {
            if (InternalMessages.TryGetValue(msg, out System.Action<object> action))
            {
                action?.Invoke(args);
            }
        }

        private void MainPlayerCreateSuccess()
        {
            MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
        }

        /// <summary>
        /// 注册触发器
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        private void Register(object args)
        {
            TriggerData data = (TriggerData)args;
            if (!WaitAddTrigger.ContainsKey(data.Key))
            {
                WaitAddTrigger.Add(data.Key, data);
            }
            else
            {
                //SGF.Debuger.LogError("Trigger  is Contains ");
            }
        }

        /// <summary>
        /// 注销触发器
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        private void UnRegister(object args)
        {
            string key = (string)args;
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            WaitRemoveTrigger.Add(key);
        }


        #endregion


        #region 内部函数
        private void Add(string key, TriggerData data)
        {
            if (!TriggersData.ContainsKey(key))
            {
                TriggersData.Add(key, data);
                //TriggersState.Add(key, false);
            }
            else
            {
                //SGF.Debuger.LogError("Trigger  is Contains ");
            }
        }

        private void Remove(string key)
        {
            if (TriggersData.ContainsKey(key))
            {
                TriggersData.Remove(key);
                TriggersState.Remove(key);
            }
            else
            {
                //SGF.Debuger.LogError("Trigger  is not find ");
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 查询触发器状态
        /// </summary>
        /// <param name="key">触发器的键值</param>
        /// <param name="result">返回结果</param>
        /// <returns>返回是否存在</returns>
        public bool QueryTrrigerState(string key, out bool result)
        {
            result = false;

            if (!string.IsNullOrEmpty(key) && TriggersState!=null && TriggersState.ContainsKey(key))
            {
                result = TriggersState[key];
                return true;
            }
            return false;
        }

        #endregion
    }

    public class TriggerData
    {
        public string Key;

        public AreaJsonData Data;
        public Vector3 Position;
        public float Radius;
        public ulong EntityID;

        public System.Action<bool, Vector3, float> TriggerEvent;
    }


}

