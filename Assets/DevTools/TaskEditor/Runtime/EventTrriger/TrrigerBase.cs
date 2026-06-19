///--------------------------------------------------------------------
/// 文件名   :   TrrigerBase.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/04/05 15:56:39
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Task;
using UnityEngine;

[HideMonoScript]
public class TrrigerBase : MonoBehaviour
{


    private IEnumerable _shapeTypes = new ValueDropdownList<AreaShape>()
        {
            { "矩形", AreaShape.Rectangle },
            { "圆形", AreaShape.Circle },
            { "多边形", AreaShape.Polygon },
        };

    [LabelText("触发器类型")]
    [ValueDropdown("GetTrrigerType")]
    public TrrigerType TrrigerType = TrrigerType.PropertyTrriger;

    [LabelText("触发器ID")]
    [OnValueChanged("FreshReference")]
    public int ID;

    [LabelText("触发次数")]
    [SuffixLabel("-1不限次数")]
    public int Count;



    //位置触发器
    [LabelText("半径")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [SuffixLabel("单位厘米")]
    [ShowIf("IsCircle")]
    public int Radius;

    [LabelText("形状")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ValueDropdown("_shapeTypes")]
    [OnValueChanged("OnAreaShapeChange")]
    [ShowIf("IsPositionTrigger")]
    public AreaShape shapType = AreaShape.Circle;

    [FoldoutGroup("$GetTriggerName", 0)]
    [LabelText("GizmosColor")]
    [ShowIf("IsPositionTrigger")]
    public Color GizmosColor = Color.green;

    [ShowIf("IsPolygon")]
    [FoldoutGroup("$GetTriggerName", 0)]
    public List<Transform> Polygons = new List<Transform>() { };

    [FoldoutGroup("$GetTriggerName", 0)]
    [LabelText("长")]
    [ShowIf("IsRectanhle")]
    public int Length;

    [FoldoutGroup("$GetTriggerName", 0)]
    [LabelText("宽")]
    [ShowIf("IsRectanhle")]
    public int Width;

    [LabelText("@GetTriggerType")]
    [FoldoutGroup("$GetTriggerName", 0)]
    public bool IsEnterTrigger = true;



    public bool IsPositionTrigger()
    {
        return TrrigerType == TrrigerType.PositionTrriger;
    }

    //定时触发器
    [LabelText("延迟时间")]
    [SuffixLabel("单位毫秒")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsTimerTrigger")]
    public int Delay;

    [LabelText("触发间隔")]
    [SuffixLabel("单位毫秒")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsTimerTrigger")]
    public int Interval;

    public bool IsTimerTrigger()
    {
        return TrrigerType == TrrigerType.TimerTrriger;
    }

    //属性触发器
    [LabelText("@GetTargetName")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsPropertyTrriger")]
    public bool IsSelf;

    [LabelText("属性名")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsPropertyTrriger")]
    public string PropName;

    [LabelText("属性值")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsPropertyTrriger")]
    public string PropValue;

    [LabelText("比较符")]
    [FoldoutGroup("$GetTriggerName", 0)]
    [ShowIf("IsPropertyTrriger")]
    [ValueDropdown("GetCompareEnumTypes")]
    public CompareEnum Compare;

    [LabelText("被NPC引用列表")]
    [ReadOnly]
    [ShowInInspector]
    private List<long> NpcList;

    public void DeleteNpc(long npcId)
    {
        if (NpcList != null && NpcList.Contains(npcId))
        {
            NpcList.Remove(npcId);
        }
    }

    public void AddNpc(long npcId)
    {
        if (NpcList == null)
        {
            NpcList = new List<long>();
        }
        if (!NpcList.Contains(npcId))
        {
            NpcList.Add(npcId);
        }
    }

    public bool IsPropertyTrriger()
    {
        return TrrigerType == TrrigerType.PropertyTrriger;
    }

    public IEnumerable GetCompareEnumTypes()
    {
        return TaskEnumUtils._compareenum;
    }

    private bool IsRectanhle()
    {

        return IsPositionTrigger() && shapType == AreaShape.Rectangle;
    }

    private bool IsCircle()
    {
        return IsPositionTrigger() && shapType == AreaShape.Circle;
    }

    private bool IsPolygon()
    {
        return IsPositionTrigger() && shapType == AreaShape.Polygon;
    }

    public void OnAreaShapeChange()
    {
        bool hide = true;
        if (IsPolygon())
        {
            if (Polygons.Count == 0)
            {
                Vector3 scale = Vector3.one * 0.2f;
                int layer = LayerMask.NameToLayer("Entity");
                GameObject point1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point1.transform.SetParent(transform);
                point1.transform.localPosition = Define.LeftUp;
                point1.transform.localRotation = Quaternion.identity;
                point1.transform.localScale = scale;
                point1.layer = layer;
                Polygons.Add(point1.transform);

                GameObject point2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point2.transform.SetParent(transform);
                point2.transform.localPosition = Define.RightUp;
                point2.transform.localRotation = Quaternion.identity;
                point2.transform.localScale = scale;
                point2.layer = layer;
                Polygons.Add(point2.transform);


                GameObject point3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point3.transform.SetParent(transform);
                point3.transform.localPosition = Define.RightDown;
                point3.transform.localRotation = Quaternion.identity;
                point3.transform.localScale = scale;
                point3.layer = layer;
                Polygons.Add(point3.transform);

                GameObject point4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point4.transform.SetParent(transform);
                point4.transform.localPosition = Define.LeftDown;
                point4.transform.localRotation = Quaternion.identity;
                point4.transform.localScale = scale;
                point4.layer = layer;

                Polygons.Add(point4.transform);
            }

            hide = false;
        }

        if (IsRectanhle())
        {
            if (Width == 0 && Length == 0)
            {
                Width = 100;
                Length = 100;
            }
        }

        if (IsCircle())
        {
            if (Radius == 0)
            {
                Radius = 100;
            }
        }

        if (Polygons != null && Polygons.Count > 0)
        {
            foreach (var item in Polygons)
            {
                item.gameObject.SetActive(!hide);
                item.gameObject.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;
            }
        }
    }

    public string GetTargetName
    {
        get
        {
            return IsSelf ? "自身" : "主角";
        }
    }

    //[FoldoutGroup("效果模块", 1)]
    //[LabelText("可执行效果")]
    //[HideInTables]
    //public List<TrrigerEffect> TrrigerEffects;


    public string GetTriggerType
    {
        get
        {
            return IsEnterTrigger ? "进入触发" : "离开触发";
        }
    }
    public string GetTriggerName()
    {
        return TrrigerType switch
        {
            TrrigerType.PositionTrriger => "位置触发器",
            TrrigerType.TimerTrriger => "时间触发器",
            TrrigerType.PropertyTrriger => "属性触发器"
        };
    }

    public IEnumerable GetTrrigerType()
    {
        return Task.TaskEnumUtils._trrigertype;
    }

    [Button("一键映射引用")]
    public void SetReference()
    {
        FreshReference();
    }

    [Button("一键贴地")]
    public virtual void OnGround()
    {
#if UNITY_EDITOR
        Vector3 position = transform.position;
        transform.position = MapEditor.MapEditorUtils.GetGroundPoint(position);

        var transforms = Polygons;

        for (int i = 0; i < transforms.Count; i++)
        {
            Polygons[i].position = MapEditor.MapEditorUtils.GetGroundPoint(transforms[i].position);
        }
#endif
    }

    void FreshReference()
    {
#if UNITY_EDITOR
        GameObject[] goes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < goes.Length; i++)
        {
            GameObject go = goes[i];
            if (go.scene.name != null)
            {
                if (go != gameObject)
                {
                    Find(go);
                }
            }
        }
#endif
    }

    void Find(GameObject go)
    {
#if UNITY_EDITOR
        if (go == null)
        {
            return;
        }
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }
            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int j = 0; j < fields.Length; j++)
            {
                List<MapEditor.TriggerGroup> list = fields[j].GetValue(component) as List<MapEditor.TriggerGroup>;
                if (list != null)
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (list[k] != null && list[k].trriger == this)
                        {
                            list[k].TriggerID = ID;
                        }
                    }
                }

            }
        }

        int ChildCount = go.transform.childCount;
        if (ChildCount > 0)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var trans = go.transform.GetChild(i);
                if (trans != null && trans.gameObject != null)
                {
                    Find(trans.gameObject);
                }
            }
        }
