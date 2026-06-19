using Sirenix.OdinInspector;
using SkillEditor;
using System;
///--------------------------------------------------------------------
/// 文件名   :   SkillEditorGlobal.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/06 09:20:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SkillEditorGlobal : MonoBehaviour
{
    //自定义黑板Key
    [LabelText("自定义黑板KEY")]
    public static List<string> DefualtKeys = new List<string>() { "", "Builder", "Owner", "AllVictims", "Victim", "EventBuilder", "InputTarget", "InputCoord", "InpuRota", "StackCount", "ShieldVal", "OriginalMaster", "DirectMaster", "EnergyInterval" };

    //Json导出目录
    private static string jsonExportPath = "Assets/DevTools/SkillEditor/Export/Json";
    //资源文件导出目录
    public static string assteExportPath = "Assets/DevTools/SkillEditor/Export/Timeline";

    //Timeline创建默认轴
    public static TimelineConfig DefaultTrack = new TimelineConfig();

    [LabelText("技能Json导出目录")]
    [HideInInspector]
    public string SkillJsonExportPath1 = "Assets/DevTools/SkillEditor/Export/Json/Skill";

    [LabelText("BUFFJson导出目录")]
    [HideInInspector]
    public string BuffJsonExportPath1 = "Assets/DevTools/SkillEditor/Export/Json/Buff";

    [LabelText("子弹Json导出目录")]
    [HideInInspector]
    public string BulletJsonExportPath1 = "Assets/DevTools/SkillEditor/Export/Json/Bullet";

    [LabelText("被动Json导出目录")]
    [HideInInspector]
    public string PassiveJsonExportPath1 = "Assets/DevTools/SkillEditor/Export/Json/Passive";

    [LabelText("全部特效的参数json目录")]
    [HideInInspector]
    public string FxJsonDetailPath = "Assets/DevTools/SkillEditor/Export/Json/FxDetail";

    [LabelText("服务器Json导出目录")]
    [HideInInspector]
    public string ServerJsonExportPath1 = "../../../../../StarsProject_Server/trunk/gameserver/res/battle";

    [LabelText("客户端Json复制目录")]
    [HideInInspector]
    public string ClientCopyPath = "DevTools/SkillEditor/Export/Json";

    [LabelText("客户端Json粘贴目录")]
    [HideInInspector]
    public string ClientPastePath = "Res/Config/Skill";

    [LabelText("客户端TimeLine路径前缀")]
    [HideInInspector]
    public string ClientSkllTimeLinePathPrefix = "Assets/DevTools/SkillEditor/Export";

    [LabelText("技能配置 最终 导入项目res目录下的路径")]
    [HideInInspector]
    public string SkillConfigPath = "Assets/Res/Config/Skill";

    [LabelText("Timeline配置")]
    [HideInInspector]
    public TimelineConfigList timelineConfig;

    public EditorCameraType EditorCameraType = EditorCameraType.Right;
    public float Distance = 12f;
    public float TopDownPerspective = 43f;

    private static SkillEditorGlobal _instance;
    public static SkillEditorGlobal Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = GameObject.Find("SkillEditor");
                if (go != null)
                {
                    _instance = go.GetComponent<SkillEditorGlobal>();
                }

                if (_instance == null)
                {
                    CreateInstance();
                }
            }
            return _instance;
        }
    }
    public static void CreateInstance()
    {
        var go = GameObject.Find("SkillEditor");
        if (go == null)
        {
            go = new GameObject("SkillEditor");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }
        _instance = go.AddComponent<SkillEditorGlobal>();
    }

    //场景中的模型
    private static GameObject _model;
    public static GameObject Model
    {
        get
        {
            if (_model == null)
            {
                var go = GameObject.Find("Model");
                if (go == null)
                {
                    go = new GameObject("Model");
                    go.transform.position = Vector3.zero;
                    go.transform.rotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                _model = go;
            }
            return _model;
        }
    }

    //场景中的敌人
    private GameObject _enemy;
    public GameObject Enemy
    {
        get
        {
            if (_enemy == null)
            {
                var go = GameObject.Find("Enemy");
                if (go == null)
                {
                    go = new GameObject("Enemy");
                    go.transform.position = Vector3.zero;
                    go.transform.rotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                _enemy = go;
            }
            return _enemy;
        }
    }

    public static PlayableDirector PlayableDirector;

    public static void SetCurOffset(Vector3 vector3)
    {
        if (Model.transform.GetChildren().Count != 0)
        {
            Model.transform.GetChild(0).transform.position = vector3;
        }
    }

    //场景中的相机
    private static GameObject _camera;
    public static GameObject Camera
    {
        get
        {
            if (_camera == null)
            {
                var go = GameObject.Find("SkilleditorCamera");
                if (go == null)
                {
                    UnityEngine.Debug.Log("该场景无法预览震屏，请回到编辑器进行预览");
                    return null;
                }
                _camera = go;
            }
            return _camera;
        }
    }
    //摄像机当前偏移
    public static Vector3 CurCameraOffset = Vector3.zero;

    //通过被施加的所有偏移计算当前摄像机的实际偏移
    public void SetCameraCurOffset(Vector3 vector3)
    {
        if (Camera != null)
        {
            var result = GetCameraOffset(vector3);
            Camera.transform.position = result.Item1;
            Camera.transform.rotation = result.Item2;
#if UNITY_EDITOR
            //俯视需要修改为正交
            if (SkillEditorData.EditorCameraType == EditorCameraType.Overlook)
            {
                Camera.GetComponent<Camera>().orthographic = true;
            }
            else
            {
                Camera.GetComponent<Camera>().orthographic = false;
            }
#endif
        }
    }

    public (Vector3,Quaternion) GetCameraOffset(Vector3 vector3)
    {
        float distance = Distance;
        float topDownPerspective = TopDownPerspective;

        distance -= vector3.z;

#if UNITY_EDITOR
        switch (SkillEditorData.EditorCameraType)
        {
            case EditorCameraType.Left:
                {
                    //先计算相机初始点
                    float startX = -distance * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float startY = distance * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);

                    //在计算传入的偏移参数
                    float offsetX = vector3.y * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);
                    float offsetY = vector3.y * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float offsetZ = vector3.x;
                    return (new Vector3(startX + offsetX, startY + offsetY, offsetZ),Quaternion.Euler(topDownPerspective,90,0));
                }
            case EditorCameraType.Front:
                {
                    //先计算相机初始点
                    float startX = distance * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float startY = distance * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);

                    //在计算传入的偏移参数
                    float offsetX = -vector3.y * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);
                    float offsetY = vector3.y * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float offsetZ = vector3.x;
                    return (new Vector3(offsetZ, startY + offsetX, startX + offsetY),Quaternion.Euler(topDownPerspective,180,0));
                }
            case EditorCameraType.Behind:
                {
                    //先计算相机初始点
                    float startX = -distance * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float startY = distance * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);

                    //在计算传入的偏移参数
                    float offsetX = vector3.y * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);
                    float offsetY = vector3.y * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float offsetZ = vector3.x;
                    return (new Vector3(offsetZ, startY + offsetX, startX + offsetY), Quaternion.Euler(topDownPerspective, 0, 0));
                }
            case EditorCameraType.Overlook:
                {
                    //先计算相机初始点
                    float startY = distance;

                    //在计算传入的偏移参数
                    float offsetX = vector3.y;
                    float offsetY = 0;
                    float offsetZ = vector3.x;
                    return (new Vector3(offsetX, startY + offsetY, offsetZ), Quaternion.Euler(90, -90, 0));
                }
            default:
                {
                    //先计算相机初始点
                    float startX = distance * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float startY = distance * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);

                    //在计算传入的偏移参数
                    float offsetX = -vector3.y * Mathf.Sin(topDownPerspective * Mathf.Deg2Rad);
                    float offsetY = vector3.y * Mathf.Cos(topDownPerspective * Mathf.Deg2Rad);
                    float offsetZ = vector3.x;
                    return (new Vector3(startX + offsetX, startY + offsetY, offsetZ),Quaternion.Euler(topDownPerspective, -90, 0));
                }
        }
