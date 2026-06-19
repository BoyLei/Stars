using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class RedPointView : MonoBehaviour
{
    public RedPoint redPoint;

    public Text redPointCount;

    private void Awake()
    {
        // SGF.Debuger.Log($"[RedPointView] Awake : {this.transform.name} , type: {redPoint.redPointType}");
        if (redPointCount != null)
        {
            redPointCount.text = "0";
        }
        redPoint.OnStateChange = OnStateChange;
        redPoint.OnRefreshRedCount = OnRefreshRedCount;
    }


    private void OnEnable()
    {
        // SGF.Debuger.Log($"[RedPointView] OnEnable : {this.transform.name} , type: {redPoint.redPointType}");

        if (redPoint == null)
        {
            return;
        }

        OnRefreshRedCount();
    }

    private void Start()
    {
        // SGF.Debuger.Log($"[RedPointView] Start : {this.transform.name} , type: {redPoint.redPointType}");

        /// 2024/1/9
        /// 脚本在执行顺序 :
        /// Awake-->OnEnable--->Start ---->Update--->...
        /// Awake: 脚本挂在，不管是否勾选enable, 都会执行一次.
        /// OnEnable: Awake 后,判定 脚本勾选enable 后， 执行.
        /// Start: OnEnable 后， 有且只有一次 执行.
        /// 对于 一个节点上的 多个组件脚本 A,B.
        /// 执行顺序 为:
        /// [A: Awake-->OnEnable]; ---> [B: Awake-->OnEnable;]
        /// [A: Star] ---> [B: Star]
        /// 
        /// 所以， 如果在 A 的 Awake / OnEnable 中 关闭节点, 都会导致 后续 B 的生命周期未执行
        redPoint.gameObject.SetActive(redPoint.redPointState);
    }

    public void OnStateChange(bool state)
    {
        if (redPoint == null)
        {
            return;
        }

        redPoint.gameObject.SetActive(state);



    }

    public void OnRefreshRedCount()
    {
        if (redPointCount == null || redPoint == null)
        {
            return;
        }

        int count = redPoint.RedCount;
        redPointCount.text = count > 99 ? "99+" : count.ToString();
    }
}
