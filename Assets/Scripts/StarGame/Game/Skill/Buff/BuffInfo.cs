using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using StarProject.Game.Entity;
using StarProject.Game.Entity.Factory;
using StarProject.Service.LocalData;
using UnityEngine;


namespace StarProject.Game.Skill
{
    public class BuffInfo : BaseConfigInfo
    {
        private string TagFlag
        {
            get => $"[BuffInfo_{BuffID}] ";
        }

        public int BuffID;

        public int Time => Cfg.Time;


        public SkillEditor.BuffConfig Cfg;

        // 特殊BuffUI显示
        private Dictionary<int, int> BuffUIShowDetails = new Dictionary<int, int>();

        // 特殊BuffUI时间显示
        private List<int> BuffUISpTimeShowTypeList = new();

        public void Init(int buffID, System.Action actionOnLoad)
        {
            BuffID = buffID;

            normalTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();
            otherTimeLineStageInfos = EntityFactory.InstanceEntity<TimeLineStageInfos>();

            // 配置理论上应该用立即同步逻辑,
            LocalDataManager.Instance.GetBuffJson(buffID, (BuffJson buffJson) =>
            {
                if (buffJson == null)
                {
                    SGF.Debuger.LogWarning($"{TagFlag} buffID : {buffID}  token cfg error!!!");
                    return;
                }

                //SGF.Debuger.Log($"{TagFlag} buffID : {buffID}  init");

                Cfg = buffJson.config;

                InitLoopEffect();
                InitBUFFSpecialBUFFUI();

                Cfg.States.ForEach((BattleState normalState) =>
                {
                    states.Add((int)normalState);
                });

                NormalStageJsons = buffJson.Normals;
                OtherStageJsons = buffJson.Others;

                normalTimeLineStageInfos.InitFrameEvents(buffJson.Normals, buffID);
                otherTimeLineStageInfos.InitFrameEvents(buffJson.Others, buffID);

                actionOnLoad.Invoke();
            });
        }

        private void InitLoopEffect()
        {
            List<EffectTypeHitEffect> loopEffectsCfg = Cfg.LoopEffectInEditors;
            if (loopEffectsCfg == null)
            {
                return;
            }

            ConverFx.LoopEffects2FxJsons(Cfg.LoopEffectInEditors, LoopEffectFxs);
        }

        public bool CheckHasTypeStage(StageType checkStageType)
        {
            for (int i = 0; i < OtherStageJsons.Count; i++)
            {
                StageJson stageJson = OtherStageJsons[i];
                if (stageJson.StageType == checkStageType)
                {
                    return true;
                }
            }
            return false;
        }

        private void InitBUFFSpecialBUFFUI()
        {
            BuffUIShowDetails.Clear();

            foreach (var item in Cfg.GlobalShows)
            {
                if (item.GlobalShowType == GlobalShowType.BUFF_SpecialBUFFUI)
                {
                    if (item.BUFF_SpecialBUFFUI != null && item.BUFF_SpecialBUFFUI.BUFFUIShowDetailsList != null)
                    {
                        foreach (var child in item.BUFF_SpecialBUFFUI.BUFFUIShowDetailsList)
                        {
                            BuffUIShowDetails.Add((int)child.BUFFUIPos, (int)child.BuffUIShowDetails);
                        }
                    }
                }
                else if (item.GlobalShowType == GlobalShowType.BUFF_SpTimeShow)
                {
                    if (item.BUFF_SpTimeShow != null && item.BUFF_SpTimeShow.SpTimeShowType != null)
                    {
                        var list = item.BUFF_SpTimeShow.SpTimeShowType;
                        for (int i = 0; i < list.Count; i++)
                        {
                            int type = (int)list[i];
                            if (!BuffUISpTimeShowTypeList.Contains(type))
                            {
                                BuffUISpTimeShowTypeList.Add(type);
                            }
                        }
                    }
                }
            }
        }

        public bool GetHaveBUFFUIShowState(int specialBUFFUIEnum)
        {
            if (BuffUIShowDetails.ContainsKey(specialBUFFUIEnum))
            {
                return true;
            }
            return false;
        }

        public int GetBUFFUIShowState(int specialBUFFUIEnum)
        {
            int state = 0;
            if (BuffUIShowDetails.TryGetValue(specialBUFFUIEnum, out state))
            {

            }
            return state;
        }

        public List<int> GetBUFFUISpTimeList()
        {
            return BuffUISpTimeShowTypeList;
        }

        protected override void Release()
        {
            base.Release();

            BuffUIShowDetails.Clear();
            BuffUISpTimeShowTypeList.Clear();
            BuffID = 0;
            Cfg = null;
        }
    }
}
