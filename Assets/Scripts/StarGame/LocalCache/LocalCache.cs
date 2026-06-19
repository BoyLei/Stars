using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SGF.Module.Framework;
using SGF.Time;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Service.SDK;
using StarProject.Service.User;
using StarProjectDef;

[XLua.LuaCallCSharp]
public interface ILocalCache
{
    bool IsDirty { get; set; }

    void Load();

    void Save();

}

[XLua.LuaCallCSharp]
public static class LocalCache
{
    public const string FileName = "LocalCache";
    public static string FileNameRole = "LocalCache_Role";
    public static Dictionary<string, ILocalCache> AccountCaches = new Dictionary<string, ILocalCache>();
    public static Dictionary<string, ILocalCache> RoleCaches = new Dictionary<string, ILocalCache>();

    public static void Init()
    {

        Load();
    }

    public static void Dispose()
    {
    }

    //角色级别缓存
    public static void RoleCreate(ulong pid)
    {
        RoleCaches.Clear();

        FileNameRole = $"{FileNameRole}_{pid}";
        if (SaveManager.Instance.KeyExists(PartnerRedPointData.Name, FileNameRole))
        {
            var data = SaveManager.Instance.Load<PartnerRedPointData>(PartnerRedPointData.Name, FileNameRole);
            RoleCaches.Add(PartnerRedPointData.Name, data);
        }
        else
        {
            RoleCaches.Add(PartnerRedPointData.Name, new PartnerRedPointData());
        }
    }

    //账号级别缓存
    public static void Load()
    {

        if (SaveManager.Instance.KeyExists(ScenePlaysStatusData.Name, FileName))
        {
            var data = SaveManager.Instance.Load<ScenePlaysStatusData>(ScenePlaysStatusData.Name, FileName);
            AccountCaches.Add(ScenePlaysStatusData.Name, data);
        }
        else
        {
            AccountCaches.Add(ScenePlaysStatusData.Name, new ScenePlaysStatusData());
        }

        /*
        foreach (var cache in Caches)
        {
            cache.Value.Load(FileName);
        }*/
    }

    public static PartnerRedPointData GetPartnerRedPointData
    {
        get
        {
            return RoleCaches[PartnerRedPointData.Name] as PartnerRedPointData;
        }
    }

    public static ScenePlaysStatusData GetScenePlaysStatusData
    {
        get
        {
            return AccountCaches[ScenePlaysStatusData.Name] as ScenePlaysStatusData;
        }
    }
}


[XLua.LuaCallCSharp]
[System.Serializable]
public class PartnerRedPointData : ILocalCache
{

    public string FileName => LocalCache.FileNameRole;
    public const string Name = "PartnerRedPoint";
    public List<int> NewPartners = new List<int>();
    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
            }

            if (_isDirty)
            {
                Save();
            }
        }
    }
    public bool IsNewPartner(int id)
    {
        return NewPartners.Contains(id);
    }
    public void AddPartner(int id)
    {
        if (!NewPartners.Contains(id))
        {
            NewPartners.Add(id);
            IsDirty = true;
        }
    }

    public void RemovePartner(int id)
    {
        if (NewPartners.Contains(id))
        {
            NewPartners.Remove(id);
            IsDirty = true;
        }
    }

    public void Load()
    {

        if (SaveManager.Instance.KeyExists(Name, FileName))
        {
            SaveManager.Instance.Load<PartnerRedPointData>(Name, FileName);
        }
    }

    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }
        SaveManager.Instance.Save<PartnerRedPointData>(Name, this, FileName);
        RedPointManager.Instance.TriggerConditionTypeBoolValue(RedPointConditionType.Partner_New, NewPartners.Count > 0);
        IsDirty = false;
    }
}


[System.Serializable]
public class ScenePlaysStatusData : ILocalCache
{
    public string FileName => LocalCache.FileName;
    public const string Name = "ScenePlaysStatus";
    public Dictionary<int, bool> ScenePlaysStatus = new Dictionary<int, bool>();
    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
            }

            if (_isDirty)
            {
                Save();
            }
        }

    }

    public void SetScenePlayStatus(int Id, bool status)
    {
        if (ScenePlaysStatus.ContainsKey(Id))
        {
            ScenePlaysStatus[Id] = status;
        }
        else
        {
            ScenePlaysStatus.Add(Id, status);
        }
        IsDirty = true;
    }

    public bool GetScenePlayStatus(int Id, bool defaultValue = false)
    {
        if (ScenePlaysStatus.ContainsKey(Id))
        {
            return ScenePlaysStatus[Id];
        }

        SetScenePlayStatus(Id, defaultValue);
        return defaultValue;
    }

    public void Load()
    {

        if (SaveManager.Instance.KeyExists(Name, FileName))
        {
            SaveManager.Instance.Load<ScenePlaysStatusData>(Name, FileName);
        }
    }

    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }
        SaveManager.Instance.Save<ScenePlaysStatusData>(Name, this, FileName);
        IsDirty = false;
    }
}

