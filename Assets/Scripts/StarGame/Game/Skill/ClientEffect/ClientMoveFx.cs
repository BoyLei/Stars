using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using StarProject.Game;
using StarProject.Game.Player;
using System;

public class ClientMoveFx : MonoBehaviour
{
    public Transform target; // 跟随的目标, 应该每帧去更新这个 目标

    // 移动的速度
    public float moveSpeed = 0;

    // 每一帧的移动 距离
    public float frameMoveOffset = 0;

    // 每一帧的移动 距离平方
    public float sqrtFrameMoveOffset = 0;


    // 移动的速度向量
    public Vector3 speed = new Vector3(0, 0, 0); // 移动的初始速度

    public Vector3 lastSpeed;   // 存储转向前的本地速度

    public int rotateSpeed = 0;   // 角度旋转的速度, 单位 度/秒

    public Vector3 finalForward;    //目标到自身连线的向量,最终朝向

    public float angleOffset;       //自己的forward朝向 和 

    private ulong targeEntityID = 0; // 目标的实体ID

    private E_ClientMoveFxType clientMoveFxType = E_ClientMoveFxType.Move2TargetPos; // 客户端移动特效的类型

    public Vector3 targetPos;

    public bool Move = false;

    public Action ActionOnMoveEnd;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init()
    {
        frameMoveOffset = moveSpeed * Time.deltaTime;

        sqrtFrameMoveOffset = Mathf.Sqrt(frameMoveOffset);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Move)
        {
            return;
        }

        CheckHint();

        if (Move)
        {
            return;
        }
        UpdateRotation();
        UpdatePosition();
    }

    void CheckHint()
    {
        var sqrMagnitude = (targetPos - transform.position).sqrMagnitude;

        // 如果 距离目标点的 距离 <=0.5 或者 距离 <= frameMoveOffset(距离小于一帧的位移,表明速度穿过了坐标)
        // 此时 结束 特效的移动
        if (sqrMagnitude <= 0.25 || sqrMagnitude <= sqrtFrameMoveOffset)
        {
            Move = false;
            // 通知外面结束
            ActionOnMoveEnd?.Invoke();
        }
    }


    void UpdatePosition()
    {
        transform.position = transform.position + speed * Time.deltaTime;
    }

    // 旋转,使其朝向目标点,要改变速度的方向
    void UpdateRotation()
    {
        //先将速度转为本地坐标,旋转之后再变为世界坐标
        lastSpeed = transform.InverseTransformDirection(speed);

        ChangeForward(rotateSpeed * Time.deltaTime);

        speed = transform.TransformDirection(lastSpeed);
    }

    void ChangeForward(float rotateChangeSpeed)
    {
        //获得目标点到自身的朝向
        finalForward = (GetTargetPos() - transform.position).normalized;

        if (finalForward != transform.forward)
        {
            angleOffset = Vector3.Angle(transform.forward, finalForward);
            if (angleOffset > rotateSpeed)
            {
                angleOffset = rotateSpeed;
            }

            // 将自身forward朝向慢慢转向最终朝向
            transform.forward = Vector3.Lerp(transform.forward, finalForward, rotateChangeSpeed / angleOffset);
        }
    }


    Vector3 GetTargetPos()
    {
        switch (clientMoveFxType)
        {
            case E_ClientMoveFxType.Move2TargetPos:
                {
                    return targetPos;
                }
                break;
            case E_ClientMoveFxType.Move2FollowTarget:
                {
                    // 每帧去取 实体的 坐标,更新 targetPos
                    EntityCtrlBase entityCtrl = GameManager.Instance.GetEntityCtr(targeEntityID);
                    if (entityCtrl != null)
                    {
                        targetPos = entityCtrl.M_Curr.Position();
                    }
                }
                break;
            default: break;
        }
        return targetPos;
    }
}
