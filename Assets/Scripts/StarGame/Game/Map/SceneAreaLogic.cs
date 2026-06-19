///--------------------------------------------------------------------
/// 文件名   :   SceneAreaLogic
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/11 15:12:13
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Game.Map //这里统一，不再细分，战斗分不干净
{
    public class SceneAreaLogic: IMapLogic
    {

        private List<AreaLogic> AreaLogics = null;


        public LogicType GetLogicType()
        {
            return LogicType.Area;
        }
        public static SceneAreaLogic Create()
        {
            SceneAreaLogic logic = new SceneAreaLogic();
            logic.OnCreate();
            return logic;
        }

        public void  OnCreate()
        {
            AreaLogics = new List<AreaLogic>();
        }

        public void OnLoad()
        {
            OnClear();
            //99触发区域  100 管辖区域
            if (GameMap.sceneJsonData.Areas != null && GameMap.sceneJsonData.Areas.Count > 0)
            {
                foreach (var areaJson in GameMap.sceneJsonData.Areas)
                {
                    //公会活动范围特判
                    bool isGuildPartyArea = (areaJson.Value.Index == 3)&& GameMap.sceneJsonData.SceneID==100;

                    if (areaJson.Value.areaType == 100 || areaJson.Value.areaType == 99 || isGuildPartyArea)
                    {
                        var logic = AreaLogic.Create(areaJson.Value);
                        logic.CreateTrigger();
                        AreaLogics.Add(logic);
                        //StarDebug.LogError($"AreaLogic OnLoad:{areaJson.Value.AreaID}");
                    }
                }
            }

        }

        public void OnUnLoad()
        {
            OnClear();
        }

        public void OnClear()
        {
            if (AreaLogics != null && AreaLogics.Count > 0)
            {
                foreach (var logic in AreaLogics)
                {
                    logic.DestroyTrigger();
                }

                AreaLogics.Clear();
            }

        }
    }

    public class AreaLogic
    {
        private AreaJsonData JsonData;

        public static AreaLogic Create(AreaJsonData data)
        {
            AreaLogic logic = new AreaLogic();
            logic.JsonData = data;
            return logic;
        }

        /// <summary>
        /// 创建触发器
        /// </summary>
        public void CreateTrigger()
        {
            string mTriggerKey = $"SceneArea_{JsonData.Index}_{JsonData.AreaID}_{JsonData.areaType}";
            StarProject.Module.TriggerData trigger = new StarProject.Module.TriggerData()
            {
                Key = mTriggerKey,
                Data = JsonData,
                Position = JsonData.Position.Convert(),
                Radius = JsonData.Radius * 0.01f,
                TriggerEvent = OnTriggerHandler
            };
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "Register", trigger);

        }

        public void DestroyTrigger()
        {
            string mTriggerKey = $"SceneArea_{JsonData.Index}_{JsonData.AreaID}_{JsonData.areaType}";
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TriggerModule, "UnRegister", mTriggerKey);
        }

        private void ExecuteEntryEffect()
        {
            if (JsonData.EnterEventEffect != null && JsonData.EnterEventEffect.Count>0)
            {
                foreach (var item in JsonData.EnterEventEffect)
                {
                    if (StarProject.Service.Function.GlobalFunctionManager.Instance.ConditionGroupMete(GameManager.Instance.mainPlayerId,item.ConditionID))
                    {
                        StarProject.Service.Function.GlobalFunctionManager.Instance.DoFunction(item.EventID);
                    }
                }
            }
        }

        private void ExecuteExitEffect()
        {
            if (JsonData.ExitEventEffect != null && JsonData.ExitEventEffect.Count>0)
            {
                foreach (var item in JsonData.ExitEventEffect )
                {
                    if (StarProject.Service.Function.GlobalFunctionManager.Instance.ConditionGroupMete(GameManager.Instance.mainPlayerId, item.ConditionID))
                    {
                        StarProject.Service.Function.GlobalFunctionManager.Instance.DoFunction(item.EventID);
                    }
                }
            }
        }

        private void OnTriggerHandler(bool isEnter, Vector3 position, float radius)
        {
            if (isEnter)
            {
                // ExecuteEntryEffect();
                //DisplayProcessDispenser.Instance.AddSpecialMessage($"进入{JsonData.AreaName}");

                //Frame.Util.ShowMessage("区域名" + JsonData.AreaName);

                if (JsonData.areaType == 100 || JsonData.areaType == 99 )
                {
                    if (GameManager.Instance.OldAreaName != JsonData.AreaName)
                    {
                        GameManager.Instance.OldAreaName = JsonData.AreaName;
                        //UIManager.Instance.OpenWidget(UIDef.ShowAreaNameWidget, false, new object[] { JsonData.AreaName,2 ,""}, null, StarProjectDef.MainPageCommond.HideNone, true, false);
                        UIManager.Instance.OpenWidgetAsync(UIDef.ShowAreaNameWidget, null, false, new object[] { JsonData.AreaName, 2, "" }, null, StarProjectDef.MainPageCommond.HideNone, true, false);
                    }
                }
                else if (JsonData.areaType == 0 && JsonData.Index == 3)
                {
                    //公会活动区域
                    GlobalEvent.OnEnterPartyArea.Invoke(true);
                }

            }
            else
            {
                if (JsonData.areaType == 0 && JsonData.Index == 3)
                {
                    //公会活动区域
                    GlobalEvent.OnEnterPartyArea.Invoke(false);
                }

                // ExecuteExitEffect();
            }

        }
    }
}