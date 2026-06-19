///--------------------------------------------------------------------
/// 文件名   :   SceneObstacleLogic.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/07 10:08:46
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using SGF.Network;
using System.Collections.Generic;
using SGF.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace StarProject.Game.Map
{
    public class SceneObstacleLogic : IMapLogic
    {
        private Dictionary<int, ObstacleGroupLogic> obstacleGroups = null;

        private Google.Protobuf.Collections.MapField<int, bool> ObstacleInfo = null; // 缓存一份服务器通知的场景消息

        public LogicType GetLogicType()
        {
            return LogicType.Obstacle;
        }

        public static SceneObstacleLogic Create()
        {
            SceneObstacleLogic logic = new();
            logic.OnCreate();
            return logic;
        }

        public void SetHideEffect(bool hide)
        {
            if (obstacleGroups != null)
            {
                foreach (var item in obstacleGroups)
                {
                    item.Value.SetHideEffect(hide);
                }
            }
        }
        public void OnCreate()
        {
            obstacleGroups = new Dictionary<int, ObstacleGroupLogic>();
            // 场景切换
            NetworkManager.Instance.OnMessageEnum(MsgIDEnum.ObstacleInfoNtfID, OnEnterSpaceNtf, this);
        }

        private void OnEnterSpaceNtf(MessageHandleData data)
        {
            if (data == null)
            {
                return;
            }

            ProtoMsg.ObstacleInfoNtf enterSpace = (ProtoMsg.ObstacleInfoNtf)data.data;
            if (enterSpace != null)
            {
                SetObstacleInfo(enterSpace.ObstacleInfo);
            }
        }

        public void SetObstacleInfo(Google.Protobuf.Collections.MapField<int, bool> obstacleInfo)
        {
            foreach (var item in obstacleInfo)
            {
                if (obstacleGroups.ContainsKey(item.Key))
                {
                    obstacleGroups[item.Key].SetState(item.Value);
                }
                else
                {
                    ObstacleInfo = obstacleInfo;
                }
            }
        }

        public void OnLoad()
        {
            OnClear();
            if (GameMap.sceneJsonData.Obstacles != null && GameMap.sceneJsonData.Obstacles.Count > 0)
            {
                foreach (var item in GameMap.sceneJsonData.Obstacles)
                {
                    ObstacleGroupLogic groupLogic = new(item.Value);
                    groupLogic.SetState(false);
                    obstacleGroups.Add(groupLogic.GroupID, groupLogic);
                }

                if (ObstacleInfo != null)
                {
                    SetObstacleInfo(ObstacleInfo);
                    ObstacleInfo = null;
                }
            }
        }

        public void OnUnLoad()
        {
            OnClear();
            ObstacleInfo = null;
        }

        public void OnClear()
        {
            if (obstacleGroups != null && obstacleGroups.Count > 0)
            {
                foreach (var item in obstacleGroups)
                {
                    item.Value.OnUnLoad();
                }

                obstacleGroups.Clear();
            }
        }
    }

    public class ObstacleGroupLogic
    {
        private GameObject mRoot;
        public int GroupID;
        public List<ObstacleLogic> Obstacles = new();
        public List<ObstacleEffect> Effects = new();

        public ObstacleGroupLogic(ObstacleGroupJsonData groupJsonData)
        {
            Obstacles.Clear();
            Effects.Clear();
            GroupID = groupJsonData.GroupID;
            mRoot = new GameObject($"{GroupID}");
            GameObject.DontDestroyOnLoad(mRoot);
            mRoot.transform.position = Vector3.zero;
            mRoot.transform.rotation = Quaternion.identity;
            mRoot.transform.localScale = Vector3.one;

            if (groupJsonData.Obstacles != null && groupJsonData.Obstacles.Count > 0)
            {
                for (int i = 0; i < groupJsonData.Obstacles.Count; i++)
                {
                    Obstacles.Add(new ObstacleLogic(GroupID, i, mRoot, groupJsonData.Obstacles[i]));
                }
            }

            if (groupJsonData.Effects != null && groupJsonData.Effects.Count > 0)
            {
                for (int i = 0; i < groupJsonData.Effects.Count; i++)
                {
                    Effects.Add(new ObstacleEffect(GroupID, i, mRoot, groupJsonData.Effects[i]));
                }
            }

            //默认开关
            SetState(groupJsonData.IsOpen);
        }

        public void SetState(bool open)
        {
            if (Obstacles != null)
            {
                foreach (var item in Obstacles)
                {
                    item.SetState(open);
                }
            }

            if (Effects != null)
            {
                foreach (var item in Effects)
                {
                    item.SetState(open);
                }
            }
        }


        public void SetHideEffect(bool hide)
        {
            if (Effects != null)
            {
                foreach (var effect in Effects)
                {
                    if (effect != null)
                    {
                        effect.HideCount += hide? 1 : -1;
                    }

                }
            }
        }
        public void OnUnLoad()
        {
            if (mRoot != null)
            {
                GameObject.Destroy(mRoot);
            }
        }
    }

    public class ObstacleLogic
    {
        public NavMeshObstacle NavMeshObstacle { get; private set; }

        public Collider Collider { get; private set; }

        public ObstacleLogic(int GroupID, int Index, GameObject root, ObstacleGroupJsonData.ObstacleJsonData jsonData)
        {
            int Shape = jsonData.Shape;
            Vector3 Rotation = new(0, jsonData.Rotation, 0);
            Vector3 Center = jsonData.Center.Convert();
            Vector3 Size = jsonData.Size.Convert();
            Vector3 Position = jsonData.Position.Convert();

            GameObject go = new($"Obstacle_{GroupID}_{Index}");
            go.transform.SetParent(GameManager.Instance.M_Map.M_rootHelper.DynamicColliderRoot);
            go.transform.position = Position;
            go.transform.rotation = Quaternion.Euler(Rotation);
            go.transform.localScale = Vector3.one;
            go.transform.SetParent(root.transform);

            NavMeshObstacle = go.AddComponent<NavMeshObstacle>();
            NavMeshObstacle.shape = (NavMeshObstacleShape)Shape;
            NavMeshObstacle.size = Size;
            NavMeshObstacle.center = Center;

            if (Shape == (int)NavMeshObstacleShape.Box)
            {
                BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                boxCollider.center = Center;
                boxCollider.size = Size;

                Collider = boxCollider;
            }
            else
            {
                CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
                capsuleCollider.center = Center;
                capsuleCollider.height = Size.x;
                capsuleCollider.radius = Size.y;
                Collider = capsuleCollider;
            }
        }

        public void SetState(bool open)
        {
            Collider.enabled = open;
            NavMeshObstacle.carving = open;
        }
    }

    public class ObstacleEffect
    {
        public GameObject Effect;
        public bool Actived = false;
        public bool isPlayWave = false;

        private int m_HideCount;

        public int HideCount
        {
            get
            {
                return m_HideCount;
            }
            set
            {
                if (m_HideCount != value)
                {             
                    m_HideCount = value;
                    
                    if (Effect != null)
                    {
                        Effect.SetActive(Actived && !isPlayWave && IsShowEffect);
                    }

                }
            }
        }
        
        private  bool IsShowEffect=>m_HideCount <1;
        
        
        public ObstacleEffect(int GroupID, int Index, GameObject root,
            ObstacleGroupJsonData.ObstacleEffectJsonData jsonData)
        {
            Vector3 Rotation = new(0, jsonData.Rotation, 0);
            Vector3 Scale = jsonData.Scale.Convert();
            Vector3 Position = jsonData.Position.Convert();
            string Path = jsonData.Path;
            m_HideCount = 0;
            if (string.IsNullOrEmpty(Path))
            {
                Path = "Effects/Scene/Cm/Fx_scene_kongqiqiang_common_01";
            }
            DelayInvoker.DelayInvoke(this, 0.2f,
                (object[] args) =>
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(
                        Path,
                        (GameObject go) =>
                        {
                            if (go == null)
                            {
                                return;
                            }

                            if (root == null)
                            {
                                SGF.Debuger.LogWarning($"ObstacleEffect 异步加载完成后 父节点已被销毁 err");
                                return;
                            }

                            var gob = GameObject.Instantiate<GameObject>(go);
                            if (gob != null)
                            {
                                SetEffectGob(gob, Position, Rotation, Scale, root.transform);
                            }
                        });
                });
        }
        private void SetEffectGob(GameObject effect, Vector3 Position, Vector3 Rotation, Vector3 Scale,
            Transform Parent)
        {
            Effect = effect;
            Effect.transform.position = Position;
            Effect.transform.rotation = Quaternion.Euler(Rotation);
            Effect.transform.localScale = Scale;
            Effect.transform.SetParent(Parent);
            if (Effect != null)
            {
                Effect.SetActive(Actived && !isPlayWave && IsShowEffect);
            }
        }

        public void SetState(bool open)
        {
            Actived = open;
            if (Effect != null)
            {
                Effect.SetActive(Actived && !isPlayWave && IsShowEffect);
            }
        }
    }
}