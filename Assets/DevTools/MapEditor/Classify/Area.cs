///--------------------------------------------------------------------
/// 文件名   :   Area
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 11:29:58
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using OfficeOpenXml;
using System.Text;
using StarProject.Service.Language;
using System.Reflection;
using Task;

namespace MapEditor
{
    [DisallowMultipleComponent]
    [HideMonoScript]
    [SelectionBase]
    public class Area : ControlRelated
    {



        private IEnumerable _areaTypes = new ValueDropdownList<AreaType>()
        {
            { "Buff区域", AreaType.BuffZone },
            { "复活区域", AreaType.ReviveZone },
            { "出生点", AreaType.BronPoint },
            {"触发区域",AreaType.TriggerZone},
            {"管辖区域",AreaType.Jurisdiction}
        };

        private IEnumerable _shapeTypes = new ValueDropdownList<AreaShape>()
        {
            { "矩形", AreaShape.Rectangle },
            { "圆形", AreaShape.Circle },
            { "多边形", AreaShape.Polygon },
        };

        private IEnumerable _colliderTypes = new ValueDropdownList<ColliderType>()
        {
            { "可进可出", ColliderType.InAndOut },
            { "可进不可出", ColliderType.OnlyIn },
            { "不可进可出", ColliderType.OnlyOut },
            { "不可进出", ColliderType.NoneInAndOut },
        };
        

        private IEnumerable _blockshowtypes = new ValueDropdownList<BlockShowType>()
        {
            { "随时可见", BlockShowType.Always },
            { "靠近可见", BlockShowType.Near },
            { "永不可见", BlockShowType.Never },

        };

        [FoldoutGroup("基础信息")]
        [LabelText("显示颜色")]
        public Color gizmosColor = new Color(1, 1, 1, 0.3137f);


        [FoldoutGroup("基础信息")]
        [LabelText("区域ID")]
        public int AreaID;

        [FoldoutGroup("基础信息")]
        [LabelText("阵营ID")]
        public int Faction;
        
        [FoldoutGroup("基础信息")]
        [LabelText("区域类型")]
        [ValueDropdown("_areaTypes")]
        public AreaType areaType = AreaType.BuffZone;

        [FoldoutGroup("基础信息")]
        [LabelText("触发次数")]
        [SuffixLabel("(-1 不限制次数)")]
        public int TriggerCount;
        
        [FoldoutGroup("基础信息")]
        [LabelText("区域形状")]
        [ValueDropdown("_shapeTypes")]
        [OnValueChanged("OnAreaShapeChange")]
        public AreaShape shapType = AreaShape.Rectangle;

        [FoldoutGroup("基础信息")]
        [LabelText("长")]
        [ShowIf("IsRectanhle")]
        public int Length;

        [FoldoutGroup("基础信息")]
        [LabelText("宽")]
        [ShowIf("IsRectanhle")]
        public int Width;

        [FoldoutGroup("基础信息")]
        [LabelText("半径")]
        [ShowIf("IsCircle")]
        public int Radius;
        