/// <summary>
/// 通用的本地缓存, 用来存放 不同刷新频率的 cache
/// </summary>
[XLua.LuaCallCSharp]
public class LocalCacheManager : ServiceModule<LocalCacheManager>
{

    public const string FileName = "LocalCache";
    public const string Day = "Day";

    public BaseCache NormalCache;
    public BaseCache ImmediatelyCache;
    public BaseCache DelayCache;
    public BaseCache DayCache;

    public BaseCache BigCache;


    public void Init()
    {
        NormalCache = new(CacheType.Normal, FileName, false);
        ImmediatelyCache = new(CacheType.Immediately, FileName, false);
        DelayCache = new(CacheType.Delay, FileName, false);
        BigCache = new(CacheType.Immediately, FileName, true);
        DayCache = new(CacheType.Delay, FileName, false);
        InitDayCache();
    }

    public void Dispose()
    {
    }

    public void Clear()
    {
        NormalCache.Clear();
        ImmediatelyCache.Clear();
        DelayCache.Clear();
        BigCache.Clear();
    }


    private StringBuilder sb = new();
    private string FormatKey(ref string key)
    {
        sb.Clear().Append(key).Append("_");
        if (SDKManager.Instance.IsInit)
        {
            sb.Append(SDKManager.Instance.SdkUserID);
        }
        sb.Append("_");
        if (UserManager.Instance.MainUserData != null)
        {
            sb.Append(UserManager.Instance.MainUserData?.playerRoleId);
        }

        return key = sb.ToString();
    }

    private string FormatDayKey(string key)
    {
        string dayStr = $"#Day_{TimeUtils.GetCurServerDay()}_{key}";

        FormatKey(ref dayStr);

        return dayStr;
    }


    /// <summary>
    /// 设置值类型数据, 值类型 会先比较值是否变化，变化后才会落盘
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="cacheType">缓存存储的类型,默认为 Normal, 下一帧落盘</param>
    public void SetValueType(string key, object v, CacheType cacheType = CacheType.Normal)
    {
        BaseCache caches = GetCaches(cacheType, false);
        caches?.Set(FormatKey(ref key), v, true);
    }

    /// <summary>
    /// 设置 引用类型数据, 直接落盘，不会比较变化
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="cacheType">缓存存储的类型,默认为 Normal, 下一帧落盘</param>
    public void SetRefrenceType(string key, object v, CacheType cacheType = CacheType.Normal)
    {
        BaseCache caches = GetCaches(cacheType, false);
        caches?.Set(FormatKey(ref key), v, false);
    }

    /// <summary>
    /// 初始化每日失效的 数据
    /// </summary>
    private void InitDayCache()
    {
        string dayStr = $"#Day_{TimeUtils.GetCurServerDay()}";

        // 删除 所有 旧的
        NormalCache.Remove("#Day_", true, dayStr);

    }

    /// <summary>
    /// 设置 当日时效数据
    /// </summary>
    /// <param name="key"></param>
    public void SetDayValue(string key, object v)
    {
        string dayStr = FormatDayKey(key);

        NormalCache.Set(dayStr, v, false);
    }

    public void RemoveDayValue(string key)
    {
        string dayStr = FormatDayKey(key);

        NormalCache.Remove(dayStr, false);
    }

    /// <summary>
    /// 获得 key 对应的 当日时效数据
    /// </summary>
    /// <param name="key"></param>
    public object GetDayValue(string key)
    {
        string dayStr = FormatDayKey(key);


        return NormalCache.Get(dayStr);

    }


    /// <summary>
    /// 获取 数据接口
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cacheType"></param>
    /// <returns></returns>
    public object Get(string key, CacheType cacheType = CacheType.Normal)
    {
        BaseCache caches = GetCaches(cacheType, false);

        return caches?.Get(FormatKey(ref key));
    }

    public bool Has(string key, CacheType cacheType = CacheType.Normal)
    {
        BaseCache caches = GetCaches(cacheType, false);
        if (caches == null)
        {
            return false;
        }
        return caches.Has(FormatKey(ref key));
    }


    private BaseCache GetCaches(CacheType cacheType, bool isBigData)
    {
        BaseCache caches = null;
        switch (cacheType)
        {
            case CacheType.Normal:
                {
                    caches = NormalCache;
                }
                break;
            case CacheType.Immediately:
                {
                    caches = ImmediatelyCache;
                }
                break;
            case CacheType.Delay:
                {
                    caches = DelayCache;
                }
                break;

            default: break;
        }
        return caches;
    }


    /// <summary>
    /// 设置大块数据, 大块数据 会单独落盘
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="cacheType"></param>
    public void SetBigData(string key, object v)
    {
        key = FormatKey(ref key);
    }

