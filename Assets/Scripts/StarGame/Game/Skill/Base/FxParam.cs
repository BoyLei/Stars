using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkillEditor;
using StarProject.Game;
using StarProject.Game.Entity.Factory;
using StarProjectDef;
using UnityEngine;

//现在此处定义的 特效参数,后续特效 部分函数接口如果用此接口，可以挪过去
public interface I_FxParam
{
    /// <summary>
    /// 特效的 key,用来标识播放的特效
    /// 在关闭特效的时候,可以根据key定位这个特效
    /// </summary>
    string Key { get; }
    string EffectName { get; }
    string EffectPath { get; }

    /// <summary>
    /// 挂点枚举
    /// </summary>
    HangPoint HangPoint { get; }

    /// <summary>
    /// 特效的最大播放时长(s) , playTime == -1 时,表示无限循环播
    /// </summary>
    float PlayTime { get; }
    /// <summary>
    /// 特效自己配置的延迟播放时间
    /// 由特效根据配置决定是否延迟播
    /// </summary>
    int StartDelay { get; }
    /// <summary>
    /// 特效 开始播放的时间点, 单位 ms
    /// </summary>
    int StartTime { get; }
    /// <summary>
    /// 特效播放速度
    /// </summary>
    float SpeedMultiplier { get; }
    /// <summary>
    /// 是否跟随移动,目前 是否跟随 决定特效是否放在人身上
    /// </summary>
    bool IsFollowMove { get; }
    /// <summary>
    /// 是否跟随旋转, 是否跟随旋转决定 特效 是否放在 不跟随旋转根节点上。
    /// note:
    ///     如果不跟随 旋转，且有绑点 ，那应该拷贝 一份 世界坐标/旋转/缩放 相同的
    ///     绑点坐标,存放在 非跟随旋转根节点上
    /// </summary>
    /// <value></value>
    bool IsFollowRot { get; }

    /// <summary>
    /// 是否面向施法者
    /// </summary>
    bool IsFaceToBuilder { get; }
    /// <summary>
    /// 是否面向摄像机
    /// </summary>
    bool IsFaceToCamera { get; }


    /// <summary>
    /// 技能状态切换是否同步取消播放配置
    /// </summary>
    bool IsChangeCancel { get; }

    /// <summary>
    /// 是否阶段循环
    ///  特效：如果不跟随阶段循环。第一次执行，后面的直接return
    /// </summary>
    bool IsFollowLoop { get; }

    /// <summary>
    /// 是否半径适应
    /// </summary>
    bool IsAdapt { get; }

    ulong BuilderID { get; }

    ulong OwnerID { get; }

    // /// <summary>
    // /// 最终的 monster, 服务器传的运行时 应该都有,但是客户端自己播放的特效没有
    // /// </summary>
    // ulong OriginalMaster { get; }


    /// <summary>
    /// 自定义设置的 朝向,直接用于特效的朝向
    /// </summary>
    // UnityEngine.Vector3 CustomRotate { get; }

    /// <summary>
    /// 是否循环播放
    /// </summary>
    bool Loop { get; }

    /// <summary>
    /// 是否全局可见,如果false,只有自己可以见
    /// </summary>
    bool IsAll { get; }

    // /// <summary>
    // /// [额外数据]【预警圈用】
    // /// </summary>
    // ShapSerialize Shap { get; }

    Vector3 ScaleXYZ { get; }

    /// <summary>
    /// 是否跟随主人隐藏
    /// </summary>
    bool IsFollowBuilderHide { get; }

    /// <summary>
    /// 特效位移 的偏移
    /// </summary>
    Vector3 MoveOffset { get; }

    /// <summary>
    /// 特效朝向 的偏移
    /// </summary>
    Vector3 DirectionOffset { get; }

    /// <summary>
    /// 所以 不跟随玩家的 特效， 其实存在三种情况；
    /// 1.不跟随玩家变化尺寸， 只在创建时 跟随玩家 碰撞盒比例 确定 特效尺寸大小；
    /// 2.跟随玩家变化尺寸， 创建过后， 跟随玩家碰撞盒比例变化 自己大小；
    /// 3.不考虑 玩家碰撞盒 比例， 创建尺寸默认为 原始大小。 如果此时后 特效挂点尺寸 倍数为 2， 那特效 的倍数为 0.5. 最终 显示为 特效原始大小
    /// </summary>
    E_FxScale EFxScale { get; }

