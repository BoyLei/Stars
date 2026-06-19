using System;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace SkillEditor
{
    [DisallowMultipleComponent]
    public class NotificationReceiver : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification != null)
            {
                bool test = SkillEditorGlobal.WwiseListenerTestRoot == null;//提前算一下
                //double time = origin.IsValid() ? origin.GetTime() : 0.0;
                if (notification is WWiseEventMarker phase)
                {
                    GameObject sameNameGob;//同事件名的会互相替换,利用InstId;  
                    if (SkillEditorGlobal.WwiseListenerTestRoot.transform.Find(phase.akEvent.Name) != null)
                    {
                        sameNameGob = SkillEditorGlobal.WwiseListenerTestRoot.transform.Find(phase.akEvent.Name).gameObject;
                    }
                    else
                    {
                        sameNameGob = new GameObject(phase.akEvent.Name);
                        sameNameGob.transform.SetParent(SkillEditorGlobal.WwiseListenerTestRoot.transform);
                    }
                    /*VarStore varStore = new VarStore();
                    varStore.AddInt(this.gameObject.GetInstanceID());
                    varStore.AddInt((int)phase.PhaseType);
                    SystemHelper.BroadcastTriggle(TriggleDefine.on_change_anim_phase, varStore);*/
                    AkSoundEngine.PostEvent(phase.akEvent.Name, sameNameGob);
                }

                //if (notification is LockMarker)
                //{
                //    LockMarker marker = notification as LockMarker;
                //    if (marker != null)
                //    {
                //        if (marker.LockType != null && marker.LockType.Length > 0)
                //        {
                //            foreach (var LockType in marker.LockType)
                //            {
                //                VarStore varStore = new VarStore();
                //                varStore.AddInt(this.gameObject.GetInstanceID());

                //                if (LockType == eLockType.LockSkill)
                //                {
                //                    varStore.AddBool(marker.LockState == eLockState.Start);
                //                    varStore.AddString(on_lock_skillKey);
                //                    SystemHelper.BroadcastTriggle(TriggleDefine.on_lock_skill, varStore);
                //                }
                //                else if (LockType == eLockType.LockPosition)
                //                {
                //                    varStore.AddBool(marker.LockState == eLockState.Start);
                //                    varStore.AddString(on_lock_moveKey);
                //                    SystemHelper.BroadcastTriggle(TriggleDefine.on_lock_move, varStore);
                //                }
                //                else if (LockType == eLockType.LockRotation)
                //                {
                //                    varStore.AddBool(marker.LockState == eLockState.Start);
                //                    varStore.AddString(on_lock_rotateKey);
                //                    SystemHelper.BroadcastTriggle(TriggleDefine.on_lock_rotate, varStore);
                //                }
                //                varStore.Destroy();
                //               // Debug.LogError(transform.name+"=====LockType " + LockType + "    locked" + (marker.LockState == eLockState.Start));
                //            }
                //        }
                //    }
                //}

                //if (notification is DamageMarker damage)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    varStore.AddInt((int)damage.triggerEvent);
                //    varStore.AddInt(damage.extentid);
                //    //轨道名字
                //   // varStore.AddString(damage.parent.name);
                //    varStore.AddInt(damage.parent.GetInstanceID());
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_trigger_damage, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is GeneralMarker general)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    varStore.AddInt((int)general.GeneralEvent);
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_trigger_general_event, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is DungenMarker dungenMarker)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(dungenMarker.EventID);
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_dungenevent, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is CommonMarker common)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt((int)common.EventID);
                //    varStore.AddUObject(this.gameObject);
                //    varStore.AddObject(common.Arguments);
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_commonevent, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is BufferMarker buffer)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_bufferevent, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is LoopSkillMarker loopSkill)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    varStore.AddFloat(loopSkill.StartTime);
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_loopskillevent, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is SpawnBulletMarker)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_spawn_bullet, varStore);
                //    varStore.Destroy();
                //}

                //if (notification is TrapEventMarker trap)
                //{
                //    VarStore varStore = new VarStore();
                //    varStore.AddInt(this.gameObject.GetInstanceID());
                //    varStore.AddInt(trap.Index);
                //    SystemHelper.BroadcastTriggle(TriggleDefine.on_timeline_trapevent, varStore);
                //    varStore.Destroy();
                //}
                //_timelineNotifyContainer?.SetMarkFlag(notification as IMarker);
            }

        }
    }
}