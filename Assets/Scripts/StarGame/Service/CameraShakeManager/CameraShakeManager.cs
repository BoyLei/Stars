using Cinemachine;
using SGF.Module.Framework;
using SGF.Unity;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.Cam;
using StarProjectDef;
///--------------------------------------------------------------------
/// 文件名   :   CameraShakeManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/18 09:45:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Service.CameraShake
{
    public class CameraShakeManager : ServiceModule<CameraShakeManager>
    {

        public const string CameraImpulseSourcePath = "Properties/ImpulseSource/";
        public const string CameraOffsetPath = "Properties/CameraOffset/";
        public GameObject mOffsetTrans;
        private Dictionary<string, CameraShakeLogic> CameraShakes;
        private List<CameraShakeJson> WaitingAddShakes;
        private List<string> WaitingRemoves;

        private CamerOffsetStage offsetStage = CamerOffsetStage.None;
        private Vector3 TargetPosition;
        private Vector3 StarPosition;
        private Vector3 NowPosition;


        private float InTime;
        private float StageTime;
        private float OutTime;
        private float RuninigTime = 0;
        private Transform MainPlayerTrans;

        private VirtualCameraMove virtualCameraMove;
        public void Init()
        {
            if (CameraShakes == null)
            {
                CameraShakes = new Dictionary<string, CameraShakeLogic>();
            }
            if (mOffsetTrans == null)
            {
                mOffsetTrans = new GameObject();
                mOffsetTrans.transform.SetParent(EntityRoot.Instance.DotRemoveRoot.transform);
            }

            if (WaitingAddShakes == null)
            {
                WaitingAddShakes = new List<CameraShakeJson>();
            }

            if (WaitingRemoves == null)
            {
                WaitingRemoves = new List<string>();
            }

            if (virtualCameraMove == null)
            {
                virtualCameraMove = new VirtualCameraMove();
            }

            GlobalEvent.OnVirtualCameraShakeEvent.AddListener(VirtualCameraShakeEvent);
            GlobalEvent.OnCameraImpluseEvent.AddListener(OnCameraImpluseEvent);
            GlobalEvent.OnCameraOffsetEvent.AddListener(OnCameraOffsetEvent);

            GlobalEvent.OnCameraMoveEvent.AddListener(OnCameraMoveEvent);


            MonoHelper.AddUpdateListener(OnUpdate, MonoHelper.E_ModuleType.Render);

        }

        public void SetMainPlayerTransfom(Transform transform)
        {
            MainPlayerTrans = transform;
        }

        private void OnCameraOffsetEvent(ulong EntityID, string effectName)
        {
            var entity = GameManager.Instance.GetEntityCtr(EntityID);
            if (entity == null || entity.Data == null)
            {
                return;
            }

            bool CanPlay = entity.Data.EntityType == E_EntityType.Player || entity.Data.EntityType == E_EntityType.Monster;
            if (CanPlay)
            {
                string path = CameraOffsetPath + effectName;
                //Service.Resource.ResourceManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
                //(GameObject go) =>
                //{
                //    if (go == null)
                //    {
                //        return;
                //    }
                //    var gob = GameObject.Instantiate<GameObject>(go);
                //    if (gob != null)
                //    {
                //        CameraOffset cameraOffset = gob.GetComponent<CameraOffset>();
                //        if (cameraOffset != null)
                //        {
                //            SetCameraOffset(cameraOffset.M_Direction, cameraOffset.Distance, cameraOffset.InTime, cameraOffset.StageTime, cameraOffset.BackTime);
                //        }
                //        StarProject.Service.Resource.ResourceManager.Instance.PushGameObject(path, gob);
                //    }
                //});
                StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(path, (go) =>
                {
                    if (go != null)
                    {
                        CameraOffset cameraOffset = go.GetComponent<CameraOffset>();
                        if (cameraOffset != null)
                        {
                            SetCameraOffset(cameraOffset.M_Direction, cameraOffset.Distance, cameraOffset.InTime, cameraOffset.StageTime, cameraOffset.BackTime);
                        }
                        StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);
                    }
                });
            }
        }

        private void SetCameraOffset(CameraOffset.Direction direction, float distance, float intime, float stagetime, float outtime)
        {
            if (mOffsetTrans == null)
            {
                return;
            }
            if (offsetStage == CamerOffsetStage.None)
            {
                mOffsetTrans.transform.position = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
                CameraManager.Instance.SetPlayerFlowTarget(mOffsetTrans.transform);
                offsetStage = CamerOffsetStage.In;
            }

            InTime = intime;
            StageTime = stagetime;
            OutTime = outtime;

            StarPosition = mOffsetTrans.transform.position;
            RuninigTime = 0;
            TargetPosition = GetTargetPosition(GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position(), direction, distance);
        }

        private Vector3 GetTargetPosition(Vector3 rolePositon, CameraOffset.Direction direction, float distance)
        {
            Vector3 targetPos = rolePositon;
            Vector3 dir = Vector3.zero;
            switch (direction)
            {
                case CameraOffset.Direction.UP:
                    dir = MainPlayerTrans.up;
                    break;
                case CameraOffset.Direction.DOWN:
                    dir = MainPlayerTrans.up * -1;
                    break;
                case CameraOffset.Direction.LEFT:
                    dir = MainPlayerTrans.right * -1;

                    break;
                case CameraOffset.Direction.RIGHT:
                    dir = MainPlayerTrans.right;

                    break;
                case CameraOffset.Direction.FRONT:
                    dir = MainPlayerTrans.forward;
                    break;
                case CameraOffset.Direction.BACK:
                    dir = MainPlayerTrans.forward * -1;
                    break;
            }
            targetPos = rolePositon + (dir * distance);
            return targetPos;
        }

        /// <summary>
        /// 摄像机 移动 事件
        /// </summary>
        /// <param name="EntityID"></param>
        private void OnCameraMoveEvent(ulong EntityID, GlobalShowGlobal_CameraMove cameraMove, bool isEnterIn)
        {
            EntityCtrlBase entity = GameManager.Instance.GetEntityCtr(EntityID);
            if (entity == null || entity.Data == null)
            {
                return;
            }

            bool CanPlay = entity.Data.isMainPlayer || entity.Data.EntityType == E_EntityType.Monster;
            if (!CanPlay)
            {
                return;
            }
            if (cameraMove == null)
            {
                return;
            }
            SetCameraMovenEventOffset(entity, cameraMove, isEnterIn);
        }

        private GlobalShowGlobal_CameraMove curCameraMove = null;
        private void StartCameraMoveOffset(EntityCtrlBase entity, GlobalShowGlobal_CameraMove cameraMove, bool isEnterIn)
        {
            if (!isEnterIn && curCameraMove != cameraMove)
            {
                return;
            }
            curCameraMove = cameraMove;
            InTime = cameraMove.EnterTime / 1000f;
            StageTime = cameraMove.LoopTime / 1000f;
            OutTime = cameraMove.EndTime / 1000f;

            RuninigTime = 0;

            int angle = cameraMove.Value;
            int distance = cameraMove.Distance;


            switch (cameraMove.CameraMoveType)
            {
                //根据角色朝向平移
                case CameraMoveType.MoveWithCha:
                    {
                        // 1. 将 玩家朝向 沿着 Y 轴 旋转 指定的角度
                        Vector3 newDir = Quaternion.AngleAxis(angle, Vector3.up) * entity.M_Curr.M_EntityAnglesDir.normalized;

                        TargetPosition = StarPosition + (newDir * distance);
                    }
                    break;
                //根据绝对朝向平移
                case CameraMoveType.MoveWithVal:
                    {

                        // 2. 绝对朝向 
                        Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * distance;
                        TargetPosition = StarPosition + offset;
                    }
                    break;
            }

            Vector3 rolePositon = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            // 有进入 时间
            if (InTime > 0 && isEnterIn)
            {
                if (offsetStage == CamerOffsetStage.None)
                {
                    mOffsetTrans.transform.position = rolePositon;
                    CameraManager.Instance.SetPlayerFlowTarget(mOffsetTrans.transform);
                    offsetStage = CamerOffsetStage.In;
                }
                StarPosition = mOffsetTrans.transform.position;
            }
            else
            {
                // 没有进入时间, 且有 结束时间,  那 就是 走 退出流程
                offsetStage = CamerOffsetStage.Out;
            }
        }


        private void SetCameraMovenEventOffset(EntityCtrlBase entity, GlobalShowGlobal_CameraMove cameraMove, bool isEnterIn)
        {
            CameraMoveType moveType = cameraMove.CameraMoveType;
            switch (moveType)
            {
                //根据角色朝向平移
                case CameraMoveType.MoveWithCha:
                    {
                        StartCameraMoveOffset(entity, cameraMove, isEnterIn);
                    }
                    break;
                //根据绝对朝向平移
                case CameraMoveType.MoveWithVal:
                    {
                        StartCameraMoveOffset(entity, cameraMove, isEnterIn);
                    }
                    break;
                //放缩（0向前、180向后）
                case CameraMoveType.Zoom:
                    {
                        virtualCameraMove.StartMove(cameraMove, isEnterIn);
                    }
                    break;
            }

        }

        private void UpdateCameraOffset()
        {
            if (mOffsetTrans == null)
            {
                return;
            }
            if (offsetStage == CamerOffsetStage.None)
            {
                return;
            }

            if (offsetStage == CamerOffsetStage.In)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                float rate = RuninigTime / InTime;
                mOffsetTrans.transform.position = Vector3.Lerp(StarPosition, TargetPosition, rate);

                if (RuninigTime >= InTime)
                {
                    offsetStage = CamerOffsetStage.Stage;
                    RuninigTime = 0;
                }
                return;
            }

            if (offsetStage == CamerOffsetStage.Stage)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                // stageTime == -1 的时候, 表示 会一致持续, 此时 不配
                if (-1 != StageTime && RuninigTime >= StageTime)
                {

                    offsetStage = CamerOffsetStage.Out;
                    RuninigTime = 0;
                    //NowPosition = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
                }
                return;
            }

            if (offsetStage == CamerOffsetStage.Out)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                float rate = RuninigTime / OutTime;
                mOffsetTrans.transform.position = Vector3.Lerp(TargetPosition, GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position(), rate);
                if (RuninigTime >= OutTime)
                {
                    offsetStage = CamerOffsetStage.None;
                    RuninigTime = 0;
                    CameraManager.Instance.SetPlayerFlowTarget(MainPlayerTrans);
                }
            }
        }
        int count = 0;
        float lasttime;
        private void OnCameraImpluseEvent(ulong EntityID, string effectName, bool isPlay)
        {
            if (!isPlay)
            {
                StopCameraImpluseEvent(EntityID, effectName);
                return;
            }

            count++;
            float dis = UnityEngine.Time.time - lasttime;
            lasttime = UnityEngine.Time.time;
            //StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 1111 EntityID={EntityID} effectName={effectName} ");
            var entity = GameManager.Instance.GetEntityCtr(EntityID);
            if (entity == null || entity.Data == null)
            {
                return;
            }
            //StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 2222 EntityID={EntityID} effectName={effectName} ");

            bool CanPlay = entity.Data.isMainPlayer || entity.Data.EntityType == E_EntityType.Monster;
            if (CanPlay)
            {
                //StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 3333 EntityID={EntityID} effectName={effectName} ");

                string path = CameraImpulseSourcePath + effectName;

                PlayCameraImpluseEvent(EntityID, effectName);

                //Service.Resource.ResourceManager.Instance.LoadResourceUniRefAsync<GameObject>(path,
                //(GameObject go) =>
                //{
                //    StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 4444 EntityID={EntityID} effectName={effectName} ");
                //    if (go == null)
                //    {
                //        return;
                //    }
                //    var gob = GameObject.Instantiate<GameObject>(go);
                //    if (gob != null)
                //    {
                //        StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 5555 EntityID={EntityID} effectName={effectName} ");

                //        gob.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
                //        gob.SetActive(true);
                //        gob.transform.position = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

                //        CinemachineImpulseSource cinemachineImpulse = gob.GetComponent<CinemachineImpulseSource>();
                //        if (cinemachineImpulse != null)
                //        {
                //            StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 6666 EntityID={EntityID} effectName={effectName} ");
                //            cinemachineImpulse.GenerateImpulseWithForce(1);
                //            DelayInvoker.DelayInvoke(this, cinemachineImpulse.m_ImpulseDefinition.m_ImpulseDuration, (args) =>
                //            {
                //                StarProject.Service.Resource.ResourceManager.Instance.PushGameObject(path, gob);
                //            });
                //        }
                //        else
                //        {
                //            StarProject.Service.Resource.ResourceManager.Instance.PushGameObject(path, gob);
                //        }
                //    }
                //});

                // StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(path, (go) =>
                // {
                //     StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 4444 EntityID={EntityID} effectName={effectName} ");
                //     if (go != null)
                //     {
                //         StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 5555 EntityID={EntityID} effectName={effectName} ");

                //         go.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
                //         go.SetActive(true);
                //         go.transform.position = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

                //         CinemachineImpulseSource cinemachineImpulse = go.GetComponent<CinemachineImpulseSource>();
                //         if (cinemachineImpulse != null)
                //         {
                //             StarDebug.Log(StarDebug.Green, $"Count={count} dis={dis} OnCameraImpluseEvent 6666 EntityID={EntityID} effectName={effectName} ");
                //             cinemachineImpulse.GenerateImpulseWithForce(1);
                //             DelayInvoker.DelayInvoke(this, cinemachineImpulse.m_ImpulseDefinition.m_ImpulseDuration, (args) =>
                //             {
                //                 StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);
                //             });
                //         }
                //         else
                //         {
                //             StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);
                //         }
                //     }
                // });
            }
        }

        private void PlayCameraImpluseEvent(ulong EntityID, string effectName)
        {
            cameraShakeEvent.Play(EntityID, effectName);
        }

        private void StopCameraImpluseEvent(ulong EntityID, string effectName)
        {
            cameraShakeEvent.Stop(EntityID, effectName);
        }

        private CameraShakeEvent cameraShakeEvent = new();

        private void VirtualCameraShakeEvent(bool start, CameraShakeJson json)
        {
            if (start)
            {
                if (CameraShakes.ContainsKey(json.VirtualCameraName))
                {
                    CameraShakes[json.VirtualCameraName].OnStop();
                    CameraShakes.Remove(json.VirtualCameraName);
                }

                if (!WaitingAddShakes.Contains(json))
                {
                    WaitingAddShakes.Add(json);
                }
            }
            else
            {
                if (CameraShakes.ContainsKey(json.VirtualCameraName))
                {
                    if (CameraShakes[json.VirtualCameraName].Index == json.Index)
                    {
                        CameraShakes[json.VirtualCameraName].OnStop();
                        CameraShakes.Remove(json.VirtualCameraName);
                    }
                }
            }
        }

        private void CreatShakeLogic(CameraShakeJson json)
        {
            if (json == null)
            {
                SGF.Debuger.LogError("CameraShakeJson is nulll ");

                return;
            }

            if (string.IsNullOrEmpty(json.VirtualCameraName))
            {
                SGF.Debuger.LogError("CameraShakeJson.VirtualCameraName  is IsNullOrEmpty ");
                return;
            }

            if (CameraShakes.ContainsKey(json.VirtualCameraName))
            {
                SGF.Debuger.LogError($"CameraShakes.ContainsKey {json.VirtualCameraName}");
                return;
            }
            if (json.Duration < 1)
            {
                SGF.Debuger.LogError("CameraShakeJson.Duration  < 1 ");
                return;
            }

            CinemachineVirtualCameraBase[] cinemachineVirtuals = (CinemachineVirtualCameraBase[])GameObject.FindObjectsOfType(typeof(CinemachineVirtualCameraBase));

            if (cinemachineVirtuals == null || cinemachineVirtuals.Length < 1)
            {
                SGF.Debuger.LogError("CinemachineVirtualCameraBase[] is nulll ");
                return;
            }

            CinemachineVirtualCameraBase cinemachine = null;
            foreach (var item in cinemachineVirtuals)
            {
                if (item.name == json.VirtualCameraName || item.name == $"{json.VirtualCameraName}(Clone)")
                {
                    cinemachine = item;
                    break;
                }
            }

            if (cinemachine == null)
            {
                SGF.Debuger.LogError("CinemachineVirtualCameraBase not find ");
                return;
            }

            if (json.FrequencyGain == null)
            {
                SGF.Debuger.LogError("CameraShakeJson FrequencyGain is nulll ");

                return;
            }

            if (json.AmplitudeGain == null)
            {
                SGF.Debuger.LogError("CameraShakeJson AmplitudeGain is nulll ");

                return;
            }

            CameraShakeLogic cameraShake = CameraShakeLogic.Create(cinemachine, json.AmplitudeGain, json.FrequencyGain, json.Duration * 0.001f, json.Index);
            CameraShakes.Add(json.VirtualCameraName, cameraShake);
        }

        private void OnUpdate()
        {
            virtualCameraMove.Update();

            UpdateCameraOffset();
            //删除
            if (WaitingRemoves != null && WaitingRemoves.Count > 0)
            {
                foreach (var item in WaitingRemoves)
                {
                    CameraShakes.Remove(item);
                }
                WaitingRemoves.Clear();
            }

            //添加
            if (WaitingAddShakes != null && WaitingAddShakes.Count > 0)
            {
                foreach (var item in WaitingAddShakes)
                {
                    CreatShakeLogic(item);
                }
                WaitingAddShakes.Clear();
            }

            //更新
            if (CameraShakes != null && CameraShakes.Count > 0)
            {
                foreach (var item in CameraShakes)
                {
                    item.Value.OnUpdate();
                    if (item.Value.ShakeStage == ShakeStage.Finished)
                    {
                        WaitingRemoves.Add(item.Key);
                    }
                }
            }

        }

        public override void Release()
        {
            GlobalEvent.OnVirtualCameraShakeEvent.RemoveListener(VirtualCameraShakeEvent);
            GlobalEvent.OnCameraImpluseEvent.RemoveListener(OnCameraImpluseEvent);
            GlobalEvent.OnCameraOffsetEvent.RemoveListener(OnCameraOffsetEvent);
            MonoHelper.RemoveUpdateListener(OnUpdate, MonoHelper.E_ModuleType.Render);
            if (CameraShakes != null)
            {
                CameraShakes.Clear();
            }
            base.Release();
        }

    }
    public class VirtualCameraMove
    {
        private CamerOffsetStage offsetStage = CamerOffsetStage.None;

        private bool startVirtualCameraMove = false;
        private Vector3 StartOffset = Vector3.zero;
        private Vector3 TargetOffset = Vector3.zero;

        private float InTime = 0;
        private float StageTime = 0;
        private float OutTime = 0;
        private float RuninigTime = 0;

        private Vector3 tempV3 = Vector3.zero;

        private GlobalShowGlobal_CameraMove curCameraMove = null;

        public void StartMove(GlobalShowGlobal_CameraMove cameraMove, bool isEnterIn)
        {
            // 虚拟相机 结束 的 必须是 当前的
            if (!isEnterIn && curCameraMove != cameraMove)
            {
                return;
            }
            curCameraMove = cameraMove;
            InTime = cameraMove.EnterTime / 1000f;
            StageTime = cameraMove.LoopTime / 1000f;
            OutTime = cameraMove.EndTime / 1000f;

            RuninigTime = 0;

            int distance = cameraMove.Distance;

            tempV3.z = distance;
            // 如果是 缩放的话, 就用 虚拟相机 的 CameraOffset 的 Z 轴来处理
            StartVirtualCameraMoveOffset(tempV3, isEnterIn);
        }

        /// <summary>
        /// 开始虚拟相机的 偏移
        /// </summary>
        private void StartVirtualCameraMoveOffset(Vector3 offset, bool isEnterIn)
        {
            if (InTime > 0 && isEnterIn)
            {
                if (offsetStage == CamerOffsetStage.None)
                {
                    offsetStage = CamerOffsetStage.In;
                }
                StartOffset = Vector3.zero;
                startVirtualCameraMove = true;
                TargetOffset = StartOffset + offset;
                return;
            }
            if (offsetStage == CamerOffsetStage.In || offsetStage == CamerOffsetStage.Stage)
            {
                // 没有进入时间, 且有 结束时间,  那 就是 走 退出流程
                offsetStage = CamerOffsetStage.Out;
            }
        }

        public void Update()
        {
            if (!startVirtualCameraMove)
            {
                return;
            }
            if (offsetStage == CamerOffsetStage.None)
            {
                return;
            }
            if (offsetStage == CamerOffsetStage.In)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                float rate = RuninigTime / InTime;
                // 开始阶段, 虚拟相机的 偏移从 0 ---> 1 走到 目标的偏移 距离
                CameraManager.Instance.UpdateVirtualCameraOffset(Vector3.Lerp(StartOffset, TargetOffset, rate));

                if (RuninigTime >= InTime)
                {
                    offsetStage = CamerOffsetStage.Stage;
                    RuninigTime = 0;
                }
                return;
            }

            if (offsetStage == CamerOffsetStage.Stage)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                if (-1 != StageTime && RuninigTime >= StageTime)
                {
                    offsetStage = CamerOffsetStage.Out;
                    RuninigTime = 0;
                }
                return;
            }

            if (offsetStage == CamerOffsetStage.Out)
            {
                RuninigTime += UnityEngine.Time.deltaTime;
                float rate = RuninigTime / OutTime;
                // 回退阶段, 虚拟相机的 偏移从 1 ---> 0 
                CameraManager.Instance.UpdateVirtualCameraOffset(Vector3.Lerp(TargetOffset, Vector3.zero, rate));

                if (RuninigTime >= OutTime)
                {
                    offsetStage = CamerOffsetStage.None;
                    RuninigTime = 0;
                    startVirtualCameraMove = false;
                }
            }
        }
    }
}
