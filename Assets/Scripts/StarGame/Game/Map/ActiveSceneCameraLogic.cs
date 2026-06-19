using SGF.UI.Framework;
using StarProject.Service.Cam;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Map
{
    public class ActiveSceneCameraLogic : IMapLogic
    {
        private Dictionary<int, VirtualCamera> Cameras = null;
        private GameObject mCameraRoot;

        public LogicType GetLogicType()
        {
            return LogicType.SceneCamera;
        }

        public static ActiveSceneCameraLogic Create()
        {
            ActiveSceneCameraLogic logic = new();
            logic.OnCreate();
            return logic;
        }

        public void OnCreate()
        {
            Cameras = new Dictionary<int, VirtualCamera>();
            GlobalEvent.OnActiveSceneVirtualCamera.AddListener(OnActiveVirtualCamera);
        }

        private void OnActiveVirtualCamera(int index, int effectID)
        {
            if (Cameras.ContainsKey(index))
            {
                SGF.Debuger.Log($"摄像机 激活 index={index},effectID={effectID}");
                Action cb = () =>
                {
                    if (Cameras.ContainsKey(index))
                    {
                        Cameras[index].OnActive(effectID);
                    }
                    else
                    {
                        SGF.Debuger.LogWarning($"OnActiveVirtualCamera index={index},Cameras not find ,,,err!!!!");
                    }
                };
                UIQueueManager.Instance.AddSpecialUIQueue($"Camera_{index}", (int)UIQueuePriorityType.BlockingPlot, UISeatType.Full, cb);
            }
            else
            {
                OnEndEffect(index, effectID);
            }
        }

        private void OnEndEffect(int index, int effectID)
        {
            SGF.Debuger.Log($"摄像机 关闭 OnEndEffect index={index},effectID={effectID}");
            UIQueueManager.Instance.DelSpecialUIQueue($"Camera_{index}", UISeatType.Full);
            if (Cameras.ContainsKey(index))
            {
                int nextIndex = Cameras[index].NextPlayIndex;
                if (nextIndex != -1)
                {
                    OnActiveVirtualCamera(nextIndex, effectID);
                    return;
                }
            }
            GlobalEvent.OnEffectEnd.Invoke(effectID);
        }

        public void OnLoad()
        {
            Cameras.Clear();
            string path = $"TimeLine/Map/Camera_{GameMap.sceneJsonData.SceneID}";

            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }
                    mCameraRoot = GameObject.Instantiate<GameObject>(go);
                    if (mCameraRoot != null)
                    {
                        mCameraRoot.transform.position = Vector3.zero;
                        mCameraRoot.transform.rotation = Quaternion.identity;
                        mCameraRoot.transform.localScale = Vector3.one;

                        int count = mCameraRoot.transform.childCount;
                        for (int i = 0; i < count; i++)
                        {
                            var tr = mCameraRoot.transform.GetChild(i);
                            if (tr != null && tr.gameObject != null)
                            {
                                var cam = tr.GetComponent<VirtualCamera>();
                                if (!Cameras.ContainsKey(cam.Index))
                                {
                                    cam.SetCallBack(OnEndEffect);
                                    cam.SetOutTime(CameraManager.Instance.GetBlendForVirtualCamerasOutTime(tr.name, "PlayerCamera"));
                                    Cameras.Add(cam.Index, cam);
                                }
                                else
                                {
                                    Debug.LogWarning($"{GameMap.sceneJsonData.SceneID} 存在相同的index {cam.Index}");
                                }
                                tr.gameObject.SetActive(false);
                            }
                        }
                    }
                });
        }

        public void OnClear()
        {
            Cameras.Clear();
            if (mCameraRoot != null)
            {
                GameObject.Destroy(mCameraRoot);
            }
        }

        public void OnUnLoad()
        {
            OnClear();
        }
    }
}