using Google.Protobuf.Collections;
using ProtoMsg;
using SGF.Network;
using SkillEditor;
using StarProject.CustomDataStruct;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Game.Player;
using StarProject.Game.Skill.Utils;
using StarProject.Service.Battle;
using StarProject.Service.LocalDynamic;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using ProVec3 = ProtoMsg.Vector3;
using Vector3 = UnityEngine.Vector3;

namespace StarProject.Game.Skill
{
    public static class EffectUtils
    {
        public static SkillTarData InitSkillTarData(ulong TarID, bool IsHited)
        {
            SkillTarData skillTarData = new();
            skillTarData.TarID = TarID;
            skillTarData.IsHited = IsHited;
            return skillTarData;
        }

        /// <summary>
        /// 取 目标实体 偏转 angle度，距离 distance的坐标
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        /// <param name="success">坐标获取是否成功,如果没有目标实体,那获取失败</param>
        public static ProVec3 GetTargetPos(ulong targetID, float angle, float distance, out bool success)
        {
            ProVec3 pos = new();
            // 取得目标节点的坐标
            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(targetID);
            success = entityCtrlBase != null;
            if (success)
            {
                UnityEngine.Vector3 targetPos;
                // 这里直接去服务器的角度计算，因为用的时候统一按服务器的数据来处理
                float curAngles = entityCtrlBase.M_Curr.ServerAngles + angle;
                targetPos = Fire.Utils.PosMoveBySeverRota(entityCtrlBase.M_Curr.Position(), curAngles, distance / 100.0f);
                pos.X = targetPos.x;
                pos.Y = targetPos.y;
                pos.Z = targetPos.z;
            }

            return pos;
        }

        /// <summary>
        /// 计算 目标entity 朝向 转到 目标pos 点朝向 偏转了 多少角度
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="pos"></param>
        public static int GetTargetEntity2PosAngle(ulong targetID, Vector3 pos, out bool succeed)
        {
            succeed = true;
            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(targetID);
            if (entityCtrlBase == null)
            {
                succeed = false;
                return 0;
            }
            var moveDir = pos - entityCtrlBase.M_Curr.Position();
            var targetDir = entityCtrlBase.M_Curr.M_EntityAnglesDir;

            return (int)Fire.Utils.GetAngle(targetDir, moveDir, Vector3.up);
        }

        public static int GetTargetServerAngle(ulong targetID, int towardOffset, out bool success)
        {
            int angle = 0;
            // 取得目标节点的 ServerAngles
            EntityCtrlBase entityCtrlBase = GameManager.Instance.GetEntityCtr(targetID);
            success = entityCtrlBase != null;
            if (success)
            {
                angle = entityCtrlBase.M_Curr.ServerAngles + towardOffset;
            }

            return angle;
        }

        /// <summary>
        /// 检查 黑板中 是否 拥有所有的 keys 的黑板数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="baseBlackBoard"></param>
        /// <param name="tag">从什么黑板中查找数据</param>
        /// <returns></returns>
        public static bool CheckIsBlackBoardHasKey(BaseBlackBoard baseBlackBoard, string key, E_BlackBoardTag tag)
        {
            return baseBlackBoard.Contain(key, tag);
        }

        /// <summary>
        /// 检查 黑板中 是否 拥有 所有 keys 的 黑板数据
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="keys"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool CheckIsBlackBoardHasKeys(BaseBlackBoard baseBlackBoard, List<string> keys, E_BlackBoardTag tag)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (!baseBlackBoard.Contain(keys[i], tag))
                {
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// 根据 hitType 确定 SkillTarData 是否是目标的类型
        /// </summary>
        /// <param name="skillTarData"></param>
        /// <param name="hitType">命中类型</param>
        /// <returns></returns>
        public static bool CheckIsSkillTarDataByHit(SkillTarData skillTarData, TransTargetIsHit hitType)
        {
            bool IsTargetHit = skillTarData.IsHited;
            bool isTarget = true;
            switch (hitType)
            {
                case TransTargetIsHit.All:
                    {
                        //如果时所有的,就全部都时目标，此处啥都不用做
                        //dont do anything
                    }
                    break;
                case TransTargetIsHit.IsHit:
                    {
                        if (!IsTargetHit)
                        {
                            isTarget = false;
                        }
                    }
                    break;
                case TransTargetIsHit.IsMiss:
                    {
                        if (IsTargetHit)
                        {
                            isTarget = false;
                        }
                    }
                    break;

                default:
                    {
                        //如果时所有的,就全部都时目标，此处啥都不用做
                        //dont do anything
                        //SGF.Debuger.Log($"[CheckIsSkillTarDataByHit] hitType : {hitType} no handle error !!!");
                    }
                    break;
            }
            return isTarget;
        }

        /// <summary>
        /// 获得 skillTarDatas 对应命中类型的坐标（坐标先计算 朝向角度偏移,再计算距离）
        /// </summary>
        /// <param name="skillTarDatas"></param>
        /// <param name="hitType"></param>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        public static RepeatedField<ProVec3> GetSkillTargetsProVec3Arr(RepeatedField<SkillTarData> skillTarDatas, TransTargetIsHit hitType, float angle, float distance)
        {
            RepeatedField<ProVec3> posArray = new();

            for (int i = 0; i < skillTarDatas.Count; i++)
            {
                SkillTarData target = skillTarDatas[i];

                bool isSkillTarget = CheckIsSkillTarDataByHit(target, hitType);
                if (!isSkillTarget)
                {
                    continue;
                }

                ProVec3 pos = GetTargetPos(target.TarID, angle, distance, out bool findResult);
                if (findResult)
                {
                    posArray.Add(pos);
                }
            }
            return posArray;
        }

        /// <summary>
        /// 获取目标targets的 protoV3数组
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="hitType"></param>
        /// <param name="angle"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetTargetsProVec3Arr(RepeatedField<ulong> targets, TransTargetIsHit hitType, float angle, float distance)
        {
            RepeatedField<ProVec3> posArray = new();

            for (int i = 0; i < targets.Count; i++)
            {
                ulong target = targets[i];

                ProVec3 pos = GetTargetPos(target, angle, distance, out bool findResult);
                if (findResult)
                {
                    posArray.Add(pos);
                }
            }
            return posArray;
        }

        /// <summary>
        /// 得到 skillTarDatas 对应命中类型的 服务器朝向angle
        /// </summary>
        /// <param name="skillTarDatas"></param>
        /// <param name="hitType"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetTargetsTowardArr(RepeatedField<SkillTarData> skillTarDatas, int towardOffset, TransTargetIsHit hitType)
        {
            RepeatedField<int> angleArr = new();

            for (int i = 0; i < skillTarDatas.Count; i++)
            {
                SkillTarData target = skillTarDatas[i];

                bool isSkillTarget = CheckIsSkillTarDataByHit(target, hitType);
                if (!isSkillTarget)
                {
                    continue;
                }

                int angle = GetTargetServerAngle(target.TarID, towardOffset, out bool findResult);
                if (findResult)
                {
                    angleArr.Add(angle);
                }
            }
            return angleArr;
        }



        /// <summary>
        /// 获得效果 targetKey 对应的 SkillTarData 列表
        /// </summary>
        /// <param name="targetKey">效果配置的目标targetKey</param>
        /// <param name="effectParam"><效果参数/param>
        /// <param name="blackBoard">黑板</param>
        /// <returns></returns>
        public static RepeatedField<SkillTarData> GetKeySkillTargetArray(string targetKey, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            RepeatedField<SkillTarData> SkillTars = new();

            // 服务器黑板key的查找,目前跟服务器约定,不查找 builder和 owner
            // 客户端 查找服务器黑板数据的key 都是 非固定的Key(非Builder/owner)
            switch (targetKey)
            {
                case GameConfig.SKILL_BUILDER:
                    {
                        ulong builder = effectParam.Builder;
                        SkillTars.Add(InitSkillTarData(builder, true));
                    }
                    break;
                case GameConfig.SKILL_OWNER:
                    {
                        ulong owner = effectParam.Owner;
                        SkillTars.Add(InitSkillTarData(owner, true));
                    }
                    break;
                default:
                    {
                        if (targetKey == "BulletTarget")
                        {
                            //DB_Close       SGF.Debuger.Log($"碰撞盒数据 客户端取黑板数据1111 key={targetKey}");
                        }
                        CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(targetKey);
                        if (customBlackBoardNode != null)
                        {
                            object v = customBlackBoardNode.Value;
                            if (v is SkillTarsMsg)
                            {
                                SkillTarsMsg skillTarsMsg = (SkillTarsMsg)customBlackBoardNode.Value;
                                SkillTars = skillTarsMsg.SkillTars;
                            }
                            else if (v is ulong)
                            {
                                ulong target = (ulong)customBlackBoardNode.Value;
                                SkillTars.Add(InitSkillTarData(target, true));
                            }
                            else
                            {
                                //DB_Close   SGF.Debuger.LogError($"[GetKeySkillTarget] key : {targetKey} error !!!");
                            }
                        }
                        else
                        {
                            //DB_Close   SGF.Debuger.LogError($"[EffectUtils] [x-x] GetKeySkillTarget blackBoard[{blackBoard.GetHashCode()}] key : {targetKey} error !!!");

                        }
                    }
                    break;
            }

            return SkillTars;
        }

        /// <summary>
        /// 获得 目标列表, 结构为简单的 int64[]
        /// </summary>
        /// <param name="targetKey"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ulong> GetKeyTargetArray(string targetKey, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            RepeatedField<ulong> SkillTars = new();

            // 服务器黑板key的查找,目前跟服务器约定,不查找 builder和 owner
            // 客户端 查找服务器黑板数据的key 都是 非固定的Key(非Builder/owner)
            switch (targetKey)
            {
                case GameConfig.SKILL_BUILDER:
                    {
                        ulong builder = effectParam.Builder;
                        SkillTars.Add(builder);
                    }
                    break;
                case GameConfig.SKILL_OWNER:
                    {
                        ulong owner = effectParam.Owner;
                        SkillTars.Add(owner);
                    }
                    break;
                default:
                    {

                        CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(targetKey);
                        if (customBlackBoardNode != null)
                        {
                            object v = customBlackBoardNode.Value;
                            if (v is ulong)
                            {
                                ulong target = (ulong)customBlackBoardNode.Value;
                                SkillTars.Add(target);
                            }
                            else if (v is SkillTarsMsg)
                            {
                                SkillTarsMsg skillTarsMsg = (SkillTarsMsg)customBlackBoardNode.Value;
                                RepeatedField<SkillTarData> skills = skillTarsMsg.SkillTars;
                                for (int i = 0; i < skills.Count; i++)
                                {
                                    if (skills[i].IsHited)
                                    {
                                        SkillTars.Add(skills[i].TarID);
                                    }
                                }
                            }
                            else
                            {
                                //DB_Close   SGF.Debuger.LogError($"[GetKeyTarget] key : {targetKey} , no handle type : {v.GetType()} error !!!");
                            }

                        }
                    }
                    break;
            }

            return SkillTars;
        }

