///--------------------------------------------------------------------
/// 文件名   :   CameraOffset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/11/15 18:35:46
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HideMonoScript]
public class CameraOffset : MonoBehaviour
{
    public enum Direction
    {
        //上下左右前后

        UP = 0,
        DOWN = 1,
        LEFT = 2,
        RIGHT = 3,
        FRONT = 4,
        BACK = 5
    }
    private IEnumerable _directions = new ValueDropdownList<Direction>()
    {
        { "上", Direction.UP },
        { "下", Direction.DOWN },
        { "左", Direction.LEFT },
        { "右", Direction.RIGHT },
        { "前", Direction.FRONT },
        { "后", Direction.BACK },
    };

    [LabelText("方向")]
    [ValueDropdown("_directions")]
    public Direction M_Direction;

    [LabelText("距离")]
    public float Distance;

    [LabelText("进入时间")]
    public float InTime;

    [LabelText("持续时间")]
    public float StageTime;

    [LabelText("退出时间")]
    public float BackTime;

    public void OnEnable()
    {

    }

}
