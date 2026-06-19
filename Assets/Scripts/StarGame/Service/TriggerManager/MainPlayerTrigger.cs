using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProject.Game;
using StarProject.Service.Function;
using Task;
using static StarProject.Service.Function.GlobalFunctionManager;

namespace Trigger
{
    public class MainPlayerTrigger 
    {
        private BaseTrigger Trigger;
        private TrrigerEffectJsonData JsonData;
        private int ConditionGroupID { get; set; }

        public MainPlayerTrigger( TriggerJsonData jsonData, TrrigerEffectJsonData effectJsonData)
        {
            TrrigerType type = (TrrigerType)jsonData.TrrigerType;
            switch (type)
            {
                case TrrigerType.TimerTrriger:
                    Trigger = new TimerTrigger(0,0, jsonData.TimerJsonData.Delay * 0.001f, jsonData.TimerJsonData.Interval * 0.001f, jsonData.Count,true, OnTriggerHandler);
                    break;
                case TrrigerType.PositionTrriger:
                    Trigger = new PositionTrigger(0,0, jsonData.PositionJsonData.shapType, jsonData.PositionJsonData.Position.Convert(),0,jsonData.PositionJsonData.IsEnterTrigger,jsonData.PositionJsonData.Radius * 0.01f,jsonData.PositionJsonData.Length*0.01f,jsonData.PositionJsonData.Width*0.01f,jsonData.PositionJsonData.Polygons, jsonData.Count,true, OnTriggerHandler);
                    break;
                case TrrigerType.PropertyTrriger:
                    if (!string.IsNullOrEmpty(jsonData.PropertyJsonData.PropName) &&
                        !string.IsNullOrEmpty(jsonData.PropertyJsonData.PropValue))
                    {
                        Trigger = new PropTrigger(0, 0, jsonData.PropertyJsonData.PropName,
                            jsonData.PropertyJsonData.PropValue, jsonData.PropertyJsonData.Compare,
                            !jsonData.PropertyJsonData.IsSelf, jsonData.Count, true, OnTriggerHandler);

                    }

                    break;
            }
            JsonData = effectJsonData;

            ConditionGroupID = 0;
            if (effectJsonData.Conditions != null && effectJsonData.Conditions.Count > 0)
            {
                ConditionGroupID = GlobalFunctionManager.Instance.GetConditionGroupID();
                for (int i = 0; i < effectJsonData.Conditions.Count; i++)
                {
                    List<ConditionData> conditions = new List<ConditionData>();
                    foreach (var item in effectJsonData.Conditions[i])
                    {
                        conditions.Add(new ConditionData(ConditionGroupID, item));
                    }
                    GlobalFunctionManager.Instance.RegisterConditions(ConditionGroupID, conditions, i);
                }
            }

        }

        public void OnTriggerHandler(ulong entityId)
        {

            //条件检查
            if (!CheckCondition())
            {
                return;
            }

            //效果执行
            ExecuteEffects();
        }

        private bool CheckCondition()
        {
            return GlobalFunctionManager.Instance.ConditionGroupMete(GameManager.Instance.mainPlayerId, ConditionGroupID);
        }


        private void ExecuteEffects()
        {
            if (JsonData.Effects != null && JsonData.Effects.Count > 0)
            {
                foreach (var item in JsonData.Effects)
                {
                    GlobalFunctionManager.Instance.ExecuteClient(item, GameManager.Instance.mainPlayerId);
                }
            }
        }

        public void EnterFrame(int frameIndex)
        {
            if (Trigger != null)
            {
                Trigger.EnterFrame(frameIndex);
            }
        }
    }
}
