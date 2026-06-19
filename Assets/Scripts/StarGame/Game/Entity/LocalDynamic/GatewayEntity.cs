using Fire;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Player.Component;
using StarProject.Module;
using StarProject.Service.FindPath;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Entity.LocalDynamic
{
    // 传送门
    public class GatewayEntity : InteractiveShowEntity
    {
        private bool m_isAllOpen = false;
        private LocalDropList m_rewardList = null;
        private float m_scale = -1.0f;

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
            TagFlag = $"GatewayEntity_{triggerKey}_{type}_{id}";

            Vector3 startPos = pos;
            SetStartPosition(startPos);
            // 秘境传送门要在出生点坐标的东北（Y45°）方向上3米
            if (triggerKey == E_LocalEntitySource.SecretArea.ToString())
            {
                GetRandomPos(ref pos);
            }
            base.Create(entityKey, type, id, triggerKey, pos, scale);
            m_isAllOpen = isAllOpen;
            m_rewardList = rewardList;
            m_scale = scale;
            // 添加事件
            {
                GlobalEvent.onClickLocalEntity.AddListener(OnClickLocalEntity);
            }

            interactDataCell = LocalDataManager.Instance.GetInteractDataCell(id);
            CreateMContainer();
            CreateView();

            ReadConfig();
            // 创建触发器
            CreateTrigger();

            ObjectUnitPendant unitPendant = new(this);
            m_listCompoent.Add(unitPendant);
        }

        protected override void Reset()
        {
            base.Reset();
            m_isAllOpen = false;
            m_rewardList = null;
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
                    SetIdleAnimation?.Invoke(interactDataCell.DefaultIdle);
                }
            }
        }

        public override void EnterFrame(int frameIndex)
        {
            base.EnterFrame(frameIndex);
        }

        // 创建显示层
        private void CreateView()
        {
            if (interactDataCell != null)
            {
                RefreshAvatarID(interactDataCell.GetAvatarID());
                if (m_scale != -1.0f)
                {
                    ModleScale = m_scale;
                }
                else
                {
                    ModleScale = interactDataCell.GetModelScaling() / 100f;
                }
                //ViewFactory.CreateViewAddressables("Roles/Template/Local_Gateway_Model", "Roles/Template/Local_Gateway_Model", this, m_container.transform);

                ViewFactory.CreateViewAsync("Roles/Template/Local_Gateway_Model", this, m_container.transform, null);
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
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, ID, TriggerKey, EntityKey });
                }
            }
            else
            {
                // 隐藏按钮框
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });
            }
        }

        // 
        private void OnClickLocalEntity(int type, long id, string triggerKey, string key)
        {
            if (type == (int)E_LocalEntityType.Gateway)
            {
                // 传送门这边啥都不管
            }
        }

        // 打开奖励
        private void OpenRewardList()
        {
            if (m_rewardList == null || m_rewardList.drops == null)
            {
                return;
            }
            for (int i = 0; i < m_rewardList.drops.Count - 1; i++)
            {
                var child = m_rewardList.drops[i];
                Vector3 pos = Position();
                object[] Os = new object[] { child.Itemid, child.ItemNum, child.EntityID, pos };
                float delayTime = (1 + i) * 0.15f;
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
            }
            // 删除触发器
            IsEntryTrigger = false;
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", EntityKey);
            // 修改实体动作
            // ......
        }

        private void GetRandomPos(ref Vector3 pos)
        {
            Vector3 dir = Utils.ServerRota2Vector(45.0f);
            Vector3 point = pos + (dir * 3);
            // 判断是否可行走
            //FindPathManager.Instance.FindValidPointNearby(point, out pos);

            FindPathManager.Instance.FindValidPoint(point, out pos);

        }
    }
}