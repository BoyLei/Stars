using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;

/// <summary>
/// 红点容器. 
/// 红点容器 包含两块:
///     1. 这个红点容器 包含的相关的 红点组(只要有一个红点 显示,这个组就会显示)
///     2. 红点容器 自己的 红点类型, 如果 红点容器的类型 为 Group, 那就是 它只关系所有 子红点的状态.
///        如果红点 有自己的 红点类型,那 就子节点 或者这个 红点容器自己 状态有一个 为true, 红点就显示
/// </summary>
public class RedPointContainer : RedPoint
{
    /// <summary>
    /// 红点组 包含的 子红点
    /// </summary>
    public List<RedPoint> redPointGroup = new();

    /// <summary>
    /// 红点组包含的 红点类型组。
    /// 有些 红点 是复合类型, 但是它 没有子红点, 就 可以用红点类型组来配置
    /// </summary>
    public List<RedPointType> redPointTypeGroup = new();

    public override void Awake()
    {
        base.Awake();

        //监听 红点组 关联的 子红点状态事件
        redPointGroup.ForEach((redPoint) =>
        {
            redPoint.OnStateChange += OnChildRedPointStateChange;
            redPoint.OnRefreshRedCount += RefreshRedCount;
        });

        // 注册 红点组 监听的 其它的类型的 红点条件
        redPointTypeGroup.ForEach((redPointType) =>
        {
            RedPointManager.Instance.RegiseterPointTypeCondition2RedPoint(redPointType, this);

            // 注册 redPointType 跟 这个红点组 之间的关联
            RedPointManager.Instance.RegisterPointType2RedPoint(redPointType, this);
        });
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        redPointGroup.ForEach((redPoint) =>
        {
            redPoint.OnStateChange -= OnChildRedPointStateChange;
            redPoint.OnRefreshRedCount -= RefreshRedCount;
        });

        // 取消 红点组 监听的 其他类型的 红点条件
        redPointTypeGroup.ForEach((redPointType) =>
        {
            RedPointManager.Instance.UnRegiseterPointTypeCondition2RedPoint(redPointType, this);

            // 注册 redPointType 跟 这个红点组 之间的关联
            RedPointManager.Instance.UnRegisterPointType2RedPoint(redPointType, this);
        });
    }

    /// <summary>
    /// 子红点的 状态发生改变的时候, 父节点也跟着处理自己的状态
    /// </summary>
    public void OnChildRedPointStateChange(bool state)
    {
        // 子红点 和 红点组的状态一致，那就不用管
        if (state == redPointState)
        {
            return;
        }
        if (state)
        {
            // 如果 子红点显示, 但是红点组 没显示, 那不需要检查直接显示红点组的红点
            if (!redPointState)
            {
                SetState(true);
            }

        }
        else
        {
            // 如果子红点 由显示--->不显示, 而 红点组是 显示状态, 那就标识 这个点组 需要检查
            if (redPointState)
            {
                // 此时可能 会收到多个 子红点的 状态刷新的通知，可以 只做标脏
                MarkDirty();
            }
        }
    }

    public override void Refresh()
    {
        // 只要有一个 子红点 是显示的状态, 红点组就需要显示
        for (int i = 0; i < redPointGroup.Count; i++)
        {
            var redPoint = redPointGroup[i];
            if (redPoint.redPointState)
            {
                SetState(true);
                RefreshRedCount();
                return;
            }
        }

        // 如果所有的子红点都不显示, 那就检查 这个红底自己 类型的 条件是否满足
        base.Refresh();
    }

    /// <summary>
    /// 红点组 刷新红点数量.
    /// 红点组 可以按不同的类型组合,如下:
    /// 1.红点组 的类型为 Group + [子红点组]redPointGroup +  [关联红点类型组]redPointTypeGroup
    /// 2.红点组 的类型为 redType + [子红点组]redPointGroup +  [关联红点类型组]redPointTypeGroup
    /// 
    /// 基于上,红点组的数量为:
    /// 1 总数量 =  [子红点组]redPointGroup + [关联红点类型组]redPointTypeGroup;
    /// 2 总数量 = redType + [子红点组]redPointGroup + [关联红点类型组]redPointTypeGroup;
    /// </summary>
    public override void RefreshRedCount()
    {
        if (!redPointState)
        {
            RedCount = 0;
            OnRefreshRedCount?.Invoke();
            return;
        }

        int count = 0;

        // 先统计所有 [子红点组]redPointGroup 的红点数量
        redPointGroup.ForEach((redPoint) =>
        {
            count += redPoint.RedCount;
        });

        // 然后再统计 跟红点组 关联类型的 红点数量
        for (int i = 0; i < redPointTypeGroup.Count; i++)
        {
            var pointType = redPointTypeGroup[i];

            // 先找 关联的红点类型 pointType 是否存储了红点数量
            int redTypeCount = RedPointManager.Instance.GetRedPointTypeCount(pointType);

            // 如果 存储了 相应的红点数量, 那就 直接使用
            if (redTypeCount > 0)
            {
                count += redTypeCount;
                continue;
            }
            else
            {
                // 检查 关联红点类型 是否满足条件, 如果 满足, 那就采用 默认的数量1
                if (RedPointManager.Instance.CheckRedPointTypeCfgConditions(pointType))
                {
                    count += 1;
                }
                else
                {
                    // 如果不满足,那就不增加 count数量
                }
            }
        }

        // 计算出当前的 相关类型的 红点总数量
        RedCount = count;

        // 如果勾选的是 group 组合类型, 那就按 子类型的所有子类型的红点 数显示数量
        if (redPointType == RedPointType.Group)
        {
            OnRefreshRedCount?.Invoke();
            return;
        }

        // 如果是特定类型, 就按这个红点的特定类型 的数量显示
        base.RefreshRedCount();
    }


    /// <summary>
    /// 红点组的条件检查, 需要分别 检查:
    /// 1.子红点的状态是否 有 显示红点的;
    /// 2.红点组的 配置 条件 是否有满足的;
    /// 3.红点组 拖进去的关联 类型 是否有满足的
    /// </summary>
    /// <returns></returns>
    public override bool CheckConditions()
    {
        //1. 先检查 子红点的状态是否有显示红点的
        for (int i = 0; i < redPointGroup.Count; i++)
        {
            var redPoint = redPointGroup[i];
            if (redPoint.redPointState)
            {
                return true;
            }
        }

        //2. 然后 检查 配置的条件是否满足
        if (base.CheckConditions())
        {
            return true;
        }

        //3. 检查红点组 拖进去的关联的 类型是否满足
        for (int i = 0; i < redPointTypeGroup.Count; i++)
        {
            var pointType = redPointTypeGroup[i];
            if (RedPointManager.Instance.CheckRedPointTypeCfgConditions(pointType))
            {
                return true;
            }
        }

        return false;
    }

}