    void Reset();
}

public class FxParam : I_FxParam
{
    private string baseKey;
    private string extralKey;

    public string Key => $"fX_{baseKey}_{extralKey}";
    private string effectName;
    public string EffectName => effectName;

    private string effectPath;
    public string EffectPath => effectPath;

    private HangPoint hangPoint;
    public HangPoint HangPoint => hangPoint;

    private float playTime;
    public float PlayTime => playTime;

    private int startDelay;
    public int StartDelay => startDelay;

    private int startTime;
    public int StartTime => startTime;

    private float speedMultiplier;
    public float SpeedMultiplier => speedMultiplier;

    private bool isFollowMove;
    public bool IsFollowMove => isFollowMove;

    private bool isFollowRot;
    public bool IsFollowRot => isFollowRot;




    private bool isFaceToBuilder;
    public bool IsFaceToBuilder => isFaceToBuilder;

    private bool isFaceToCamera;
    public bool IsFaceToCamera => isFaceToCamera;


    private bool isFollowLoop;
    public bool IsFollowLoop => isFollowLoop;

    private bool isChangeCancel;
    public bool IsChangeCancel => isChangeCancel;

    private bool isAdapt;
    public bool IsAdapt => isAdapt;

    private ulong builderID;
    public ulong BuilderID => builderID;

    private ulong ownerID;

    public ulong OwnerID => ownerID;

    // private ulong _originalMaster = 0;

    // public ulong OriginalMaster => _originalMaster;

    private bool loop;
    /// <summary>
    /// 是否 自身循环播放
    /// </summary>
    public bool Loop => loop;

    private bool isAll;

    public bool IsAll => isAll;

    private Vector3 scaleXYZ = Vector3.one;
    public Vector3 ScaleXYZ { get => scaleXYZ; }

    public bool isFollowOwnerHide = true;
    public bool IsFollowBuilderHide => isFollowOwnerHide;

    private Vector3 moveOffset = Vector3.zero;

    public Vector3 MoveOffset => moveOffset;

    private Vector3 directionOffset = Vector3.zero;
    public Vector3 DirectionOffset => directionOffset;

    // 默认不跟随玩家特效大小变化， 但是 跟随模型大小创建
    private E_FxScale eFxScale = E_FxScale.Origin;
    public E_FxScale EFxScale => eFxScale;

    // protected override void Release()
    // {
    //     Reset();
    // }

    private static FieldInfo[] fields = null;
    private static IEnumerable<PropertyInfo> properties = null;

    public static void Clone(I_FxParam originFxParam, I_FxParam toFxParam)
    {
        if (fields == null)
        {
            var type = typeof(FxParam);

            fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // 获取所有属性，并筛选出包含 set 方法的属性
            properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where<PropertyInfo>(prop => prop.GetSetMethod(nonPublic: true) != null);
        }

        if (fields != null)
        {
            foreach (var item in fields)
            {
                item.SetValue(toFxParam, item.GetValue(originFxParam));
            }

            foreach (var item in properties)
            {
                item.SetValue(toFxParam, item.GetValue(originFxParam));
            }

        }
    }

    public void Reset()
    {
        baseKey = "";
        effectName = "";
        effectPath = "";
        hangPoint = HangPoint.Root;
        playTime = 0;
        startDelay = 0;
        startTime = 0;
        speedMultiplier = 1;
        isFollowMove = false;
        isFollowRot = false;
        isFaceToBuilder = false;
        isFaceToCamera = false;
        isChangeCancel = false;
        isFollowLoop = false;
        isAdapt = false;
        // shap = null;
        loop = false;
        scaleXYZ = Vector3.one;
        eFxScale = E_FxScale.NoFollow;

        // _originalMaster = 0;
        // customRotate = UnityEngine.Vector3.zero;
    }

