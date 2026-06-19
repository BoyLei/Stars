using UnityEngine;
using StarProject.Service.Cam.Data;
using DG.Tweening;
using StarProjectDef;
using static UnityEngine.ParticleSystem;
using StarProject;
using SkillEditor;

using StarProject.Service.LocalData;
using SGF.Unity;
using System.Collections;
using System;


public class TestDL : MonoBehaviour
{
    private Vector3 _currentPostion;
    public float shakeCD = 0.002f;

    public int shakeCount = -1;

    private float _shakeTime;

    private Transform cameraTransform;

    public CameraMoveType cameraMoveType = CameraMoveType.Zoom;

    public Transform gob;

    public float rX = 0;
    public float rY = 0;
    public float rZ = 0;

    public float angle = 0;

    public static TestDL Instance;

    // Start is called before the first frame update
    void Start()
    {
        // Invoke("TestCameraShake", 20);
        // Invoke("TestShake", 20);

        // cameraTransform = Camera.main.transform;
        // _currentPostion = cameraTransform.position;
        // shakeCount = Random.Range(50, 60);

        // var c = new C();
        // c.Test();
        //StarProject.Service.Resource.ResourceManager.Instance.Init();
        // StarProject.Service.Resource.ResourceFormalManager.Instance.Init();

        // LocalDataManager.Instance.Init();

        // Instance = this;

        TestMonoUpdaterEvent();
    }

    public delegate void MonoUpdaterEvent();

    MonoUpdaterEvent monoUpdaterEvent;
    void TestMonoUpdaterEvent()
    {
        monoUpdaterEvent += Print;
        monoUpdaterEvent += RemovePrint;
        monoUpdaterEvent += Print1;
        monoUpdaterEvent += Print2;
        monoUpdaterEvent += Print3;

        monoUpdaterEvent.Invoke();
        Debug.Log($"==============");

        monoUpdaterEvent.Invoke();

    }

    void Print()
    {
        Debug.Log(0);
    }

    void Print1()
    {
        Debug.Log(1);

    }

    void Print2()
    {
        Debug.Log(2);

    }
    void Print3()
    {
        Debug.Log(3);

    }

    void RemovePrint()
    {
        Debug.Log($"准备移除 {monoUpdaterEvent.GetInvocationList().Length}");
        monoUpdaterEvent -= Print1;
        monoUpdaterEvent -= Print2;
        Debug.Log($"剩余 {monoUpdaterEvent.GetInvocationList().Length}");

    }


    void TestDestroy()
    {
        var go = new GameObject();
    }



