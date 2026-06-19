using System;
using Cinemachine;
using SGF.Unity;
using StarProject.Game;
using UnityEngine;

public class CameraShakeEvent
{

    public static string CameraImpulseSourcePath = "Properties/ImpulseSource/";

    private DictionaryEx<string, TempKeyClass> historyKey2PrefabMainKey = new();

    public void Play(ulong entityID, string effectName)
    {
        string key = $"{entityID}_{effectName}";

        PlayWithKey(key, effectName);
    }

    public void Stop(ulong entityID, string effectName)
    {
        string key = $"{entityID}_{effectName}";

        // 删除 历史的 
        CloseTempKeyClass(key);
    }

    private void PlayWithKey(string key, string effectName)
    {
        // 删除 历史的 
        CloseTempKeyClass(key);

        var newMainKey = new TempKeyClass();
        historyKey2PrefabMainKey[key] = newMainKey;
        string path = CameraImpulseSourcePath + effectName;

        SGF.Debuger.Log($"[CameraEvent] PlayWithKey: {key}");

        StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(path, (GameObject go) =>
               {
                   if (go == null)
                   {
                       return;
                   }

                   newMainKey.Play(path, go, (tmpKeyClass) =>
                   {
                       CloseTempKeyClass(key);
                   });

               });
    }



    private void CloseTempKeyClass(string key)
    {
        if (!historyKey2PrefabMainKey.ContainsKey(key))
        {
            return;
        }
        //SGF.Debuger.Log($"[CameraEvent] CloseTempKeyClass: {key}");

        historyKey2PrefabMainKey[key].Close();
        historyKey2PrefabMainKey.Remove(key);
    }

    internal class TempKeyClass
    {
        public bool IsValid = true;

        private GameObject Go;

        private string Path;

        private CinemachineImpulseManager.ImpulseEvent impulseEvent;


        private Action<TempKeyClass> CloseCb;

        /// <summary>
        /// close 存在两种情况:
        ///     1. 资源未加载成功, 那么 直接 设置 IsValid = false 即可,在 资源加载成功后, 内部再去根据 IsValid 来判定是否可以播放;
        ///     2. 资源已经加载成功, 那么 立即结束这个 震屏事件, 并且 把节点还入节点池;
        /// </summary>
        public void Close()
        {
            if (!IsValid)
            {
                return;
            }

            IsValid = false;

            // 取消 定时关闭
            DelayInvoker.CancelInvoke(this);

            ReturnGo(Go);
        }


        public void Play(string path, GameObject go, Action<TempKeyClass> closeCb)
        {
            CloseCb = closeCb;
            Path = path;
            Go = go;

            // 如果被关闭了, 那外部应该会 移除这个 类的引用
            if (!IsValid)
            {
                ReturnGo(go);
                return;
            }


            go.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
            go.SetActive(true);
            go.transform.position = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();

            CinemachineImpulseSource cinemachineImpulse = go.GetComponent<CinemachineImpulseSource>();
            if (cinemachineImpulse == null)
            {
                ReturnGo(go);
                return;
            }


            impulseEvent = cinemachineImpulse.GenerateImpulseWithForce(1);
            DelayInvoker.DelayInvoke(this, cinemachineImpulse.m_ImpulseDefinition.m_ImpulseDuration, (args) =>
            {
                Close();
            });
        }

        private void ReturnGo(GameObject go)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(Path, go);

            // 发送 虚拟相机震屏结束事件
            if (impulseEvent != null)
            {
                impulseEvent.Cancel(0, true);
            }
            CloseCb?.Invoke(this);
        }


    }

}