using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarProject;
public class CameraShake : MonoBehaviour
{

    public Vector3 positionShake;//震动幅度
    public Vector3 angleShake;   //震动角度
    public float cycleTime = 0.2f;//震动周期
    public int cycleCount = 3;    //震动次数
    public bool fixShake = false; //为真时每次幅度相同，反之则递减
    public bool unscaleTime = false;//不考虑缩放时间
    public bool bothDir = true;//双向震动

    float currentTime;
    int curCycle;
    Vector3 curPositonShake;
    Vector3 curAngleShake;
    float curFovShake;
    Vector3 startPosition;
    Vector3 startAngles;
    Transform myTransform;

    bool shaking = false;

    int randomDir = 1;

    int curCameraEventID = 0;


    private DateTime now;

    void Awake()
    {
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform = transform;
        startPosition = myTransform.localPosition;
        startAngles = myTransform.localEulerAngles;

        GlobalEvent.OnCameraShakeEvent.AddListener(StartShake);
        GlobalEvent.OnCameraStopShakeEvent.AddListener(StopShake);

    }

    private void OnDestroy()
    {
        GlobalEvent.OnCameraShakeEvent.RemoveListener(StartShake);
        GlobalEvent.OnCameraStopShakeEvent.RemoveListener(StopShake);

    }

    void OnDisable()
    {
        myTransform.localPosition = startPosition;
        myTransform.localEulerAngles = startAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration">总时间</param>
    /// <param name="cycleItemTime">单个时间周期</param>
    /// <param name="amplitude">振幅</param>
    void StartShake(int cameraEventID, float duration, float cycleItemTime, Vector3 amplitude)
    {
        cycleTime = cycleItemTime;
        cycleCount = (int)Math.Round(duration / cycleItemTime);
        positionShake.x = amplitude.x;
        positionShake.y = amplitude.y;
        positionShake.z = amplitude.z;
        curCameraEventID = cameraEventID;
        Restart();
    }

    void StopShake(int cameraEventID)
    {
        if (curCameraEventID != cameraEventID)
        {
            return;
        }
        shaking = false;
        currentTime = 0f;
        curCycle = 0;
        Reset();
    }

    void Reset()
    {
        curCameraEventID = 0;
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform.localPosition = startPosition;
        myTransform.localEulerAngles = startAngles;
        randomDir = UnityEngine.Random.Range(0f, 1f) <= 0.5f ? 1 : -1;
    }


    //重置
    public void Restart()
    {
        Reset();
        shaking = true;

        now = DateTime.Now;

    }

    // Update is called once per frame
    void Update()
    {
        Test();

        if (!shaking)
        {
            return;
        }

        if (curCycle >= cycleCount)
        {
            return;
        }

        float deltaTime = unscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
        currentTime += deltaTime;
        while (currentTime >= cycleTime)
        {
            currentTime -= cycleTime;
            curCycle++;
            if (curCycle >= cycleCount)
            {
                //end shaking
                myTransform.localPosition = startPosition;
                myTransform.localEulerAngles = startAngles;
                shaking = false;
                double cost = (DateTime.Now - now).TotalMilliseconds;
                //SGF.Debuger.Log($"total cost ---> {cost} ms");
                return;
            }

            if (!fixShake)
            {
                if (positionShake != Vector3.zero)
                    curPositonShake = (cycleCount - curCycle) * positionShake / cycleCount;
                if (angleShake != Vector3.zero)
                    curAngleShake = (cycleCount - curCycle) * angleShake / cycleCount;
            }
        }
        // SGF.Debuger.Log($"currentTime ---> {currentTime}   deltaTime {deltaTime} ms");

        if (curCycle < cycleCount)
        {
            float offsetScale = randomDir * Mathf.Sin((bothDir ? 2 : 1) * Mathf.PI * currentTime / cycleTime);
            if (positionShake != Vector3.zero)
            {
                myTransform.localPosition = startPosition + curPositonShake * offsetScale;
                double cost = (DateTime.Now - now).TotalMilliseconds;
                //SGF.Debuger.Log($"cost ---> {cost} ms");
            }
            if (angleShake != Vector3.zero)
                myTransform.localEulerAngles = startAngles + curAngleShake * offsetScale;
        }
    }

    private void Test()
    {
        return;
        //测试代码
        //if (Input.GetMouseButtonDown(0))
        //{
        //    cycleCount = 3;
        //    Restart();
        //    SGF.Debuger.Log($"cycleCount ---> {cycleCount}");
        //}

        //if (Input.GetMouseButtonDown(1))
        //{
        //    cycleCount = 2;
        //    Restart();
        //    SGF.Debuger.Log($"cycleCount ---> {cycleCount}");

        //}

        //if (Input.GetMouseButtonDown(2))
        //{
        //    Restart();
        //    SGF.Debuger.Log($"Restart ");

        //}
    }

}
