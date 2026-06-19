using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class TestShake : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }


//此脚本加在相机上
//单次震动和长震动为两种震动模式
// public class ShockScreen : MonoBehaviour

public class TestShake : MonoBehaviour
{
    [Tooltip("震动时间")]
    public float duration = 0.2f;
    [Tooltip("震动方向")]
    public Vector3 directionStrength = new Vector3(0, 1, 0);
    [Tooltip("长震动强度")]
    public float strength = 0.05f;
    [Tooltip("震动曲线")]
    public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.13f, 0.4f), new Keyframe(0.28f, -0.33f), new Keyframe(0.45f, 0.4f), new Keyframe(0.63f, -0.3f), new Keyframe(0.84f, 0.35f), new Keyframe(1, 0));
    [Tooltip("长震动")]
    public bool shakeForever = false;
    Vector3 thisPosition;
    Vector3 shakePosition;
    Vector3 startPos;
    bool shaking = false;
    float shakeTime = 0;

    // public bool 


    public void OnEnable()
    {
        startPos = transform.localPosition;
    }
    private void OnDisable()
    {
        transform.localPosition = startPos;
    }

    private void FixedUpdate()
    {
        Test();
        if (!shaking)
            return;
        if (duration <= 0)
        {
            return;
        }
        shakeTime += Time.deltaTime;
        if (shakeForever)
        {
            //长震动
            transform.localPosition = startPos + Random.insideUnitSphere * 1 * strength;
        }
        else
        {
            if (shakeTime < duration)
            {
                //单次震动
                thisPosition = startPos - shakePosition;
                shakePosition = new Vector3(curve.Evaluate((shakeTime % 1) / duration) * directionStrength.x, curve.Evaluate((shakeTime % 1) / duration) * directionStrength.y, curve.Evaluate((shakeTime % 1) / duration) * directionStrength.z);
                transform.localPosition = shakePosition + thisPosition;
            }
            else
                ShutShake();
        }
    }

    public int limit = 3;
    //private int count = 0;
    private void Test()
    {
        // return;
        //测试代码
        if (Input.GetMouseButtonDown(0))
        {
            shaking = true;
            duration = 0.2f;
            shakeTime = 0;
            Debug.Log($"duration ---> {duration}");
        }

        if (Input.GetMouseButtonDown(1))
        {
            shaking = true;
            //count = 0;
            duration = 0.3f;
            shakeTime = 0;
            Debug.Log($"duration ---> {duration}");

        }

        if (Input.GetMouseButtonDown(2))
        {
            shakeForever = !shakeForever;
            Debug.Log($"shakeForever ---> {shakeForever}");

        }
    }
    //停止震动
    private void ShutShake()
    {
        //Debug.LogErrorFormat("结束震动！！！！！！！！！！！！！！！！！！！！");
        transform.localPosition = startPos;
        shaking = false;
        shakeTime = 0;
    }

    //震动入口函数
    //ev:参数	单次震动 skonce(x,y,z)  xyz分别为Vector3三个轴的强度，可正可负可0
    //			长震动 skoncex  x为长震动强度
    //			终止震动 skover  终止震动
    public void PlayShake(string ev)
    {
        if (ev.Length <= 6)
            return;
        //终止已经存在的震动
        if (shaking)
            ShutShake();
        //重新记录相机震动前的位置
        startPos = transform.localPosition;
        //震动事件帧的参数处理
        if (ev.Substring(0, 6) == "skonce")
        {
            duration = 0.2f;
            shakeForever = false;
            if (ev.Substring(6, 1) == "(")
            {
                //将string类型的(1,1,1) 转换为 Vector3(1,1,1)
                ev = ev.Substring(6, ev.Length - 6);
                ev = ev.Replace("(", "").Replace(")", "");
                string[] s = ev.Split(',');
                directionStrength = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
            }
        }
        else if (ev.Substring(0, 6) == "sklong")
        {
            strength = float.Parse(ev.Substring(6, ev.Length - 6));
            shakeForever = true;
        }
        else if (ev.Substring(0, 6) == "skover")
        {
            duration = 0;
            shakeForever = false;
        }
        if (shaking)
            transform.localPosition = startPos;
        //Debug.LogErrorFormat("开始震动！！！！！！！！！！！！！！！！！！！！");
        shaking = true;
    }
}