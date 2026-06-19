using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProject.Game.Entity;
using SkillEditor;
using StarProject.Service.LocalData;

namespace StarProject.Game.Skill
{
    public class PassiveInfo : BaseConfigInfo
    {
        private string TagFlag
        {
            get => $"[PassiveInfo_{PassiveID}] ";
        }
        public int PassiveID => ID;

        public PassiveSkillConfig Cfg;

        // 特殊BuffUI显示
        private Dictionary<int, int> BuffUIShowDetails = new Dictionary<int, int>();
        // 特殊BuffUI时间显示
        private List<int> BuffUISpTimeShowTypeList = new();

        public override void Init(int id)
        {
            base.Init(id);

            InitData();
        }

        private void InitData()
        {
            // 配置理论上应该用立即同步逻辑,
            LocalDataManager.Instance.GetPassiveJson(PassiveID, (PassiveJson passiveJson) =>
            {
                if (passiveJson == null)
                {
                    //DB_Close    SGF.Debuger.LogError($"{TagFlag} PassiveID : {PassiveID}  token cfg error!!!");
                    return;
                }

                //DB_Close       SGF.Debuger.Log($"{TagFlag} PassiveID : {PassiveID}  init");

                Cfg = passiveJson.config;

                ConverFx.LoopEffects2FxJsons(Cfg.LoopEffectInEditors, LoopEffectFxs);


                Cfg.States.ForEach((BattleState normalState) =>
                {
                    states.Add((int)normalState);
                });

                InitBUFFSpecialBUFFUI();

                NormalStageJsons = passiveJson.Normals;
                OtherStageJsons = passiveJson.Others;
                BulletStageJsons = passiveJson.Bullets;

                normalTimeLineStageInfos.InitFrameEvents(NormalStageJsons, PassiveID);
                otherTimeLineStageInfos.InitFrameEvents(OtherStageJsons, PassiveID);
                bulletTimeLineStageInfos.InitFrameEvents(BulletStageJsons, PassiveID);
            });
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
            Cfg = null;
        }
    }
}