    /// <summary>
    /// 技能编译器轴上的效果参数初始化接口
    /// </summary>
    /// <param name="fXJson"></param>
    public void InitWithFxJson(FXJson fXJson, ulong builderEntityID, ulong ownerEntityID)
    {
        Reset();
        SpecEffect config = fXJson.config;
        if (config == null)
        {
            return;
        }
        effectName = fXJson.EffectName;
        effectPath = fXJson.EffectPath;
        baseKey = $"{fXJson.Index}_{effectName}";

        hangPoint = config.HangPoint;
        playTime = fXJson.Duration / 1000.0f;
        startDelay = 0;
        speedMultiplier = (float)fXJson.SpeedMultiplier;
        isFollowMove = config.IsFollowMove;
        isFollowRot = config.IsFollowRot;
        isFaceToBuilder = config.IsFaceToBuilder;
        isFaceToCamera = config.IsFaceToCamera;
        isFollowLoop = config.IsFollowLoop;
        loop = config.IsLoop;
        isFollowOwnerHide = config.IsFollowOwnerHide;

        moveOffset.x = config.XOffset / 100.0f;
        moveOffset.y = config.YOffset / 100.0f;
        moveOffset.z = config.ZOffset / 100.0f;

        directionOffset.x = config.XOffsetTowards;
        directionOffset.y = config.YOffsetTowards;
        directionOffset.z = config.ZOffsetTowards;

        // 如果是自身循环,且没有配置最大时间, 那么设置playTime = -1; 表示无限循环播放
        if (loop && playTime == 0)
        {
            playTime = -1;
        }
        isAll = config.IsAll;

        builderID = builderEntityID;
        ownerID = ownerEntityID;

        switch (config.IsFollowOwnerScale)
        {
            case SpecEffectScale.Origin:
                {
                    eFxScale = E_FxScale.Origin;

                }
                break;
            case SpecEffectScale.NoFollow:
                {
                    eFxScale = E_FxScale.NoFollow;
                }
                break;
            case SpecEffectScale.Follow:
                {
                    eFxScale = E_FxScale.Follow;
                }
                break;
            default:
                {
                    eFxScale = E_FxScale.Origin;
                }
                break;
        }

        // 设置 配置的 scale
        SetScale(fXJson.config.XScale / 100f, fXJson.config.YScale / 100f, fXJson.config.ZScale / 100f);
        // eFxScale = E_FxScale.Follow;
    }

    // public void SetOriginalMaster(ulong originalMaster)
    // {
    //     _originalMaster = originalMaster;
    // }

    /// <summary>
    /// 设置播放时间. 
    /// note: 
    ///     有些特效 比如受击特效,策划配置的时候没有配置 特效时间, 所以增加一个默认
    /// </summary>
    /// <param name="time"></param> 
    public void SetPlayTime(float time)
    {
        playTime = time;
    }
    public void InitWithLogic(string EffectPath, HangPoint _hangPoint = HangPoint.Root)
    {
        effectName = EffectPath;
        effectPath = EffectPath;
        hangPoint = _hangPoint;
        playTime = 0;
        startDelay = 0;
        speedMultiplier = 1;
        loop = false;
    }

    /// <summary>
    /// 设置fxParam 的额外的key, 用来做特效唯一id的区分
    /// </summary>
    /// <param name="key"></param>
    public void SetExtralKey(string key)
    {
        extralKey = key;
    }

    public void SetShapEffectName(ShapeSerialize shap, UnityEngine.Vector3 dir)
    {
        // 特效名字
        switch (shap.ShapeType)
        {
            case Shape.None:
                break;
            case Shape.Round:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.HollowCircle:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.Sector:
                {
                    effectName = "WarningRing_Area_60";
                }
                break;
            case Shape.RingFan:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.Arrow:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.Rect:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.RotRoute:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            case Shape.InputTarget:
                {
                    effectName = "WarningRing_Area";
                }
                break;
            default:
                break;
        }
        baseKey = $"WarningRingEffect_{effectName}";
    }

    public void SetScale(float x = 1, float y = 1, float z = 1)
    {
        scaleXYZ.x = x;
        scaleXYZ.y = y;
        scaleXYZ.z = z;
    }

    public void SetScale(Vector3 scale)
    {
        scaleXYZ = scale;
    }

    public void SetFxStartTime(int fxStartTime)
    {
        startTime = fxStartTime;
    }