        public static RepeatedField<ulong> GetBlackBoardKeyTarget(string targetKey, BaseBlackBoard blackBoard)
        {
            RepeatedField<ulong> targets = new();

            CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(targetKey);
            if (customBlackBoardNode != null)
            {
                object v = customBlackBoardNode.Value;
                if (v is ulong)
                {
                    ulong target = (ulong)customBlackBoardNode.Value;
                    targets.Add(target);
                }
                else if (v is RepeatedField<ulong>)
                {
                    targets = (RepeatedField<ulong>)customBlackBoardNode.Value;
                }
                else if (v is SkillTarsMsg)
                {
                    SkillTarsMsg skillTarsMsg = (SkillTarsMsg)customBlackBoardNode.Value;
                    RepeatedField<SkillTarData> skills = skillTarsMsg.SkillTars;
                    for (int i = 0; i < skills.Count; i++)
                    {
                        if (skills[i].IsHited)
                        {
                            targets.Add(skills[i].TarID);
                        }
                    }
                }
                else
                {
                    SGF.Debuger.LogWarning($"[GetBlackBoardKeyTarget] key : {targetKey} , no handle type : {v.GetType()} error !!!");
                }
            }

            return targets;
        }

        /// <summary>
        /// 获得效果 posKey 对应的 ProVec3 
        /// </summary>
        /// <param name="posKey"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetKeyPosArray(string posKey, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            RepeatedField<ProVec3> posArray = new();

            // 服务器黑板key的查找,目前跟服务器约定,不查找 builder和 owner
            // 客户端 查找服务器黑板数据的key 都是 非固定的Key(非Builder/owner)
            switch (posKey)
            {
                case GameConfig.SKILL_BUILDER:
                    {
                        ulong builder = effectParam.Builder;

                        ProVec3 v3 = GetTargetPos(builder, 0, 0, out bool success);
                        if (success)
                        {
                            posArray.Add(v3);
                        }
                    }
                    break;
                case GameConfig.SKILL_OWNER:
                    {
                        ulong owner = effectParam.Owner;
                        ProVec3 v3 = GetTargetPos(owner, 0, 0, out bool success);
                        if (success)
                        {
                            posArray.Add(v3);
                        }
                    }
                    break;
                default:
                    {
                        if (posKey == "BulletTarget")
                        {
                            //DB_Close       SGF.Debuger.Log($"碰撞盒数据 客户端取黑板数据 key={posKey}");
                        }
                        CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(posKey);
                        if (customBlackBoardNode != null)
                        {

                            object v = customBlackBoardNode.Value;
                            if (v is ProVec3)
                            {
                                ProVec3 v3 = (ProVec3)customBlackBoardNode.Value;

                                posArray.Add(v3);
                            }
                            else if (v is PosArray)
                            {
                                PosArray posArray2 = (PosArray)customBlackBoardNode.Value;
                                posArray = posArray2.Pos;
                            }
                            else if (v is ulong)
                            {
                                ulong target = (ulong)customBlackBoardNode.Value;
                                ProVec3 v3 = GetTargetPos(target, 0, 0, out bool success);
                                if (success)
                                {
                                    posArray.Add(v3);
                                }
                            }
                            else
                            {
                                //DB_Close   SGF.Debuger.LogError($"[GetKeyPos] key : {posKey} error !!!");
                            }
                        }
                    }
                    break;
            }

            return posArray;
        }



        /// <summary>
        /// 获得 key 对应的 朝向
        /// </summary>
        /// <param name="posKey"></param>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetKeyTowardArray(string posKey, int towardOffset, I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            RepeatedField<int> arr = new();

            // 服务器黑板key的查找,目前跟服务器约定,不查找 builder和 owner
            // 客户端 查找服务器黑板数据的key 都是 非固定的Key(非Builder/owner)
            switch (posKey)
            {
                case GameConfig.SKILL_BUILDER:
                    {
                        ulong builder = effectParam.Builder;

                        int angle = GetTargetServerAngle(builder, towardOffset, out bool success);
                        if (success)
                        {
                            arr.Add(angle);
                        }
                    }
                    break;
                case GameConfig.SKILL_OWNER:
                    {
                        ulong owner = effectParam.Owner;
                        int angle = GetTargetServerAngle(owner, towardOffset, out bool success);
                        if (success)
                        {
                            arr.Add(angle);
                        }
                    }
                    break;
                default:
                    {
                        CustomBlackBoardNode customBlackBoardNode = blackBoard.GetKey<CustomBlackBoardNode>(posKey);
                        if (customBlackBoardNode != null)
                        {

                            object v = customBlackBoardNode.Value;
                            if (v is ArrayInt32)
                            {
                                ArrayInt32 arrayInt321 = (ArrayInt32)customBlackBoardNode.Value;
                                arr = arrayInt321.Value;
                            }
                            if (v is int)
                            {
                                int angle = (int)customBlackBoardNode.Value;
                                arr.Add(angle);
                            }
                            else if (v is ulong)
                            {
                                ulong target = (ulong)customBlackBoardNode.Value;
                                int angle = GetTargetServerAngle(target, towardOffset, out bool success);
                                if (success)
                                {
                                    arr.Add(angle);
                                }
                            }
                            else
                            {
                                //DB_Close   SGF.Debuger.LogError($"[GetKeyToward] key : {posKey} error !!!");
                            }
                        }
                    }
                    break;
            }