    /// <summary>
    /// 获取 数据接口
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cacheType"></param>
    /// <returns></returns>
    public object GetBigData(string key)
    {
        key = FormatKey(ref key);

        return null;
    }
}

/// <summary>
/// 通用的 缓存数据类型
/// </summary>
[XLua.LuaCallCSharp]
public class BaseCache
{
    private CacheType cacheType;
    private bool isBigData = false;

    private Dictionary<string, object> Caches = new();

    private bool isDirty = false;

    /// <summary>
    /// 存入 的文件名
    /// </summary>

    private string FileName = "";

    private const string LocalCacheKey = "LocalCache";

    public BaseCache(CacheType _cacheType, string _fileName, bool _isBigData)
    {
        cacheType = _cacheType;
        isBigData = _isBigData;

        FileName = $"{_fileName}_{cacheType.ToString()}";

        Load();
    }

    /// <summary>
    /// 加载 缓存, 默认只加载 小块数据
    /// </summary>
    private void Load()
    {
        if (isBigData)
        {
            return;
        }

        if (SaveManager.Instance.KeyExists(LocalCacheKey, FileName))
        {
            Caches = SaveManager.Instance.Load<Dictionary<string, object>>(LocalCacheKey, FileName);
        }
        else
        {
            Caches = new();
        }
    }

    /// <summary>
    /// 小的数据 会统一存放在 一个 数据块 中, Caches 就是
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="checkValue">是否检查值,默认不检查, 只有值类型，检查才有意义。引用类型不确定外部是否引用，所以不需要检查</param>
    public void Set(string key, object v, bool checkValue = false)
    {

        if (Caches.ContainsKey(key))
        {
            // 如果需要检查值, 并且值 一样, 就不落盘。 只有值类型才有意义，引用类型不确定外部是否引用，所以不需要检查
            if (checkValue && Object.Equals(v, Caches[key]))
            {
                return;
            }
            Caches[key] = v;
        }
        else
        {
            // 如果 v 为null 并且 本来也没有数据,那就不需要落盘
            if (v == null)
            {
                return;
            }
            // v 可能是引用数据类型, 不太好 使用equal 判断是否变化,除非基础数据类型单独抛接口存储
            Caches.Add(key, v);
        }

        MarkDirty();
    }

    public object Get(string key)
    {
        if (!Has(key))
        {
            return null;
        }
        return Caches[key];
    }

    public bool Has(string key)
    {
        return Caches.ContainsKey(key);
    }

    public void Remove(string key, bool isMatchAll = false, string ignoreKey = "")
    {
        if (!isMatchAll)
        {
            Caches.Remove(key);
        }
        else
        {
            var cachesKeys = Caches.Keys.KToList();
            foreach (var item in cachesKeys)
            {
                if (item.Contains(key))
                {

                    if (ignoreKey != null && !item.Contains(ignoreKey) || Caches[item] == null)
                    {
                        Caches.Remove(item);
                    }
                }

            }
        }
    }

    /// <summary>
    /// 设置 大的数据块,大的数据块 是单个 数据快 存储为一个 文件, Caches 中存储的是 所有 key 的 数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetBigData(string key, object v)
    {
        // SaveManager.Instance.Save<BaseCache>($"{cacheType.ToString()}", this, $"{fileName}_{cacheType.ToString()}");
    }

    /// <summary>
    /// 获得 大的 数据块, 大的数据块 一开始并不会 load, 只有在需要 GetBigData 的时候去 load
    /// </summary>
    /// <param name="key"></param>
    public object GetBigData(string key)
    {
        return Caches[key];
    }

    public void MarkDirty()
    {
        // 不需要重复标脏
        if (isDirty)
        {
            return;
        }
        isDirty = true;
        Save();
    }

    private void Save()
    {
        float delayTime = -1;
        switch (cacheType)
        {
            case CacheType.Normal:
                {
                    // 下一帧 save
                    delayTime = 0;
                }
                break;
            case CacheType.Immediately:
                {
                    // -1 立即save
                    delayTime = -1;
                }
                break;
            case CacheType.Delay:
                {
                    // 100ms
                    delayTime = 0.1f;
                }
                break;
            default: delayTime = 0; break;
        }

        if (delayTime < 0)
        {
            // 立即存储
            SaveCache();
        }
        else
        {
            DelayInvoker.DelayInvoke(delayTime, (object[] args) =>
            {
                SaveCache();
            });
        }

    }

    private void SaveCache()
    {
        isDirty = false;
        SaveManager.Instance.Save<Dictionary<string, object>>(LocalCacheKey, Caches, FileName);
    }

    public void Clear()
    {
        Caches.Clear();
        SaveManager.Instance.Save<Dictionary<string, object>>(LocalCacheKey, Caches, FileName);
    }


}