using ProtoMsg;
using SGF.Module.Framework;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Map;
using StarProject.Game.Player.Component;
using StarProject.Module;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game.Entity.LocalDynamic
{
    // 通缉玩法NPC实体
    public class WantedEntity : InteractiveShowEntity
    {
        private TeamWantedDataCell m_teamWantedDataCell;

        public WTaskPointTarInfo WTaskPointTar;
        public int PointID;    // 点位ID
        public int PointType;  // 点位类型
        public ulong SrvID; // 分线ID
        public ulong SpaceID;   // 地图id
        private bool isLoadModelSuc = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityKey">随机实体key</param>
        /// <param name="type">实体类型</param>
        /// <param name="pointType">点位类型</param>
        /// <param name="srvID">分线id</param>
        /// <param name="spaceID">地图id</param>
        /// <param name="wTaskPointTarInfo">通缉点位信息</param>
        public void Create(string entityKey, E_LocalEntityType type, int pointType, ulong srvID, ulong spaceID, WTaskPointTarInfo wTaskPointTarInfo)
        {
            TagFlag = $"WantedEntity_{pointType}_{wTaskPointTarInfo.PointID}_{entityKey}";

            m_teamWantedDataCell = LocalDataManager.Instance.GetTeamWantedDataCell(pointType);
            if (m_teamWantedDataCell != null)
            {
                interactDataCell = LocalDataManager.Instance.GetInteractDataCell(m_teamWantedDataCell.GetInteractID());
            }
            WTaskPointTar = wTaskPointTarInfo;
            PointID = wTaskPointTarInfo.PointID;
            PointType = pointType;
            SrvID = srvID;
            SpaceID = spaceID;
            if (GameMap.sceneJsonData != null && GameMap.sceneJsonData.WantedTasks != null && GameMap.sceneJsonData.WantedTasks.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.WantedTasks)
                {
                    if (areaJson.Value != null && areaJson.Value.Index == PointID && areaJson.Value.Type == pointType)
                    {
                        long id = interactDataCell != null ? interactDataCell.GetID() : PointID;
                        SetStartPosition(areaJson.Value.Position.Convert());
                        base.Create(entityKey, type, id, PointID.ToString(), StartPosition(), -1);
                        break;
                    }
                }
            }
            else
            {
                GameManager.Instance.RemoveLocalEntity(EntityKey);
                return;
            }

            // 添加事件
            {
                GlobalEvent.onClickLocalEntity.AddListener(OnClickLocalEntity);
            }

            CreateMContainer();
            CreateView();
            ReadConfig();
            CreateTrigger();

            ObjectUnitPendant unitPendant = new(this);
            bool hide = WTaskPointTar.CurState == 1;

            //IsHide = WTaskPointTar.CurState == 1;
            unitPendant.SetFlashHide(hide);
            m_listCompoent.Add(unitPendant);

            if (!isLoadModelSuc)
            {
                GameManager.Instance.RemoveLocalEntity(EntityKey);
            }
        }

        public void RefreshWTaskPointTarInfo(WTaskPointTarInfo wTaskPointTarInfo)
        {
            WTaskPointTar = wTaskPointTarInfo;
            PointID = wTaskPointTarInfo.PointID;

            bool hide = WTaskPointTar.CurState == 1;
            SetHide(hide);
            if (IsEntryTrigger && hide)
            {
                // 隐藏按钮框
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });
            }
        }

        protected override void Reset()
        {
            base.Reset();
            m_teamWantedDataCell = null;
            WTaskPointTar = null;
            PointID = -1;
            PointType = -1;
            SrvID = 0;
            SpaceID = 0;
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
            bool hide = WTaskPointTar.CurState == 1;

            //IsHide = WTaskPointTar.CurState == 1;
            SetHide(hide);
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
                ModleScale = interactDataCell.GetModelScaling() / 100f;
                try
                {
                    //ViewFactory.CreateViewAddressables("Roles/Template/Local_Wanted_Model", "Roles/Template/Local_Wanted_Model", this, m_container.transform);
                    ViewFactory.CreateViewAsync("Roles/Template/Local_Wanted_Model", this, m_container.transform, null);
                    isLoadModelSuc = true;
                }
                catch (System.Exception t)
                {
                    isLoadModelSuc = false;
                    SGF.Debuger.LogError($"{TagFlag} CreateView {t}");
                }
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
            if (IsHide)
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

        private void OnClickLocalEntity(int type, long id, string triggerKey, string key)
        {
            if (type == (int)E_LocalEntityType.WantedEnity)
            {
                if (triggerKey == TriggerKey && key == EntityKey)
                {
                    ModuleManager.Instance.SendMessage(ModuleDef.Name.ObjectInteractiveModule, "OnRefeshLocalObjectInteractive", new object[] { (int)Type, 0, TriggerKey, EntityKey });
                    // 通知挑战通缉实体 分线id、地图id、点位id、点位类型
                    GlobalEvent.onChallengeWantedEntity.Invoke(SrvID, SpaceID, PointID, PointType);

                    //SGF.Debuger.LogError($"{TagFlag} 通缉任务 单个 点击 SrvID={SrvID},SpaceID={SpaceID},PointID={PointID},PointType={PointType}");
                }
            }
        }

    }
}