#endif
    }

    public List<Vector3> PolygonsToString()
    {
        List<Vector3> vectors = new List<Vector3>();
        if (Polygons != null && Polygons.Count > 0)
        {
            foreach (var item in Polygons)
            {
                vectors.Add(item.position);
            }
        }

        return vectors;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Color color = UnityEditor.Handles.color;
        UnityEditor.Handles.color = GizmosColor;
        UnityEditor.Handles.Label(transform.position, GetTriggerName());
        if (IsPositionTrigger())
        {

            if (shapType == AreaShape.Rectangle)
            {
                var leftUnit = Vector3.Cross(transform.forward, Vector3.up);
                float halflen = Length * 0.01f / 2;
                float halfwid = Width * 0.01f / 2;
                var leftBot = transform.position + (leftUnit * halflen) - (transform.forward * halfwid);
                var leftTop = transform.position + (leftUnit * halflen) + (transform.forward * halfwid);
                var rightBot = transform.position - (leftUnit * halflen) - (transform.forward * halfwid);
                var righTop = transform.position - (leftUnit * halflen) + (transform.forward * halfwid);
                Vector3[] rectVerts = new Vector3[4] { leftBot, leftTop, righTop, rightBot };

                UnityEditor.Handles.DrawAAConvexPolygon(rectVerts);
            }

            if (shapType == AreaShape.Circle)
            {
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, Radius * 0.01f);
            }

            if (shapType == AreaShape.Polygon)
            {
                if (Polygons.Count > 0)
                {
                    List<Vector3> vectors = new List<Vector3>();
                    foreach (var item in Polygons)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        vectors.Add(item.position);// + transform.position;
                    }
                    vectors.Add(vectors[0]);
                    UnityEditor.Handles.DrawAAConvexPolygon(vectors.ToArray());
                }

            }
        }
        UnityEditor.Handles.color = color;

