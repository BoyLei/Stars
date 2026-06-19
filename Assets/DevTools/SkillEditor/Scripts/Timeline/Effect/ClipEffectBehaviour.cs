///--------------------------------------------------------------------
/// 文件名   :   ClipEffectBehaviour.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/01 18:35:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Sirenix.OdinInspector;
using StarProject.OffLine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace SkillEditor
{
    [System.Serializable]
    public class ClipEffectBehaviour : PlayableBehaviour
    {
        [LabelText("效果数据")]
        [ShowInInspector]
        public List<EffectData> frameEffect = new List<EffectData>();

        [HideInInspector]
        private List<GameObject> useGameObject = new List<GameObject>();

        [HideInInspector]
        private List<ShowBulletData> bullets = new List<ShowBulletData>();

        [HideInInspector]
        private List<GameObject> hitEffects = new List<GameObject>();

        [HideInInspector]
        public int Index = -1;
        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);

        }

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            foreach (var effect in frameEffect)
            {
                if (effect.EffectArgs.BaseEffect == null)
                {
                    continue;
                }
                if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeCollisionBox))
                {
                    EffectTypeCollisionBox collisionBox = (EffectTypeCollisionBox)effect.EffectArgs.BaseEffect;
                    SGAMESkillShapeIndicator shapeIndicator = null;
                    if (collisionBox.Shape.ShapeType == Shape.Round)
                    {
                        var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_InnerArea.prefab");
                        shapeIndicator = GameObject.Instantiate(go, SkillEditorGlobal.Instance.Effect.transform).GetComponent<SGAMESkillShapeIndicator>();
                        useGameObject.Add(shapeIndicator.transform.gameObject);

                        SGAMECircle curSi = (SGAMECircle)shapeIndicator;
                        //curSi.OnEnable();
                        curSi.SetSectorRange((float)collisionBox.Shape.Round.Radius / 100f);
                        curSi.Update();
                        curSi.transform.localPosition = Vector3.zero;
                    }
                    if (collisionBox.Shape.ShapeType == Shape.Sector)
                    {
                        var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Sector.prefab");
                        shapeIndicator = GameObject.Instantiate(go, SkillEditorGlobal.Instance.Effect.transform).GetComponent<SGAMESkillShapeIndicator>();
                        useGameObject.Add(shapeIndicator.transform.gameObject);

                        SGAMEConeCircle curSi = (SGAMEConeCircle)shapeIndicator;
                        //curSi.OnEnable();
                        curSi.SetSectorRange((float)collisionBox.Shape.Sector.Radius / 100f);
                        curSi.SetSectorAngle((float)collisionBox.Shape.Sector.Angle * 2);
                        curSi.Update();
                        curSi.transform.localEulerAngles = Vector3.zero;
                        curSi.transform.localPosition = Vector3.zero;
                    }
                    if (collisionBox.Shape.ShapeType == Shape.Rect)
                    {
                        var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/UI/StarWorld/Prefab/SkillIndicator/SkillIndicatorView_Dir.prefab");
                        //var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Effects/Skill/WarningRing/WarningRing_Dir1111.prefab");
                        shapeIndicator = GameObject.Instantiate(go, SkillEditorGlobal.Instance.Effect.transform).GetComponent<SGAMESkillShapeIndicator>();
                        useGameObject.Add(shapeIndicator.transform.gameObject);

                        SGAMEArrow curSi = (SGAMEArrow)shapeIndicator;
                        //curSi.OnEnable();
                        //curSi.SetRectWidthAndLength((float)collisionBox.Shape.Rect.Width / 100f, (float)collisionBox.Shape.Rect.Length / 100f);
                        curSi.SetRectRange(collisionBox.Shape.Rect.Width / 100f);
                        curSi.SetRectLength(collisionBox.Shape.Rect.Length / 100f);
                        //curSi.Update();
                        curSi.transform.localPosition = Vector3.zero;
                        curSi.transform.localEulerAngles = Vector3.zero;
                    }
                    if (shapeIndicator != null)
                    {
                        //设置碰撞盒角度
                        Vector3 rotation = new Vector3(0, -collisionBox.TowardArray.TowardOffset, 0);
                        shapeIndicator.transform.rotation = Quaternion.Euler(rotation);
                        //设置碰撞盒坐标
                        float angleInRadians = (collisionBox.CenterPosArray.Angle + 90) * Mathf.Deg2Rad;
                        //先拿到基础坐标
                        Vector3 v3 = SkillEditorGlobal.Model.transform.GetChild(0).position + new Vector3(collisionBox.CenterPosArray.Distance * Mathf.Cos(angleInRadians) / 100f, 0, collisionBox.CenterPosArray.Distance * Mathf.Sin(angleInRadians) / 100f);

                        if (collisionBox.Shape.ShapeType == Shape.Rect)
                        {
                            float length = collisionBox.Shape.Rect.Length / 100f / 2f;

                            float sinRot = Mathf.Sin(-collisionBox.TowardArray.TowardOffset * Mathf.Deg2Rad);
                            float cosRot = Mathf.Cos(-collisionBox.TowardArray.TowardOffset * Mathf.Deg2Rad);

                            //如果是矩形，还需要计算旋转后导致的偏移
                            v3.x -= length * sinRot;
                            v3.z -= length * cosRot;
                        }

                        shapeIndicator.transform.localPosition = v3;
                    }
                }
                if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeCreateBullet))
                {
                    EffectTypeCreateBullet createBullet = (EffectTypeCreateBullet)effect.EffectArgs.BaseEffect;
                    if (SkillEditorData.Bullets.ContainsKey(createBullet.BulletID))
                    {
                        //拿到子弹特效配置
                        foreach (var curEffect in SkillEditorData.Bullets[createBullet.BulletID].bulletConfig.LoopEffectInEditors)
                        {
                            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(curEffect.PrefabPathInEditor);
                            if (go != null)
                            {
                                GameObject bullet = GameObject.Instantiate(go, SkillEditorGlobal.Instance.Effect.transform);
                                useGameObject.Add(bullet);
                                bullets.Add(new ShowBulletData()
                                {
                                    bulletEffect = createBullet,
                                    Height = curEffect.YOffsetTowards,
                                    bulletObj = bullet
                                });
                            }
                            else
                            {
                                Debug.Log("子弹循环特效配置不存在");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("子弹配置不存在");
                    }
                }
                if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeDamage))
                {
                    EffectTypeDamage damage = (EffectTypeDamage)effect.EffectArgs.BaseEffect;
                    List<GameObject> gos = new List<GameObject>();
                    foreach (var hitEffect in damage.HitEffect)
                    {
                        gos.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(hitEffect.PrefabPathInEditor));
                    }
                    //要在所有敌人身上播放受击特效和受击动作
                    foreach (var enemy in SkillEditorGlobal.Instance.Enemy.transform.GetChildren())
                    {
                        foreach (var go in gos)
                        {
                            if (go != null)
                            {
                                hitEffects.Add(GameObject.Instantiate(go, enemy.GetComponent<ModelOffLineData>().BindDummyPos["Hurt_D"]));
                            }
                            else
                            {
                                Debug.Log("受击特效不存在，请检查");
                            }
                        }
                    }
                }
            }
