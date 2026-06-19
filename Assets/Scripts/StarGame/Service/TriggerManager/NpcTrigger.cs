///--------------------------------------------------------------------
/// 文件名   :   NpcTrigger.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/05 17:49:30
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using StarProject.Game;
using StarProject.Service.Function;
using System.Collections.Generic;
using SGF.Unity;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Player;
using Task;
using static StarProject.Service.Function.GlobalFunctionManager;

namespace Trigger
{
    public class NpcTrigger
    {
        private long ConfigID { get; set; }
        private BaseTrigger Trigger;
        private TrrigerEffectJsonData JsonData;
        private int ConditionGroupID { get; set; }

        public void SetEntityID(ulong uid)
        {
            if (Trigger != null)
            {
                Trigger.SetEntityID(uid);
            }
        }

        public NpcTrigger(int index, long configID, TriggerJsonData jsonData, TrrigerEffectJsonData effectJsonData)
        {
            ConfigID = configID;
            TrrigerType type = (TrrigerType)jsonData.TrrigerType;
            switch (type)
            {
                case TrrigerType.TimerTrriger:
                    Trigger = new TimerTrigger(index, ConfigID, jsonData.TimerJsonData.Delay * 0.001f,
                        jsonData.TimerJsonData.Interval * 0.001f, jsonData.Count, false, OnTriggerHandler);
                    break;
                case TrrigerType.PositionTrriger:
                    Trigger = new PositionTrigger(index, ConfigID, jsonData.PositionJsonData.shapType,
                        jsonData.PositionJsonData.Position.Convert(), 0, jsonData.PositionJsonData.IsEnterTrigger,
                        jsonData.PositionJsonData.Radius * 0.01f, jsonData.PositionJsonData.Length * 0.01f,
                        jsonData.PositionJsonData.Width * 0.01f, jsonData.PositionJsonData.Polygons, jsonData.Count,
                        false, OnTriggerHandler);
                    break;
                case TrrigerType.PropertyTrriger:
                    if (!string.IsNullOrEmpty(jsonData.PropertyJsonData.PropName) &&
                        !string.IsNullOrEmpty(jsonData.PropertyJsonData.PropValue))
                    {
                        Trigger = new PropTrigger(index, ConfigID, jsonData.PropertyJsonData.PropName,
                            jsonData.PropertyJsonData.PropValue, jsonData.PropertyJsonData.Compare,
                            !jsonData.PropertyJsonData.IsSelf, jsonData.Count, false, OnTriggerHandler);
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

        public void OnTriggerHandler(ulong entityID)
        {
            //显隐状态
            //var  entityID=GameManager.Instance.GetNPCEntityIDByConfig(ConfigID);
            var entity = GameManager.Instance.GetEntityCtr(entityID);
            if (entity != null)
            {
                var npc = entity as GameNPCCtrlGroup;
                if (npc != null)
                {
                    bool active = false;
                    active = npc.IsShow;
                    if (!active)
                    {
                        return;
                    }

                    if (npc.Container == null)
                    {
                        return;
                    }

                    ViewAOI viewAOI = npc.Container.GetComponentInChildren<ViewAOI>();
                    if (viewAOI == null)
                    {
                        return;
                    }

                    if (!viewAOI.GetViewIsShow())
                    {
                        return;
                    }
                }

                //条件检查
                if (!CheckCondition())
                {
                    return;
                }

                //效果执行
                ExecuteEffects(entityID);
            }
        }

        private bool CheckCondition()
        {
            return GlobalFunctionManager.Instance.ConditionGroupMete(GameManager.Instance.mainPlayerId,
                ConditionGroupID);
        }


        private void ExecuteEffects(ulong entityID)
        {
            if (entityID == 0)
            {
                return;
            }

            if (JsonData.Effects != null && JsonData.Effects.Count > 0)
            {
                ExecuteEffect(0, entityID);
                /*var executeIndex = 0;
                var maxIndex = JsonData.Effects.Count;

                foreach (var item in JsonData.Effects)
                {

                    GlobalFunctionManager.Instance.ExecuteClient(item, entityID);
                }*/
            }
        }

        private void ExecuteEffect(int index, ulong entityID)
        {
            var maxIndex = JsonData.Effects.Count;
            if (index > -1 && index < maxIndex)
            {
                var effect = JsonData.Effects[index];
                if (effect != null)
                {
                    //执行当前效果
                    GlobalFunctionManager.Instance.ExecuteClient(effect, entityID);

                    if (effect.OutTime > 0)
                    {
                        DelayInvoker.DelayInvoke(effect.OutTime * 0.001f, (args) =>
                        {
                            //执行下一个效果
                            ExecuteEffect(index + 1, entityID);
                        });
                    }
                    else
                    {
                        //执行下一个效果
                        ExecuteEffect(index + 1, entityID);
                    }
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