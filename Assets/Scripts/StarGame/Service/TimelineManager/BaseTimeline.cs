///--------------------------------------------------------------------
/// 文件名   :   BaseTimeline.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/05/08 13:51:57
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Cinemachine;
using SGF;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game.Player;
using StarProject.Service.Cam;
using StarProject.Service.Resource;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace StarProject.Service.Timeline
{
    public class BaseTimeline
    {
        public string TimelinePath { get; private set; }
        public System.Action<string, int, int, bool> OnEndAction { get; private set; }

        public System.Action<object> OnTimeLineEffectEnd { get; private set; }

        public int EffectID { get; private set; }

        private bool IsEnd = false;
        PlayableDirector director;

        bool isFinishHide;

        bool isHideUI;

        bool isHideRole;

        bool isHidePartner;

        bool isCanMove;

        bool isHideMapNPC;

        bool isHideOtherPlayer;

        bool isHideMon;

        bool isHideObj;
        bool isHideAirWall;

        private int ConfigID;
        private AnimationTrack animationTrack;
        public int TimelineType { get; private set; }

        public int ID;

        public bool NeedFadeOut = false;

        private bool IsSkip = false;
        
        public PlayableDirector PlayableDirector{get{return director;}}

        public BaseTimeline()
        {
        }

        public void OnInit(int id, string timelinepath, int timelineType, int effectID, System.Action<string, int, int, bool> complete, System.Action<object> cb)
        {
            this.ConfigID = id;
            this.TimelinePath = timelinepath;
            this.TimelineType = timelineType;
            this.EffectID = effectID;
            this.OnEndAction = complete;
            this.OnTimeLineEffectEnd = cb;
        }

        virtual protected void OnPrepare(PlayableDirector director)
        {
            if (CameraManager.Instance.CurrentPlayCamera != null)
            {
                var cinemachine = CameraManager.Instance.CurrentPlayCamera.GetComponent<CinemachineBrain>();
                if (cinemachine != null)
                {
                    cinemachine.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                    cinemachine.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
                }
            }

            director.stopped += OnStoped;
            //SignalReceiver signalReceiver= director.GetComponent<SignalReceiver>();
            //signalReceiver.OnNotify();
            // director.gameObject.SetActive(true);
        }

        private Vector3 localPosition;
        private Quaternion localRotation;
        private Vector3 localScale;

        private GameObject mModel;
        private Transform Modleparent;

        private void SetPlayer(PlayableDirector director)
        {
            foreach (var item in director.playableAsset.outputs)
            {
                if (item.sourceObject != null)
                {
                    if (item.sourceObject is AnimationTrack track)
                    {
                        if (track.name == "Player")
                        {
                            animationTrack = track;

                            foreach (var timelineClip in animationTrack.GetClips())
                            {
                                var avatarDataCell = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.avatarDataCell;
                                if (avatarDataCell != null)
                                {
                                    var clipPath = ResourceFormalManager.Instance.GetRecursionLoadAssetPath(
                                        GetAnimPath(timelineClip.displayName, avatarDataCell.AnimsPath),
                                        E_AssetType.Animation);
                                    var clip =
                                        ResourceFormalManager.Instance
                                            .LoadAssetSyncForTimeline<AnimationClip>(clipPath);
                                    if (clip != null)
                                    {
                                        AnimationPlayableAsset asset = timelineClip.asset as AnimationPlayableAsset;
                                        OnCompleteLoad(clip, asset);
                                    }
                                    else
                                    {
                                        Debuger.LogWarning(
                                            $"动画替换失败 avatarID={avatarDataCell.AvatarID},Name={avatarDataCell.Desc}  加载动作失败 {timelineClip.displayName} 失败  Path={avatarDataCell.AnimsPath}");
                                    }
                                }
                            }

                            var player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
                            var model = player.Container.GetComponentInChildren<Animator>();
                            var trans = director.gameObject.transform
                                .Find("PlayerRoot"); //GetGenericBinding(track) as GameObject;
                            var collider = player.Container.GetComponentInChildren<CharacterController>();
                            if (collider != null)
                            {
                                collider.enabled = false;
                            }

                            // ViewVitalAnim view = player.Container.gameObject.GetComponentInChildren<ViewVitalAnim>();
                            // view.SetIsSyncPos(false);
                            if (model != null)
                            {
                                Modleparent = model.transform.parent;
                                localPosition = model.transform.localPosition;
                                localRotation = model.transform.localRotation;
                                localScale = model.transform.localScale;
                                mModel = model.gameObject;
                                if (trans != null)
                                {
                                    model.transform.SetParent(trans.transform);
                                    model.transform.localRotation = Quaternion.identity;
                                    model.transform.localPosition = Vector3.zero;
                                    model.transform.localScale = Vector3.one;
                                }
                                else
                                {
                                    model.transform.SetParent(director.transform);
                                }

                                director.SetGenericBinding(track, model);
                            }
                        }
                    }
                }
            }
        }

        private void OnCompleteLoad(AnimationClip clip, AnimationPlayableAsset asset)
        {
            if (asset != null)
            {
                asset.clip = clip;
            }
        }

        protected string GetAnimPath(string animName, string AnimsPath)
        {
            string path = $"{AnimsPath}/{animName}";
            path = path.Replace("Assets/Res/", string.Empty);
            path = path.Replace(".anim", string.Empty);
            return path.ToLower();
        }

        private void OnStoped(PlayableDirector director)
        {
            SGF.Debuger.Log($"OnStoped PlayableDirector");
            director.stopped -= OnStoped;
            //  director.gameObject.SetActive(false);
            OnEnd();
        }


        virtual public void Play(PlayableDirector director, TimelineConfigDataCell timelineConfigData)
        {
            IsSkip = false;
            GameManager.Instance.EventPreNewPlayerEvent($"8_{ConfigID}_1");
            this.director = director;
            if (this.director == null)
            {
                SGF.Debuger.LogError($"timeline ConfigID={ConfigID},director=null..err!!!");
                return;
            }
            this.director.gameObject.SetActive(true);
            this.isFinishHide = timelineConfigData.GetFinishHide();
            this.isHideUI = timelineConfigData.GetHideUI();
            this.isHideRole = timelineConfigData.GetHideRole();
            this.isHidePartner = timelineConfigData.GetHidePartner();
            isCanMove = timelineConfigData.GetIsCanMove();
            isHideMapNPC = timelineConfigData.GetHideMapNPC();
            isHideOtherPlayer = timelineConfigData.GetHideOtherPlayer();
            isHideMon = timelineConfigData.GetHideMon();
            isHideObj = timelineConfigData.GetHideObj();
            isHideAirWall = timelineConfigData.GetHideAirWall();
            NeedFadeOut=timelineConfigData.GetIsOutBlack();
            ID=timelineConfigData.GetID();

            if (isHideUI)
            {
                UIManager.Instance.CloseWidget(UIDef.ShowAreaNameWidget);
                UIManager.Instance.SetShowTimeLineToHideHud(true);
                GlobalEvent.OnPlayTimelineEvent?.Invoke(true);
                //TimelineManager.Instance.IsPlayTimeLine = true;
            }

            if (isHideRole)
            {
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStartHidden?.Invoke(false);
            }

            if (isHidePartner)
            {
                if (PartnerManager.Instance.CurConcretizationPartner != null)
                {
                    if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                    {
                        PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStartHidden?.Invoke(false);
                    }
                }
            }

            // 禁止移动
            if (!isCanMove)
            {
                SGF.Debuger.LogWarning($"timeline里面要 禁止 主角移动 朝向攻击技能 TimelinePath={TimelinePath},EffectID={EffectID} 11111111111");

                GameManager.Instance.RegisterMainPlayerClientBattleStates();
            }

            List<E_EntityType> mTypes = new();

            // 隐藏地图NPC
            if (isHideMapNPC)
            {
                mTypes.Add(E_EntityType.Npc);
                //GameManager.Instance.SetNpcEntityHide(true);
            }

            // 隐藏其他玩家
            if (isHideOtherPlayer)
            {
                mTypes.Add(E_EntityType.Player);
                // GameManager.Instance.SetPlayerEntityHide(true);
            }

            //怪物
            if (isHideMon)
            {
                mTypes.Add(E_EntityType.Monster);
                mTypes.Add(E_EntityType.Summon);
                mTypes.Add(E_EntityType.ClientSummon);
            }

            //交互物
            if (isHideObj)
            {
                mTypes.Add(E_EntityType.Interact);
            }

            GameManager.Instance.SetEntityHide(true, mTypes);

            // 隐藏空气墙
            if (isHideAirWall)
            {
                GameManager.Instance.SetHideAirWallEffect(true);
            }
            GlobalEvent.OnTimelinePlayStateChangeEvent.Invoke(ConfigID, true);
            OnPrepare(director);
            SetPlayer(director);
            director.Play();

            DelayInvoker.DelayInvoke((float)director.duration + .1f, (a) =>
            {
                if (!IsEnd)
                {
                    OnEnd();
                }
            });
        }

        virtual public void Stop(bool isSkip)
        {
            IsSkip = true;
            if (director != null)
            {
                director.Stop();
            }
        }

        virtual protected void OnEvent(string eventName)
        {
        }

        virtual protected void OnPause()
        {
        }

        virtual protected void OnEnd()
        {
            IsEnd = true;
            GlobalEvent.OnTimelineEvent.Invoke("", this.director, new TimelineEventData()
            {
                EventType=TimelineEventDefine.CtrMainLight,
                EventArgs=new List<string>()
                {
                    "1"
                }
            }, 0);

            GlobalEvent.OnTimelinePlayStateChangeEvent.Invoke(ConfigID, false);

            UIManager.Instance.CloseWidget(UIDef.QTEWidget, null, true);

            if (IsSkip)
            {
                GameManager.Instance.EventPreNewPlayerEvent($"8_{ConfigID}_2");
            }
            else
            {
                GameManager.Instance.EventPreNewPlayerEvent($"8_{ConfigID}_3");
            }

            SGF.Debuger.LogWarning($"解开---- 主角移动  TimelinePath = {TimelinePath}");

            if (director == null)
            {
                SGF.Debuger.LogWarning($"解开---- 主角移动 完犊子  被删除了，解不开了");
                //Debug.Break();
            }

            if (isHideUI)
            {
                UIManager.Instance.SetShowTimeLineToHideHud(false);
                TimelineManager.Instance.IsPlayTimeLine = false;
            }

            if (isHideRole)
            {
                if (GameManager.Instance.M_MainPlayerCtrlBase != null &&
                    GameManager.Instance.M_MainPlayerCtrlBase.M_Curr != null)
                {
                    GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.ActionOnStopHidden?.Invoke(false);
                }
            }

            if (isHidePartner)
            {
                if (PartnerManager.Instance.CurConcretizationPartner != null)
                {
                    if (PartnerManager.Instance.CurConcretizationPartner.M_Curr != null)
                    {
                        PartnerManager.Instance.CurConcretizationPartner.M_Curr.ActionOnStopHidden?.Invoke(false);
                    }
                }
            }

            if (!isCanMove)
            {
                SGF.Debuger.LogWarning(
                    $"timeline里面要 解开 主角移动 朝向攻击技能 TimelinePath={TimelinePath},EffectID={EffectID} 11111111111");

                GameManager.Instance.UnRegisterMainPlayerClientBattleStates();
            }

            List<E_EntityType> mTypes = new();
            // 隐藏地图NPC
            if (isHideMapNPC)
            {
                mTypes.Add(E_EntityType.Npc);
            }

            // 隐藏其他玩家
            if (isHideOtherPlayer)
            {
                mTypes.Add(E_EntityType.Player);
            }

            //怪物
            if (isHideMon)
            {
                mTypes.Add(E_EntityType.Monster);
                mTypes.Add(E_EntityType.Summon);
                mTypes.Add(E_EntityType.ClientSummon);
            }

            //交互物
            if (isHideObj)
            {
                mTypes.Add(E_EntityType.Interact);
            }
            GameManager.Instance.SetEntityHide(false, mTypes);

            //空气墙
            if (isHideAirWall)
            {
                GameManager.Instance.SetHideAirWallEffect(false);
            }
            string key = string.Format("{0}_{1}", TimelinePath, TimelineType);
            OnEndAction?.Invoke(key, EffectID, ID, NeedFadeOut);
            GlobalEvent.OnEffectEnd.Invoke(EffectID);
            if (mModel != null)
            {
                mModel.transform.SetParent(Modleparent);
                mModel.transform.localPosition = localPosition;
                mModel.transform.localRotation = localRotation;
                mModel.transform.localScale = localScale;
            }

            var player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (player != null)
            {
                //  ViewVitalAnim view = player.Container.gameObject.GetComponentInChildren<ViewVitalAnim>();
                // view.SetIsSyncPos(true);
                if (player.Container != null)
                {
                    var collider = player.Container.GetComponentInChildren<CharacterController>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }

                NPCEntityBase npc = player.M_Curr as NPCEntityBase;
                if (npc != null)
                {
                    npc.ForceSetState(npc.GetForceDefaultState(), null);
                }
            }

            if (CameraManager.Instance.CurrentPlayCamera != null)
            {
                var cinemachine = CameraManager.Instance.CurrentPlayCamera.GetComponent<CinemachineBrain>();
                if (cinemachine != null)
                {
                    cinemachine.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
                    cinemachine.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
                }
            }

            OnTimeLineEffectEnd?.Invoke(EffectID);

            if (director != null && director.gameObject != null)
            {
                if (isFinishHide)
                {
                    GameObject.Destroy(director.gameObject);
                }
            }

            //CinemachineBrain.UpdateMethod.FixedUpdate
            SGF.Debuger.Log($"Timeline end TimelinePath={TimelinePath}  EffectID={EffectID}");
        }
    }
}