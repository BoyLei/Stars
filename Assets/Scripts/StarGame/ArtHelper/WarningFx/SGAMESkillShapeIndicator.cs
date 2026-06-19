using UnityEngine;

public class SGAMESkillShapeIndicator : MonoBehaviour
{

    // 设置圆、扇形、环扇形 大半径
    public virtual void SetSectorRange(float radius)
    {

    }

    // 设置扇形角度
    public virtual void SetSectorAngle(float _angle)
    {

    }

    // 设置【环扇形】的小半径
    public virtual void SetRingFanMiddleRingSize(float value)
    {

    }

    public virtual void SetRectWidthAndLength(float width, float length)
    {

    }

    public virtual void SetRectLength(float value)
    {

    }

    public virtual void SetRectRange(float value)
    {

    }

    public virtual void SetShapeActive(bool isShow)
    {
        gameObject.SetActive(isShow);
    }

    // 设置进度
    public virtual void SetGlowRange(float value)
    {

    }


}