#endif
    }
}

[System.Serializable]
public class TrrigerEffect
{

    [FoldoutGroup("条件模块")]
    [ShowInInspector]
    [LabelText("执行条件")]
    [InlineProperty]
    public List<TaskSubContionGroup> Conditions;


    [FoldoutGroup("效果模块")]
    [ShowInInspector]
    [LabelText("效果列表")]
    [InlineProperty]
    public List<EffectSerialize> EditorEffects;

    [HideInInspector]
    public List<EffectJsonData> Effects;

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        if (EditorEffects == null)
        {
            EditorEffects = new List<EffectSerialize>();
        }
        EditorEffects.Clear();
        if (Effects != null)
        {
            foreach (var item in Effects)
            {
                EffectSerialize effectSerialize = new EffectSerialize(item);
                effectSerialize.EffectType = item.EffectType;
                EditorEffects.Add(effectSerialize);
            }
        }

    }

    [OnSerializing]
    public void OnSerializd(StreamingContext context)
    {
        if (Effects == null)
        {
            Effects = new List<EffectJsonData>();
        }
        Effects.Clear();
        if (EditorEffects != null)
        {
            foreach (var item in EditorEffects)
            {
                EffectJsonData effectJson = new EffectJsonData(item);
                Effects.Add(effectJson);
            }
        }
    }
}
