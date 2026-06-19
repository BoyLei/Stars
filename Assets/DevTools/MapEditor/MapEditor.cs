// ReSharper disable once InvalidXmlDocComment
///--------------------------------------------------------------------
/// 文件名   :   MapEditor
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 18:10:28
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace MapEditor
{
    public class MapEditor : EditorWindow
    {
        private Dictionary<string, MapEditorBaseView> ChildViews = new Dictionary<string, MapEditorBaseView>()
        {
            { "Spawners", new MapEditorSpawnerView("Spawners", ShowMessage) },
            { "Trigger", new MapEditorTriggerView("Trigger", ShowMessage) },
            { "Areas", new MapEditorAreaView("Areas", ShowMessage) },
            { "Paths", new MapEditorPathView("Paths", ShowMessage) },
            { "Obstacles", new MapEditorObstacleView("Obstacles", ShowMessage) },
            { "RandomMonster", new MapEditorRandomMonsterView("RandomMonster", ShowMessage) },
            { "MainPlayer", new MapEditorMainPlayerView("MainPlayer", ShowMessage) },
            { "GVEBoss", new MapEditorGVEBossView("GVEBoss", ShowMessage) },
            { "WantedTask", new MapEditorWantedTaskView("WantedTask", ShowMessage) },
            { "VirtualCamera", new MapEditorVirtualCameraView("VirtualCamera", ShowMessage) },
            { "Monsters", new MapEditorMonsterView("Monsters", ShowMessage) },
            { "Mines", new MapEditorMineView("Mines", ShowMessage) },
            { "Npcs", new MapEditorNpcView("Npcs", ShowMessage) }

        };

        private GameObject mSceneObject;
        private GameObject mNavMeshObject;
        private GameObject mRootObj;
        private GameObject mCameraRoot;
        private GameObject CameraRoot;
        private Vector3 CameraPos;
        private int _SceneID;

        //*****************GUI相关*********************************/
        private int SceneID;
        private bool foldScene = true;
        private bool foldCreate = true;
        private bool foldSave = true;
        private bool UpdateCamera = true;
        private bool svnState = true;
        private bool serverfold = true;
        private bool IsLoad = false;
        private bool PrefabIsNew = false;
        private bool UseInput = false;
        private int SceneIndex = 0;
        private int DungeonIndex = 0;
        private Vector2 scrollView = Vector2.zero;
        private string MessageTips = "      1.Ctrl + 鼠标右键弹出菜单栏 \n 2、编辑器前请先锁定\n";
        private int TabIndex = 0;
        private string newDir;
        private Color Discolor = Color.white;
        private string MapName = string.Empty;

        private Vector3 postion;

        private Event e;
        private GenericMenu menu;
        private string[] Tabs = new string[] { "场景", "副本" };


        //**********************静态变量*************************/
        private static string serverJsonDir;
        private static string SaveDir = "Assets/DevTools/MapEditor/Prefabs/";

        private static string SaveVirDir = "Assets/Res/TimeLine/Map/";

        //********************************************************/
        static private MapEditor mapEditor;
        Scene EditorScene;

        [MenuItem("Tools/场景编辑器")]
        static public void OpenWindow()
        {
            mapEditor = GetWindow<MapEditor>("MapEditor");
            mapEditor.Show();
        }

        void OnEnable()
        {
            string path = "Assets/DevTools/MapEditor/MapEditor.unity";
            menu = new GenericMenu();
            if (EditorSceneManager.GetActiveScene().path != path)
            {
                EditorScene = EditorSceneManager.OpenScene(path /*, OpenSceneMode.Single*/);
                //InvalidOperationException: Calling OpenScene Raisefrom assembly reloading callbacks are not supported.
            }
            //1，Default物件不能编辑。
            //  MapEditorUtils.LockLayer(LayerMask.NameToLayer("Default"));

            serverJsonDir = PlayerPrefs.GetString("serverJsonDir", Application.dataPath);
            EditorConfigUtils.Init();
            if (EditorConfigUtils.SceneCfgs != null && EditorConfigUtils.SceneCfgs.SceneMenus != null &&
                EditorConfigUtils.SceneCfgs.SceneMenus.Length > 0)
            {
                SceneID = EditorConfigUtils.SceneCfgs.SceneIDs[0];
            }

            CameraRoot = GameObject.Find("CameraRoot");
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            MapEditorUtils.UnLockLayer(LayerMask.NameToLayer("Default"));
        }

        public void Load(bool force = false)
        {
            if (!force)
            {
                if (SceneID == _SceneID)
                {
                    return;
                }
            }

            ClearScene(force);

            string path = string.Empty;
            string navMeshPath = string.Empty;
            // EditorConfigUtils.MonsterExcel.Init(SceneID);
            if (!EditorConfigUtils.SceneCfgs.Scenes.TryGetValue(SceneID, out var data) || data == null)
            {
                if (!EditorConfigUtils.LevelCfgs.Scenes.TryGetValue(SceneID, out var leveldata) || leveldata == null)
                {
                    ShowNotification(new GUIContent($"读取场景失败,{SceneID}"));
                    return;
                }
                else
                {
                    path = string.Format("Assets/Res/Map/{0}.prefab", leveldata.MapName);
                    navMeshPath = string.Format("Assets/Res/Map/{0}/NavMesh.asset", leveldata.MapName);
                }
            }
            else
            {
                path = string.Format("Assets/Res/Map/{0}.prefab", data.MapName);
                navMeshPath = string.Format("Assets/Res/Map/{0}/NavMesh.asset", data.MapName);
            }

            var sceneobj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (sceneobj == null)
            {
                ShowNotification(new GUIContent($"读取场景失败加载不到资源,{path}"));
                return;
            }

            mSceneObject = GameObject.Instantiate(sceneobj);
            mSceneObject.transform.position = Vector3.zero;
            mSceneObject.transform.rotation = Quaternion.identity;
            mSceneObject.transform.localScale = Vector3.one;

            var navMeshobj = AssetDatabase.LoadAssetAtPath<NavMeshData>(navMeshPath);
            if (navMeshobj == null)
            {
                ShowNotification(new GUIContent($"读取场景navMesh失败加载不到资源,{navMeshPath}"));
            }
            else
            {
                mNavMeshObject = new("NavMesh");
                mNavMeshObject.SetActive(false);
                mNavMeshObject.transform.position = Vector3.zero;
                mNavMeshObject.transform.rotation = Quaternion.identity;
                mNavMeshObject.transform.localScale = Vector3.one;
                mNavMeshObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontSave;
                var navMeshSurface = mNavMeshObject.AddComponent<NavMeshSurface>();
                navMeshSurface.navMeshData = navMeshobj;
                mNavMeshObject.SetActive(true);
            }

            //2，场景内物件不能操作也看不到。
            //mSceneObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontSave;
            MapSceneConfig sceneConfig = null;


            //SaveDir
            /*var mRootObjbase = AssetDatabase.LoadAssetAtPath<GameObject>(SaveDir + SceneID + ".prefab");
            if (mRootObjbase == null)
            {
                mRootObj = new GameObject(SceneID.ToString());
                PrefabIsNew = true;
            }
            else
            {
                mRootObj = (GameObject)PrefabUtility.InstantiateAttachedAsset(mRootObjbase);
            }

            mRootObj.transform.position = Vector3.zero;
            mRootObj.transform.rotation = Quaternion.identity;
            mRootObj.transform.localScale = Vector3.one;*/

            /*sceneConfig = mRootObj.GetComponent<MapSceneConfig>();
            if (sceneConfig == null)
            {
                sceneConfig = mRootObj.AddComponent<MapSceneConfig>();
            }*/


            _SceneID = SceneID;
            Transform SceneRoot = null;
            var SceneRootObj = GameObject.Find(SceneID.ToString());
            if (SceneRootObj != null)
            {
                SceneRoot = SceneRootObj.transform;
            }

            if (SceneRoot == null)
            {
                SceneRoot = MapEditorUtils.CreatePort(_SceneID.ToString(), null);
            }


            mRootObj = SceneRoot.gameObject;
            sceneConfig = mRootObj.AddComponent<MapSceneConfig>();
            MapEditorUtils.SetMapSceneConfig(sceneConfig);
            var jsonData = CreateMapObject(_SceneID);
            var camera = (MapEditorVirtualCameraView)ChildViews["VirtualCamera"];
            if (jsonData != null)
            {
                if (!string.IsNullOrEmpty(jsonData.Tag))
                {
                    sceneConfig.tag = jsonData.Tag;
                }

                sceneConfig.GridSize = jsonData.GridSize;
                sceneConfig.IsFixedBlock = jsonData.IsFixedBlock;
                sceneConfig.IsHidePartner = jsonData.IsHidePartner;
                sceneConfig.IsHideOtherPlayer = jsonData.IsHideOtherPlayer;
            }




            /*var CameraRoot=SceneRoot.Find($"Camera_{SceneID}");
            if (CameraRoot == null)
            {
                CameraRoot = MapEditorUtils.CreatePort($"Camera_{SceneID}",SceneRoot);
            }*/
            //camera.Load(jsonData, null);

            var mCameraRootbase = AssetDatabase.LoadAssetAtPath<GameObject>(SaveVirDir + $"Camera_{SceneID}.prefab");
            if (mCameraRootbase == null)
            {
                mCameraRoot = new GameObject($"Camera_{SceneID}");
                mCameraRoot.transform.position = Vector3.zero;
                mCameraRoot.transform.rotation = Quaternion.identity;
                mCameraRoot.transform.localScale = Vector3.one;
            }
            else
            {
                mCameraRoot = (GameObject)PrefabUtility.InstantiateAttachedAsset(mCameraRootbase);
            }

            camera.OnLoad(mCameraRoot);
            foreach (var view in ChildViews)
            {
                if (view.Key == "VirtualCamera")
                {
                    continue;
                }

                view.Value.Load(jsonData, SceneRoot);
            }
            IsLoad = true;
        }

        private SceneJsonData CreateMapObject(int sceneID)
        {
            string path = $"{serverJsonDir}/{sceneID}/data.json";

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(text);
                return json;
            }

            return null;
        }

        public void Save()
        {
            Save(_SceneID);
        }

        private void Commit()
        {
            Commit(SceneID);
        }

        private void ExportJson()
        {
            ExportJson(_SceneID);
        }


        static public void ShowMessage(string message)
        {
            if (mapEditor != null)
                mapEditor.ShowNotification(new GUIContent(message));
        }

        void OnGUI()
        {
            newDir = string.Empty;
            EditorGUILayout.HelpBox(MessageTips, MessageType.Info);
            scrollView = EditorGUILayout.BeginScrollView(scrollView, "box");

            GUILayout.BeginVertical("box");
            serverfold = EditorGUILayout.Foldout(serverfold, "服务器数据");
            if (serverfold)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("服务器Json格式存储目录", serverJsonDir);
                if (GUILayout.Button("选择"))
                {
                    newDir = EditorUtility.OpenFolderPanel("存储目录", "Open Folder", "");
                    if (!string.IsNullOrEmpty(newDir))
                    {
                        serverJsonDir = newDir;
                        PlayerPrefs.SetString("serverJsonDir", serverJsonDir);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(20);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            foldScene = EditorGUILayout.Foldout(foldScene, "场景信息");
            if (foldScene)
            {
                int newTabIndex = GUILayout.Toolbar(TabIndex, Tabs);
                if (newTabIndex != TabIndex)
                {
                    TabIndex = newTabIndex;
                    if (TabIndex == 0)
                    {
                        SceneID = EditorConfigUtils.SceneCfgs.SceneIDs[SceneIndex];
                    }
                    else
                    {
                        SceneID = EditorConfigUtils.SceneCfgs.SceneIDs[DungeonIndex];
                    }
                }

                GUILayout.BeginHorizontal("box");
                Discolor = GUI.color;
                MapName = string.Empty;
                if (TabIndex == 0)
                {
                    GUI.color = Color.green;
                    if (UseInput)
                    {
                        SceneID = EditorGUILayout.IntField("场景ID", SceneID);
                        if (EditorConfigUtils.SceneCfgs.Scenes.ContainsKey(SceneID))
                        {
                            MapName = EditorConfigUtils.SceneCfgs.Scenes[SceneID].MapName;
                        }
                    }
                    else
                    {
                        if (EditorConfigUtils.SceneCfgs != null && EditorConfigUtils.SceneCfgs.SceneMenus != null &&
                            EditorConfigUtils.SceneCfgs.SceneMenus.Length > 0)
                        {
                            int newIndex = EditorGUILayout.Popup("场景ID", SceneIndex,
                                EditorConfigUtils.SceneCfgs.SceneMenus);
                            if (newIndex != SceneIndex)
                            {
                                SceneIndex = newIndex;
                            }

                            SceneID = EditorConfigUtils.SceneCfgs.SceneIDs[SceneIndex];
                            if (EditorConfigUtils.SceneCfgs.Scenes.ContainsKey(SceneID))
                            {
                                MapName = EditorConfigUtils.SceneCfgs.Scenes[SceneID].MapName;
                            }
                        }
                    }
                }
                else
                {
                    GUI.color = Color.yellow;

                    if (UseInput)
                    {
                        SceneID = EditorGUILayout.IntField("副本ID", SceneID);
                        if (EditorConfigUtils.LevelCfgs.Scenes.ContainsKey(SceneID))
                        {
                            MapName = EditorConfigUtils.LevelCfgs.Scenes[SceneID].MapName;
                        }
                    }
                    else
                    {
                        if (EditorConfigUtils.LevelCfgs != null && EditorConfigUtils.LevelCfgs.SceneMenus != null &&
                            EditorConfigUtils.LevelCfgs.SceneMenus.Length > 0)
                        {
                            int newIndex = EditorGUILayout.Popup("副本ID", DungeonIndex,
                                EditorConfigUtils.LevelCfgs.SceneMenus);
                            if (newIndex != DungeonIndex)
                            {
                                DungeonIndex = newIndex;
                            }

                            SceneID = EditorConfigUtils.LevelCfgs.SceneIDs[DungeonIndex];
                            if (EditorConfigUtils.LevelCfgs.Scenes.ContainsKey(SceneID))
                            {
                                MapName = EditorConfigUtils.LevelCfgs.Scenes[SceneID].MapName;
                            }
                        }
                    }
                }

                if (GUILayout.Button("读取"))
                {
                    Load();
                    SceneNameListConfig sceneNameListConfig = SceneOpenListener.GetSceneNameListConfig();
                    SceneOpenListener.OnSceneOpened(EditorScene, OpenSceneMode.Single, sceneNameListConfig);
                    //1，Default物件不能编辑。
                    MapEditorUtils.LockLayer(LayerMask.NameToLayer("Default"));
                    //2，场景内物件不能操作也看不到。
                    mSceneObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontSave;
                    //mSceneObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"场景名称:{MapName}");
                if (GUILayout.Button("重新载入"))
                {
                    Load(true);
                }

                GUI.color = Discolor;
                GUILayout.BeginHorizontal();


                UpdateCamera = EditorGUILayout.Toggle("自动更新摄像机", UpdateCamera);
                UseInput = EditorGUILayout.Toggle("是否使用输入框", UseInput);
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
            }

            GUILayout.EndVertical();

            if (IsLoad)
            {
                GUILayout.BeginVertical("box");
                foldCreate = EditorGUILayout.Foldout(foldCreate, "创建信息");
                if (foldCreate)
                {
                    GUILayout.BeginVertical("box");
                    foreach (var view in ChildViews)
                    {
                        view.Value.OnGUI();
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(20);
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                foldSave = EditorGUILayout.Foldout(foldSave, "存储信息");
                if (foldSave)
                {
                    if (GUILayout.Button("保存"))
                    {
                        Save();
                    }

                    if (GUILayout.Button("提交"))
                    {
                        Commit();
                    }
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }

        private void Commit(object userdata)
        {
            string cJsonPath = Application.dataPath + $"/Res/MapData/{SceneID}";
            bool commitJson = System.IO.Directory.Exists(cJsonPath);
            if (!commitJson)
            {
                Debug.LogError($"请先导出 {cJsonPath}/data.json");
            }

            if (PrefabIsNew)
            {

                if (commitJson)
                {
                    MapEditorUtils.RunBat(EditorConfigUtils.TortoiseProc,
                        string.Format($"/command:add /path:{cJsonPath} --force -m 新增 客户端Json{cJsonPath}"));
                }
            }
            if (commitJson)
            {
                MapEditorUtils.RunBat(EditorConfigUtils.TortoiseProc,
                    string.Format($"/command:commit /path:{cJsonPath} -m 修改 {cJsonPath}"));
            }
        }


        private void SavePrefabs(int SceneID)
        {
            string sceneCamPath = SaveVirDir + $"Camera_{SceneID}.prefab";
            bool prefabCamSuccess;
            PrefabUtility.SaveAsPrefabAsset(mCameraRoot, sceneCamPath, out prefabCamSuccess);

            if (prefabCamSuccess)
                Debug.Log("Prefab was saved successfully");
            else
                Debug.Log("Prefab failed to save" + prefabCamSuccess);

            ShowNotification(new GUIContent(prefabCamSuccess ? $"储存{sceneCamPath}成功" : $"储存{sceneCamPath}失败"));
        }

        private void Save(object userdata)
        {
            ExportJson(userdata);
            SavePrefabs(_SceneID);
            //WriteExcel(null);

            ExportMapTextStrByMapEditorSave();
        }

        private void ExportJson(object userdata)
        {
            string dir = serverJsonDir + $"/{_SceneID}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string dir1 = Application.dataPath + "/Res/MapData" + $"/{_SceneID}";
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
            }

            StringBuilder stringBuilder = new StringBuilder();
            SceneJsonData sceneJson = new SceneJsonData();
            sceneJson.SceneID = _SceneID;
            if (sceneJson.SceneID <= 0)
            {
                stringBuilder.AppendLine("地图ID 不能小于等于0");
            }

            var sceneConfig = mRootObj.GetComponent<MapSceneConfig>();
            if (sceneConfig != null)
            {
                sceneJson.GridSize = sceneConfig.GridSize;
                if (sceneJson.GridSize <= 0)
                {
                    stringBuilder.AppendLine("地图格子大小不能小于等于0");
                }
                sceneJson.Tag = sceneConfig.Tag;
                sceneJson.IsFixedBlock = sceneConfig.IsFixedBlock;
                sceneJson.IsHidePartner = sceneConfig.IsHidePartner;
                sceneJson.IsHideOtherPlayer = sceneConfig.IsHideOtherPlayer;
            }

            foreach (var view in ChildViews)
            {
                view.Value.ExportJson(sceneJson);
            }

            if (stringBuilder.Length > 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("错误", stringBuilder.ToString(), "ok");
                return;
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(sceneJson, setting);

            // string jsonstr = JsonUtility.ToJson(sceneJson);

            jsonstr = ConvertJsonString(jsonstr);

            //导出给服务器
            string filePath = dir + "/data.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllText(filePath, jsonstr);

            //导出给客户端
            string filePath1 = dir1 + "/data.json";
            if (File.Exists(filePath1))
            {
                File.Delete(filePath1);
            }

            File.WriteAllText(filePath1, jsonstr);
            AssetDatabase.Refresh();
            ShowNotification(new GUIContent("导出Json成功" + filePath));
        }

        static public string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public static void ExportAllJson()
        {
            serverJsonDir = PlayerPrefs.GetString("serverJsonDir", Application.dataPath);

            List<string> paths = new List<string>();
            string[] SubFolders = System.IO.Directory.GetFiles(SaveDir);
            foreach (var item in SubFolders)
            {
                if (item.EndsWith(".meta"))
                {
                    continue;
                }

                paths.Add(item);
            }

            EditorUtility.ClearProgressBar();

            for (int i = 0; i < paths.Count; i++)
            {
                var mRootObjbase = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (mRootObjbase == null)
                {
                    continue;
                }

                var RootObj = GameObject.Instantiate(mRootObjbase);
                if (RootObj == null)
                {
                    continue;
                }

                string mSceneID = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
                ExportJson(RootObj, mSceneID);
                GameObject.DestroyImmediate(RootObj);
                // EditorUtility.DisplayProgressBar("导出Json", "加速导出中", i * 1.0f / (paths.Count - 1));
            }

            EditorUtility.ClearProgressBar();
            //ShowNotification(new GUIContent("全部导出Json成功"));
        }

        private static void ExportJson(GameObject RootObj, string mSceneID)
        {
            string dir = serverJsonDir + $"/{mSceneID}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string dir1 = Application.dataPath + "/Res/MapData" + $"/{mSceneID}";
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
            }

            SceneJsonData sceneJson = new SceneJsonData();
            sceneJson.SceneID = System.Int32.Parse(mSceneID);


            Monster[] monsters = RootObj.GetComponentsInChildren<Monster>();
            if (monsters != null && monsters.Length > 0)
            {
                int index = 1;
                foreach (var item in monsters)
                {
                    sceneJson.Monsters.Add(index++, new MonsterJsonData(index, item));
                }
            }

            NPC[] npcs = RootObj.GetComponentsInChildren<NPC>();
            if (npcs != null && npcs.Length > 0)
            {
                foreach (var item in npcs)
                {
                    sceneJson.Npcs.Add(item.Index, new NPCJsonData(item.Index, item));
                }
            }


            Area[] areas = RootObj.GetComponentsInChildren<Area>();
            if (areas != null && areas.Length > 0)
            {
                int index = 1;
                foreach (var item in areas)
                {
                    sceneJson.Areas.Add(item.AreaID, new AreaJsonData(index++, item));
                }
            }

            Path[] paths = RootObj.GetComponentsInChildren<Path>();
            if (paths != null && paths.Length > 0)
            {
                int index = 1;
                foreach (var item in paths)
                {
                    sceneJson.Paths.Add(item.PathID, new PathJsonData(index++, item));
                }
            }


            ObstacleGroup[] obstacles = RootObj.GetComponentsInChildren<ObstacleGroup>();
            if (obstacles != null && obstacles.Length > 0)
            {
                int index = 1;
                foreach (var item in obstacles)
                {
                    sceneJson.Obstacles.Add(item.ID, new ObstacleGroupJsonData(index++, item));
                }
            }

            TrrigerBase[] trrigers = RootObj.GetComponentsInChildren<TrrigerBase>();
            if (trrigers != null && trrigers.Length > 0)
            {
                foreach (var item in trrigers)
                {
                    if (sceneJson.Triggers.ContainsKey(item.ID))
                    {
                        Debug.LogError($"触发器ID重复  {item.ID}");
                        continue;
                    }

                    sceneJson.Triggers.Add(item.ID, new TriggerJsonData(item));
                }
            }

            Spawner[] spawners = RootObj.GetComponentsInChildren<Spawner>();
            if (spawners != null && spawners.Length > 0)
            {
                int index = 1;
                foreach (var item in spawners)
                {
                    sceneJson.Spawners.Add(item.SpawnerID, new SpawnerJsonData(index++, item));
                }
            }

            RandomMonster[] randomMonsters = RootObj.GetComponentsInChildren<RandomMonster>();
            if (randomMonsters != null && randomMonsters.Length > 0)
            {
                int index = 1;
                foreach (var item in randomMonsters)
                {
                    sceneJson.RandomMonsters.Add(item.IndexID, new RandomMonsterJsonData(index++, item));
                }
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(sceneJson, setting);

            jsonstr = ConvertJsonString(jsonstr);

            string filePath = dir + "/data.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllText(filePath, jsonstr);

            string filePath1 = dir1 + "/data.json";
            if (File.Exists(filePath1))
            {
                File.Delete(filePath1);
            }

            File.WriteAllText(filePath1, jsonstr);
        }

        IEnumerator ExportAllJsonAsync()
        {
            List<string> paths = new List<string>();
            string[] SubFolders = System.IO.Directory.GetFiles(SaveDir);
            foreach (var item in SubFolders)
            {
                if (item.EndsWith(".meta"))
                {
                    continue;
                }

                paths.Add(item);
            }

            EditorUtility.ClearProgressBar();

            for (int i = 0; i < paths.Count; i++)
            {
                var mRootObjbase = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (mRootObjbase == null)
                {
                    continue;
                }

                var RootObj = GameObject.Instantiate(mRootObjbase);
                if (RootObj == null)
                {
                    continue;
                }

                string mSceneID = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
                yield return ExportJsonAsync(RootObj, mSceneID);
                GameObject.DestroyImmediate(RootObj);
                EditorUtility.DisplayProgressBar("导出Json", "加速导出中", i * 1.0f / (paths.Count - 1));
            }

            yield return null;
            EditorUtility.ClearProgressBar();
            ShowNotification(new GUIContent("全部导出Json成功"));
        }

        IEnumerator ExportJsonAsync(GameObject RootObj, string mSceneID)
        {
            string dir = serverJsonDir + $"/{mSceneID}";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            SceneJsonData sceneJson = new SceneJsonData();
            sceneJson.SceneID = System.Int32.Parse(mSceneID);

            sceneJson.Monsters.Clear();
            sceneJson.Mines.Clear();

            Monster[] monsters = RootObj.GetComponentsInChildren<Monster>();
            if (monsters != null && monsters.Length > 0)
            {
                int index = 1;
                foreach (var item in monsters)
                {
                    sceneJson.Monsters.Add(index++, new MonsterJsonData(index, item));
                }
            }

            NPC[] npcs = RootObj.GetComponentsInChildren<NPC>();
            if (npcs != null && npcs.Length > 0)
            {
                foreach (var item in npcs)
                {
                    sceneJson.Npcs.Add(item.Index, new NPCJsonData(item.Index, item));
                }
            }

            Mine[] mines = RootObj.GetComponentsInChildren<Mine>();
            if (mines != null && mines.Length > 0)
            {
                foreach (var item in mines)
                {
                    sceneJson.Mines.Add(item.Index, new MineJsonData(item.Index, item));
                }
            }

            Area[] areas = RootObj.GetComponentsInChildren<Area>();
            if (areas != null && areas.Length > 0)
            {
                int index = 1;
                foreach (var item in areas)
                {
                    sceneJson.Areas.Add(item.AreaID, new AreaJsonData(index++, item));
                }
            }

            Path[] paths = RootObj.GetComponentsInChildren<Path>();
            if (paths != null && paths.Length > 0)
            {
                int index = 1;
                foreach (var item in paths)
                {
                    sceneJson.Paths.Add(item.PathID, new PathJsonData(index++, item));
                }
            }

            ObstacleGroup[] obstacles = RootObj.GetComponentsInChildren<ObstacleGroup>();
            if (obstacles != null && obstacles.Length > 0)
            {
                int index = 1;
                foreach (var item in obstacles)
                {
                    sceneJson.Obstacles.Add(item.ID, new ObstacleGroupJsonData(index++, item));
                }
            }

            TrrigerBase[] trrigers = RootObj.GetComponentsInChildren<TrrigerBase>();
            if (trrigers != null && trrigers.Length > 0)
            {
                foreach (var item in trrigers)
                {
                    if (sceneJson.Triggers.ContainsKey(item.ID))
                    {
                        Debug.LogError($"触发器ID重复  {item.ID}");
                        continue;
                    }

                    sceneJson.Triggers.Add(item.ID, new TriggerJsonData(item));
                }
            }

            Spawner[] spawners = RootObj.GetComponentsInChildren<Spawner>();
            if (spawners != null && spawners.Length > 0)
            {
                int index = 1;
                foreach (var item in spawners)
                {
                    sceneJson.Spawners.Add(item.SpawnerID, new SpawnerJsonData(index++, item));
                }
            }

            RandomMonster[] randomMonsters = RootObj.GetComponentsInChildren<RandomMonster>();
            if (randomMonsters != null && randomMonsters.Length > 0)
            {
                int index = 1;
                foreach (var item in randomMonsters)
                {
                    sceneJson.RandomMonsters.Add(item.IndexID, new RandomMonsterJsonData(index++, item));
                }
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(sceneJson, setting);

            jsonstr = ConvertJsonString(jsonstr);

            string filePath = dir + "/data.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllText(filePath, jsonstr);
            yield return null;
        }

        void OnDestroy()
        {
            EditorConfigUtils.Destory();
            SceneView.duringSceneGui -= OnSceneGUI;
            ClearScene();
        }

        private void ClearScene(bool skipSave = false)
        {
            if (!skipSave)
            {
                if (mSceneObject != null && mRootObj != null)
                {
                    if (EditorSceneManager.GetActiveScene().path == "Assets/DevTools/MapEditor/MapEditor.unity")
                    {
                        if (EditorUtility.DisplayDialog("提示", "窗口即将关闭，是否自动存储", "确定", "取消"))
                        {
                            Save();
                        }
                    }
                }
            }

            if (mSceneObject != null)
            {
                GameObject.DestroyImmediate(mSceneObject);
            }

            if (mRootObj != null)
            {
                GameObject.DestroyImmediate(mRootObj);
            }

            if (mNavMeshObject != null)
            {
                GameObject.DestroyImmediate(mNavMeshObject);
            }

            //清理场景缓存数据 除了摄像机 和 灯光
            var scene = EditorSceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            foreach (var item in gameObjects)
            {
                if (item.name == "CameraRoot")
                {
                    continue;
                }

                if (item.name == "Directional Light")
                {
                    continue;
                }

                GameObject.DestroyImmediate(item);
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void OnUpdateCamera()
        {
            if (UpdateCamera)
            {
                if (MapEditorUtils.GetScreenPosition(out CameraPos))
                {
                    CameraRoot.transform.position = CameraPos;
                    //CameraRoot.transform.rotation = Quaternion.Euler(new Vector3(0, SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles.y, 0));
                }
                //mCamera.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                // mCamera.fieldOfView = SceneView.lastActiveSceneView.camera.fieldOfView;
                // mCamera.nearClipPlane = SceneView.lastActiveSceneView.camera.nearClipPlane;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            e = Event.current;
            if (e != null)
            {
                OnUpdateCamera();
                if (e.control && e.button == 1 && e.type == EventType.MouseUp)
                {
                    //右键单击啦，在这里显示菜单
                    menu.AddItem(new GUIContent("载入场景/重新载入场景"), false, ReLoadScene, null);
                    foreach (var item in EditorConfigUtils.SceneCfgs.SceneIDs)
                    {
                        menu.AddItem(new GUIContent(string.Format("载入场景/{0}", item)), false, LoadScene, item);
                    }

                    postion = Vector3.zero;
                    bool suc = MapEditorUtils.MousePosition(sceneView, out postion);
                    foreach (var item in EditorConfigUtils.MonsterCfgs.Monsters)
                    {
                        menu.AddItem(new GUIContent(string.Format("布怪/{0}", item.Key)), false,
                            ChildViews["Monsters"].CreateChild, new MonsterEditorData(postion, item.Value, 0, suc));
                    }

                    foreach (var item in EditorConfigUtils.NpcCfgs.Npcs)
                    {
                        menu.AddItem(new GUIContent(string.Format("布NPC/{0}", item.Key)), false,
                            ChildViews["Npcs"].CreateChild,
                            new NPCEditorData(postion, item.Value, 0, suc));
                    }

                    foreach (var item in EditorConfigUtils.MineCfgs.Mines)
                    {
                        menu.AddItem(new GUIContent(string.Format("布矿/{0}", item.Key)), false,
                            ChildViews["Mines"].CreateChild,
                            new MineEditorData(postion, item.Value, 0, suc));
                    }

                    menu.AddItem(new GUIContent("区域/矩形"), false, ChildViews["Areas"].CreateChild,
                        new AreaEditorData(postion, AreaShape.Rectangle, suc));
                    menu.AddItem(new GUIContent("区域/圆形"), false, ChildViews["Areas"].CreateChild,
                        new AreaEditorData(postion, AreaShape.Circle, suc));
                    menu.AddItem(new GUIContent("区域/多边形"), false, ChildViews["Areas"].CreateChild,
                        new AreaEditorData(postion, AreaShape.Polygon, suc));
                    menu.AddItem(new GUIContent("路径"), false, ChildViews["Paths"].CreateChild,
                        new PathEditorData(postion, suc));
                    menu.AddItem(new GUIContent("Spawner"), false, ChildViews["Spawners"].CreateChild,
                        new SpawnerEditorData(postion, suc));
                    menu.AddItem(new GUIContent("触发器"), false, ChildViews["Trigger"].CreateChild,
                        new TriggerEditorData(postion, suc));

                    menu.AddItem(new GUIContent("动态阻挡"), false, ChildViews["Obstacles"].CreateChild,
                        new SpawnerEditorData(postion, suc));
                    menu.AddItem(new GUIContent("随机怪"), false, ChildViews["RandomMonster"].CreateChild,
                        new RandomMonsterEditorData(postion, suc));
                    menu.AddItem(new GUIContent("虚拟相机"), false, ChildViews["VirtualCamera"].CreateChild,
                        new RandomMonsterEditorData(postion, suc));
                    menu.AddItem(new GUIContent("创建GVEBoss点"), false, ChildViews["GVEBoss"].CreateChild,
                        new PositionData(postion, suc));
                    menu.AddItem(new GUIContent("创建通缉任务"), false, ChildViews["WantedTask"].CreateChild,
                        new PositionData(postion, suc));

                    menu.AddItem(new GUIContent("保存"), false, Save, _SceneID);
                    menu.AddItem(new GUIContent("导出Json"), false, ExportJson, _SceneID);
                    menu.AddItem(new GUIContent("提交"), false, Commit, _SceneID);
                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }


        private void LoadScene(object userData)
        {
            SceneID = (int)userData;
            Load();
        }

        private void ReLoadScene(object userData)
        {
            Load(true);
        }


        private void CheckSelection()
        {
            Vector2 mousePosition = Event.current.mousePosition;
            Ray m_Ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Debug.DrawLine(m_Ray.origin, m_Ray.direction * 100);
            RaycastHit m_HitInfo;
            if (Physics.Raycast(m_Ray, out m_HitInfo, LayerMask.GetMask("Entity")))
            {
                if (m_HitInfo.collider.gameObject.name == "Sphere")
                {
                    Selection.SetActiveObjectWithContext(m_HitInfo.collider.transform.parent.gameObject, null);
                    //Selection.activeGameObject = m_HitInfo.collider.transform.parent.gameObject;
                    // Repaint();
                }
            }
#if UNITY_EDITOR
            else
            {
                //Debug.LogWarningFormat("无法在{0}处获取场景地面的高度信息。", point);
            }
#endif
        }


        public class MonsterEditorData : PositionData
        {
            public MonsterJsonReadData Monster { get; private set; }
            public int Group;

            public MonsterEditorData(Vector3 position, MonsterJsonReadData monster, int group, bool creat) : base(
                position, creat)
            {
                this.Group = group;
                this.Monster = monster;
            }
        }

        public class NPCEditorData : PositionData
        {
            public NPCJsonDataRead Npc { get; private set; }
            public int Group;

            public NPCEditorData(Vector3 position, NPCJsonDataRead npc, int group, bool creat) : base(position, creat)
            {
                this.Group = group;
                this.Npc = npc;
            }
        }

        public class MineEditorData : PositionData
        {
            public MineData Mine { get; private set; }
            public int Group;

            public MineEditorData(Vector3 position, MineData monster, int group, bool creat) : base(position, creat)
            {
                this.Group = group;
                this.Mine = monster;
            }
        }

        public class AreaEditorData : PositionData
        {
            public AreaShape areaShape { get; private set; }

            public AreaEditorData(Vector3 position, AreaShape shape, bool creat) : base(position, creat)
            {
                this.areaShape = shape;
            }
        }


        public class PathEditorData : PositionData
        {
            public PathEditorData(Vector3 position, bool creat) : base(position, creat)
            {
            }
        }


        public class SpawnerEditorData : PositionData
        {
            public SpawnerEditorData(Vector3 position, bool creat) : base(position, creat)
            {
            }
        }

        public class TriggerEditorData : PositionData
        {
            public TriggerEditorData(Vector3 position, bool creat) : base(position, creat)
            {
            }
        }

        public class RandomMonsterEditorData : PositionData
        {
            public RandomMonsterEditorData(Vector3 position, bool creat) : base(position, creat)
            {
            }
        }


        public class PositionData
        {
            public Vector3 Position { get; private set; }
            public bool CanCreat { get; private set; }

            public PositionData(Vector3 position, bool creat)
            {
                this.Position = position;
                this.CanCreat = creat;
            }
        }

        #region 多语言导出
        public class MapExportArr
        {
            public int MapID;
            public string Key;
            public string Val;
            public string Des;

            public void SetData(int mapID, string key, string val, string des)
            {
                MapID = mapID;
                Key = key;
                Val = val;
                Des = des;
            }

            public void ExportExcel(int row, ExcelRange excel)
            {
                int index = 1;
                excel[row, index++].Value = Key;
                excel[row, index++].Value = Val;
                excel[row, index++].Value = MapID;
                excel[row, index++].Value = Des;
            }
        }
        private static readonly string m_MapDataFilePath = "Assets/Res/MapData";
        private static List<MapExportArr> m_mapTextStr = new();
        private static Dictionary<int, Dictionary<string, List<string>>> m_mapdataTextStr = new();

        /// <summary>
        /// 地图编辑器保存时自动导出多语言xlsm文件，及生成对应的策划目录和客户端目录下的json明文对应的key
        /// </summary>
        public static void ExportMapTextStrByMapEditorSave()
        {
            Release();
            //先替换保存策划目录源数据
            ExportMapText(serverJsonDir);
            //多语言excel生成
            WriteMapExcel();
            Release();

            //再保存客户端目录
            string clientDir = Application.dataPath + "/Res/MapData";
            ExportMapText(clientDir);
            Release();
        }

        private static void ExportMapText(string sourceDir)
        {
            m_mapdataTextStr.Clear();

            string[] files = Directory.GetFiles(sourceDir, "*.json", SearchOption.AllDirectories);
            foreach (var item in files)
            {

                //var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(item);
                //var jsonAsset = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(textAsset.text);
                var textAsset = File.ReadAllText(item);
                var jsonAsset = Newtonsoft.Json.JsonConvert.DeserializeObject<SceneJsonData>(textAsset);
                if (jsonAsset != null)
                {
                    {
                        foreach (var item1 in jsonAsset.Areas)
                        {
                            if (item1.Value != null && item1.Value.areaType == (int)AreaType.Jurisdiction)
                            {
                                if (!string.IsNullOrEmpty(item1.Value.AreaName) && !string.IsNullOrWhiteSpace(item1.Value.AreaName))
                                {
                                    MapExportArr mapExportArr = new();
                                    string key = $"AreaName_{jsonAsset.SceneID}_{item1.Value.AreaID}";
                                    mapExportArr.SetData(jsonAsset.SceneID, key, item1.Value.AreaName, "地图区域名");
                                    m_mapTextStr.Add(mapExportArr);

                                    item1.Value.AreaName_Key = key;

                                    //AddMapDataStr(jsonAsset.SceneID, item1.Value.Index.ToString(), item1.Value.AreaName);
                                    //Debug.Log($"区域id={item1.Value.AreaID} ,AreaName={item1.Value.AreaName}");
                                }
                            }
                        }
                    }
                }

                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(jsonAsset, setting);
                jsonstr = ConvertJsonString(jsonstr);

                //string newJson = Newtonsoft.Json.JsonConvert.SerializeObject(jsonAsset, Newtonsoft.Json.Formatting.Indented,);
                if (File.Exists(item))
                {
                    File.Delete(item);
                }
                File.WriteAllText(item, jsonstr);
            }
        }

        private static void WriteMapExcel()
        {
            DirectoryInfo directoryInfo = new(serverJsonDir);
            if (directoryInfo.Parent == null)
            {
                Debug.LogError($"停止生成MapEditor多语言表，路径错误");
                return;
            }

            DirectoryInfo parentDirectory = directoryInfo.Parent;
            string sourceFolderPath = parentDirectory.FullName + "\\多语言工具\\source";
            // 检查文件夹是否存在
            if (!Directory.Exists(sourceFolderPath))
            {
                Debug.LogError($"停止生成多语言表，未找到\"\\多语言工具\\source\"文件夹");
                return;
            }

            var path = sourceFolderPath + "\\地图编辑器文本_MapEditorText.xlsx";
            if (path != "")
            {
                FileInfo newFile = new(path);
                if (newFile.Exists)
                {
                    //创建一个新的excel文件
                    newFile.Delete();
                    newFile = new FileInfo(path);
                }

                //通过ExcelPackage打开文件
                using (ExcelPackage package = new(newFile))
                {
                    //在excel空文件添加新sheet
                    ExcelWorksheet taskConfig = package.Workbook.Worksheets.Add("MapEditorText");
                    int index = 1;
                    taskConfig.Cells[1, index].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    taskConfig.Cells[1, index++].Value = "多语言key";
                    taskConfig.Cells[1, index++].Value = "文本";
                    taskConfig.Cells[1, index++].Value = "地图ID";
                    taskConfig.Cells[1, index++].Value = "描述";

                    int row = 6;
                    foreach (var item in m_mapTextStr)
                    {
                        item.ExportExcel(row++, taskConfig.Cells);
                    }

                    taskConfig.Cells.AutoFitColumns();
                    //保存excel
                    package.Save();

                    EditorUtility.DisplayDialog("消息", $"一共:{m_mapTextStr.Count}条\n\n\t导出成功", "您辛苦了");
                }
            }
        }

        private static void Release()
        {
            m_mapdataTextStr.Clear();
            m_mapTextStr.Clear();
        }

        #endregion
    }
}
#endif