    public static E_Render_PRI GetFxRenderType(I_FxParam fxParam, StarProject.Game.Entity.RemoteDynamic.AOIEntityObject m_entity)
    {
        bool isMainPlayer = m_entity.IsMainPlayer;

        ulong mainPlayerId = GameManager.Instance.mainPlayerId;

        bool isMainPlayerBuild = fxParam.BuilderID == mainPlayerId;

        // 如果是主角播放特效, 就需要判断 特效的 builder 是否是自己
        if (isMainPlayer)
        {
            if (isMainPlayerBuild)
            {
                return E_Render_PRI.PlayerATTACK_FX;
            }
            else
            {
                return E_Render_PRI.PlayerHIT_FX;
            }
        }

        // 首先判断效果 是否是属于主角
        bool isMainPlayerOwener = fxParam.OwnerID == mainPlayerId;

        // 如果不是主角，但是有时主角owner，那不管是 队友给的buff 还是 怪物攻击的特效, 都认为是 主角的受击特效
        if (isMainPlayerOwener)
        {
            return E_Render_PRI.PlayerHIT_FX;
        }

        // 是否是主角的召唤物
        bool isMainPlayerSummon = mainPlayerId == m_entity.SummonHostID;

        // 是否是主角的召唤物 释放的特效
        bool isBuilder = fxParam.BuilderID == m_entity.EntityId;

        // 如果不是主角播放特效, 那就去判断 是否是 伙伴的召唤物播放的特效
        if (isMainPlayerSummon)
        {
            // 如果是主角的召唤物释放的特效
            if (isBuilder)
            {
                return E_Render_PRI.TeammateATTACK_FX;
            }
            else
            {
                return E_Render_PRI.TeammateHIT_FX;
            }
        }

        var entityType = m_entity.Data.EntityType;

        // 如果 也不是主角的 召唤物, 那就判断 是否是主角的 队友
        bool isPlayer = entityType == E_EntityType.Player;
        if (isPlayer)
        {
            bool isSameTeam = GameManager.Instance.IsPlayerInTeam(m_entity.EntityId);

            // 如果是相同的 队伍, 那就看 这个特效的 builder 是否是自己
            if (isSameTeam)
            {
                if (isBuilder)
                {
                    return E_Render_PRI.PartyATTACK_FX;
                }
                else
                {
                    return E_Render_PRI.PartyHIT_FX;
                }
            }

            // 如果不是相同的队伍, 那就看 是否是同一个工会
            // TODO:
            //      工会的特效类型 判断
            // bool isSameUnion = GameManager.Instance.IsPlayerInTeam(m_entity.EntityId);
            {

            }

            return E_Render_PRI.Other_Player_FX;
        }

        // 如果不是玩家, 那看看 子弹/伙伴/召唤物 的拥有者是不是 其它玩家     

        var owenerEntity = GameManager.Instance.GetEntityByEntityID(fxParam.OwnerID);

        var summonHostID = owenerEntity == null ? 0 : owenerEntity.SummonHostID;

        var summonHostEntity = GameManager.Instance.GetFinalSummonHostEntity(summonHostID);

        // 如果是伙伴,就看伙伴的 主人是否是 主角
        if ((entityType == E_EntityType.Partner || entityType == E_EntityType.Summon || entityType == E_EntityType.BulletEntity) && summonHostEntity != null)
        {
            // 伙伴的最终 summonHost

            // 伙伴的主人 是 玩家, 且不是 自己
            if (summonHostEntity.EntityId != mainPlayerId && summonHostEntity.EntityType == E_EntityType.Player)
            {
                return E_Render_PRI.Other_Player_FX;
            }
        }

        // 默认返回 other 类型       
        return E_Render_PRI.Other;
    }
}

public static class ConverFx
{
    public static FXJson LoopEffect2FxJson(EffectTypeHitEffect loopEffectcfg)
    {
        if (loopEffectcfg.fxJson == null || loopEffectcfg.fxJson.config == null)
        {
            return null;
        }
        return loopEffectcfg.fxJson;
    }

    public static void LoopEffects2FxJsons(List<EffectTypeHitEffect> effectTypeHitEffects, List<FXJson> fXJsons)
    {
        effectTypeHitEffects.ForEach((EffectTypeHitEffect effectTypeHitEffect) =>
        {
            FXJson fXJson = LoopEffect2FxJson(effectTypeHitEffect);
            if (fXJson == null)
            {
                return;
            }
            fXJsons.Add(fXJson);
        });
    }


}

