using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Entity;
using StarProjectDef;
using UnityEngine;

/// <summary>
/// 本地模拟的 召唤物实体
/// 跟高磊确定的召唤物如下:
///     1. 召唤物 可以显示 模型/特效;
///     2. 召唤物可以播动作/特效/位移;
///     3. 类似于召唤物喷射一个射线造成伤害这种效果, 走的也是buff 运行时里效果连线.
///        召唤物本身不会释放技能；
/// 
/// </summary>
public class LocalSummonEntity : LocalSimulateEntity
{
    public override string ModelPath => "Roles/Template/Local_Summon_Model";

    public override E_LocalEntityType LocalEntityType => E_LocalEntityType.Summon;

    private Transform followNode;

    public Transform FollowNode
    {
        get => followNode;
    }

    private bool followNodeIsParent;
    public bool FollowNodeIsParent => followNodeIsParent;

    public ulong SummonHostID = 0;
    public void Create(ulong entityId, int avatarID, Vector3 pos, Transform follow, bool isParent, ulong summonHostID)
    {
        base.Create(entityId, pos);

        // 父节点, 如何 设置？？ 
        // buff 创建一个 本地召唤物, 根据配置的 类型, 决定这个 召唤物 是否挂在主角节点下 还是  
        // 在 自己去lerp 跟随主角位置.
        // 所以 说 这些 移动/ 旋转/ 挂点的配置, 其实应该是 在buff 中
        // buff 根据上述配置, 决定 子弹的挂点在哪
        followNodeIsParent = isParent;
        followNode = follow;

        SummonHostID = summonHostID;
        /// <summary>
        /// 这个函数要做什么?
        /// 1. 如果 是 直接设置到 主人的 跟节点下, 那移动 不需要直接设置, 但是 有可能存在 围绕主角的旋转 这种逻辑;
        ///    而旋转 ，也应该是 根节点做的逻辑;
        /// 
        /// 2.如果 是不再主角的 根结点下, 那移动 其实就是 跟随这个节点, 自己做 缓动插值过去.
        /// 
        /// 综上:
        ///     1.一定需要传入 一个 节点,  同时, 标识这个节点是 父节点 还是 跟随移动的节点
        ///     2.对于 召唤物来说，  如果传入的是 父节点类型, 那就 不做自己的 移动.  如果 是跟随的节点, 那就跟随移动
        /// 
        /// </summary>you
        CreateView(avatarID, isParent ? followNode : null);
    }

    protected override void Release()
    {
        base.Release();
        followNode = null;
        followNodeIsParent = false;
    }

    protected override void OnActionOnViewCreateFinish()
    {
        base.OnActionOnViewCreateFinish();
        LookAtSummonHostForward();
    }

    private void LookAtSummonHostForward()
    {
        ActionFollowTargetForward?.Invoke(SummonHostID);
    }
}