            return arr;
        }



        /// <summary>
        /// 得到 CenterPosArray  中心目标Key (CenterPosTargetKey) 对应的 坐标数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="centerPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetSkillTargetsCenterPos(I_EffectParam effectParam, CenterPosArray centerPos, BaseBlackBoard blackBoard)
        {
            float angle = centerPos.Angle;
            float distance = centerPos.Distance;
            EffectTypeCollisionBox collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;
            // 如果碰撞盒形状是矩形的话，客户端这边要的中线点是玩家脚下（一个边的中心点，，不是矩形的中心点）
            if (collisionBox != null)
            {
                if (collisionBox.Shape.ShapeType == Shape.Rect)
                {
                    float temp = distance;
                    if (distance >= 0)
                    {
                        distance = temp - (collisionBox.Shape.Rect.Length / 2);
                    }
                    else
                    {
                        distance = -temp - (collisionBox.Shape.Rect.Length / 2);
                    }
                }
                if (collisionBox.Shape.ShapeType == Shape.Arrow)
                {
                    float temp = distance;
                    if (distance >= 0)
                    {
                        distance = temp - (collisionBox.Shape.Arrow.Length / 2);
                    }
                    else
                    {
                        distance = -temp - (collisionBox.Shape.Arrow.Length / 2);
                    }
                }
            }

            string centerPosTargetKey = centerPos.TargetKey.Result;

            //计算 centerPosTargetKey  对于的target
            RepeatedField<SkillTarData> centerPosTargets = GetKeySkillTargetArray(centerPosTargetKey, effectParam, blackBoard);

            RepeatedField<ProVec3> posArray = GetSkillTargetsProVec3Arr(centerPosTargets, centerPos.TransTargetIsHit, angle, distance);

            return posArray;
        }

        /// <summary>
        ///  得到 centerPosTargets  中 centerPos 配置的 是否命中 对应的 坐标数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="centerPos"></param>
        /// <param name="centerPosTargets"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetSkillTargetsCenterPos(I_EffectParam effectParam, CenterPosArray centerPos, RepeatedField<SkillTarData> centerPosTargets)
        {
            float angle = centerPos.Angle;
            float distance = centerPos.Distance;
            EffectTypeCollisionBox collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;

            // 如果碰撞盒形状是矩形的话，客户端这边要的中线点是玩家脚下（一个边的中心点，，不是矩形的中心点）
            if (collisionBox != null)
            {
                if (collisionBox.Shape.ShapeType == Shape.Rect)
                {
                    distance = distance - (collisionBox.Shape.Rect.Length / 2);
                }
                if (collisionBox.Shape.ShapeType == Shape.Arrow)
                {
                    distance = distance - (collisionBox.Shape.Arrow.Length / 2);
                }
            }

            RepeatedField<ProVec3> posArray = GetSkillTargetsProVec3Arr(centerPosTargets, centerPos.TransTargetIsHit, angle, distance);

            return posArray;
        }



        /// <summary>
        /// 得到 centerPos PosKey 对应的 坐标数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="centerPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetPosKeyPosArray(I_EffectParam effectParam, CenterPosArray centerPos, BaseBlackBoard blackBoard)
        {
            string posKey = centerPos.PosKey.Result;

            // 如果是  posKey ,直接取 黑板的坐标即可,不需要 再去计算偏移之类的
            RepeatedField<ProVec3> posArray = GetKeyPosArray(posKey, effectParam, blackBoard);

            return posArray;
        }

        /// <summary>
        /// 得到碰撞合 CollisionPos 中心点 的 posArr
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetCollisionPosArray(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            //碰撞合参数 
            EffectTypeCollisionBox effectTypeCollisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;
            // 中心点
            CenterPosArray centerPos = effectTypeCollisionBox.CenterPosArray;

            RepeatedField<ProVec3> posTargetPos = GetSkillTargetsCenterPos(effectParam, centerPos, blackBoard);
            RepeatedField<ProVec3> posKeyPos = GetPosKeyPosArray(effectParam, centerPos, blackBoard);


            RepeatedField<ProVec3> posArray = posTargetPos.MergeProtoArray<ProVec3>(posKeyPos,
             (ProVec3 pos1, ProVec3 pos2) =>
             {
                 // 注意: 
                 //  服务器KL 在 此处并没有做 坐标过滤 !!!!
                 //  策划 的意思 是说 不应该 同时配置 中心目标CenterPosTargetKey 和 坐标Key,可以不管
                 //  所以 本地 还是 做个去重,防止 以后还是会有 
                 bool samePos = pos1.X == pos2.X && pos1.Y == pos2.Y && pos1.Z == pos2.Z;
                 return samePos;
             });

            return posArray;
        }

        /// <summary>
        /// 获得 黑板 中, EffectType类型为 CenterPosArray 的 坐标数组
        /// note:
        ///     1.TargetKey :  中心目标Key
        ///         计算的是 TargetKey 对应的 角度 和 距离 对应的坐标点;
        ///     2.PosKey : 坐标Key
        ///         计算的是 对于 黑板 key的坐标即可
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="centerPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<ProVec3> GetCenterPosArray(I_EffectParam effectParam, CenterPosArray centerPos, BaseBlackBoard blackBoard)
        {
            RepeatedField<ProVec3> posTargetPos = GetSkillTargetsCenterPos(effectParam, centerPos, blackBoard);
            RepeatedField<ProVec3> posKeyPos = GetPosKeyPosArray(effectParam, centerPos, blackBoard);


            RepeatedField<ProVec3> posArray = posTargetPos.MergeProtoArray<ProVec3>(posKeyPos,
             (ProVec3 pos1, ProVec3 pos2) =>
             {
                 // 注意: 
                 //  服务器KL 在 此处并没有做 坐标过滤 !!!!
                 //  策划 的意思 是说 不应该 同时配置 中心目标CenterPosTargetKey 和 坐标Key,可以不管
                 //  所以 本地 还是 做个去重,防止 以后还是会有 
                 bool samePos = pos1.X == pos2.X && pos1.Y == pos2.Y && pos1.Z == pos2.Z;
                 return samePos;
             });

            return posArray;
        }

        /// <summary>
        /// 2023/9/25
        ///  获取 中心点 坐标, 客户端 这边 如果拿不到就 返回 null
        ///  而 对于服务器来说,拿不到 会给一个 运行时的 owner 的坐标. 
        ///  这个 跟 客户端 的不同。 
        ///  目前 gl 也不清楚 这个细节。 目前 或者 中心点 坐标 一般是客户端预播时 才会用到(服务器线 播放这个效果,会采用这个效果的outPutKey)。
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="singleCenterPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static ProVec3 GetSingleCenterTargetKeyPos(I_EffectParam effectParam, SingleCenterPos singleCenterPos, BaseBlackBoard blackBoard)
        {
            //计算 centerPosTargetKey  对于的target
            RepeatedField<SkillTarData> centerPosTargets = GetKeySkillTargetArray(singleCenterPos.TargetKey.Result, effectParam, blackBoard);

            List<ProVec3> posArray = new();
            for (int i = 0; i < centerPosTargets.Count; i++)
            {
                SkillTarData target = centerPosTargets[i];

                ProVec3 pos = GetTargetPos(target.TarID, singleCenterPos.Angle, singleCenterPos.Distance, out bool findResult);
                if (findResult)
                {
                    posArray.Add(pos);
                }
            }
            if (posArray.Count > 0)
            {
                return posArray[0];
            }
            return null;
        }

        /// <summary>
        /// 获得 PosKey 对应的坐标. 
        /// 跟gl 确定 如下:
        ///     对于 CenterPos 和 SingleCenterPos 来说, PosKey 的坐标 就是目标的坐标, 不需要再去 计算 它的 angle 和 Distance.
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="singleCenterPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static ProVec3 GetSingleCenterKeyPos(I_EffectParam effectParam, SingleCenterPos singleCenterPos, BaseBlackBoard blackBoard)
        {
            string posKey = singleCenterPos.PosKey.Result;

            RepeatedField<ProVec3> posArray = GetKeyPosArray(posKey, effectParam, blackBoard);

            if (posArray.Count > 0)
            {
                return posArray[0];
            }
            return null;
        }

        /// <summary>
        /// 获得 中心点 坐标
        /// <param name="effectParam"></param>
        /// <param name="centerPos"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static ProVec3 GetSingleCenterPos(I_EffectParam effectParam, SingleCenterPos singleCenterPos, BaseBlackBoard blackBoard)
        {
            // 先获取 TargetKey 目标对应的坐标, 如果找不到, 就采用 Poskey 对应的 坐标.
            ProVec3 posTargetPos = GetSingleCenterTargetKeyPos(effectParam, singleCenterPos, blackBoard);
            if (posTargetPos != null)
            {
                return posTargetPos;
            }

            return GetSingleCenterKeyPos(effectParam, singleCenterPos, blackBoard);
        }

        /// <summary>
        /// 得到 TowardArray 朝向目标TowardTargetKey 对应目标的 朝向数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="toward"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetTargetKeyTowardArr(I_EffectParam effectParam, TowardArray toward, BaseBlackBoard blackBoard)
        {
            RepeatedField<SkillTarData> centerPosTargets = GetKeySkillTargetArray(toward.TargetKey.Result, effectParam, blackBoard);

            RepeatedField<int> angleArr = GetTargetsTowardArr(centerPosTargets, toward.TowardOffset, toward.TransTargetIsHit);

            return angleArr;
        }

        /// <summary>
        /// 得到 朝向Key 对应的 朝向数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="toward"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetTowardKeyArr(I_EffectParam effectParam, TowardArray toward, BaseBlackBoard blackBoard)
        {
            RepeatedField<int> angleArr = GetKeyTowardArray(toward.TowardKey.Result, toward.TowardOffset, effectParam, blackBoard);

            return angleArr;
        }

        /// <summary>
        /// 获得 黑板中 towardArray 数组
        /// note:
        ///     1.TargetKey :  朝向目标Key
        ///         得到 TowardArray 朝向目标TowardTargetKey 对应目标的 朝向数组
        ///     2.TowardKey : 朝向Key
        ///         得到 朝向Key 对应的 朝向数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="towardArray"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetTowardArray(I_EffectParam effectParam, TowardArray towardArray, BaseBlackBoard blackBoard)
        {
            RepeatedField<int> targetKeyTowardArr = GetTargetKeyTowardArr(effectParam, towardArray, blackBoard);

            RepeatedField<int> towardKeyarr = GetTowardKeyArr(effectParam, towardArray, blackBoard);



            RepeatedField<int> angleArr = targetKeyTowardArr.MergeProtoArray<int>(towardKeyarr,
            (int angle1, int angle2) =>
            {
                return angle1 == angle2;
            });

            if (angleArr.Count == 0)
            {
                // 取的就是 服务器角度
                int angle = towardArray.TowardOffset;
                angleArr.Add(angle);
            }

            return angleArr;
        }

        /// <summary>
        /// 得到碰撞合 CollisionTowards 朝向 的 数组
        /// </summary>
        /// <param name="effectParam"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static RepeatedField<int> GetCollisionTowardArr(I_EffectParam effectParam, BaseBlackBoard blackBoard)
        {
            EffectTypeCollisionBox collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;

            TowardArray towardArray = collisionBox.TowardArray;

            RepeatedField<int> angleArr = GetTowardArray(effectParam, towardArray, blackBoard);
            return angleArr;
        }

        /// <summary>
        /// 获取技能的朝向
        /// </summary>
        /// <param name="centerPos"></param>
        /// <param name="curAngles"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 GetSkillDirection(CenterPosArray centerPos, float curAngles)
        {
            float anglesCfg = centerPos.Angle;
            float angles = curAngles + anglesCfg;
            return Fire.Utils.ServerRota2Vector(angles);
        }

        /// <summary>
        /// 得到碰撞盒 命中的 skillTargetData
        /// </summary>
        public static RepeatedField<SkillTarData> GetCollisionHitTargets(
            ulong builderId,
            int factionType,
            ProVec3 centerPos,
            UnityEngine.Vector3 skillDir,   // 先留着，测试好了，确定不用，再删除
            int angle,
            ShapeSerialize Shape,
            SelectType selectType,
            bool IsSettleHit,
            int MaxTar = 10
            )
        {
            RepeatedField<SkillTarData> skillTarDatas = new();

            // 释放技能的中心点
            UnityEngine.Vector3 skillPos = ProtoUtils.ConvertProtoVec3ToUnityVec3(centerPos);
            // 释放技能的朝向
            UnityEngine.Vector3 skillDir2 = Fire.Utils.ServerRota2Vector(angle);

            switch (Shape.ShapeType)
            {
                case SkillEditor.Shape.Round:
                    {
                        SkillUtils.GetTargetInCircle(ref skillTarDatas, builderId, skillPos, (float)Shape.Round.Radius / 100, factionType, selectType, IsSettleHit, MaxTar);
                    }
                    break;
                case SkillEditor.Shape.HollowCircle:
                    {
                        SkillUtils.GetTargetInCircle(ref skillTarDatas, builderId, skillPos, (float)Shape.HollowCircle.MinRadius / 100, factionType, selectType, IsSettleHit, MaxTar);
                    }
                    break;
                case SkillEditor.Shape.Sector:
                    {
                        SkillUtils.GetTargetInFan(ref skillTarDatas, builderId, skillPos, skillDir2, Shape.Sector.Angle, (float)Shape.Sector.Radius / 100, factionType, selectType, IsSettleHit, MaxTar);
                    }
                    break;
                case SkillEditor.Shape.RingFan:
                    {
                        SkillUtils.GetTargetInRingFan(ref skillTarDatas, builderId, skillPos, skillDir2, Shape.RingFan.Angle, (float)Shape.RingFan.MaxRadius / 100, (float)Shape.RingFan.MinRadius / 100, factionType, selectType, IsSettleHit, MaxTar);
                    }
                    break;
                case SkillEditor.Shape.Arrow:
                    {
                        // 矩形要计算技能的右朝向
                        UnityEngine.Vector3 dirRightInterpolation = UnityEngine.Vector3.zero;
                        dirRightInterpolation.x = skillDir2.z;
                        dirRightInterpolation.z = -skillDir2.x;
                        //SGF.Debuger.Log($"矩形右朝向  前 = {dir.normalized} 右 = {dirRightInterpolation.normalized} ");
                        SkillUtils.GetTargetInRectangle(ref skillTarDatas, builderId, skillPos, skillDir2, dirRightInterpolation.normalized, (float)Shape.Arrow.Length / 100, (float)Shape.Arrow.Width / 100, factionType, selectType, IsSettleHit, MaxTar);
                    }
                    break;
                case SkillEditor.Shape.Rect:
                    {
                        // 矩形要计算技能的右朝向
                        UnityEngine.Vector3 dirRightInterpolation = UnityEngine.Vector3.zero;
                        dirRightInterpolation.x = skillDir2.z;
                        dirRightInterpolation.z = -skillDir2.x;
                        //SGF.Debuger.Log($"矩形右朝向  前 = {dir.normalized} 右 = {dirRightInterpolation.normalized} ");
                        SkillUtils.GetTargetInRectangle(ref skillTarDatas, builderId, skillPos, skillDir2, dirRightInterpolation.normalized, (float)Shape.Rect.Length / 100, (float)Shape.Rect.Width / 100, factionType, selectType, IsSettleHit, MaxTar);
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
                        //DB_Close   SGF.Debuger.LogError($"[GetCollisionHitTargets] ShapeType : {Shape.ShapeType} no handle error !!!");
                    }
                    break;
            }

            return skillTarDatas;
        }

        /// <summary>
        /// 2023/2/1
        /// 将 effectParam 注册进入 reg 黑板中
        /// note1:
        ///     目前的 reg 黑板数据 由单个的 outputkey的注册监听，改为一个outputkey对应多个效果的监听方式.
        /// note2:
        ///     虽然改为了 1个outputkey ---> 多个 效果 监听的方式, 但还是不允许策划配置成 连续效果 输出相同的outputkey的情况;
        ///     eg:
        ///         存在 A/B/C 三个效果, 均输出 outputkey X. 如果 服务器在 B 执行结束后 通过Aoi 通知玩家技能恢复,
        ///         此时，客户端 的状态恢复 可能在 A--->B 或者 B--->C 之间(客户端由于网络延迟推算的位置可能在服务器前面,
        ///         也可能在后面)。
        ///         如果在 A--->B 之间, 客户端会注册 了 两次 X（B、C） 的监听，但服务器只会发过来一次 C的 X。
        ///         从而导致 客户端 执行异常。
        /// 
        ///     跟夏哥沟通，青空之前也 考虑过这种问题, 解决的方案是 限制策划 配置连续的 相同的 outputkey的效果.
        /// </summary>
        /// <param name="regBlackBoard"></param>
        /// <param name="effectParam"></param>
        public static void RegisterOutputKeyEffect(BaseBlackBoard regBlackBoard, I_EffectParam effectParam)
        {
            E_BlackBoardTag tag = E_BlackBoardTag.Reg;
            string outputKey = effectParam.OutputKey;
            if (outputKey.Length == 0)
            {
                return;
            }
            RegisterKeyEffect(regBlackBoard, outputKey, effectParam, tag);
        }

        public static void RegisterInputKeysEffect(BaseBlackBoard regBlackBoard, I_EffectParam effectParam)
        {
            E_BlackBoardTag tag = E_BlackBoardTag.Reg;
            List<string> inputKeys = effectParam.InputKeys;

            RegisterKeyEffects(regBlackBoard, inputKeys, effectParam, tag);
        }

        /// <summary>
        /// 注册 多个 keys 的效果监听
        /// </summary>
        /// <param name="regBlackBoard"></param>
        /// <param name="keys"></param>
        /// <param name="effectParam"></param>
        /// <param name="tag"></param>
        public static void RegisterKeyEffects(BaseBlackBoard regBlackBoard, List<string> keys, I_EffectParam effectParam, E_BlackBoardTag tag)
        {
            keys.ForEach((string key) =>
            {
                RegisterKeyEffect(regBlackBoard, key, effectParam, tag);
            });
        }

        /// <summary>
        /// 2023/3/6
        ///     注册 key 对应的 effect 效果 监听
        ///     note:
        ///         提出这个接口的目的是为了 通用. 现 由如下情况, 效果 1 outputKey = A , 效果 2 outputKey = C , inputKey = [A,B]
        ///         之前的 设计, 服务器的触发 主要依赖的是 outputKey. 先改为 由服务器 inputKey 和 outputKey 共同触发.
        ///         如上 示例所示,  效果 1 注册 A , 效果 2 注册 [A,B,C] 的监听.
        ///         当 A 到达时, 先 触发 效果 1 成功, 接着 触发 效果 2, 失败;
        ///         当 B 到达时,
        ///              如果 B 的数据 满足, 触发 效果 2 成功, 移除 效果2 的监听 [A,B,C]. 当C 到达时,已经触发 成功, 存入黑板即可.
        ///              如果 B 的数据 不满足, 触发 2 失败, 等待 C 到了之后触发 成功,移除 [A,B,C];
        ///         当 C 到达时, 看效果是否执行 完毕 且成功, 成功的话 存入黑板,不成功 触发 C, 移除 [A,B,C];
        /// 
        /// </summary>
        /// <param name="regBlackBoard"></param>
        /// <param name="key"></param>
        /// <param name="effectParam"></param>
        /// <param name="tag"></param>
        public static void RegisterKeyEffect(BaseBlackBoard regBlackBoard, string key, I_EffectParam effectParam, E_BlackBoardTag tag)
        {
            bool containRegKey = regBlackBoard.Contain(key, tag);
            CusListQueue<I_EffectParam> regEffectParams;
            if (!containRegKey)
            {
                regEffectParams = new CusListQueue<I_EffectParam>();
            }
            else
            {
                regEffectParams = (CusListQueue<I_EffectParam>)regBlackBoard.Get(key, tag);
            }
            regEffectParams.Enqueue(effectParam);
            // SGF.Debuger.Log($"{TagFlag} [client]  RegisterOutputKeyEffect outputKey : {outputKey} ");

            regBlackBoard.Set(key, regEffectParams, tag);
            //SGF.Debuger.Log($"注册监听 key: {key}, 效果 outputKey : {effectParam.OutputKey}, effectID: {effectParam.EffectID}, {key}剩余监听: {regEffectParams.Count}  ");
        }

        /// <summary>
        /// 取消 key 对于的 效果 注册.  
        /// note:
        ///     如果 效果 1 注册 [A] , 效果 2 注册 [A,B,C] . 则 执行逻辑如下:
        ///     1. 效果 A 到了, A 监听下 有 [1,2] 两个效果, 执行 1 成功, 查找 后续监听中是否有 1, 有的话 移除;
        ///     2. 效果 B 到了, B 监听下 有 [2].
        ///         如果 2 执行成功,  检查 效果注册 中的 所有 监听, 分别 移除 [A,B,C] 中的 效果 2 监听.
        /// </summary>
        /// <param name="regBlackBoard">黑板</param>
        /// <param name="key"> 注册监听时, 存在的 key </param>
        /// <param name="tag"></param>
        /// <param name="effectParam">需要取消的 注册监听 效果</param>
        public static void UnRegisterKeyEffect(BaseBlackBoard regBlackBoard, string key, E_BlackBoardTag tag, I_EffectParam effectParam)
        {

            bool containRegKey = regBlackBoard.Contain(key, tag);
            CusListQueue<I_EffectParam> regEffectParams;
            if (!containRegKey)
            {
                return;
            }

            regEffectParams = (CusListQueue<I_EffectParam>)regBlackBoard.Get(key, tag);

            bool result = regEffectParams.Remove(effectParam);


            // SGF.Debuger.Log($"{TagFlag} [client]  unRegisterOutputKeyEffect outputKey : {outputKey} least count : {regEffectParams.Count}  ");

            if (regEffectParams.Count == 0)
            {
                regBlackBoard.Remove(key, tag);
            }
            else
            {
                regBlackBoard.Set(key, regEffectParams, tag);
            }
            //SGF.Debuger.Log($"取消注册监听 key: {key}, 效果 outputKey : {effectParam.OutputKey}, effectID: {effectParam.EffectID}, {key}剩余监听: {regEffectParams.Count}  ");
        }

        public static void UnRegisterKeyEffects(BaseBlackBoard regBlackBoard, List<string> keys, E_BlackBoardTag tag, I_EffectParam effectParam)
        {
            keys.ForEach((string key) =>
            {
                UnRegisterKeyEffect(regBlackBoard, key, tag, effectParam);
            });
        }

        public static void UnRegisterOutputKeyEffect(BaseBlackBoard regBlackBoard, string outputKey, I_EffectParam effectParam)
        {
            E_BlackBoardTag tag = E_BlackBoardTag.Reg;

            UnRegisterKeyEffect(regBlackBoard, outputKey, tag, effectParam);
        }

        public static void UnRegisterinputKeysEffect(BaseBlackBoard regBlackBoard, List<string> intputKeys, I_EffectParam effectParam)
        {
            E_BlackBoardTag tag = E_BlackBoardTag.Reg;

            UnRegisterKeyEffects(regBlackBoard, intputKeys, tag, effectParam);
        }

        /// <summary>
        /// 取消 注册效果 的 效果监听
        /// </summary>
        /// <param name="regBlackBoard"></param>
        /// <param name="regEffectParam"></param>
        public static void UnRegisterRegEffect(BaseBlackBoard regBlackBoard, I_EffectParam regEffectParam)
        {
            List<string> inputKeys = regEffectParam.InputKeys;
            UnRegisterinputKeysEffect(regBlackBoard, inputKeys, regEffectParam);

            string outputKey = regEffectParam.OutputKey;
            UnRegisterOutputKeyEffect(regBlackBoard, outputKey, regEffectParam);
        }


        /// <summary>
        /// 对 收到的 黑板数据blackList 按照服务器注册的 regIndex 顺序排序, 用来解决服务器 数据 发过来乱序的问题
        /// </summary>
        /// <param name="blackBoardNodes"></param>
        /// <param name="baseBlackBoard"></param>
        public static void SortServerCustomBlackList(List<CustomBlackBoardNode> blackBoardNodes, BaseBlackBoard baseBlackBoard)
        {
            blackBoardNodes.Sort((CustomBlackBoardNode a, CustomBlackBoardNode b) =>
            {
                return baseBlackBoard.GetRegServerIndex(a.Key) - baseBlackBoard.GetRegServerIndex(b.Key);
            });
        }



        #region 【碰撞盒、预警圈】效果

        /// <summary>
        /// 处理预警圈效果
        /// </summary>
        /// <param name="posArr">坐标点数组</param>
        /// <param name="angleArr">角度数组</param>
        /// <param name="effectParam">效果参数</param>
        /// <param name="effectParam">效果参数</param>
        public static void HandleEffectWarning(RepeatedField<ProVec3> posArr, RepeatedField<int> angleArr, I_EffectParam effectParam, float startTime = 0)
        {
            var collisionBox = effectParam.BaseEffect as EffectTypeCollisionBox;
            //SGF.Debuger.LogWarning($"预警圈  HandleEffectWarning 11111111111111111111111111111");

            if (collisionBox != null && collisionBox.IsForewarn && posArr.Count > 0)
            {
                //SGF.Debuger.LogWarning($"预警圈  HandleEffectWarning 22222222222222222222222222 posArr={posArr.Count},angleArr={angleArr.Count}");

                ShapeSerialize shape = collisionBox.Shape;   // 碰撞盒形状
                float playTime = collisionBox.ForewarnTime;
                string effectName = GetWarningRingEffectName(shape);
                // 预警圈中心点 数组
                for (int i = 0; i < posArr.Count; i++)
                {
                    ProVec3 centerPos = posArr[i];
                    Vector3 vector3 = ProtoUtils.ConvertProtoVec3ToUnityVec3(centerPos);

                    for (int j = 0; j < angleArr.Count; j++)
                    {
                        float angle = angleArr[j];
                        Vector3 targetPos = vector3;

                        // 矩形的中心点，客户端要修改在一侧
                        if (collisionBox.Shape.ShapeType == Shape.Rect)
                        {
                            float distance = collisionBox.CenterPosArray.Distance;
                            float temp = distance;

                            float width = (float)shape.Rect.Width / 100;
                            float lenght = (float)shape.Rect.Length / 100;
                            float scale = Mathf.Min(lenght, width);

                            if (distance >= 0)
                            {
                                distance = temp - (collisionBox.Shape.Rect.Length / 2) + (50.0f * scale);
                            }
                            else
                            {
                                distance = -temp - (collisionBox.Shape.Rect.Length / 2) - (50.0f * scale);
                            }
                            targetPos = Fire.Utils.PosMoveBySeverRota(vector3, angle, distance / 100.0f);
                        }
                        // 矩形的中心点，客户端要修改在一侧
                        if (collisionBox.Shape.ShapeType == Shape.Arrow)
                        {
                            float distance = collisionBox.CenterPosArray.Distance;
                            float temp = distance;

                            float width = (float)shape.Rect.Width / 100;
                            float lenght = (float)shape.Rect.Length / 100;
                            float scale = Mathf.Min(lenght, width);

                            if (distance >= 0)
                            {
                                distance = temp - (collisionBox.Shape.Arrow.Length / 2) + (50.0f * scale);
                            }
                            else
                            {
                                distance = -temp - (collisionBox.Shape.Arrow.Length / 2) - (50.0f * scale);
                            }
                            targetPos = Fire.Utils.PosMoveBySeverRota(vector3, angle, distance / 100.0f);
                        }

                        Vector3 serverDir = Fire.Utils.ServerRota2Vector(angle);
                        Vector3 clientDir = Vector3.zero;
                        clientDir.y = (float)(Mathf.Atan2(serverDir.x, serverDir.z) * Mathf.Rad2Deg) % 360;
                        LocalFxManager.Instance.AddWarningRingEffect(effectName, targetPos, clientDir, startTime, playTime, shape);
                    }
                }
            }
        }

        /// <summary>
        /// 根据形状类型获得预警圈的名字
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        private static string GetWarningRingEffectName(ShapeSerialize shape)
        {
            string effectName = "";
            switch (shape.ShapeType)
            {
                case Shape.None:
                    break;
                case Shape.Round:
                    {
                        effectName = "WarningRing_Area";
                    }
                    break;
                case Shape.HollowCircle:
                    {
                        effectName = "WarningRing_InnerArea";
                    }
                    break;
                case Shape.Sector:
                    {
                        if (((float)shape.Sector.Angle * 2) > 30)
                        {
                            effectName = "WarningRing_Sector";
                        }
                        else
                        {
                            effectName = "WarningRing_Sector_Lt30";
                        }
                    }
                    break;
                case Shape.RingFan:
                    {
                        effectName = "WarningRing_RingFan";
                    }
                    break;
                case Shape.Arrow:
                    {
                        if (((float)shape.Rect.Width / 100) > 1)
                        {
                            effectName = "WarningRing_Dir";
                        }
                        else
                        {
                            effectName = "WarningRing_Dir1111";
                        }
                    }
                    break;
                case Shape.Rect:
                    {
                        if (((float)shape.Rect.Width / 100) > 1)
                        {
                            effectName = "WarningRing_Dir";
                        }
                        else
                        {
                            effectName = "WarningRing_Dir1111";
                        }
                    }
                    break;
                case Shape.RotRoute:
                case Shape.InputTarget:
                    break;
                default:
                    break;
            }

            return effectName;
        }

        #endregion

        #region 【伤害】效果

        /// <summary>
        /// 处理伤害效果
        /// </summary>
        /// <param name="hurtMsg">伤害信息</param>
        /// <param name="effectParam">效果参数</param>
        /// <param name="effectsPath">效果路径</param>
        /// <param name="isPlayEffect">是否播放效果</param>
        /// <param name="cfgID">技能ID\BUFFID\被动ID</param>
        public static void HandleEffectDamage(HurtNodeMsg hurtMsg, I_EffectParam effectParam, string effectsPath, bool isPlayEffect, int cfgID)
        {
            int len = hurtMsg.DataList.Count;
            for (int i = 0; i < len; i++)
            {
                HurtData hurtData = hurtMsg.DataList[i];
                //SGF.Debuger.Log($"黑板阶段同步 【客户端】 伤害效果  i={i},hurtData={hurtData}");
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(hurtData.TargetID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                var damage = effectParam.BaseEffect as EffectTypeDamage;
                if (isPlayEffect)
                {
                    // ----被击特效
                    if (damage.HitEffect.Count > 0)
                    {
                        var hitEffectArr = damage.HitEffect;
                        for (int j = 0; j < hitEffectArr.Count; j++)
                        {
                            EffectTypeHitEffect effectTypeHitEffect = hitEffectArr[j];
                            if (effectTypeHitEffect == null)
                            {
                                continue;
                            }
                            FXJson fXJson = ConverFx.LoopEffect2FxJson(effectTypeHitEffect);
                            FxParam fxParam = new();
                            fxParam.InitWithFxJson(fXJson, effectParam.Builder, hurtData.TargetID);
                            // 受击特效唯一，不需要做引用记录
                            fxParam.SetExtralKey(fxParam.GetHashCode().ToString());

                            // 受击特效给它一个 默认2s
                            fxParam.SetPlayTime(2f);
                            entityCtrlGroup.M_Curr.ActionOnPlaySpecialEffects?.Invoke(fxParam, "", effectsPath);
                        }
                    }
                    // ----被击动作
                    if (hurtData.Ishit && damage.isActor)
                    {
                        I_AnimParam animParam = entityCtrlGroup.M_Curr.GetAnimParamByState(E_ULayerSubState.Hurt);
                        //bool isCanEqual = entityCtrlGroup.Data.dataType == E_EntityDataType.Monster;
                        bool isCanEqual = entityCtrlGroup.Data.EntityType != E_EntityType.Player;
                        entityCtrlGroup.M_Curr.ChangeState((GameKeyCommand)E_ULayerSubState.Hurt, animParam, isCanEqual);
                        entityCtrlGroup.M_Curr.ActionOnBeAttackFlashColor?.Invoke();
                    }
                }
                //Debug.LogError("伤害飘字");
                // ----伤害飘字
                if (damage != null)
                {
                    //if (damage.FlutteringWordsID <= 0 && effectParam.StageType == E_StageType.Skill)
                    //{
                    //    bool isHave = false;
                    //    // 判断是否为伙伴打出来的伤害
                    //    // 1.伤害施法者是否是伙伴
                    //    {
                    //        var entity = GameManager.Instance.GetEntityCtr(effectParam.Builder);
                    //        if (entity != null && entity.M_Curr != null && entity.M_Curr.EntityType == E_EntityType.Partner)
                    //        {
                    //            isHave = LocalDataManager.Instance.GetPartnerSkillIDISHaveCfg((int)entity.M_Curr.ConfigIndex, cfgID);
                    //        }
                    //    }
                    //    // 2.伤害拥有者是否是伙伴
                    //    // 3.传过来的配置ID（技能ID\BUFFID\被动ID）对比伙伴配置是否存在
                    //    {
                    //        if (!isHave)
                    //        {
                    //            var entity = GameManager.Instance.GetEntityCtr(effectParam.Owner);
                    //            if (entity != null && entity.M_Curr != null && entity.M_Curr.EntityType == E_EntityType.Partner)
                    //            {
                    //                isHave = LocalDataManager.Instance.GetPartnerSkillIDISHaveCfg((int)entity.M_Curr.ConfigIndex, cfgID);
                    //            }
                    //        }
                    //    }
                    //    if (isHave)
                    //    {
                    //        // 100006是伤害飘字，，， 100007是治疗飘字
                    //        damage.FlutteringWordsID = hurtData.Ishit && hurtData.Hurt > 0 ? 100006 : 100007;
                    //    }
                    //}
                    entityCtrlGroup.M_Curr.HandleHurtNodeMsg(hurtData, effectParam.Builder, effectParam.Owner, damage.FlutteringWordsID, effectParam.StageType);
                }
            }
        }

        /// <summary>
        /// 处理伤害效果--客户端线
        /// </summary>
        /// <param name="skillTarDatas">伤害信息</param>
        /// <param name="effectParam">效果参数</param>
        /// <param name="effectsPath">效果路径</param>
        public static void HandleEffectDamageTar(RepeatedField<SkillTarData> skillTarDatas, I_EffectParam effectParam, string effectsPath)
        {
            for (int i = 0; i < skillTarDatas.Count; i++)
            {
                SkillTarData skillTarData = skillTarDatas[i];
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(skillTarData.TarID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                EffectTypeDamage damage = effectParam.BaseEffect as EffectTypeDamage;

                // 被击特效
                if (damage.HitEffect.Count > 0)
                {
                    var hitEffectArr = damage.HitEffect;
                    for (int j = 0; j < hitEffectArr.Count; j++)
                    {
                        EffectTypeHitEffect effectTypeHitEffect = hitEffectArr[j];
                        if (effectTypeHitEffect == null)
                        {
                            continue;
                        }
                        FXJson fXJson = ConverFx.LoopEffect2FxJson(effectTypeHitEffect);
                        FxParam fxParam = new();
                        fxParam.InitWithFxJson(fXJson, effectParam.Builder, skillTarData.TarID);
                        // 受击特效唯一，不需要做引用记录
                        fxParam.SetExtralKey(fxParam.GetHashCode().ToString());

                        fxParam.SetPlayTime(2f);

                        entityCtrlGroup.M_Curr.ActionOnPlaySpecialEffects.Invoke(fxParam, "", effectsPath);
                    }
                }
                // ----被击动作
                if (damage.isActor)
                {
                    I_AnimParam animParam = entityCtrlGroup.M_Curr.GetAnimParamByState(E_ULayerSubState.Hurt);
                    bool isCanEqual = entityCtrlGroup.Data.EntityType != E_EntityType.Player;
                    entityCtrlGroup.M_Curr.ChangeState((GameKeyCommand)E_ULayerSubState.Hurt, animParam, isCanEqual);
                    entityCtrlGroup.M_Curr.ActionOnBeAttackFlashColor?.Invoke();
                }
            }
        }

        #endregion

        #region 【二次伤害】效果

        /// <summary>
        /// 处理二次伤害效果
        /// </summary>
        /// <param name="hurtMsg"></param>
        /// <param name="effectParam"></param>
        public static void HandleEffectDamageSecond(HurtNodeMsg hurtMsg, I_EffectParam effectParam)
        {
            int len = hurtMsg.DataList.Count;
            for (int i = 0; i < len; i++)
            {
                HurtData hurtData = hurtMsg.DataList[i];
                //SGF.Debuger.Log($"黑板阶段同步 【客户端】 伤害效果  i={i},hurtData={hurtData}");
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(hurtData.TargetID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                //Debug.LogError("二次伤害飘字");
                // ----伤害飘字
                var damageSecond = effectParam.BaseEffect as EffectTypeDamageSecond;
                if (damageSecond != null)
                {
                    entityCtrlGroup.M_Curr.HandleHurtNodeMsg(hurtData, effectParam.Builder, effectParam.Owner, damageSecond.FlutteringWordsID, effectParam.StageType);
                }
            }
        }

        #endregion

        #region 【位移】效果

        /// <summary>
        /// 处理位移效果
        /// </summary>
        /// <param name="effectParam">效果参数</param>
        /// <param name="offsetNodeMsg">位移信息</param>
        /// <param name="durningTime">持续时间</param>
        /// <param name="moveLabel">位移标签</param>
        /// <param name="isThrough">是否穿墙</param>
        public static void HandleEffectOffset(I_EffectParam effectParam, OffsetNodeMsg offsetNodeMsg, float durningTime, MoveLabel moveLabel, MoveType moveType)
        {
            int len = offsetNodeMsg.DataList.Count;
            for (int i = 0; i < len; i++)
            {
                OffsetData offseData = offsetNodeMsg.DataList[i];
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(offseData.TargetID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                //SGF.Debuger.LogError($"黑板阶段同步 位移效果 des={effectParam.EffectData.Desc} , {effectParam.OutputKey} ,i={i},durningTime={durningTime},offseData={offseData.Pos}");
                entityCtrlGroup.M_Curr.HandleActionOnOffsetDataMsg(offseData, durningTime, moveLabel, moveType, effectParam.OutputKey);
            }
        }

        #endregion

        #region 【旋转】效果

        /// <summary>
        /// 处理旋转效果
        /// </summary>
        /// <param name="effectParam">效果参数</param>
        /// <param name="upRotaNodeMsg">旋转信息</param>
        public static void HandleEffectRotate(UpRotaNodeMsg upRotaNodeMsg, int lerpTimeMs)
        {
            int len = upRotaNodeMsg.DataList.Count;

            lerpTimeMs = lerpTimeMs < 0 ? 0 : lerpTimeMs;
            bool isLerp = lerpTimeMs != 0;
            float lerpTime = lerpTimeMs / 1000.0f;
            for (int i = 0; i < len; i++)
            {
                var upRotaData = upRotaNodeMsg.DataList[i];
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(upRotaData.TargetID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                //SGF.Debuger.LogError($"[Rotate] 黑板阶段同步 旋转效果  i={i},Rota={upRotaData.Rota}");
                entityCtrlGroup.M_Curr.ClientSetRotation(upRotaData.Rota, isLerp, lerpTime, lerpTime > 0);
            }
        }

        #endregion

        #region 治疗效果

        /// <summary>
        /// 处理治疗效果
        /// </summary>
        /// <param name="cureDatas">治疗信息</param>
        /// <param name="effectParam">效果参数</param>
        public static void HandleEffectCure(CureNodeMsg cureNodeMsg, I_EffectParam effectParam, string effectsPath)
        {
            int len = cureNodeMsg.DataList.Count;
            for (int i = 0; i < len; i++)
            {
                CureData hurtData = cureNodeMsg.DataList[i];
                //SGF.Debuger.Log($"黑板阶段同步 【客户端】 伤害效果  i={i},hurtData={hurtData}");
                EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(hurtData.TargetID);
                if (entityCtrlGroup == null)
                {
                    continue;
                }
                var damage = effectParam.BaseEffect as EffectTypeTreat;
                // ----被击特效
                if (damage.HitEffect.Count > 0)
                {
                    var hitEffectArr = damage.HitEffect;
                    for (int j = 0; j < hitEffectArr.Count; j++)
                    {
                        EffectTypeHitEffect effectTypeHitEffect = hitEffectArr[j];
                        if (effectTypeHitEffect == null)
                        {
                            continue;
                        }
                        FXJson fXJson = ConverFx.LoopEffect2FxJson(effectTypeHitEffect);
                        FxParam fxParam = new();
                        fxParam.InitWithFxJson(fXJson, effectParam.Builder, hurtData.TargetID);
                        // 受击特效唯一，不需要做引用记录
                        fxParam.SetExtralKey(fxParam.GetHashCode().ToString());

                        // 受击特效给它一个 默认2s
                        fxParam.SetPlayTime(2f);
                        entityCtrlGroup.M_Curr.ActionOnPlaySpecialEffects?.Invoke(fxParam, "", effectsPath);
                    }
                }
                //PrefabPathInEditor
                // ----治疗飘字
                BattleManager.Instance.OnHurtData(hurtData, effectParam.Builder, effectParam.Owner);
            }
        }

        #endregion

        #region 【在目标点播放特效】效果

        /// <summary>
        /// 播放子弹的 移动 特效, 目前 主要是:
        /// 1.追踪弹的 特效;
        /// 2.播放 一个 特效， 从 A 点 移动 ----> B 点
        /// </summary>
        /// <returns></returns>
        private static bool PlayClientMoveFx()
        {
            // 1.先确定 开始点 ----> 目标点的 初始朝向;
            // 2.创建 环境特效, 添加 对应的 脚本
            // 3.启动脚本,脚本自己 根据 移动的 类型, 自己控制特效的生命周期
            //UnityEngine.Vector3 uV3 = Vector3.zero;
            //int angle = 0;
            FxParam fxParam = new();
            // fxParam.InitWithFxJson(fXJson, builderEntityID, ownerEntityID);
            fxParam.SetFxStartTime(0);
            // PlayFxAtPoint(fxParam, pos, angle);

            return true;
        }

        /// <summary>
        /// 在特定的坐标点(ProVec3 服务器的点 和 服务器的朝向) , 播放环境特效
        /// </summary>
        public static void PlayFxAtPoint(I_FxParam fxParam, ProVec3 pos, int angle)
        {
            Vector3 vPos = ProtoUtils.ConvertProtoVec3ToUnityVec3(pos);
            Vector3 serverDir = Fire.Utils.ServerRota2Vector(angle);
            Vector3 clientDir = Vector3.zero;
            clientDir.y = (float)(Mathf.Atan2(serverDir.x, serverDir.z) * Mathf.Rad2Deg) % 360;

            TargetPlayEnvEffect(fxParam, vPos, clientDir);
        }

        private static void TargetPlayEnvEffect(I_FxParam fxParam, Vector3 pos, Vector3 dirction)
        {
            EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(fxParam.BuilderID);
            if (entityCtrlGroup == null)
            {
                return;
            }
            AvatarDataCell avatarData = entityCtrlGroup.M_Curr.avatarDataCell;
            if (fxParam.BuilderID != entityCtrlGroup.M_Curr.EntityId)
            {
                AvatarDataCell model = GameManager.Instance.GetEntityAvatarById(fxParam.BuilderID);
                if (model != null)
                {
                    avatarData = model;
                }
            }
            if (avatarData == null)
            {
                SGF.Debuger.LogWarning($"TargetPlayEnvEffect() avatarData=null");
                return;
            }
            PlayEnvFxAtPoint(fxParam, pos, dirction, avatarData.EffectsPath);
        }

        private static void PlayEnvFxAtPoint(I_FxParam fxParam, Vector3 pos, Vector3 dirction, string effectsPath)
        {
            // TODO: 曲
            // LocalEffectTags 这个 在 I_FxParam 内部 统一处理
            string effectName = $"{effectsPath}/{fxParam.EffectPath}";
            // 防报错
            effectName = effectName.Replace("Assets/Res/", string.Empty);
            effectName = effectName.Replace(".prefab", string.Empty);

            /// TODO : 曲
            /// 跟gl约定(2022/11/29):
            ///     目前 在点位上 播放特效，都是 在人身上 找特效, 不从 环境目录上取
            ///     后续, 特效可能会统一放在一个目录里面，这样就不需要 每次单独指定tag去拼路径，导致各种问题
            /// 

            LocalEffectTags tag = LocalEffectTags.ActorFx;

            // 目前来说,想要跟随玩家的环境特效 策划应该还没有这种需求
            // 后面 IsFollowMove / IsFollowRot / HangPoint 需要分离
            // if (fxParam.IsFollowMove)
            // {
            //     tag = LocalEffectTags.ActorFx;
            // }

            int startTime = fxParam.StartTime;

            Vector3 fxPos = pos + fxParam.MoveOffset;
            Vector3 fxDirection = dirction + fxParam.DirectionOffset;

            LocalFxManager.Instance.AddEnvEffectAsync(
                effectName: effectName,
                startWorldPos: fxPos,
                startWorldRotate: fxDirection,
                localEffectTags: tag,
                scale: Vector3.one,
                playTime: fxParam.PlayTime,
                startDelay: 0,
                isLoop: false,
                speedMultiplier: 1,
                startTime: startTime,
                isFollowMove: false,
                isFollowRotate: false,
                isFollowBuilderHide: fxParam.IsFollowBuilderHide
            );
        }

        #endregion

        #region 目标身上播放特效

        public static void TargetPlayEffect(ulong targeEntityID, I_FxParam fxParam)
        {
            EntityCtrlBase entityCtrlGroup = GameManager.Instance.GetEntityCtr(targeEntityID);
            if (entityCtrlGroup == null)
            {
                return;
            }
            entityCtrlGroup.M_Curr.PlaySpecialEffect(fxParam);
        }

        #endregion

        /// <summary>
        /// 获得 一个 判定(类似 IsNotEmpty)效果的 黑板结果数据.采用 outPutKey为 黑板key.
        /// </summary>
        /// <param name="baseBlackBoard"></param>
        /// <param name="inputKey"></param>
        /// <param name="outputKey"></param>
        /// <returns></returns>
        public static CustomBlackBoardNode GetIsEffectResult(BaseBlackBoard baseBlackBoard, string inputKey, string outputKey, E_BlackBoardTag tag)
        {
            // 先采用 outputKey 生成一个默认的 判定结果 为 false 的 黑板数据
            CustomBlackBoardNode customBlackBoardNode = new(outputKey, false);
            // 先检查 黑板中是否存在 inputKey 对应的结果
            bool hasKey = CheckIsBlackBoardHasKey(baseBlackBoard, inputKey, tag);

            // 如果不存在 inputKey 对应的数据, 那就返回 false 结果的黑板数据
            if (!hasKey)
            {
                return customBlackBoardNode;
            }

            // 如果存在数据, 就取出对应的数据
            object data = baseBlackBoard.Get(inputKey, tag);

            if (data == null)
            {
                return customBlackBoardNode;
            }

            customBlackBoardNode.Value = !IsBlackBoardDefaultNull((CustomBlackBoardNode)data);

            return customBlackBoardNode;
        }

        /// <summary>
        /// 判断 一个黑板数据 是否是 默认为空. 此处 依照服务器的黑板 数据是否为默认 空数据逻辑编写.
        /// 对不同 类型的数据, 此处 需要 不同的处理逻辑
        /// </summary>
        /// <param name="customBlackBoardNode"></param>
        public static bool IsBlackBoardDefaultNull(CustomBlackBoardNode customBlackBoardNode)
        {
            // 如果 黑板数据不存在 
            if (customBlackBoardNode == null)
            {
                return true;
            }

            switch (customBlackBoardNode.Value)
            {
                case SkillTarsMsg skillTarsMsg:
                    {
                        // 对于 技能目标类型的 黑板数据,只有数量 大于0,才认为这个数据存在
                        return skillTarsMsg.SkillTars.Count == 0;
                    }

                default:
                    {
                        // 其它的类型 现在只要 有这个黑板数据, 不管它的 value 是否存在
                        // 就认为 这个 黑板数据 存在
                        return false;
                    };
            }
        }

        private static System.Random random = new();
        public static Vector2 GetRandomVector2(RandomVector2 randomVector2, float radio)
        {
            Vector2 v2 = Vector2.zero;
            v2.x = random.Next(randomVector2.MinX, randomVector2.MaxX) / radio;
            v2.y = random.Next(randomVector2.MinY, randomVector2.MaxY) / radio;

            return v2;
        }


    }
}
