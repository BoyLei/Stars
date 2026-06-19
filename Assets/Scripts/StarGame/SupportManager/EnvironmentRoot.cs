using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentRoot : SGF.Unity.MonoSingletonEx<EnvironmentRoot>
{
    //进入游戏开始调用点来创建
    private Transform localStaticRoot;//本地美术环境
    private Transform localDynamicRoot;//本地机关
    private Transform remoteStaticRoot;//自己也是服务器创建的，虽然avatar可以替换被角色管理，依然属于确定资源
    private Transform remoteDynamicRoot;//其他服务器创建的动态物件
    private AudioSource soundRoot1;//声音节点
    private AudioSource soundRoot2;//声音节点
    private AudioSource soundRoot3;//声音节点
    private AudioSource soundRoot4;//声音节点
    public Transform LocalStaticRoot { 
        get {
            if (localStaticRoot == null)
            {
                localStaticRoot = EnsureMyGobPoint("LocalStaticRoot");
            }
            return localStaticRoot;
        } 
        set => localStaticRoot = value; }

    public Transform LocalDynamicRoot
    {
        get
        {
            if (localDynamicRoot == null)
            {
                localDynamicRoot = EnsureMyGobPoint("LocalDynamicRoot");
            }
            return localDynamicRoot;
        }
        set => localDynamicRoot = value; }

    public Transform RemoteStaticRoot
    {
        get
        {
            if (remoteStaticRoot == null)
            {
                remoteStaticRoot = EnsureMyGobPoint("RemoteStaticRoot");
            }
            return remoteStaticRoot;
        }
        set => remoteStaticRoot = value; }

    public Transform RemoteDynamicRoot
    {
        get
        {
            if (remoteDynamicRoot == null)
            {
                remoteDynamicRoot = EnsureMyGobPoint("RemoteDynamicRoot");
            }
            return remoteDynamicRoot;
        }
        set => remoteDynamicRoot = value; }

    #region 声音节点组
    public AudioSource SoundRoot_UISystem1
    {
        get
        {
            if (soundRoot1 == null)
            {
                soundRoot1 = EnsureMyGobPoint("SoundRoot_UISystem1").gameObject.AddComponent<AudioSource>();
            }
            return soundRoot1;
        }
        set => soundRoot1 = value;
    }
    public AudioSource SoundRoot_RoleAudio2
    {
        get
        {
            if (soundRoot2 == null)
            {
                soundRoot2 = EnsureMyGobPoint("SoundRoot_RoleAudio2").gameObject.AddComponent<AudioSource>();
            }
            return soundRoot2;
        }
        set => soundRoot2 = value;
    }
    public AudioSource SoundRoot_EnvTri3
    {
        get
        {
            if (soundRoot3 == null)
            {
                soundRoot3 = EnsureMyGobPoint("SoundRoot_EnvTri3").gameObject.AddComponent<AudioSource>();
            }
            return soundRoot3;
        }
        set => soundRoot3 = value;
    }
    public AudioSource SoundRoot_Bgm4
    {
        get
        {
            if (soundRoot4 == null)
            {
                soundRoot4 = EnsureMyGobPoint("SoundRoot_Bgm4").gameObject.AddComponent<AudioSource>();
            }
            return soundRoot4;
        }
        set => soundRoot4 = value;
    }
    #endregion

    public Transform EnsureMyGobPoint(string name) 
    {
        Transform @object = new GameObject(name).transform;
        @object.SetParent(Instance.transform);
        return @object;

    }

    //离开场景清理
    public void Clear() 
    {
    
    }
}