#endif
        return (Vector3.zero, Quaternion.identity);
    }

    //场景中的展示效果
    private GameObject _effect;
    public GameObject Effect
    {
        get
        {
            if (_effect == null)
            {
                var go = GameObject.Find("ShowEffect");
                if (go == null)
                {
                    go = new GameObject("ShowEffect");
                    go.transform.position = Vector3.zero;
                    go.transform.rotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                _effect = go;
            }
            return _effect;
        }
    }

    //根据目标的类型返回存储的位置
    public static string GetJsonPath(BattleRuntimeTypeEnum explorerItemType)
    {
        return jsonExportPath + explorerItemType.ToString();
    }

    //Wwise是利用Gob的instId或者md5
    //同类替换就同名，目前
    //完全重新就都new
    //全部共享就gob一个
    private static GameObject _WwiseListenerTestRoot;
    public static GameObject WwiseListenerTestRoot
    {
        get
        {
            if (_WwiseListenerTestRoot == null)
            {
                var go = GameObject.Find("WwiseListenerTestRoot");

                if (go == null)
                {
                    //创建
                    _WwiseListenerTestRoot = new GameObject("WwiseListenerTestRoot");
                }
                else
                {
                    //补关联
                    _WwiseListenerTestRoot = go;
                }
            }
            //补缓存
            return _WwiseListenerTestRoot;
        }
    }

    public BaseGenera GetGenera()
    {
#if UNITY_EDITOR
        if (UnityEditor.Timeline.TimelineEditor.masterDirector != null)
        {
            BaseGenera bulletGenera = UnityEditor.Timeline.TimelineEditor.masterDirector.gameObject.transform.GetComponentInParent<BaseGenera>();
            return bulletGenera;
        }
        else
        {
            if (UnityEditor.Selection.activeGameObject != null)
            {
                BaseGenera bulletGenera = UnityEditor.Selection.activeGameObject.transform.GetComponentInParent<BaseGenera>();
                return bulletGenera;
            }
        }
#endif
        return null;
    }

    public int GetStageIndex()
    {
        BaseGenera baseGenera = GetGenera();
        if (baseGenera != null)
        {
            return baseGenera.GetStageIndex();
        }
        return 0;
    }

    public static Type GetStrType(string str)
    {
        System.Type type = System.Type.GetType(str);
        return type;
    }
}