/// <summary>
/// 特效 封装的 几个相关参数
/// </summary>
public class FxGeParam
{
    public StarProject.Service.LocalDynamic.Fx.GameEffect ge;

    // 根部 transform
    public Transform RootTransform;
    // 是否在 骨骼之下
    public bool IsUnderViewBone = false;

    public E_FxScale EFxScale;

    public Vector3 CreateScale = Vector3.one;

    /// <summary>
    /// 配置的 特效基础scale
    /// </summary>
    public Vector3 CfgScale = Vector3.one;

    public Vector3 CurScale = Vector3.one;

    private Vector3 tempV3 = Vector3.zero;

    private Vector3 pos = Vector3.zero;
    public Vector3 CurPos => pos;
    public bool IsClosed = false;
    private System.Action OnClose;

    private E_Render_PRI fxRenderType;

    /// <summary>
    /// 特效的渲染类型, 目前 博哥 要求 特效按类型分类,最多只有 50个同屏
    /// </summary>
    public E_Render_PRI FxRenderType => fxRenderType;

    /// <summary>
    /// 特效类型的 int 值, 因为外部 需要不停的 做 FxRenderType 类型的排序, 所以此处直接转成 缓存的 int 值,避免多次 类型转换
    /// </summary>
    public int FxRenderTypeValue => fxRenderTypeDic[fxRenderType];


    private Dictionary<E_Render_PRI, int> fxRenderTypeDic = new();

    public string Path;

    // public string EffectName;


    public FxGeParam(Transform rootTransform, bool isUnderViewBone, E_FxScale e_fxScaleType, Vector3 cfgScale)
    {
        RootTransform = rootTransform;
        IsUnderViewBone = isUnderViewBone;
        EFxScale = e_fxScaleType;
        IsClosed = false;

        CfgScale = cfgScale;

        // 设置默认的特效类型 为 other 类型
        fxRenderType = E_Render_PRI.Other;
    }

    /// <summary>
    /// 设置特效的类型
    /// </summary>
    /// <param name="e_Render_PRI"></param>
    public void SetFxType(E_Render_PRI e_Render_PRI)
    {
        fxRenderType = e_Render_PRI;
        if (!fxRenderTypeDic.ContainsKey(fxRenderType))
        {
            fxRenderTypeDic.Add(fxRenderType, (int)fxRenderType);
        }
    }

    public void Reset()
    {
        RootTransform = null;
        EFxScale = E_FxScale.None;

        CreateScale = Vector3.one;
        CurScale = Vector3.one;
        tempV3 = Vector3.zero;
        if (ge != null)
        {
            StarProject.Service.LocalDynamic.LocalFxManager.Instance.ReturnEffect(ge);
        }
        ge = null;
        IsClosed = true;
        OnClose = null;
    }

    public void SetGameEffect(StarProject.Service.LocalDynamic.Fx.GameEffect gameEffect)
    {
        ge = gameEffect;
        if (IsClosed)
        {
            // 异步加载，如果资源加载到了，但是特效已经被上层关闭了，那就回收了
            if (ge != null)
            {
                ge.OnReturnEffect();
                StarProject.Service.LocalDynamic.LocalFxManager.Instance.ReturnEffect(ge);
            }
            ge = null;
            return;
        }
        // 注册一个 特效关闭时 移除 此处 特效引用的逻辑
        ge.SetCloseAction(() =>
        {
            OnClose?.Invoke();
        });
    }

    public void SetCloseAction(System.Action closeAction)
    {
        if (closeAction != null)
        {
            OnClose = closeAction;
        }
    }

    /// <summary>
    /// 直接关闭 这个特效。 它会执行它的 OnClose ，在OnClose中 执行ui 节点的关闭
    /// </summary>
    public void Close()
    {
        if (IsClosed)
        {
            return;
        }
        OnClose?.Invoke();
    }