    void TestGc()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        Debug.LogError("GC 完成");
    }



    void TestCameraShake()
    {
        Debug.Log("TestCameraShake~~~~~~~~~~~~~~~");
        CameraFxParam cameraFxParam = new CameraFxParam();
        cameraFxParam.E_CameraEvent = CameraEvent.ShakeCam;
        // CameraManager.Instance.DoCamFxSync(new[] { cameraFxParam }, E_CameraType.StarWorldCam);
    }

    void TestShake()
    {
        // transform.DOShakePosition(0.2, 1.0f);
        // transform.DOMove(new Vector3(10, 0, 0), 0.1f);
        // transform.DOMove(new Vector3(-10, 0, 0), 0.1f);
        // transform.DOMove(new Vector3(15, 0, 0), 0.1f);
        // transform.DOMove(new Vector3(-15, 0, 0), 0.1f);
        // transform.DOMove(new Vector3(6, 0, 0), 0.1f);
        // transform.DOMove(new Vector3(-6, 0, 0), 0.1f);

        // transform.DOMoveX(10, 0.1f);
        // transform.dok
        // Camera.main.transform.

        // if (_shakeTime + shakeCD < Time.time && shakeCount > 0)
        // {
        //     shakeCount--;
        //     float radio = Random.Range(-0.01f, 0.01f);

        //     if (shakeCount == 1)
        //     {
        //         radio = 0;
        //     }
        //     _shakeTime = Time.time;
        //     cameraTransform.position = _currentPostion + Vector3.one * radio;
        // }

        cameraTransform.DOShakePosition(5, new Vector3(1, 1, 0), 10, 90, false);
    }


    // void Update()
    // {
    //     TestShake();
    // }


    public Vector3 positionShake;//震动幅度
    public Vector3 angleShake;   //震动角度
    public float cycleTime = 0.2f;//震动周期
    public int cycleCount = 3;    //震动次数
    public bool fixShake = false; //为真时每次幅度相同，反之则递减
    public bool unscaleTime = false;//不考虑缩放时间
    public bool bothDir = true;//双向震动
    public bool autoDisable = false;//自动disbale


    float currentTime;
    int curCycle;
    Vector3 curPositonShake;
    Vector3 curAngleShake;
    float curFovShake;
    Vector3 startPosition;
    Vector3 startAngles;
    Transform myTransform;



    int randomDir = 1;
    void OnEnable()
    {
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform = transform;
        startPosition = myTransform.localPosition;
        startAngles = myTransform.localEulerAngles;
    }

    void OnDisable()
    {
        myTransform.localPosition = startPosition;
        myTransform.localEulerAngles = startAngles;
    }

    // Update is called once per frame
    void Update()
    {


        Test();
        return;
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
                myTransform.localPosition = startPosition;
                myTransform.localEulerAngles = startAngles;
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

        if (curCycle < cycleCount)
        {
            float offsetScale = randomDir * Mathf.Sin((bothDir ? 2 : 1) * Mathf.PI * currentTime / cycleTime);
            if (positionShake != Vector3.zero)
                myTransform.localPosition = startPosition + curPositonShake * offsetScale;
            if (angleShake != Vector3.zero)
                myTransform.localEulerAngles = startAngles + curAngleShake * offsetScale;
        }
    }
    //重置
    public void Restart()
    {
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform.localPosition = startPosition;
        myTransform.localEulerAngles = startAngles;
        randomDir = UnityEngine.Random.Range(0f, 1f) <= 0.5f ? 1 : -1;
    }

    private void Test()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // TestGc();
            TestDestroy();
        }


        return;

        if (Input.GetMouseButtonDown(1))
        {
            // TestSimulatePs();
            TestCameraMove();
        }

        if (Input.GetMouseButtonDown(0))
        {
            // TestSimulatePs();
            TestCameraMove(false);
        }


        return;
        // return;
        //测试代码
        if (Input.GetMouseButtonDown(0))
        {
            cycleCount = 3;
            Restart();
            Debug.Log($"cycleCount ---> {cycleCount}");
        }

        if (Input.GetMouseButtonDown(1))
        {
            cycleCount = 2;
            Restart();
            Debug.Log($"cycleCount ---> {cycleCount}");

        }

        if (Input.GetMouseButtonDown(2))
        {
            Restart();
            Debug.Log($"Restart ");

        }
    }

    public Transform psParent;

    public GameObject psPrefab;

    public ParticleSystem mainPs;

    public ParticleSystem simulatePs;

    public float simulateTime = 0;

    public void TestSimulatePs()
    {
        psParent.DestroyChildren();
        GameObject gob = UnityEngine.Object.Instantiate<GameObject>(psPrefab);
        // mainPs = UnityEngine.Object.Instantiate<ParticleSystem>(psPrefab);
        mainPs = gob.transform.GetComponent<ParticleSystem>();
        mainPs.transform.parent = psParent;
        MainModule main = mainPs.main;
        mainPs.Simulate(0, true, true, true);
        mainPs.Play();

        GameObject gob2 = UnityEngine.Object.Instantiate<GameObject>(psPrefab);
        simulatePs = gob2.transform.GetComponent<ParticleSystem>();
        //simulatePs = Instantiate<ParticleSystem>(psPrefab);
        simulatePs.transform.parent = psParent;
        MainModule simulateMain = simulatePs.main;
        mainPs.Simulate(simulateTime, true, true, true);
        mainPs.Play();
    }


    GlobalShowGlobal_CameraMove cameraMove = new GlobalShowGlobal_CameraMove();
    public void TestCameraMove(bool isEnterIn = true)
    {
        {
            cameraMove.CameraMoveType = cameraMoveType;
        }

        cameraMove.Value = 30;
        cameraMove.Distance = 5;
        cameraMove.EnterTime = 500;
        cameraMove.LoopTime = -1000;
        cameraMove.EndTime = 200;

        GlobalEvent.OnCameraMoveEvent.Invoke(StarProject.Game.GameManager.Instance.mainPlayerId, cameraMove, isEnterIn);
    }


    public void DelayInvokerOnTime(float delayTime, DelayFunction func, params object[] args)
    {
        StartCoroutine(DelayInvokerOnTimeWorker(delayTime, func, args));
    }

    private static IEnumerator DelayInvokerOnTimeWorker(float delayTime, DelayFunction func, params object[] args)
    {
        yield return new WaitForSeconds(delayTime);
        ;

        try
        {
            func(args);
        }
        catch (Exception e)
        {
            SGF.Debuger.LogError("DelayInvoker", "DelayInvokerOnEndOfFrame() Error:{0}\n{1}", e.Message, e.StackTrace);
        }
        //Profiler.EndSample();
    }

    public static void DelayInvoker(float delayTime, DelayFunction func, params object[] args)
    {
        Instance.DelayInvokerOnTime(delayTime, func, args);
    }


}
