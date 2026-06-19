///--------------------------------------------------------------------
/// 文件名   :   EditorConfigUtils
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/27 10:38:25
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using StarProjectDef;

namespace MapEditor
{
    static public class EditorConfigUtils
    {
        public const string TortoiseProc = "TortoiseProc";

        public static AvatarConfigs avatarConfigs = new AvatarConfigs();
        public static MonsterConfigs MonsterCfgs = new MonsterConfigs();
        public static NPCConfigs NpcCfgs = new NPCConfigs();
        public static MineConfigs MineCfgs = new MineConfigs();
        public static SceneConfigs SceneCfgs = new SceneConfigs();
        public static ModelConfigs ModelCfgs = new ModelConfigs();
        public static LevelDataConfigs LevelCfgs = new LevelDataConfigs();
        public static AreaEffectConfigs AreaEffectConfigs = new AreaEffectConfigs();


        // public static Excels MonsterExcel = new Excels();
        public static void Init()
        {
            avatarConfigs.Init();
            SceneCfgs.Init();
            ModelCfgs.Init();
            MonsterCfgs.Init();
            NpcCfgs.Init();
            MineCfgs.Init();
            LevelCfgs.Init();
            AreaEffectConfigs.Init();
        }

        public static Vector3 StringToVector3(string str)
        {
            Vector3 result = Vector3.zero;
            string[] splits = str.Split(',');
            if (splits.Length >= 3)
            {
                result.x = float.Parse(splits[0]);
                result.y = float.Parse(splits[1]);
                result.z = float.Parse(splits[2]);
            }

            return result;
        }

        public static string Vector3ToString(Vector3 vector3)
        {
            return string.Format("{0},{1},{2}", vector3.x, vector3.y, vector3.z);
        }

        public static List<string> GetKeyWorld(DataRow KeyRow)
        {
            List<string> KeyWorlds = new List<string>();
            if (KeyRow != null)
            {
                int Count = KeyRow.Table.Columns.Count;
                for (int i = 0; i < Count; i++)
                {
                    string Name = KeyRow.ItemArray[i].ToString();
                    if (string.IsNullOrEmpty(Name))
                    {
                        break;
                    }
                    KeyWorlds.Add(Name);

                }
            }
            return KeyWorlds;
        }

        public static void Destory()
        {
            MonsterCfgs.Destroy();
            NpcCfgs.Destroy();
            MineCfgs.Destroy();
            SceneCfgs.Destroy();
            ModelCfgs.Destroy();
            LevelCfgs.Destroy();
            AreaEffectConfigs.Destroy();
            avatarConfigs.Destroy();
        }

        public static DataSet ReadBook(string path)
        {
            DataSet dataSet = null;
            try
            {
                FileStream mStream = File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path),
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
                dataSet = mExcelReader.AsDataSet();
            }
            catch (Exception e)
            {
                Debug.LogError("文件读取失败，" + e.ToString());
            }

            return dataSet;
        }

        public static T ReadJson<T>(string JsonPath)
        {
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            if (textAsset != null)
            {
                T data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(textAsset.text);
                return data;
            }
            return default(T);
        }

