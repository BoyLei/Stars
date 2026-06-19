using Google.Protobuf.Collections;
using ProtoMsg;
using Reign;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.RemoteDynamic;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Service.FindPath;
using StarProject.Service.LocalData;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game.Skill.Utils
{
    public static class SkillUtils
    {

        #region 碰撞区域算法

        private static void GetTriggleEntitys(ref List<AOIEntityObject> currentTriggetNtts, ulong entityID, int factionType, SelectType selectType, Predicate<AOIEntityObject> mathch)
        {
            List<EntityCtrlBase> listEntity = GameManager.Instance.M_listEntityCtrl;
            for (int i = 0; i < listEntity.Count; ++i)
            {
                AOIEntityObject AOIEntity = listEntity[i].M_Curr;

                bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(AOIEntity, entityID, factionType, selectType);
                //说明实体类型不是目标类型
                if (!isTargetNtt)
                {
                    continue;
                }
                if (mathch.Invoke(AOIEntity))
                {
                    if (currentTriggetNtts.Contains(AOIEntity))
                    {
                        SGF.Debuger.LogWarning($"SkillUtils GetTriggleEntitys currentTriggetNtts have equal AOIEntity,,error!!!");
                    }
                    else
                    {
                        currentTriggetNtts.Add(AOIEntity);
                    }
                }
            }
        }

        private static void GetTriggleEntitys(ref RepeatedField<ProtoMsg.SkillTarData> skillTarDatas, ulong entityID, int factionType, SelectType selectType, bool isSettleHit, int maxTar, Predicate<AOIEntityObject> mathch)
        {
            List<EntityCtrlBase> listEntity = GameManager.Instance.M_listEntityCtrl;
            for (int i = 0; i < listEntity.Count; ++i)
            {
                AOIEntityObject AOIEntity = listEntity[i].M_Curr;

                bool isTargetNtt = EntityFactoryUtils.CheckIsTriggleAOIEntity(AOIEntity, entityID, factionType, selectType);
                //说明实体类型不是目标类型
                if (!isTargetNtt)
                {
                    continue;
                }

                if (mathch.Invoke(AOIEntity))
                {
                    skillTarDatas.Add(EffectUtils.InitSkillTarData(AOIEntity.EntityId, isSettleHit));
                }

                if (skillTarDatas.Count >= maxTar)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 获取目标在【圆】内【只判断目标坐标】
        /// <param name="currentTriggetNtts">返回的目标实体数组</param>
        /// <param name="builderID">使用者id</param>
        /// <param name="skillPos">技能位置</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// </summary>
        public static void GetTargetInCircle(ref List<AOIEntityObject> currentTriggetNtts, ulong builderID, Vector3 skillPos, float radius, int builderFaction, SelectType selectType)
        {
            GetTriggleEntitys(ref currentTriggetNtts, builderID, builderFaction, selectType, (AOIEntity) =>
            {
                return MathUtilities.CircleAttack(AOIEntity.Position(), skillPos, radius);
            });
        }

        /// <summary>
        /// 获取目标在【圆】内+怪物的半径
        /// </summary>
        /// <param name="currentTriggetNtts">返回的目标实体数组</param>
        /// <param name="builderID">使用者id</param>
        /// <param name="skillPos">技能位置</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        public static void GetTargetInCircle2(ref List<AOIEntityObject> currentTriggetNtts, ulong builderID, Vector3 skillPos, float radius, int builderFaction, SelectType selectType)
        {
            GetTriggleEntitys(ref currentTriggetNtts, builderID, builderFaction, selectType, (AOIEntity) =>
            {
                return MathUtilities.CalIntersection(AOIEntity.Position(), AOIEntity.ModelRadius, skillPos, radius);
            });
        }

        /// <summary>
        /// 获取目标在【圆】内
        /// <param name="skillTarDatas">返回的目标实体数组</param>
        /// <param name="builderID">使用者id</param>
        /// <param name="skillPos">技能位置</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// <param name="isSettleHit">是否伤害</param>
        /// <param name="maxTar">最大数量</param>
        /// </summary>
        public static void GetTargetInCircle(ref RepeatedField<ProtoMsg.SkillTarData> skillTarDatas, ulong builderID, Vector3 skillPos, float radius, int builderFaction, SelectType selectType, bool isSettleHit, int maxTar)
        {
            GetTriggleEntitys(ref skillTarDatas, builderID, builderFaction, selectType, isSettleHit, maxTar, (AOIEntity) =>
            {
                return MathUtilities.CircleAttack(AOIEntity.Position(), skillPos, radius);
            });
        }

        /// <summary>
        /// 获取目标在【扇形】内
        /// <param name="currentTriggetNtts">返回的目标实体数组</param>
        /// <param name="builderID">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">技能朝向</param>
        /// <param name="angle">扇形角度（半径）</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// </summary>
        public static void GetTargetInFan(ref List<AOIEntityObject> currentTriggetNtts, ulong builderID, Vector3 skillPos, Vector3 skillDir, float angle, float radius, int builderFaction, SelectType selectType)
        {
            GetTriggleEntitys(ref currentTriggetNtts, builderID, builderFaction, selectType, (AOIEntity) =>
            {
                return MathUtilities.UmbrellaAttact(skillPos, skillDir.normalized, AOIEntity.Position(), angle, radius);
            });
        }

        /// <summary>
        /// 获取目标在【扇形】内
        /// <param name="skillTarDatas">返回的目标实体数组</param>
        /// <param name="builderID">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">技能朝向</param>
        /// <param name="angle">扇形角度（半径）</param>
        /// <param name="radius">攻击半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// <param name="isSettleHit">是否伤害</param>
        /// <param name="maxTar">最大数量</param>
        /// </summary>
        public static void GetTargetInFan(ref RepeatedField<ProtoMsg.SkillTarData> skillTarDatas, ulong builderID, Vector3 skillPos, Vector3 skillDir, float angle, float radius, int builderFaction, SelectType selectType, bool isSettleHit, int maxTar)
        {
            GetTriggleEntitys(ref skillTarDatas, builderID, builderFaction, selectType, isSettleHit, maxTar, (AOIEntity) =>
            {
                return MathUtilities.UmbrellaAttact(skillPos, skillDir.normalized, AOIEntity.Position(), angle, radius);
            });
        }

        /// <summary>
        /// 获取目标在【环扇形】内
        /// </summary>
        /// <param name="currentTriggetNtts">返回的目标实体数组</param>
        /// <param name="selfEnityId">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">技能朝向</param>
        /// <param name="angle">扇形角度（半径）</param>
        /// <param name="maxRadius">最大半径</param>
        /// <param name="minRadius">最小半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        public static void GetTargetInRingFan(ref List<AOIEntityObject> currentTriggetNtts, ulong selfEnityId, Vector3 skillPos, Vector3 skillDir, float angle, float maxRadius, float minRadius, int builderFaction, SelectType selectType)
        {
            GetTriggleEntitys(ref currentTriggetNtts, selfEnityId, builderFaction, selectType, (AOIEntity) =>
            {
                return MathUtilities.RingFanAttact(skillPos, skillDir.normalized, AOIEntity.Position(), angle, maxRadius, minRadius);
            });
        }

        /// <summary>
        /// 获取目标在【环扇形】内
        /// </summary>
        /// <param name="skillTarDatas">返回的目标实体数组</param>
        /// <param name="selfEnityId">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">技能朝向</param>
        /// <param name="angle">扇形角度（半径）</param>
        /// <param name="maxRadius">最大半径</param>
        /// <param name="minRadius">最小半径</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// <param name="isSettleHit">是否伤害</param>
        /// <param name="maxTar">最大数量</param>
        public static void GetTargetInRingFan(ref RepeatedField<ProtoMsg.SkillTarData> skillTarDatas, ulong selfEnityId, Vector3 skillPos, Vector3 skillDir, float angle, float maxRadius, float minRadius, int builderFaction, SelectType selectType, bool isSettleHit, int maxTar)
        {
            GetTriggleEntitys(ref skillTarDatas, selfEnityId, builderFaction, selectType, isSettleHit, maxTar, (AOIEntity) =>
            {
                return MathUtilities.RingFanAttact(skillPos, skillDir.normalized, AOIEntity.Position(), angle, maxRadius, minRadius);
            });
        }

        /// <summary>
        /// 获取目标在【矩形】内
        /// <param name="currentTriggetNtts">触发的实体</param>
        /// <param name="selfEnityId">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">使用者朝向</param>
        /// <param name="rightDir">使用者右朝向</param>
        /// <param name="rectangleLength">矩形高度</param>
        /// <param name="rectangleWidth">矩形宽度</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// </summary>
        public static void GetTargetInRectangle(ref List<AOIEntityObject> currentTriggetNtts, ulong selfEnityId, Vector3 skillPos, Vector3 skillDir, Vector3 rightDir, float rectangleLength, float rectangleWidth, int builderFaction, SelectType selectType)
        {
            GetTriggleEntitys(ref currentTriggetNtts, selfEnityId, builderFaction, selectType, (AOIEntity) =>
            {
                return MathUtilities.RectangleAttack(skillPos, skillDir, rightDir, AOIEntity.Position(), rectangleLength, rectangleWidth);
            });
        }

        /// <summary>
        /// 获取目标在【矩形】内
        /// <param name="skillTarDatas">触发的实体</param>
        /// <param name="selfEnityId">使用者id</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillDir">使用者朝向</param>
        /// <param name="rightDir">使用者右朝向</param>
        /// <param name="rectangleLength">矩形高度</param>
        /// <param name="rectangleWidth">矩形宽度</param>
        /// <param name="builderFaction">施法者的阵营</param>
        /// <param name="selectType">目标选择类型</param>
        /// <param name="isSettleHit">是否伤害</param>
        /// <param name="maxTar">最大数量</param>
        /// </summary>
        public static void GetTargetInRectangle(ref RepeatedField<ProtoMsg.SkillTarData> skillTarDatas, ulong selfEnityId, Vector3 skillPos, Vector3 skillDir, Vector3 rightDir, float rectangleLength, float rectangleWidth, int builderFaction, SelectType selectType, bool isSettleHit, int maxTar)
        {
            GetTriggleEntitys(ref skillTarDatas, selfEnityId, builderFaction, selectType, isSettleHit, maxTar, (AOIEntity) =>
            {
                return MathUtilities.RectangleAttack(skillPos, skillDir, rightDir, AOIEntity.Position(), rectangleLength, rectangleWidth);
            });
        }

        #endregion


        /// <summary>
        /// 获得最大范围内的目标
        /// </summary>
        /// <param name="builderId">施法者ID</param>
        /// <param name="factionType">施法者阵营</param>
        /// <param name="currentPos">当前坐标</param>
        /// <param name="maxRadius">最大半径</param>
        /// <param name="selectType">查找类型</param>
        /// <returns></returns>
        public static List<AOIEntityObject> GetTargetInMaxRadius(
            ulong builderId,
            int factionType,
            Vector3 currentPos,
            int maxRadius,
            SelectType selectType,
            bool isAddRadius = false
        )
        {
            List<AOIEntityObject> currentTriggetNtts = new();
            float radius = (float)maxRadius / 100;
            if (isAddRadius)
            {
                GetTargetInCircle2(ref currentTriggetNtts, builderId, currentPos, radius, factionType, selectType);
            }
            else
            {
                GetTargetInCircle(ref currentTriggetNtts, builderId, currentPos, radius, factionType, selectType);
            }

            return currentTriggetNtts;
        }

        /// <summary>
        /// 根据技能形状获得在目标中的人
        /// </summary>
        /// <param name="builderId">施法者ID</param>
        /// <param name="factionType">施法者阵营</param>
        /// <param name="skillPos">技能坐标</param>
        /// <param name="skillServerAngle">释放技能的服务器角度</param>
        /// <param name="selectType">查找类型</param>
        /// <param name="Shape">技能形状</param>
        /// <returns></returns>
        public static List<AOIEntityObject> GetTargetInWheelCfg(
                    ulong builderId,
                    int factionType,
                    UnityEngine.Vector3 skillPos,
                    int skillServerAngle,   // 这里是服务器朝向
                    ShapeSerialize Shape,
                    SelectType selectType
            )
        {
            List<AOIEntityObject> currentTriggetNtts = new();
            // 释放技能的朝向
            UnityEngine.Vector3 skillDir = Fire.Utils.ServerRota2Vector(skillServerAngle);

            switch (Shape.ShapeType)
            {
                case SkillEditor.Shape.Round:
                    {
                        GetTargetInCircle(ref currentTriggetNtts, builderId, skillPos, Shape.Round.Radius / 100f, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.HollowCircle:
                    {
                        GetTargetInCircle(ref currentTriggetNtts, builderId, skillPos, Shape.HollowCircle.MinRadius / 100f, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.Sector:
                    {
                        GetTargetInFan(ref currentTriggetNtts, builderId, skillPos, skillDir, Shape.Sector.Angle, Shape.Sector.Radius / 100f, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.RingFan:
                    {
                        GetTargetInRingFan(ref currentTriggetNtts, builderId, skillPos, skillDir, Shape.RingFan.Angle, Shape.RingFan.MaxRadius / 100f, (float)Shape.RingFan.MinRadius / 100, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.Arrow:
                    {
                        // 矩形要计算技能的右朝向
                        UnityEngine.Vector3 dirRightInterpolation = UnityEngine.Vector3.zero;
                        dirRightInterpolation.x = skillDir.z;
                        dirRightInterpolation.z = -skillDir.x;
                        //SGF.Debuger.Log($"矩形右朝向  前 = {dir.normalized} 右 = {dirRightInterpolation.normalized} ");
                        GetTargetInRectangle(ref currentTriggetNtts, builderId, skillPos, skillDir, dirRightInterpolation.normalized, (float)Shape.Arrow.Length / 100, (float)Shape.Arrow.Width / 100, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.Rect:
                    {
                        // 矩形要计算技能的右朝向
                        UnityEngine.Vector3 dirRightInterpolation = UnityEngine.Vector3.zero;
                        dirRightInterpolation.x = skillDir.z;
                        dirRightInterpolation.z = -skillDir.x;
                        //SGF.Debuger.Log($"矩形右朝向  前 = {dir.normalized} 右 = {dirRightInterpolation.normalized} ");
                        GetTargetInRectangle(ref currentTriggetNtts, builderId, skillPos, skillDir, dirRightInterpolation.normalized, (float)Shape.Rect.Length / 100, (float)Shape.Rect.Width / 100, factionType, selectType);
                    }
                    break;
                case SkillEditor.Shape.RotRoute:
                    {
                        // TODO: 曲 路径应该不是攻击敌人
                    }
                    break;
                case SkillEditor.Shape.InputTarget:
                    {
                    }
                    break;
                default:
                    {
                        //DB_Close  SGF.Debuger.LogError($"[GetTargetInWheelCfg] ShapeType : {Shape.ShapeType} no handle error !!!");
                    }
                    break;
            }


            return currentTriggetNtts;

        }

        // 根据技能ID获取是否自动转向
        public static bool GetIsAutoRotBySkillId(int skillID)
        {
            return LocalDataManager.Instance.GetIsAutoRotBySkillId(skillID);
        }

        /// <summary>
        /// 获得距离坐标最近的目标
        /// </summary>
        /// <param name="currentTriggetNtts">目标列表</param>
        /// <param name="currentPos">坐标</param>
        /// <returns></returns>
        public static NPCEntityBase GetClosestTargetId(List<AOIEntityObject> currentTriggetNtts, UnityEngine.Vector3 currentPos)
        {
            // 找距离最近的敌人---追击目标
            NPCEntityBase closestTarget = null;
            float closestDistance = 0f;
            for (int i = 0; i < currentTriggetNtts.Count; i++)
            {
                // 判断和敌方的距离
                UnityEngine.Vector3 curAtkEntityIdPos = currentTriggetNtts[i].Position();
                UnityEngine.Vector3 dirInterpolation = curAtkEntityIdPos - currentPos;
                float distance = dirInterpolation.magnitude;
                if (closestTarget == null)
                {
                    closestTarget = (NPCEntityBase)currentTriggetNtts[i];
                    closestDistance = distance;
                }
                else if (distance < closestDistance)
                {
                    closestTarget = (NPCEntityBase)currentTriggetNtts[i];
                    closestDistance = distance;
                }
            }
            return closestTarget;
        }

        public static NPCEntityBase GetClosestTargetIdByFindPath(List<AOIEntityObject> currentTriggetNtts, UnityEngine.Vector3 currentPos)
        {
            // 找距离最近的敌人---追击目标
            NPCEntityBase closestTarget = null;
            float closestDistance = 0f;
            for (int i = 0; i < currentTriggetNtts.Count; i++)
            {
                // 判断和敌方的距离
                UnityEngine.Vector3 curAtkEntityIdPos = currentTriggetNtts[i].Position();
                FindPathManager.Instance.FindPath(currentPos, curAtkEntityIdPos, out var corners, out var distance);
                // UnityEngine.Vector3 dirInterpolation = curAtkEntityIdPos - currentPos;
                // float distance = dirInterpolation.magnitude;
                if (closestTarget == null)
                {
                    closestTarget = (NPCEntityBase)currentTriggetNtts[i];
                    closestDistance = distance;
                }
                else if (distance < closestDistance)
                {
                    closestTarget = (NPCEntityBase)currentTriggetNtts[i];
                    closestDistance = distance;
                }
            }
            return closestTarget;
        }

        /// <summary>
        /// 获得最大范围内的目标
        /// </summary>
        /// <param name="builderId">施法者ID</param>
        /// <param name="factionType">施法者阵营</param>
        /// <param name="currentPos">当前坐标</param>
        /// <param name="maxRadius">最大半径</param>
        /// <param name="selectType">查找类型</param>
        /// <param name="checkIsValidEntity">检查实体是否是需要的合法的实体, 功能跟上面的 ignoreNttList 其实等同, 主要是为了避免外面调用的地方需要反复多次的 调用 GetClosestTargetIdByTargets </param>
        /// <returns></returns>
        public static NPCEntityBase GetClosestTargetIdByTargets(
                    ulong builderId,
                    int factionType,
                    UnityEngine.Vector3 currentPos,
                    int maxRadius,
                    SelectType selectType,
                    bool isAddRadius = false,
                    Func<AOIEntityObject, bool> checkIsValidEntity = null
            )
        {
            NPCEntityBase curAtkEntity = null;
            List<AOIEntityObject> currentTriggetNtts = GetTargetInMaxRadius(builderId, factionType, currentPos, maxRadius, selectType, isAddRadius);
            if (currentTriggetNtts != null && currentTriggetNtts.Count > 0)
            {

                List<AOIEntityObject> result = new();
                // 如果检查实体是否有效的检查函数存在, 那就对实体做过滤
                if (checkIsValidEntity != null)
                {
                    currentTriggetNtts.ForEach((target) =>
                    {
                        if (checkIsValidEntity.Invoke(target))
                        {
                            result.Add(target);
                        }
                    });
                }
                else
                {
                    result = currentTriggetNtts;
                }

                curAtkEntity = GetClosestTargetId(result, currentPos);
            }
            return curAtkEntity;
        }

        /// <summary>
        /// 找到 以 startPos 为圆心,  maxRadius 为半径 范围内的所有怪物中, 距离 curPos 最近的怪物
        /// </summary>
        /// <param name="builderId"></param>
        /// <param name="factionType"></param>
        /// <param name="startPos"></param>
        /// <param name="curPos"></param>
        /// <param name="maxRadius"></param>
        /// <param name="selectType"></param>
        /// <param name="isAddRadius"></param>
        /// <param name="checkIsValidEntity"></param>
        public static NPCEntityBase GetClosestTargetByStartPos(
                   ulong builderId,
                   int factionType,
                   Vector3 startPos,
                   Vector3 curPos,
                   int maxRadius,
                   SelectType selectType,
                   bool isAddRadius = false,
                   Func<AOIEntityObject, bool> checkIsValidEntity = null
           )
        {
            NPCEntityBase curAtkEntity = null;
            List<AOIEntityObject> currentTriggetNtts = GetTargetInMaxRadius(builderId, factionType, startPos, maxRadius, selectType, isAddRadius);
            if (currentTriggetNtts != null && currentTriggetNtts.Count > 0)
            {

                List<AOIEntityObject> result = new();
                // 如果检查实体是否有效的检查函数存在, 那就对实体做过滤
                if (checkIsValidEntity != null)
                {
                    currentTriggetNtts.ForEach((target) =>
                    {
                        if (checkIsValidEntity.Invoke(target))
                        {
                            result.Add(target);
                        }
                    });
                }
                else
                {
                    result = currentTriggetNtts;
                }

                curAtkEntity = GetClosestTargetIdByFindPath(result, curPos);
            }
            return curAtkEntity;
        }

        /// <summary>
        /// 获得 blackList 中 输入朝向的 值,默认会返回 -1
        /// </summary>
        /// <param name="blackList"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetInputRotaInBlackList(RepeatedField<BlackBoardNode> blackList)
        {
            foreach (var item in blackList)
            {
                if (item.Key == "InputRota")
                {
                    return item.Int32Value;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获得 skillUseReq 的 技能朝向
        /// </summary>
        /// <param name="skillUseReq"></param>
        /// <returns></returns>
        public static int GetSkillReqRot(SkillUseReq skillUseReq)
        {
            int intputRota = GetInputRotaInBlackList(skillUseReq.BlackList);
            int skillRota = intputRota == -1 ? skillUseReq.Rot : intputRota;

            return skillRota;
        }

        /// <summary>
        /// 获得 skillUseRet 的技能朝向
        /// </summary>
        /// <param name="skillUseRet"></param>
        /// <returns></returns>
        public static int GetSkillRetRot(SkillUseRet skillUseRet)
        {
            int intputRota = GetInputRotaInBlackList(skillUseRet.BlackList);
            int skillRota = intputRota == -1 ? skillUseRet.Rot : intputRota;

            return skillRota;
        }


        /// <summary>
        /// 获得实体是否是主角的敌人
        /// </summary>
        /// <param name="AOIEntity"></param>
        /// <returns></returns>
        public static bool GetEntityIsMainPlayerEnemy(AOIEntityObject AOIEntity)
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase == null)
            {
                return false;
            }
            return EntityFactoryUtils.CheckIsTriggleAOIEntity(AOIEntity, GameManager.Instance.mainPlayerId, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Faction, SelectType.Enemy, false);
        }

        public static bool GetEntityIsMainPlayerFriendly(AOIEntityObject AOIEntity, SelectType select)
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase == null)
            {
                return false;
            }
            return EntityFactoryUtils.CheckIsTriggleAOIEntity(AOIEntity, GameManager.Instance.mainPlayerId, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Faction, select, false);
        }

        // 获取面向施法者的角度【Y轴】
        public static float GetOrientationBuilderRotate(ulong builderID, ulong ownerID)
        {
            Vector3 builderPos = GameManager.Instance.GetEntityPosById(builderID);
            Vector3 ownerPos = GameManager.Instance.GetEntityPosById(ownerID);
            Vector3 dirInterpolation = builderPos - ownerPos;
            float y = (float)(Math.Atan2(dirInterpolation.x, dirInterpolation.z) * Mathf.Rad2Deg) % 360;
            return y;
        }


    }
}
