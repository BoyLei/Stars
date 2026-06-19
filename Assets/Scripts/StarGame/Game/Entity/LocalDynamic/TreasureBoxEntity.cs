using SGF.Module.Framework;
using SGF.Time;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player.Component;
using StarProject.Module;
using StarProject.Service.FindPath;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

public class LocalDropData
{
    public int Itemid;
    public int ItemNum;
    public ulong EntityID;
    public LocalDropData(int itemId, int itemNum, ulong entityId)
    {
        Itemid = itemId;
        ItemNum = itemNum;
        EntityID = entityId;
    }
}

[XLua.LuaCallCSharp]
public class LocalDropList
{
    public List<LocalDropData> drops = new();
    public bool CanOpen = true;
    public System.Action<bool> OpenCallBackFunc;

    public LocalDropList()
    {
        drops = new List<LocalDropData>();
    }

    public void AddData(int itemId, int itemNum, ulong entityId)
    {
        LocalDropData localDropData = new(itemId, itemNum, entityId);
        drops.Add(localDropData);
    }

    public void Add(LocalDropData data)
    {
        if (data != null)
        {
            drops.Add(data);
        }
    }
}

namespace StarProject.Game.Entity.LocalDynamic
{
    // 宝箱
    public class TreasureBoxEntity : InteractiveShowEntity
    {
        private bool m_isAllOpen = false;
        private LocalDropList m_rewardList = null;
        private float m_scale = -1.0f;

        private float m_Countdown = 0f;     // 宝箱倒计时，自动打开

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityKey">随机实体key</param>
        /// <param name="type">实体类型</param>
        /// <param name="id">交互物件ID</param>
        /// <param name="triggerKey">交互回调key</param>
        /// <param name="pos">坐标</param>
        /// <param name="rewardList">奖励</param>
        /// <param name="isAllOpen">是否同类型全部一起打开</param>
        public void Create(string entityKey, E_LocalEntityType type, int id, string triggerKey, Vector3 pos, LocalDropList rewardList, bool isAllOpen, float scale)
        {
            TagFlag = $"TreasureBoxEntity_{triggerKey}_{type}_{id}";

            Vector3 startPos = pos;
            SetStartPosition(startPos);
            // 秘境宝箱要随机一下坐标
            if (triggerKey == E_LocalEntitySource.SecretArea.ToString() || triggerKey == E_LocalEntitySource.DailyTeamEctype.ToString() || triggerKey == E_LocalEntitySource.DailyEctype.ToString())
            {
                //GetRandomPosByArea(ref pos, 3f);
                //pos = BusinessManager.Instance.GetRandomPos(pos,2.5f);
            }
            base.Create(entityKey, type, id, triggerKey, pos, scale);

            m_isAllOpen = isAllOpen;
            m_rewardList = rewardList;
            m_scale = scale;

            int count = m_rewardList != null && m_rewardList.drops != null ? m_rewardList.drops.Count : 0;
            SGF.Debuger.LogWarning($"本地宝箱掉落 id={id},triggerKey={triggerKey},奖励数量={count}");

            // 添加事件
            {
                GlobalEvent.onClickLocalEntity.AddListener(OnClickLocalEntity);
            }

            interactDataCell = LocalDataManager.Instance.GetInteractDataCell(id);

            CreateMContainer();
            CreateView();
            ReadConfig();
            CreateTrigger();

            ObjectUnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);

            if (triggerKey == E_LocalEntitySource.DailyEctype.ToString() || triggerKey == E_LocalEntitySource.DailyTeamEctype.ToString())
            {
                m_Countdown = 5.0f;
            }

            if (triggerKey == E_LocalEntitySource.WildBoss.ToString())
            {
                m_Countdown = 5.0f;
            }
        }

        protected override void Reset()
        {
            base.Reset();
            m_isAllOpen = false;
            m_rewardList = null;
            m_Countdown = 0f;
            m_scale = -1.0f;
        }

        protected override void Release()
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", EntityKey);
            GlobalEvent.onClickLocalEntity.RemoveListener(OnClickLocalEntity);

            ViewFactory.ReleaseView(this);

            Reset();
            base.Release();

            if (m_container != null)
            {
                GameObject.Destroy(m_container);
                m_container = null;
            }

