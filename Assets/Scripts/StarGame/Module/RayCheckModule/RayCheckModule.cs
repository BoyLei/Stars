using SGF.Module.Framework;
using SGF.Network;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProject.Service.DisplayProcess;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class RayCheckItem
    {
        public bool IsInCollision;
        public RayCheckMark Go;
    }


    public class RayCheckModule : BusinessModule
    {
        //战斗相机
        public CameraBase StarWorldCam;
        //主角
        private PlayerCtrlGroup MainPlayer;

        Dictionary<string, RayCheckItem> allHitList = new Dictionary<string, RayCheckItem>();
        List<string> needToDel = new List<string>();


        public override void Create(object args = null)
        {
            base.Create(args);
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
            MonoHelper.AddFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
        }

        private void OnRoleCreateComplete(object obj)
        {
            MainPlayerCreateSuccess();
        }

        private void MainPlayerCreateSuccess()
        {
            MainPlayer = (PlayerCtrlGroup)GameManager.Instance.GetEntityCtr(GameManager.Instance.mainPlayerId);
            StarWorldCam = CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam);
        }

        protected override void Show(object arg)
        {

        }


        public override void Release()
        {
            GlobalEvent.OnRoleCreateComplete.RemoveListener(OnRoleCreateComplete);
            MonoHelper.RemoveFixedUpdateListener(FixedUpdate, MonoHelper.E_ModuleType.CommonService);
            base.Release();
        }


        // Update is called once per frame
        void FixedUpdate()
        {

            if (StarWorldCam == null || MainPlayer == null || MainPlayer.M_Curr == null)
            {
                return;
            }


            //所有老的打标记
            foreach (var kvp in allHitList)
            {
                kvp.Value.IsInCollision = false;
            }

            //int layer = LayerMask.NameToLayer("Default");
            //RaycastHit[] allHit = Physics.RaycastAll(StarWorldCam.Camera.transform.position, MainPlayer.M_Curr.Position() - StarWorldCam.Camera.transform.position,5000, layer);
            RaycastHit[] allHit = Physics.RaycastAll(StarWorldCam.Camera.transform.position, MainPlayer.M_Curr.Position() + new Vector3(0, 1, 0) - StarWorldCam.Camera.transform.position);
            if (allHit.Length > 0) //如果碰撞检测到物体
            {
                for (int i = 0; i < allHit.Length; i++)
                {
                    RaycastHit hit = allHit[i];
                    RayCheckMark rcm = hit.collider.GetComponent<RayCheckMark>();
                    if (rcm != null)
                    {
                        if (allHitList.ContainsKey(rcm.GroupName))
                        {
                            allHitList[rcm.GroupName].IsInCollision = true;
                            allHitList[rcm.GroupName].Go.SetMatState(true);
                        }
                        else
                        {
                            rcm.SetMatState(true);
                            allHitList.Add(rcm.GroupName, new RayCheckItem { IsInCollision = true, Go = rcm });
                        }
                    }
                }
            }

            needToDel.Clear();
            //所有false标记的全部还原，清除
            foreach (var kvp in allHitList)
            {
                if (!kvp.Value.IsInCollision)
                {
                    needToDel.Add(kvp.Key);
                    RevertOldHit(kvp.Value.Go);
                }
            }

            for (int i = 0; i < needToDel.Count; i++)
            {
                allHitList.Remove(needToDel[i]);
            }

        }

        void RevertOldHit(RayCheckMark oldHit)
        {
            if (oldHit != null)
            {
                oldHit.SetMatState(false);
                oldHit = null;
            }
        }

    }
}
