using Fire;
using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using SkillEditor;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player;
using StarProject.Game.Skill;
using StarProject.Service.LocalDynamic;
using StarProjectDef;
using UnityEngine;
using ProVec3 = ProtoMsg.Vector3;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game.SnapShot
{

    public static class SnapShotUtils
    {
        public static string TagFlag = "[SnapShotUtils]";


        //public static void HandleBlackListEffects(List<BlackBoardNode> blackList, ulong ownerEntityID, ulong runtimeID, bool isPlaySpecialEffect = true)
        //{
        //    SGF.Debuger.Log($"{TagFlag} HandleBlackListEffects runtimeID {runtimeID} ownerEntityID {ownerEntityID} blackList {blackList.Count} ");

        //    foreach (var item in blackList)
        //    {
        //        HandleBlackNodeEffect(item, runtimeID, ownerEntityID, isPlaySpecialEffect);
        //    }
        //}

        //public static void HandleBlackListEffects(RepeatedField<BlackBoardNode> blackList, ulong ownerEntityID, ulong runtimeID, bool isPlaySpecialEffect = true)
        //{
        //    SGF.Debuger.Log($"{TagFlag} HandleBlackListEffects runtimeID {runtimeID} ownerEntityID {ownerEntityID} blackList {blackList.Count} ");

        //    foreach (var item in blackList)
        //    {
        //        HandleBlackNodeEffect(item, runtimeID, ownerEntityID, isPlaySpecialEffect);
        //    }
        //}

        ///// <summary>
        ///// 处理黑板效果数据
        ///// note：
        /////     目前会有两种
        /////     1.只需要播放 ：
        /////         对应的效果（如位移的坐标，伤害飘字的飘字动画），
        /////         但这个效果相关的特效并不播放（技能效果中的服务器效果线只播放效果，播放动画）；
        /////     2.播放所有效果 和 特效：
        /////         如普通的子弹 黑板效果，收到了服务器是数据之后，
        /////         需要播放这个效果的所有效果 和 特效。
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="runtimeID">根据传入的runtimeID作为计时器的 group，如果需要取消，这个变量会有效</param>
        ///// <param name="builderID">造成黑板数据的 施法者ID</param>
        ///// <param name="isPlaySpecialEffect">是否播放特效</param>
        //public static void HandleBlackNodeEffect(BlackBoardNode item, ulong runtimeID, ulong builderID, bool isPlaySpecialEffect = true)
        //{
        //    Dictionary<int, SkillEffectDataCell> SkillEffectDataCellMap = LocalDataManager.Instance.M_SkillEffectData.StaticSkillEffectDatas;
        //    //只处理效果 effectID
        //    int idx;
        //    if (int.TryParse(item.Key, out idx))//服务器发的key就是id
        //    {
        //        SkillEffectDataCell skillEffectDataCell;
        //        if (!SkillEffectDataCellMap.TryGetValue(idx, out skillEffectDataCell))
        //        {
        //            SGF.Debuger.LogError($"{TagFlag} id {idx} 配置有问题啦，跟服务器对不上啦！！！");
        //            return;
        //        }
        //        E_SkillEffect effectType = (E_SkillEffect)skillEffectDataCell.GetEffectType();

        //        switch (effectType)
        //        {
        //            case E_SkillEffect.Empty:
        //                break;
        //            case E_SkillEffect.Treat:
        //                break;
        //            case E_SkillEffect.Damage:
        //                {
        //                    HurtNodeMsg hurtMsg = ProtoUtils.DeserializeBlackBoard<HurtNodeMsg>(item);
        //                    int len = hurtMsg.DataList.Count;

        //                    for (int i = 0; i < len; i++)
        //                    {
        //                        HurtData hurtData = hurtMsg.DataList[i];
        //                        //HandleHurtNodeMsg(hurtData, builderID, skillEffectDataCell, runtimeID, isPlaySpecialEffect);
        //                    }
        //                }
        //                break;
        //            case E_SkillEffect.AddBuff:
        //                break;
        //            case E_SkillEffect.RemoveBuff:
        //                break;
        //            case E_SkillEffect.OffsetNodeMsg:
        //                {
        //                    OffsetNodeMsg oNodeMsg = ProtoUtils.DeserializeBlackBoard<OffsetNodeMsg>(item);
        //                    int len = oNodeMsg.DataList.Count;

        //                    for (int i = 0; i < len; i++)
        //                    {
        //                        OffsetData od = oNodeMsg.DataList[i];
        //                        //HandleOffsetNodeMsg(od, builderID, skillEffectDataCell, runtimeID, isPlaySpecialEffect);
        //                    }
        //                }
        //                break;
        //            case E_SkillEffect.CreateBullet:
        //                break;
        //            case E_SkillEffect.UseSkill:
        //                break;
        //            case E_SkillEffect.AddMagic:
        //                break;
        //            case E_SkillEffect.DecMagic:
        //                break;
        //            case E_SkillEffect.IgnoreDamage:
        //                {
        //                    //忽略一些额外判定的伤害，buff伤害等
        //                    HurtNodeMsg hurtMsg = ProtoUtils.DeserializeBlackBoard<HurtNodeMsg>(item);
        //                    int len = hurtMsg.DataList.Count;

        //                    for (int i = 0; i < len; i++)
        //                    {
        //                        HurtData hurtData = hurtMsg.DataList[i];
        //                        //HandleHurtNodeMsg(hurtData, builderID, skillEffectDataCell, runtimeID, isPlaySpecialEffect);
        //                    }
        //                }
        //                break;
        //            case E_SkillEffect.FixCDAbs:
        //                break;
        //            case E_SkillEffect.FixCDPercent:
        //                break;
        //            case E_SkillEffect.SetSkillCD:
        //                break;
        //            case E_SkillEffect.SetSkillCDPercent:
        //                break;
        //            //强制转向效果
        //            case E_SkillEffect.Rotation:
        //                UpRotaNodeMsg rotaNodeMsg = ProtoUtils.DeserializeBlackBoard<UpRotaNodeMsg>(item);
        //                int rlen = rotaNodeMsg.DataList.Count;

        //                for (int i = 0; i < rlen; i++)
        //                {
        //                    UpRotaData od = rotaNodeMsg.DataList[i];
        //                    //HandleRotationNodeMsg(od, builderID, skillEffectDataCell, runtimeID, isPlaySpecialEffect);
        //                }
        //                break;
        //            default:
        //                break;
        //        }

        //    }
        //}


        //public static void HandleDamageEffect(HurtNodeMsg hurtMsg,ulong builderID, int flutteringWordsID)
        //{
        //    //int len = hurtMsg.DataList.Count;

        //    //for (int i = 0; i < len; i++)
        //    //{
        //    //    HurtData hurtData = hurtMsg.DataList[i];
        //    //    EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(hurtData.TargetID);
        //    //    entityCtrlGroup.M_Curr.HandleHurtNodeMsg(hurtData, builderID, flutteringWordsID);
        //    //}
        //}

        ///// <summary>
        ///// 处理位移效果
        ///// </summary>
        ///// <param name="od"></param>
        //public static void HandleRotationNodeMsg(UpRotaData od, ulong builderID, SkillEffectDataCell skillEffectCfg, ulong runtimeID, bool isPlaySpecialEffect)
        //{
        //    // 1.播放当前的位移效果
        //    {
        //        EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(od.TargetID);
        //        if (entityCtrlGroup == null)
        //        {
        //            SGF.Debuger.LogError($"{TagFlag} HandleOffsetNodeMsg TargetID {od.TargetID} cant find target!!!");
        //            return;
        //        }
        //        entityCtrlGroup.HandleActionRotationDataMsg(od, builderID, skillEffectCfg, isPlaySpecialEffect);
        //    }
        //    SGF.Debuger.Log($"{TagFlag} HandleOffsetNodeMsg TargetID {od.TargetID}  runtimeID : {runtimeID}");
        //}

        //public static void StopEffects(ulong runtimeID)
        //{
        //    DelayInvoker.CancelInvoke(runtimeID);
        //}

    }
}