            //EntityFactory.ReleaseEntity(this);
        }

        /// <summary> 加载模型完成 </summary>
        protected override void OnActionOnViewCreateFinish()
        {
            base.OnActionOnViewCreateFinish();

            if (interactDataCell != null)
            {
                if (!string.IsNullOrEmpty(interactDataCell.DefaultIdle))
                {
                    //SetIdleAnimation?.Invoke(interactDataCell.DefaultIdle);
                    var VitalState = PlayAnimation(GetAnimParam("Idle_show"), "Idle_show");

                    DG.Tweening.DOVirtual.DelayedCall(0.5f, () =>
                    {
                        SetIdleAnimation?.Invoke(interactDataCell.DefaultIdle);
                        //PlayAnimation(GetAnimParam("Idle_01"), "Idle_01");
                    });
                }
            }

            //PlaySpecialEffect("Fx_Chest_loop");
            LoadEffect("Fx_Chest_loop");
        }

        public override void EnterFrame(int frameIndex)
        {
            base.EnterFrame(frameIndex);

            //if (TriggerKey == E_LocalEntitySource.DailyEctype.ToString())
            {
                if (m_Countdown > 0)
                {
                    m_Countdown -= Time.fixedDeltaTime;
                    if (m_Countdown <= 0)
                    {
                        CountdownFinish();
                    }
                }
            }
        }

        // 创建显示层
        private void CreateView()
        {
            if (interactDataCell != null)
            {
                RefreshAvatarID(interactDataCell.GetAvatarID());
                ModleScale = interactDataCell.GetModelScaling() / 100f;
                if (m_scale != -1.0f)
                {
                    ModleScale = m_scale;
                }
                else
                {
                    ModleScale = interactDataCell.GetModelScaling() / 100f;
                }
                // ViewFactory.CreateViewAddressables("Roles/Template/Local_TreasureBox_Model", "Roles/Template/Local_TreasureBox_Model", this, m_container.transform);
                ViewFactory.CreateViewAsync("Roles/Template/Local_TreasureBox_Model", this, m_container.transform, null);
            }
            else
            {
                SGF.Debuger.LogError($"{TagFlag} Create interactDataCell=null,_index={ID},error!!");
            }
        }

        // 创建/注册触发器
        private void CreateTrigger()
        {
            TriggerData trigger = new()
            {
                Key = EntityKey,
                Position = Position(),
                Radius = Range,
                TriggerEvent = OnTriggerHandler
            };
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "Register", trigger);
        }

        // 读取配置
        private void ReadConfig()
        {
            if (interactDataCell == null)
            {
                return;
            }
            SpecialEffect = interactDataCell.GetSpecialEffect();
            EffectAddress = interactDataCell.EffectAddress;
            Range = interactDataCell.GetTriggerRange() * 0.01f;
            m_AssetIndex = interactDataCell.GetAssetIndex();
            ObjectName = interactDataCell.ModelName;
            EndinterIdle = interactDataCell.EndinterIdle;
        }
        private bool IsAotuInter(int type)
        {
            return type == 1;
        }
        // 触发器回调
        private void OnTriggerHandler(bool isEnter, Vector3 position, float radius)
        {
            if (interactDataCell == null)
            {
                return;
            }
            IsEntryTrigger = isEnter;
            if (isEnter)
            {
                if (IsAotuInter(interactDataCell.GetIsAutoInter()))
                {
                    // 直接执行回调
                    GlobalEvent.onClickLocalEntity.Invoke((int)Type, ID, TriggerKey, EntityKey);
                }
                else
                {
                    // 出现按钮框
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, ID, TriggerKey, EntityKey, m_rewardList.OpenCallBackFunc });
                }
            }
            else
            {
                // 隐藏按钮框
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });
            }
        }

        private void CountdownFinish()
        {
            // 直接执行回调
            GlobalEvent.onClickLocalEntity.Invoke((int)Type, ID, TriggerKey, EntityKey);
        }

        private void OnClickLocalEntity(int type, long id, string triggerKey, string key)
        {
            // 宝箱 只处理 宝箱 【不管其他东西】
            if (type == (int)E_LocalEntityType.TreasureBox)
            {
                if (triggerKey == TriggerKey)
                {
                    //只要执行到开启就把倒计时开启timer去除
                    m_Countdown = 0f;
                    if (triggerKey == E_LocalEntitySource.WildBoss.ToString())
                    {
                        if (m_rewardList.CanOpen)
                        {
                            // 目前宝箱是全部打开
                            if (m_isAllOpen)
                            {
                                OpenRewardList();
                            }
                            else if (EntityKey == key)
                            {
                                // 只开一个
                                OpenRewardList();
                            }
                        }
                        else
                        {
                            IsEntryTrigger = false;
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", EntityKey);
                            // 延迟8秒删除实体
                            DelayInvoker.DelayInvoke(8f, OnDelayDelete);
                        }
                        m_rewardList.OpenCallBackFunc?.Invoke(m_rewardList.CanOpen);
                    }
                    else
                    {
                        // 目前宝箱是全部打开
                        if (m_isAllOpen)
                        {
                            OpenRewardList();
                        }
                        else if (EntityKey == key)
                        {
                            // 只开一个
                            OpenRewardList();
                        }
                    }
                }
            }
        }

        // 打开奖励
        private void OpenRewardList()
        {
            // 隐藏按钮框
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", EntityKey);
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });

            if (m_rewardList == null || m_rewardList.drops == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(EndinterIdle))
            {
                var VitalState = PlayAnimation(GetAnimParam(EndinterIdle), EndinterIdle);
                if (VitalState != null)
                {
                    VitalState.OnVitalStateChanged += OnVitalStateComplte;
                }
            }
            //PlaySpecialEffect("Fx_Chest_open_once");
            LoadEffect("Fx_Chest_open_once");

            //List<ProtoMsg.ChatMsgNotice> msgList = new();
            //string strLanVal = LanguageManager.Instance.GetLanguageByKey("Local_Str_Tips_GetReward");
            //int jobID = (int)GameManager.Instance.GetPlayerJob();
            for (int i = 0; i < m_rewardList.drops.Count; i++)
            {
                var child = m_rewardList.drops[i];
                Vector3 pos = Position();
                object[] Os = new object[] { child.Itemid, child.ItemNum, child.EntityID, pos };
                float delayTime = (1 + i) * 0.15f;
                delayTime = 0;
                DelayInvoker.DelayInvoke("", delayTime,
                (object[] args) =>
                {
                    int Itemid = (int)Os[0];
                    int ItemNum = (int)Os[1];
                    ulong EntityID = (ulong)Os[2];
                    Vector3 ownerPos = (Vector3)Os[3];
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.LootControllerModule, "OnAddLoot", new object[] { Itemid, ownerPos, ItemNum, EntityID });
                }
                , Os);

                //var itemCfg = LocalDataManager.Instance.GetItemDataCell(child.Itemid);
                //if (itemCfg != null)
                //{
                //    var chatMsgNotice = new ProtoMsg.ChatMsgNotice();
                //    chatMsgNotice.EID = 0;
                //    chatMsgNotice.PID = 0;
                //    var color = ColorDefine.Instance.GetColor($"Item_Quality_{itemCfg.GetQuality()}");
                //    string tips = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{itemCfg.Name}</color>x {child.ItemNum}";
                //    chatMsgNotice.Data = string.Format(strLanVal, tips);
                //    chatMsgNotice.Channel = ProtoMsg.ChatChannel.ChannelSystem;
                //    chatMsgNotice.ChatTime = TimeUtils.ServerNowStampMilli;
                //    chatMsgNotice.ChatLevel = 1;
                //    chatMsgNotice.JobID = jobID;
                //    msgList.Add(chatMsgNotice);
                //}
            }
            //ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnAnalogReceptionChatMsg", new object[] { msgList });
            // 删除触发器
            IsEntryTrigger = false;
            // 修改实体动作【打开动作】
            // ......

            // 自动删除实体
            DelayInvoker.DelayInvoke(1.8f, OnDelayDelete);

        }

        void LoadEffect(string name)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("Effects/Obstacle/Chest/"+ name,
            (GameObject go) =>
            {
                if (go == null)
                {
                    return;
                }

                var gob = GameObject.Instantiate<GameObject>(go);
                if (gob != null)
                {
                    gob.transform.SetParent(m_container.transform, false);
                    gob.transform.position = Position();
                }

            });
        }

        private void OnDelayDelete(object[] args)
        {
            GameManager.Instance.RemoveLocalEntity(EntityKey);
        }

        private void OnVitalStateComplte(VitalState vitalState, VitalStateEnum state)
        {
            vitalState.OnVitalStateChanged -= OnVitalStateComplte;

            if (state == VitalStateEnum.Play)
            {
                //PlaySpecialEffect("Fx_Chest_open_once");
            }
            else if (state == VitalStateEnum.End)
            {
                //PlaySpecialEffect("Fx_Chest_open_loop");
                //LoadEffect("Fx_Chest_open_loop");
            }
        }

        private void GetRandomPosByArea(ref Vector3 pos, float radius)
        {
            float x = Random.Range(-radius, radius);
            float z = Random.Range(-radius, radius);

            pos.x += x;
            pos.z += z;
        }

        private void GetRandomPos(ref Vector3 pos)
        {
            float angles = Random.Range(-45, 45) + 10;
            float radius = 3.0f;
            Vector3 point = Fire.Utils.PosMoveBySeverRota(pos, angles, radius);
            FindPathManager.Instance.FindValidPointNearby(point, out pos);
        }

    }
}