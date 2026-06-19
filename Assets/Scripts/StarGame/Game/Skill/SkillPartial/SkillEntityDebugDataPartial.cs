using System.Collections;
using System.Collections.Generic;
using BattleDebug;
using Google.Protobuf.Collections;
using ProtoMsg;
using StarProject.Game.Entity;
using UnityEngine;

namespace StarProject.Game.Skill
{

    /// <summary>
    /// 每一个技能创建时,都会创建一个SkillEntity。
    /// SkillEntity包含技能的配置信息,以及根据技能配置和玩家数据,综合计算出的 如 技能的CD等等数据
    /// </summary>
    public partial class SkillEntity : EntityRemoteStatic
    {
        private List<string> formatKeys = new List<string>();
        private List<string> FormatDebugKeys(RepeatedField<BlackBoardNode> blackBoardNodes)
        {
            formatKeys.Clear();
            foreach (BlackBoardNode item in blackBoardNodes)
            {
                formatKeys.Add(item.Key);
            }
            return formatKeys;
        }

        /// <summary>
        /// 汇报 技能的 开始/结束 点信息
        /// </summary>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugSkillDebugData(bool IsStart, string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {

                SkillDebugData data = new SkillDebugData(skillId, RuntimeID.ToString(), IsStart);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        int index = 0;
        /// <summary>
        /// 汇报 技能的事件 
        /// </summary>
        /// <param name="IsStart"></param>
        /// <param name="IsClient"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugSkillEventData(bool IsStart, bool IsClient, string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                SkillStateDebugData data = new SkillStateDebugData(RuntimeID.ToString(), (index++).ToString(), IsStart, IsClient);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        /// <summary>
        /// 汇报 动画 事件
        /// </summary>
        /// <param name="animUID">用来标识动画的唯一 id</param>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugAnimationData(string animUID, bool IsStart, string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                AnimationDebugData data = new AnimationDebugData(RuntimeID.ToString(), animUID, IsStart);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        /// <summary>
        /// 汇报 特效 事件
        /// </summary>
        /// <param name="fxUID"></param>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugFxData(string fxUID, bool IsStart, string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                EffectDebugData data = new EffectDebugData(RuntimeID.ToString(), fxUID, IsStart);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        /// <summary>
        /// 汇报 音效 事件
        /// </summary>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugAudioData(string audioUID, bool IsStart, string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                AudioDebugData data = new AudioDebugData(RuntimeID.ToString(), audioUID, IsStart);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        /// <summary>
        /// 汇报 客户端效果 事件
        /// </summary>
        /// <param name="effectUID"></param>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugClientEffectData(string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                string effectUID = GetUniqueID().ToString();
                ClientEventDebugData data = new ClientEventDebugData(RuntimeID.ToString(), effectUID);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }

        /// <summary>
        /// 汇报 服务器效果 事件
        /// </summary>
        /// <param name="IsStart"></param>
        /// <param name="DisplayName"></param>
        /// <param name="Args"></param>
        private void DebugServerEffectData(string DisplayName, List<string> Args)
        {
#if (UNITY_EDITOR && BATTLE_DEBUG)
            {
                string effectUID = GetUniqueID().ToString();
                ServerEventDebugData data = new ServerEventDebugData(RuntimeID.ToString(), effectUID);
                data.DisplayName = DisplayName;
                data.Args = Args;
                BattleDebugHelper.Debug(data);
            }
#endif
        }
    }
}