    /// <summary>
    /// 刷新 特效的 scale
    /// </summary>
    /// <param name="fxBoxScale">模型半径的缩放scale</param>
    /// <param name="isInit"></param>
    /// <returns></returns>
    public FxGeParam UpdateFxScale(Vector3 fxBoxScale, bool isInit = false)
    {
        CurScale = CalculateFxScale(fxBoxScale, isInit);
        if (isInit)
        {
            CreateScale = CurScale;
        }
        if (ge != null)
        {
            SGF.Debuger.LogWarning($"[FxScaleParam] updateScale: {CurScale}");
            ge.SetScale(CurScale);
        }
        return this;
    }

    /// <summary>
    /// 计算 当前的 boxScale 尺寸下 特效的实际 scale 大小.
    /// note:
    ///     特效的实际scale， 需要考虑 特效它 挂在什么节点之下。
    ///     如果不挂在 主角模型之下, 那 根据 配置的 EFxScale 类型, 直接计算即可.
    ///     如果 是挂在 模型之下, 模型 已经放大， 那特效尺寸应该相应的 缩小. 
    ///     所以 此处 用的 RootTransform 是 特效挂在 的 根结点。  实际计算过程 在函数内部.
    /// </summary>
    /// <param name="boxScale"></param>
    /// <param name="isInit"></param>
    /// <returns></returns>
    private Vector3 CalculateFxScale(Vector3 boxScale, bool isInit = false)
    {
        // 如果 节点被销毁了， 那就直接返回 当前的 scale.
        if (RootTransform == null)
        {
            return CurScale;
        }

        tempV3 = Vector3.one;
        switch (EFxScale)
        {
            case E_FxScale.Follow:
                {
                    /// 如果是 跟随 玩家scale 缩放, 那不管 挂不挂在 玩家 骨骼之下, 特效的 scale =  BoxScale/RootTransform.scale
                    /// eg:
                    ///     1.如果不在骨骼之下, RootTransform 的scale 为2，  boxScale=2 -----> 最终的 scale 为 2/2 =1；
                    ///     2.如果在 骨骼之下, 骨骼 的 scale 为2 ,  boxScale=2 -----> 最终的 scale 为 2/2 =1；
                    ///     
                    /// 由此可见, 不管在不在 玩家的 骨骼之下, 特效的 scale  = BoxScale/RootTransform.scale
                    // boxScale = Vector3.one * 2;

                    tempV3 = GetFxScale(boxScale, RootTransform.localScale);
                }
                break;
            case E_FxScale.NoFollow:
                {
                    /// 不跟随 玩家变化, 只在 创建时 根据玩家的 尺寸创建特效， 后续 玩家尺寸变化, 特效尺寸都不变化
                    /// 
                    /// 如果是创建, 那 scale =  BoxScale/RootTransform.localScale
                    /// 
                    /// 如果是后续尺寸变化,分两种情况:
                    ///     1.不在玩家 骨骼之下,  玩家尺寸变化 后续特效的 尺寸均不变化.
                    ///     2.在玩家 骨骼之下, 玩家尺寸放大, 特效 相对应缩小.

                    if (isInit)
                    {
                        tempV3 = GetFxScale(boxScale, RootTransform.localScale);
                    }
                    else
                    {
                        if (!IsUnderViewBone)
                        {
                            // 骨骼的尺寸为 原始尺寸.
                            tempV3 = CurScale;
                        }
                        else
                        {
                            // originBoxScale/RootTransform.localScale
                            tempV3 = GetFxScale(CreateScale, RootTransform.localScale);
                        }
                    }
                }
                break;
            case E_FxScale.Origin:
                {
                    /// <summary>
                    /// 原始尺寸，它的 显示scale = 1/RootTransform.localScale;
                    /// </summary>
                    tempV3 = GetFxScale(Vector3.one, RootTransform.localScale);
                }
                break;
            default:
                {
                    SGF.Debuger.LogWarning($"[FxScaleParam] 没有 E_FxScale: {EFxScale} 类型处理方法 ");
                }
                break;
        }

        return tempV3;
    }

    private Vector3 GetFxScale(Vector3 boxScale, Vector3 modelScale)
    {
        tempV3.Set(CfgScale.x * boxScale.x / modelScale.x, CfgScale.y * boxScale.y / modelScale.y, CfgScale.z * boxScale.z / modelScale.z);
        return tempV3;
    }

    public void SetPosition(Vector3 v3)
    {
        pos = v3;
    }


}