        [FoldoutGroup("基础信息")]
        [ShowInInspector, PropertyOrder(-1),LabelText("区域名称")]
        public string AreaName
        {
            get
            {
#if UNITY_EDITOR
                //编辑器模式直接返回中文
                if (!Application.isPlaying)
                {
                    return areaName;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(AreaName_Key);
            }
            set { areaName = value; }
        }
        private string areaName;
        [HideInInspector]
        public string AreaName_Key;

        [FoldoutGroup("基础信息")]
        [LabelText("节点")]
        [ShowIf("IsPolygon")]
        public List<Transform> Polygons = new List<Transform>() { };

        //[FoldoutGroup("基础信息")]
        //[LabelText("区域阻挡类型")]
        //[ValueDropdown("_colliderTypes")]
        //public ColliderType colliderTypes = ColliderType.InAndOut;

        [FoldoutGroup("基础信息")] 
        [LabelText("阻挡效果特效ID")]
        public int blockEffect;

        [FoldoutGroup("基础信息")]
        [LabelText("阻挡边缘显示类型")]
        [ValueDropdown("_blockshowtypes")]
        public BlockShowType showType = BlockShowType.Always;

        [FoldoutGroup("基础信息")]
        [LabelText("生效人数")]
        [PropertyTooltip("0为不限制")]
        public int EffectNums;

        [FoldoutGroup("基础信息")]
        [LabelText("死亡是否生效")]
        public bool DeadValid = true;

        //[FoldoutGroup("基础信息")]
        //[LabelText("进入任务")]
        //public int EntryTaskID;

        //[FoldoutGroup("基础信息")]
        //[LabelText("离开任务")]
        //public int ExitTaskID;

        //[FoldoutGroup("基础信息")]
        //[LabelText("进入技能")]
        //public int EntrySkillID;

        //[FoldoutGroup("基础信息")]
        //[LabelText("进入技能等级")]
        //public int EntrySkillLevel;

        //[FoldoutGroup("基础信息")]
        //[LabelText("离开技能")]
        //public int ExitSkillID;

        //[FoldoutGroup("基础信息")]
        //[LabelText("离开技能等级")]
        //public int ExitSkillLevel;


        [FoldoutGroup("基础信息")]
        [LabelText("进入效果")]
        public List<EventEffectData> EnterEventEffect;


        [FoldoutGroup("基础信息")]
        [LabelText("退出效果")]
        public List<EventEffectData> ExitEventEffect;

        [ShowInInspector]
        [ShowIf("IsSpawnAreaType")]
        [LabelText("出生点生效条件")]
        [InlineProperty]
        public List<TaskSubContionGroup> SpawnPointActiveConditions;

        [Button("一键映射引用")]
        public void SetReference()
        {
            FreshReference();
        }

        public void AddPoint()
        {
            Vector3 position = Vector3.zero;
            MapEditorUtils.GetScreenPosition(out position);
            Vector3 scale = Vector3.one * 0.2f;
            int layer = LayerMask.NameToLayer("Entity");
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.SetParent(transform);
            point.transform.localPosition = (position - transform.position);
            point.transform.localRotation = Quaternion.identity;
            point.transform.localScale = scale;
            point.layer = layer;
            Polygons.Add(point.transform);
        }



        public override void OnGround()
        {
            Vector3 position = transform.position;
            transform.position = MapEditorUtils.GetGroundPoint(position);

            var transforms = Polygons;

            for (int i = 0; i < transforms.Count; i++)
            {
                Polygons[i].position = MapEditorUtils.GetGroundPoint(transforms[i].position);
            }
        }

        void FreshReference()
        {
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
        }

        void Find(GameObject go)
        {
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
                    Area value = fields[j].GetValue(component) as Area;
                    if (value == this)
                    {
                        FreshRule freshRule = go.GetComponent<FreshRule>();
                        if (freshRule != null)
                        {
                            freshRule.ChaseAreaID = AreaID;
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

        private bool IsSpawnAreaType()
        {
            return areaType == AreaType.BronPoint;
        }

        private bool IsRectanhle()
        {
            return shapType == AreaShape.Rectangle;
        }

        private bool IsCircle()
        {
            return shapType == AreaShape.Circle;
        }

        private bool IsPolygon()
        {
            return shapType == AreaShape.Polygon;
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

        public override void ExportExcel(int row, ExcelRange excel, int index)
        {
            base.ExportExcel(row, excel, index);
        }

        public override void Load()
        {
        }

        protected override void Awake()
        {
            base.Awake();


        }

        private Color GetGizmosColor()
        {
            if (MapEditorUtils.SceneConfig != null)
            {
                switch (areaType)
                {
                    case AreaType.BronPoint:
                        return MapEditorUtils.SceneConfig.bronColor;
                    case AreaType.ReviveZone:
                        return MapEditorUtils.SceneConfig.reliveColor;
                  //  case AreaType.SafeZone:
                       // return MapEditorUtils.SceneConfig.safeColor;
                }
            }
            return gizmosColor;
        }

        protected override void OnDrawGizmos()
        {
            Color color = Handles.color;
            Handles.color = GetGizmosColor();
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

                Handles.DrawAAConvexPolygon(rectVerts);
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
                    Handles.DrawAAConvexPolygon(vectors.ToArray());
                }

            }
            Handles.color = color;

        }
        private void ClearChild()
        {
            int count = transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);

            }
        }
        public void ReshModel()
        {
            ClearChild();
            var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            model.layer = LayerMask.NameToLayer("Entity");
            model.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
#endif