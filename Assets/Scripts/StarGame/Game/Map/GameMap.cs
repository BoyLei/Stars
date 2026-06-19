using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game.Data; //这里统一，不再细分，战斗分不干净
using StarProject.Module;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarProject.Game.Map //这里统一，不再细分，战斗分不干净
{
    public enum E_DealMapType
    {
        Default,
        SameId,
        ChangeScene,
        CommonChangeWaveFB, //(可能切换UI也可能不切换）
    }

    /// <summary>
    /// 换场景，GameMap会拼装一个新的
    /// </summary>
    public class GameMap
    {
        //private string LOG_TAG = "[GameMap]";

        //===========================================================================

        private MapScript m_script;
        private GameObject m_view;

        public GameObject View
        {
            get { return m_view; }
        }

        private /*static*/ MapRootHelper m_rootHelper;

        /// <summary>
        /// 
        /// 换场景初始化会获得
        /// </summary>
        public /*static*/ MapRootHelper M_rootHelper
        {
            get { return m_rootHelper; }
        }

        private Vector3 m_size = Vector3.one;

        public Vector3 Size
        {
            get { return m_size; }
        }

        public float CurMimiMapScale = 1.0f;

        private MapCfgData mapCfgData;
        private MapBaseDataCell mapBaseDataCell;

        /// <summary>
        /// 场景地图数据，编辑器 data.json 数据
        /// </summary>
        private static SceneJsonData _sceneJsonData;

        public static SceneJsonData sceneJsonData
        {
            get { return _sceneJsonData; }
        }

        // 地图配置的AOI格子范围（米）
        public static float SceneJsonGridSize
        {
            get
            {
                if (_sceneJsonData != null)
                {
                    return (float)_sceneJsonData.GridSize / 100;
                }

                return 15f;
            }
        }

        private Dictionary<LogicType, IMapLogic> MapLogics = new()
        {
            { LogicType.Area, SceneAreaLogic.Create() },
            { LogicType.Obstacle, SceneObstacleLogic.Create() },
            { LogicType.SceneCamera, ActiveSceneCameraLogic.Create() },
        };

        private MapData mapData;
        string mapName = string.Empty;
        string pageName = "";
        public static bool SceneReady = false;
        public static bool DataReady = false;
        public static bool IsCreateFootprint = false;


        /// <summary>
        /// //物理算出来MapSize--->上下文--->赋给脚本
        /// 1，是否切换场景。
        /// 2，是否切换UI。
        /// 客户端设计：
        /// 切换地图必然切换场景：《所谓的副本》；或所谓的《场景切换》
        /// 什么都不做：《所谓的波纹副本》
        /// </summary>
        /// <param name="data">网络data，需要本地处理找地图</param>
        public void Load(MapData data, bool isChangeScene)
        {
            DataReady = false;
            SceneReady = false;
            SGF.Debuger.LogWarning($"初始登录 切地图------------------------------isChangeScene={isChangeScene}-----------------------111111111111111");

            if (isChangeScene)
            {
                mapName = string.Empty;
                mapCfgData = LocalDataManager.Instance.GetMapCfgData(data.MapID);
                if (mapCfgData == null)
                {
                    SGF.Debuger.LogWarning($"地图ID没找到配置 ID={data.MapID}");
                    return;
                }
                mapBaseDataCell = LocalDataManager.Instance.GetMapBaseDataCell(mapCfgData.MapBaseID);
                IsCreateFootprint = mapBaseDataCell.GetID() == GameConfig.FOOTPRINT_MAP_ID;
                // 播放BGM
                if (mapBaseDataCell != null && !string.IsNullOrEmpty(mapBaseDataCell.BGMName) &&
                    !string.IsNullOrWhiteSpace(mapBaseDataCell.BGMName))
                {
                    SoundManager.Instance.PlayEventBGMName(mapBaseDataCell.BGMName, null, null);
                }

                GameManager.Instance.SetCfgRotateCameraState(mapCfgData.IsCameraRotate);

                mapName = mapBaseDataCell.MapName;
                if (string.IsNullOrEmpty(mapName))
                {
                    return;
                }

                SGF.UI.Framework.UIManager.MainScene = /*"Map/" +*/ mapName; //"MainTown";//内部机制  [配置]
                /*SGF.UI.Framework.UIManager.MainPage = UIDef.StarWorldPage;*/
                SGF.UI.Framework.UIManager.MainPageSpaceType = data.SpaceType;
            }

            //===========================================================================================
            mapData = data;
            //===========================================================================================  StarWorld/StarWorldPage
            //string _newPage = LocalDataManager.Instance.GetUniPageName(mapData.MapID);
            string _newPage = UIDef.StarWorldPage;
            //无需关心老的，老的报错早就错了，本次约定熟成老的没问题，所以需要新的没问题且不同
            if (!string.IsNullOrEmpty(_newPage) && string.Compare(_newPage, pageName) != 0)
            {
                pageName = _newPage;
                SGF.UI.Framework.UIManager.MainPage = pageName; //UIDef.StarWorldPage;//相同的不会切换     [配置]
            }

            SGF.Debuger.LogWarning($"初始登录 切地图-----------------------------------------------------开始");
            GameManager.Instance.M_ChanageMapIsSuccess = false;
            //注册
            GlobalEvent.onSceneBeginChange?.Invoke(true);

            GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadSuccess);
            GlobalEvent.onSceneLoaded.AddListener(OnSceneLoadSuccess);
            //等异步
            SGF.UI.Framework.UIManager.Instance.EnterMainPage(ChangePageType.MeekFromFb); //这里两种副本类型都需要清理缓存目前先这样                                 
            //OpenPage(page)//不切换场景  //OpenPage(SceneAreaLogic,page)//切换场景，能回到上一个
        }

        public void SetHideEffect(bool hide)
        {
            if (MapLogics[LogicType.Obstacle] != null)
            {
                var logic = (SceneObstacleLogic)MapLogics[LogicType.Obstacle];
                if (logic != null)
                {
                    logic.SetHideEffect(hide);
                }
            }
        }

        // 切换分线设置地图数据
        public void ChanageLineSetMapData(ulong ServerID)
        {
            mapData.ServerID = ServerID;
            // 同地图切换分线，配置场景加载完成的回调直接触发
            GlobalEvent.OnSceneLoadedBinded?.Invoke(m_view.name);
            GlobalEvent.OnMapConfigLoaded?.Invoke(m_view.name);
            GlobalEvent.OnSceneMapConfigLoad?.Invoke(mapData.MapID);
        }

        public void ChanageLineSetMapServerID(ulong ServerID, int ServerIDShow)
        {
            mapData.ServerID = ServerID;
            mapData.ServerIDShow = ServerIDShow;
        }

        public void Unload()
        {
            foreach (var item in MapLogics)
            {
                item.Value.OnUnLoad();
            }

            m_script = null;
            if (m_view != null)
            {
                GameObject.Destroy(m_view);
                m_view = null;
            }

            GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadSuccess);
        }

        public void EnterFrame(int frameIndex)
        {
            if (m_script != null)
            {
                m_script.EnterFrame(frameIndex);
            }
        }

        public E_DealMapType CheckIsSameData(GameParam param, int newMapid, ProtoMsg.SpaceType spaceType)
        {
            E_DealMapType e_DealMapType = E_DealMapType.Default;
            //相同数据
            if (param.mapData.MapID == newMapid && param.mapData.SpaceType == spaceType)
            {
                return E_DealMapType.SameId;
            }

            var mapCfgData = LocalDataManager.Instance.GetMapCfgData(newMapid);
            if (mapCfgData == null)
            {
                SGF.Debuger.LogWarning($"地图ID没找到配置 ID={newMapid}");
                return E_DealMapType.Default;
            }

            MapBaseDataCell mapBaseDataCell = LocalDataManager.Instance.GetMapBaseDataCell(mapCfgData.MapBaseID);
            //不同数据，不同地图
            string __tempStringName = mapBaseDataCell.MapName;
            if (string.Compare( /*CurrentSceneName*/__tempStringName, mapName) != 0)
            {
                return E_DealMapType.ChangeScene;
            }

            //不同数据，相同地图，其他不同
            e_DealMapType = E_DealMapType.CommonChangeWaveFB;
            return e_DealMapType;
        }

        public void SetObstacleInfo(Google.Protobuf.Collections.MapField<int, bool> ObstacleInfo)
        {
            SceneObstacleLogic sceneObstacle = MapLogics[LogicType.Obstacle] as SceneObstacleLogic;
            if (sceneObstacle != null)
            {
                sceneObstacle.SetObstacleInfo(ObstacleInfo);
            }
        }

        /// <summary>
        /// 切地图和换场景都会来到这里
        /// </summary>
        /// <param name="loadedName"></param>
        /// <param name="sameScene">false 不同的场景才需要绑定</param>
        private void OnSceneLoadSuccess(string loadedName, bool sameScene = false)
        {
            //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 333333333333333 ");
            GlobalEvent.onSceneLoaded.RemoveListener(OnSceneLoadSuccess);

            //场景没加载完毕时就找物件，找不到
            if (sameScene == false || m_view == null)
            {
                //加载场景Gob
                //GameObject mapPrefab = ResourceManager.LoadAsset("Map/" + SceneMapDataCfg.MapNmae);
                m_view = GameObject.FindWithTag("GameMapRoot"); //GameObject.Find(mapName);
                if (m_view == null)
                {
                    /*m_view = GameObject.FindObjectsOfType<MapRootHelper>()[0].gameObject;*/
                    // 获取当前激活的场景
                    Scene currentScene = SceneManager.GetActiveScene();

                    // 遍历当前场景中的所有根节点
                    foreach (GameObject rootGameObject in currentScene.GetRootGameObjects())
                    {
                        // 对每个根节点进行处理，例如输出其名称
                        if (rootGameObject.name.Contains("Map_", System.StringComparison.Ordinal) && rootGameObject.GetComponent<MapRootHelper>() != null)
                        {
                            m_view = rootGameObject;
                        }
                    }
                }

                MapRootHelper mh;
                if (m_view != null)
                {
                    mh = m_view.GetComponent<MapRootHelper>(); //不可能多个挂脚本
                                                               //m_view = mh.gameObject; 
                                                               //m_view = mapPrefab;//GameObject.Instantiate(mapPrefab);
                    m_view.name = mapName; //"Map_" + mapData.MapID;
                    /*CurrentSceneName = m_view.name;//Perfab == SceneName*/
                    m_rootHelper = mh; //m_view.GetComponent<MapRootHelper>();                //前置挂载确认

                }


                if (mapBaseDataCell != null)
                {
                    Vector3 size = Vector3.zero;
                    size.x = mapBaseDataCell.GetMapWidth();
                    size.y = mapBaseDataCell.GetMapHeight();
                    m_size = size;
                    CurMimiMapScale = (float)mapBaseDataCell.GetMiniMapScale() / 100;
                }
                /*  size.x = m_rootHelper.Terrain.terrainData.size.x * m_view.transform.localScale.x;
                  size.y = m_rootHelper.Terrain.terrainData.size.y * m_view.transform.localScale.y;
                  size.z = m_rootHelper.Terrain.terrainData.size.z * m_view.transform.localScale.z;*/

                //Vector3 size = m_view.GetComponent<SpriteRenderer>().sprite.bounds.size;
                m_script = GameObjectUtils.EnsureComponent<MapScript>(m_view); //需后动态挂载
            }

            m_rootHelper.ClearDynamicCollider(); //只要切换，不论不同场景——相同场景，都清

            //登录状态记录，无所谓了。
            GlobalEvent.OnSceneLoadedBinded?.Invoke(m_view?.name);
            //没配置不加载=策划不想要；找不着不加载=bug
            if (mapCfgData != null)
            {
                if (!string.IsNullOrEmpty(mapCfgData.LevelCollider)) //【如果不是】没填，无法加载，不需要填【那就找】
                {
                    m_rootHelper.ShowHideCollider(mapCfgData.LevelCollider, true); //注意离开的时候是否要关闭，这要看每次加载的时候是找内存缓存，还是初始化的本地资源
                }
            }
            //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 4444444444444444 ");

            System.Action callBack = () =>
            {
                //Map加载成功->【Map配置加载成功】
                GlobalEvent.OnMapConfigLoaded?.Invoke(loadedName);
                SceneReady = true;
                // 定时器+0.17秒  1/60 = 0.0167，给他10帧
                //切换加载地图的时间最少是100ms。具体是100ms-无限长（需要切换场景的具体时长）；但是不能小于0.1秒
                //因为涉及到同地图切换，甚至0延迟，开启和关闭时间再一帧之内，即在两次指令输入（Update)频率之中，所以锁不上。
                //1便捷性，1网络性质，1同步和机制设定，1和客户端卡顿
                //一，如果交给server处理可以获得两次网络通讯时长的延迟，1先确定固定规则然后跟服务器发送锁定，2等结束后告诉服务器加载完毕再解开：没形成机制，影响体验，客户端能做
                //【目前】二，客户端锁只是临时锁是一种基于动态帧的锁定,减少网络延迟的问题，按照自己锁定，但是不可忽略卡顿问题
                //切换新场景，技能清理，原子锁锁定，状态重置（除了策划规划的buff等等），物理按键，新场景相当于重开始
                //DelayInvoker.DelayInvoke(0.17f,
                //        (object[] args) =>
                //        {
                //            m_ChanageMapSuccess = true;
                //            GameManager.Instance.UnRegisterMainPlayerClientBattleStates();
                //            SGF.Debuger.LogError($"状态切换 切地图----------------------------------------------------- 结束");
                //        }
                //    , new object[] { });
                //SGF.Debuger.LogWarning($"新手关新流程 时间={SGF.Time.TimeUtils.ServerNow} 切图 55555555555555 ");

                OnClientReady();

                GameManager.Instance.M_ChanageMapIsSuccess = true;
                SGF.Debuger.LogWarning($"初始登录 切地图----------------------------------------------------- 结束");
            };

            LoadMapJson(mapData.MapID, callBack); // 加载地图配置，通知配置中的信息

            //if (mapData.MapID == 20201)
            //{
            //    callBack();
            //}
            //else
            //{
            //    LoadMapJson(mapData.MapID, callBack); // 加载地图配置，通知配置中的信息
            //}

        }
        ScenePreLoading innerScenePreLoading = new();
        private void LoadMapJson(int mapID, System.Action cb)
        {

            string path = $"MapData/{mapID}/data";
            StarDebug.Log($" GameMap LoadMapJson:{path}");
            //#############################创建配置#######################################
            //_sceneJsonData = LocalDataManager.Instance.LoadCfgJsonSync<SceneJsonData>(path);
            var jsonAsset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(path);
            if (jsonAsset != null)
            {
                _sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(jsonAsset.text);
            }
            if (_sceneJsonData == null)
            {
                StarDebug.LogError($" GameMap LoadMapJson:{path}  _sceneJsonData==null");
                cb?.Invoke();
                return;
            }
            foreach (var item in MapLogics)
            {
                item.Value.OnLoad();
            }
            //场景配置文件加载完毕
            GlobalEvent.OnSceneMapConfigLoad?.Invoke(mapID);
            DataReady = true;
            ///
            //OnClientReady();

            cb?.Invoke();

            innerScenePreLoading.End();
            // 新手关 10004 提前预加载
            if (mapID == 10004)
            {
                innerScenePreLoading.Start(sceneJsonData);
            }


            // StarProject.Service.Resource.ResourceManager.Instance.LoadAssetAsync<TextAsset>(path, (TextAsset textasset, int index, object obj) =>
            // {
            //     _sceneJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(textasset.text);

            //     foreach (var item in MapLogics)
            //     {
            //         item.Value.OnLoad();
            //     }

            //     //清除所有服务
            //     Service.ServerService.ServerServiceManager.Instance.ClearAllService();

            //     //场景配置文件加载完毕
            //     GlobalEvent.OnSceneMapConfigLoad?.Invoke(mapID);

            //     DataReady = true;
            //     ///
            //     OnClientReady();
            // });
        }

        private void OnClientReady()
        {
            StarWorldModule starWorld = ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule) as StarWorldModule;
            if (starWorld != null)
            {
                starWorld.OnClientReady();
            }
        }

        public int GetMapId()
        {
            int mapId = -1;
            if (mapData != null)
            {
                mapId = mapData.MapID;
            }

            return mapId;
        }

        public ulong GetSpaceID()
        {
            ulong SpaceId = 0;
            if (mapData != null)
            {
                SpaceId = mapData.SpaceId;
            }

            return SpaceId;
        }

        public ulong GetServerID()
        {
            ulong ServerID = 0;
            if (mapData != null)
            {
                ServerID = mapData.ServerID;
            }

            return ServerID;
        }

        public int GetServerIDShow()
        {
            int ServerID = 0;
            if (mapData != null)
            {
                ServerID = mapData.ServerIDShow;
            }

            return ServerID;
        }

        public string GetSceneLevelName()
        {
            string name = string.Empty;
            if (mapCfgData != null)
            {
                name = mapCfgData.MapName;
            }

            return name;
        }

        public string GetMiniMapPath()
        {
            string path = string.Empty;
            if (mapBaseDataCell != null)
            {
                path = mapBaseDataCell.MiniMap;
            }

            return path;
        }

        public int GetBaseMapId()
        {
            int mapId = -1;
            if (mapCfgData != null)
            {
                mapId = mapCfgData.MapBaseID;
            }

            return mapId;
        }

        public ProtoMsg.SpaceType GetMapType()
        {
            ProtoMsg.SpaceType spaceType = ProtoMsg.SpaceType.SpaceScene;
            if (mapData != null)
            {
                spaceType = mapData.SpaceType;
            }

            return spaceType;
        }

        public string GetMapSubType()
        {
            string subType = GameConfig.INSTANCE_NORMAL;
            if (mapCfgData != null)
            {
                subType = mapCfgData.SubType;
            }

            return subType;
        }

        public bool GetMapIsHidePartner()
        {
            bool isHidePartner = false;
            if (mapCfgData != null)
            {
                isHidePartner = mapCfgData.IsHidePartner;
            }

            return isHidePartner;
        }

        public bool GetMapIsCameraRotate()
        {
            bool isCameraRotate = true;
            if (mapCfgData != null)
            {
                isCameraRotate = mapCfgData.IsCameraRotate;
            }

            return isCameraRotate;
        }

        public bool GetMapIsCameraTeleport()
        {
            bool isCameraTeleport = false;
            if (mapCfgData != null)
            {
                isCameraTeleport = mapCfgData.IsCameraTeleport;
            }

            return isCameraTeleport;
        }

    }
}