#endif
            base.OnBehaviourPlay(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            foreach (var obj in useGameObject)
            {
                GameObject.DestroyImmediate(obj);
            }
            useGameObject.Clear();
            foreach (var hiteffect in hitEffects)
            {
                GameObject.DestroyImmediate(hiteffect);
            }
            hitEffects.Clear();
            bullets.Clear();
#endif
            base.OnBehaviourPause(playable, info);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
#if UNITY_EDITOR
            //子弹移动
            foreach (var bullet in bullets)
            {
                bullet.bulletObj.GetComponent<ParticleSystem>().Simulate((float)playable.GetTime(), true);
                ExplorerItem bulletData = SkillEditorData.Bullets[bullet.bulletEffect.BulletID];
                //计算子弹坐标
                Vector3 bulletPos = Vector3.zero;

                float angleInRadians = (bullet.bulletEffect.CenterPosArray.Angle + 90) * Mathf.Deg2Rad;
                float toward = (bullet.bulletEffect.TowardArray.TowardOffset + 90) * Mathf.Deg2Rad;
                bulletPos.x = bullet.bulletEffect.CenterPosArray.Distance / 100f * Mathf.Cos(angleInRadians) + bulletData.bulletConfig.MoveSpeed / 100f * (float)playable.GetTime() * Mathf.Cos(toward);
                bulletPos.y = bulletData.bulletConfig.Hight / 100f + bullet.Height / 100f;
                bulletPos.z = bullet.bulletEffect.CenterPosArray.Distance / 100f * Mathf.Sin(angleInRadians) + bulletData.bulletConfig.MoveSpeed / 100f * (float)playable.GetTime() * Mathf.Sin(toward);

                bullet.bulletObj.transform.position = bulletPos;
            }
            Vector3 vector3 = Vector3.zero;
            foreach (var effect in frameEffect)
            {
                if (effect.EffectArgs.BaseEffect == null)
                {
                    continue;
                }
                if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeMoveWithRot))
                {
                    EffectTypeMoveWithRot moveWithRot = (EffectTypeMoveWithRot)effect.EffectArgs.BaseEffect;
                    if (moveWithRot.DurningTime != 0)
                    {
                        float curTime = Mathf.Min((float)playable.GetTime(), moveWithRot.DurningTime / 1000f);
                        vector3.z += moveWithRot.Distance / 100f * (curTime / (moveWithRot.DurningTime / 1000f));
                    }
                    else
                    {
                        vector3.z += moveWithRot.Distance / 100f;
                    }
                }
                if (effect.EffectArgs.BaseEffect.GetType() == typeof(EffectTypeDamage))
                {
                    EffectTypeDamage damage = (EffectTypeDamage)effect.EffectArgs.BaseEffect;
                    //要刷新受击特效
                    foreach (var hitEffect in hitEffects)
                    {
                        hitEffect.GetComponent<ParticleSystem>().Simulate((float)playable.GetTime(), true);
                    }
                }
            }
#endif
            base.ProcessFrame(playable, info, playerData);
        }
    }

    public class ShowBulletData
    {
        public GameObject bulletObj;
        public EffectTypeCreateBullet bulletEffect;
        public int Height;
    }
}