        public static T ReadBytes<T>(string bytesPath)
        {
            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(bytesPath);
            if (textAsset != null)
            {
                T data = MessagePack.MessagePackSerializer.Deserialize<T>(textAsset.bytes);
                return data;
            }
            return default(T);
        }
    }

    #region Avatar数据
    public class AvatarConfigs
    {
        public Dictionary<int, int> Avatars = new Dictionary<int, int>();

        public void Init()
        {
            Avatars.Clear();
            //AvatarData avatarData = EditorConfigUtils.ReadJson<AvatarData>("Assets/Res/Config/Excel/Avatar.json");
            AvatarData avatarData = EditorConfigUtils.ReadBytes<AvatarData>("Assets/Res/Config/ExcelBytes/Avatar.bytes");
            if (avatarData != null)
            {
                foreach (var item in avatarData.StaticAvatarDatas)
                {
                    if (!Avatars.ContainsKey(item.Key))
                    {
                        Avatars.Add(item.Key, item.Value.GetModelId());
                    }
                }
            }
        }

        public void Destroy()
        {
            if (Avatars != null)
            {
                Avatars.Clear();
            }
        }


    }

    #endregion

    #region 怪物数据

    public class MonsterConfigs
    {
        public Dictionary<long, MonsterJsonReadData> Monsters = new Dictionary<long, MonsterJsonReadData>();
        public List<long> MonsterIDs = new List<long>();

        public void Init()
        {
            Monsters.Clear();
            MonsterIDs.Clear();
            //MonsterData monsterData = EditorConfigUtils.ReadJson<MonsterData>("Assets/Res/Config/Excel/Monster.json");
            MonsterData monsterData = EditorConfigUtils.ReadBytes<MonsterData>("Assets/Res/Config/ExcelBytes/Monster.bytes");
            if (monsterData!=null && monsterData.StaticMonsterDatas!=null && monsterData.StaticMonsterDatas.Count>0)
            {
                foreach (var item in monsterData.StaticMonsterDatas)
                {
                    if (!Monsters.ContainsKey(item.Key))
                    {
                        MonsterJsonReadData monster = new MonsterJsonReadData(item.Value.GetID(),item.Value.GetMonType(), item.Value.Name,item.Value.GetAvatarID());
                        Monsters.Add(item.Key, monster);
                        MonsterIDs.Add(item.Key);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (Monsters != null)
            {
                Monsters.Clear();
            }

            if (MonsterIDs != null)
            {
                MonsterIDs.Clear();
            }
        }
    }

    public class MonsterJsonReadData 
    {
        public long ID;
        public int Type;
        public int ModelID;
        public string MonsterName;


        public MonsterJsonReadData(long id,int type,string monName,int avatarID)
        {
            this.ID = id;
            this.Type = type;
            this.MonsterName = monName;
            if (EditorConfigUtils.avatarConfigs.Avatars.ContainsKey(avatarID))
            {
                this.ModelID = EditorConfigUtils.avatarConfigs.Avatars[avatarID];
            }
            else
            {
                this.ModelID = 0;
            }
        }

        public string GetMonsterName()
        {
            return $"{ID}_{MonsterName}";
        }
    }

    #endregion

    #region NPC数据

    public class NPCConfigs
    {
        public Dictionary<long, NPCJsonDataRead> Npcs = new Dictionary<long, NPCJsonDataRead>();
        public List<long> NpcIDs = new List<long>();
        public void Init()
        {
            Npcs.Clear();
            NpcIDs.Clear();

            NpcData npcData = EditorConfigUtils.ReadBytes<NpcData>("Assets/Res/Config/ExcelBytes/Npc.bytes");
            if(npcData!=null)
            {
                foreach (var item in npcData.StaticNpcDatas)
                {
                    if (!Npcs.ContainsKey(item.Key))
                    {
                        NPCJsonDataRead npc = new NPCJsonDataRead(item.Key, item.Value.GetNpcType(), item.Value.GetAvatarID(), item.Value.Name);

                        NpcIDs.Add(npc.ID);
                        Npcs.Add(npc.ID, npc);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (Npcs != null)
            {
                Npcs.Clear();
            }

            if (NpcIDs != null)
            {
                NpcIDs.Clear();
            }
        }
    }

    public class NPCJsonDataRead 
    {
        public long ID;
        public int Type;
        public int ModelID;
        public string Name;
        public int AvatarID;

        public NPCJsonDataRead(long id,int type, int avatarID, string name)
        {
            this.ID = id;
            this.Type = type;
            this.Name = name;
            this.AvatarID = avatarID;
            if (EditorConfigUtils.avatarConfigs.Avatars.ContainsKey(avatarID))
            {
                this.ModelID = EditorConfigUtils.avatarConfigs.Avatars[avatarID];
            }
            else
            {
                this.ModelID = 0;
            }
        }

        public string GetNpcName()
        {
            return $"{ID}_{Name}";
        }
    }

    #endregion


    #region 矿物表

    public class MineConfigs
    {
        public Dictionary<long, MineData> Mines = new Dictionary<long, MineData>();
        public List<long> MineIDs = new List<long>();
        public void Destroy()
        {
            if (MineIDs != null)
            {
                MineIDs.Clear();
            }

            if (Mines != null)
            {
                Mines.Clear();
            }
        }

        public void Init()
        {
            Mines.Clear();
            MineIDs.Clear();
            //InteractData interactData = EditorConfigUtils.ReadJson<InteractData>("Assets/Res/Config/Excel/Interact.json");
            InteractData interactData = EditorConfigUtils.ReadBytes<InteractData>("Assets/Res/Config/ExcelBytes/Interact.bytes");
            if (interactData!=null)
            {
                foreach (var item in interactData.StaticInteractDatas)
                {
                    MineData mine = new MineData(item.Value);

                    if (!Mines.ContainsKey(mine.ID))
                    {
                        MineIDs.Add(mine.ID);
                        Mines.Add(mine.ID, mine);
                    }
                }
            }
        }
    }

    #endregion

    #region 模型表

    public class ModelConfigs
    {
        public Dictionary<int, string> Models = new Dictionary<int, string>();
        public List<string> KeyWorlds = new List<string>();

        public int GetIndex(string key)
        {
            return KeyWorlds.IndexOf(key);
        }
        public void Destroy()
        {
            if (Models != null)
            {
                Models.Clear();
            }
        }

        public void Init()
        {
            Models.Clear();
            KeyWorlds.Clear();
            //ModelData modelData = EditorConfigUtils.ReadJson<ModelData>("Assets/Res/Config/Excel/Model.json");
            ModelData modelData = EditorConfigUtils.ReadBytes<ModelData>("Assets/Res/Config/ExcelBytes/Model.bytes");
            if (modelData!=null)
            {
                foreach (var item in modelData.StaticModelDatas)
                {
                    ModeljsonDataRead model = new ModeljsonDataRead(item.Key,item.Value.ModelsPath);
                    if (!Models.ContainsKey(model.ModelID) && !string.IsNullOrEmpty(model.ModelPath))
                    {
                        Models.Add(model.ModelID, model.ModelPath);
                    }

                }
            }
        }
    }

    #endregion

    #region 场景表

    public class SceneConfigs
    {
        public Dictionary<int, SceneData> Scenes = new Dictionary<int, SceneData>();
        public List<int> SceneIDs = new List<int>();
        public string[] SceneMenus;
        public List<string> KeyWorlds = new List<string>();

        public int GetIndex(string key)
        {
            return KeyWorlds.IndexOf(key);
        }
        public void Destroy()
        {
            if (SceneIDs != null)
            {
                SceneIDs.Clear();
            }

            if (Scenes != null)
            {
                Scenes.Clear();
            }

            SceneMenus = null;
        }

        public void Init()
        {
            Scenes.Clear();
            SceneIDs.Clear();
            SceneMapData sceneMapData = EditorConfigUtils.ReadBytes<SceneMapData>("Assets/Res/Config/ExcelBytes/SceneMap.bytes");
            MapBaseData mapBaseData = EditorConfigUtils.ReadBytes<MapBaseData>("Assets/Res/Config/ExcelBytes/MapBase.bytes");
            if (sceneMapData!=null && mapBaseData!= null)
            {
                foreach (var item in sceneMapData.StaticSceneMapDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    SceneData sceneData = new SceneData(item.Value.GetID(), mapName, item.Value.SceneName);
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }
            SceneMenus = new string[SceneIDs.Count];
            for (int i = 0; i < SceneIDs.Count; i++)
            {
                SceneMenus[i] = $"{SceneIDs[i]}_{Scenes[SceneIDs[i]].SceneName}";//SceneIDs[i].ToString();
            }
        }
    }

    #endregion

    #region 场景表

    public class LevelDataConfigs
    {
        public Dictionary<int, LevelData> Scenes = new Dictionary<int, LevelData>();
        public List<int> SceneIDs = new List<int>();
        public string[] SceneMenus;

        public void Destroy()
        {
            if (SceneIDs != null)
            {
                SceneIDs.Clear();
            }

            if (Scenes != null)
            {
                Scenes.Clear();
            }

            SceneMenus = null;
        }

        public void Init()
        {
            Scenes.Clear();
            SceneIDs.Clear();
            DailyLevelData dailyLevel = EditorConfigUtils.ReadBytes<DailyLevelData>("Assets/Res/Config/ExcelBytes/DailyLevel.bytes");
            MapBaseData mapBaseData = EditorConfigUtils.ReadBytes<MapBaseData>("Assets/Res/Config/ExcelBytes/MapBase.bytes");

            if (dailyLevel!=null)
            {
                foreach (var item in dailyLevel.StaticDailyLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"DailyLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }


            SecretLevelData secretLevel = EditorConfigUtils.ReadBytes<SecretLevelData>("Assets/Res/Config/ExcelBytes/SecretLevel.bytes");
            if (secretLevel != null)
            {
                foreach (var item in secretLevel.StaticSecretLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"SecretLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }


            MirrorLevelData mirrorLevel = EditorConfigUtils.ReadBytes<MirrorLevelData>("Assets/Res/Config/ExcelBytes/MirrorLevel.bytes");
            if (mirrorLevel != null)
            {
                foreach (var item in mirrorLevel.StaticMirrorLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"MirrorLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }


            PlotLevelData plotLevel = EditorConfigUtils.ReadBytes<PlotLevelData>("Assets/Res/Config/ExcelBytes/PlotLevel.bytes");
            if (plotLevel != null)
            {
                foreach (var item in plotLevel.StaticPlotLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"PlotLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }

            }

            TeamDailyLevelData dailyCopy = EditorConfigUtils.ReadBytes<TeamDailyLevelData>("Assets/Res/Config/ExcelBytes/TeamDailyLevel.bytes");
            if (dailyCopy != null)
            {
                foreach (var item in dailyCopy.StaticTeamDailyLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"TeamDailyLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }

            }

            
            WantedLevelData WantedLevel = EditorConfigUtils.ReadBytes<WantedLevelData>("Assets/Res/Config/ExcelBytes/WantedLevel.bytes");
            if (WantedLevel != null)
            {
                foreach (var item in WantedLevel.StaticWantedLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"WantedLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }

            }
            
            GNGLevelData GNGLevel = EditorConfigUtils.ReadBytes<GNGLevelData>("Assets/Res/Config/ExcelBytes/GNGLevel.bytes");
            if (GNGLevel != null)
            {
                foreach (var item in GNGLevel.StaticGNGLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName,"GNGLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }

            BFLevelData BFLevel = EditorConfigUtils.ReadBytes<BFLevelData>("Assets/Res/Config/ExcelBytes/BFLevel.bytes");
            if (BFLevel != null)
            {
                foreach (var item in BFLevel.StaticBFLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName, "BFLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
            }

            
            
            TowerLevelData Tower = EditorConfigUtils.ReadBytes<TowerLevelData>("Assets/Res/Config/ExcelBytes/TowerLevel.bytes");
            if (Tower != null)
            {
                foreach (var item in Tower.StaticTowerLevelDatas)
                {
                    string mapName = string.Empty;
                    if (mapBaseData != null && mapBaseData.StaticMapBaseDatas.TryGetValue(item.Value.GetBaseID(), out var mapBaseDataCell))
                    {
                        mapName = mapBaseDataCell.MapName;
                    }
                    LevelData sceneData = new LevelData(item.Value.GetID(), mapName, item.Value.LevelName, "TowerLevel");
                    if (!Scenes.ContainsKey(sceneData.MapID))
                    {
                        SceneIDs.Add(sceneData.MapID);
                        Scenes.Add(sceneData.MapID, sceneData);
                    }
                }
                
            }
            SceneMenus = new string[SceneIDs.Count];
            for (int i = 0; i < SceneIDs.Count; i++)
            {
                SceneMenus[i] = $"{Scenes[SceneIDs[i]].ExcelName}/{ SceneIDs[i]}_{Scenes[SceneIDs[i]].SceneName}"; //SceneIDs[i].ToString();
            }
        }
    }

    #endregion


    #region 区域效果表

    public class AreaEffectConfigs
    {
        public Dictionary<int, AreaEffectData> Effects = new Dictionary<int, AreaEffectData>();
        public List<string> KeyWorlds = new List<string>();

        public void Destroy()
        {
            if (Effects != null)
            {
                Effects.Clear();
            }
        }

        public void Init()
        {
        
            Effects.Clear();
            DataSet data = EditorConfigUtils.ReadBook(Application.dataPath +
                                                      "/../../../../../StarsProject_Design/trunk/配置文件/配置说明/编辑器配置说明.xlsx");
            if (data == null)
            {
                return;
            }
            DataTable sheet = data.Tables["区域效果"];
            var rowCount = sheet.Rows.Count;
            for (int i = 1; i < rowCount; i++)
            {
                DataRow dataRow = sheet.Rows[i];
                if (dataRow.IsNull(0))
                {
                    continue;
                }

                AreaEffectData effectData = new AreaEffectData();
                effectData.Init(dataRow);
                if (!Effects.ContainsKey(effectData.EffectType))
                {
                    Effects.Add(effectData.EffectType, effectData);
                }
            }
        }
    }

    #endregion

    public class Excels
    {
        public Dictionary<int, MonsterExcelData> MonsterConfigs = new Dictionary<int, MonsterExcelData>();

        public Dictionary<int, MineExcelData> MineConfigs = new Dictionary<int, MineExcelData>();

        public void Init(int SceneID)
        {
            MonsterConfigs.Clear();
            MineConfigs.Clear();
            string mapPath = Application.dataPath + $"/../../../Export/{SceneID}/Excel/maps.xlsx";
            if (File.Exists(mapPath))
            {
                DataSet data = EditorConfigUtils.ReadBook(mapPath);
                DataTable monstersheet = data.Tables["布怪"];
                var rowCount = monstersheet.Rows.Count;
                for (int i = 1; i < rowCount; i++)
                {
                    DataRow dataRow = monstersheet.Rows[i];
                    if (dataRow.IsNull(0))
                    {
                        continue;
                    }

                    MonsterExcelData monsetr = new MonsterExcelData();
                    monsetr.Init(dataRow);
                    if (!MonsterConfigs.ContainsKey(monsetr.Index))
                    {
                        MonsterConfigs.Add(monsetr.Index, monsetr);
                    }
                }

                rowCount = 0;
                DataTable minesheet = data.Tables["矿物"];
                rowCount = minesheet.Rows.Count;
                for (int i = 1; i < rowCount; i++)
                {
                    DataRow dataRow = minesheet.Rows[i];
                    if (dataRow.IsNull(0))
                    {
                        continue;
                    }

                    MineExcelData mine = new MineExcelData();
                    mine.Init(dataRow);
                    if (!MineConfigs.ContainsKey(mine.Index))
                    {
                        MineConfigs.Add(mine.Index, mine);
                    }
                }
            }
        }
    }

    public class MineData 
    {
        public long ID;
        public int ModelID;
        public string ObjectName;
        public float Range;
        public int Type;


        public MineData(InteractDataCell dataCell)
        {

            this.ID = dataCell.GetID();
            this.ObjectName = dataCell.ModelName;
            this.Type = dataCell.GetObjectType();
            this.Range = dataCell.GetTriggerRange() * 0.01f;
            if (EditorConfigUtils.avatarConfigs.Avatars.ContainsKey(dataCell.GetAvatarID()))
            {
                this.ModelID = EditorConfigUtils.avatarConfigs.Avatars[dataCell.GetAvatarID()];
            }
            else
            {
                this.ModelID = 0;
            }

        }
        public string GetMineName()
        {
            return $"{ID}_{ObjectName}";
        }
    }

    public class ModeljsonDataRead 
    {
        public int ModelID;
        public string ModelPath;
        public GetIndexDelegate GetIndex;

        public ModeljsonDataRead(int id,string path)
        {
            this.ModelID = id;
            this.ModelPath = "Assets/Res/" + path + ".prefab";
        }
    }

    public class AreaEffectData : EConfig
    {
        public int EffectType;
        public List<string> Args;

        public override void Init(DataRow dataRow)
        {
            Args = new List<string>();
            this.EffectType = System.Int32.Parse(dataRow[0].ToString());
            for (int i = 2; i < 12; i++)
            {
                string arg = dataRow[i].ToString();
                if (string.IsNullOrEmpty(arg))
                {
                    break;
                }

                Args.Add(arg);
            }
        }
    }

    public class SceneData 
    {
        public int MapID;
        public string MapName;
        public string SceneName;

        public SceneData(int id,string name,string sceneName)
        {
            this.MapID = id;
            this.MapName = name;
            this.SceneName = sceneName;
        }
    }

    public class LevelData 
    {
        public int MapID;
        public string MapName;
        public string SceneName;
        public string ExcelName;
        public LevelData(int mapid,string mapname,string sceneName,string excelName)
        {
  
            this.MapID = mapid;
            this.MapName = mapname;
            this.SceneName = sceneName;
            this.ExcelName = excelName;
        }
    }

    public class MonsterExcelData : EConfig
    {
        public int Index;
        public int MonsterID;
        public int MonsterType;
        public bool IsMain;
        public int GroupID;
        public Vector3 Position;
        public Vector3 Rotation;
        public int Num;
        public int Range;
        public int Count;
        public int FreshType;
        public int DeadTime; //  死亡间隔刷新时间
        public int OpenServerTime; //   开服间隔刷新时间 
        public string FixTime; // 固定时刻刷新时间
        public int WaveID; // 波次ID
        public int ControlID; //    控制器ID

        public override void Init(DataRow dataRow)
        {
            int index = 0;
            this.Index = System.Int32.Parse(dataRow[index++].ToString());
            this.MonsterID = System.Int32.Parse(dataRow[index++].ToString());
            this.MonsterType = System.Int32.Parse(dataRow[index++].ToString());
            this.IsMain = dataRow[index++].ToString() == "TRUE";
            this.GroupID = System.Int32.Parse(dataRow[index++].ToString());
            this.Position = EditorConfigUtils.StringToVector3(dataRow[index++].ToString());
            this.Rotation = EditorConfigUtils.StringToVector3(dataRow[index++].ToString());
            this.Num = System.Int32.Parse(dataRow[index++].ToString());
            this.Range = System.Int32.Parse(dataRow[index++].ToString());
            this.Count = System.Int32.Parse(dataRow[index++].ToString());
            this.FreshType = System.Int32.Parse(dataRow[index++].ToString());
            this.DeadTime = System.Int32.Parse(dataRow[index++].ToString());
            this.OpenServerTime = System.Int32.Parse(dataRow[index++].ToString());
            this.FixTime = dataRow[index++].ToString();
            this.WaveID = System.Int32.Parse(dataRow[index++].ToString());
            this.ControlID = System.Int32.Parse(dataRow[index++].ToString());
        }
    }

    public class MineExcelData : EConfig
    {
        public int Index;
        public int MineID;
        public int MineType;
        public bool IsShowRange;
        public Vector3 Position;
        public Vector3 Rotation;
        public int Num;
        public int Range;
        public int Count;
        public int FreshType;
        public int DeadTime; //  死亡间隔刷新时间
        public int OpenServerTime; //   开服间隔刷新时间 
        public string FixTime; // 固定时刻刷新时间
        public int WaveID; // 波次ID
        public int ControlID; //    控制器ID

        public override void Init(DataRow dataRow)
        {
            int index = 0;
            this.Index = System.Int32.Parse(dataRow[index++].ToString());
            this.MineID = System.Int32.Parse(dataRow[index++].ToString());
            this.MineType = System.Int32.Parse(dataRow[index++].ToString());
            this.IsShowRange = dataRow[index++].ToString() == "TRUE";
            this.Position = EditorConfigUtils.StringToVector3(dataRow[index++].ToString());
            this.Rotation = EditorConfigUtils.StringToVector3(dataRow[index++].ToString());
            this.Num = System.Int32.Parse(dataRow[index++].ToString());
            this.Range = System.Int32.Parse(dataRow[index++].ToString());
            this.Count = System.Int32.Parse(dataRow[index++].ToString());
            this.FreshType = System.Int32.Parse(dataRow[index++].ToString());
            this.DeadTime = System.Int32.Parse(dataRow[index++].ToString());
            this.OpenServerTime = System.Int32.Parse(dataRow[index++].ToString());
            this.FixTime = dataRow[index++].ToString();
            this.WaveID = System.Int32.Parse(dataRow[index++].ToString());
            this.ControlID = System.Int32.Parse(dataRow[index++].ToString());
        }
    }

    public abstract class EConfig
    {
        abstract public void Init(DataRow dataRow);
    }

    public delegate int GetIndexDelegate(string key);
}
#endif