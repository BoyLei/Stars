//using System;
//using System.Collections;
using StarProject.Service.Cam;
using System.Collections.Generic;
//using NPOI.SS.Formula.Functions;
//using System.Linq;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Unity.Mathematics;
using UnityEngine;
//using UnityEngine.ProBuilder.Shapes;
using Object = UnityEngine.Object;

//用来控制贴花云影box大小与位置的脚本, 让云影box尽量和地表模型贴合并相交, 保证云影正常显示
public class DecalCloudShadowBoxController : MonoBehaviour
{
    [Header("贴花云影运动控制脚本")]
    private MeshRenderer thisMeshRenderer; //云影Box的MeshRender

    private BoxCollider thisBoxCollider;

    private Bounds meshBound;//云影box的Bound
    private float boundLength;//bound的长
    private float boundWidth;//bound的宽
    private float boundHeight;//bound的高

    [Header("云的运动方向,只设置X和Z即可,游戏启动后不要动,否则运动路径有误")]
    //[Tooltip("半透明扩散模型的初始的alpha强度")]
    public Vector3 direction;
    [Header("云的运动速度")]
    [Range(0.0f, 1f)]
    public float speed;
    [Header("规划好云影区域的Box,云影会在这个区域内运动")]
    public GameObject Box;
    // [Header("目标地形Terrain或者Mesh,启动游戏前需要设置,否则无法适配地形")]
    // public GameObject targetTerrain; //需要贴合的目标地形,可能是Terrain也可能是Mesh
    [Header("目标地形Terrain(必须得是terrain,注意位置要正确),启动游戏前需要设置,否则无法适配地形")]
    public Terrain targetTerrainObj;
    private Bounds terrainBounds; //地形的Bounds
    private Vector3 TerrainBoundsVertexA, TerrainBoundsVertexB, TerrainBoundsVertexC, TerrainBoundsVertexD; //地形Bounds的四个顶点(从上往下看的二维平面)
    private Vector3 lineVecBA, lineVecBC, lineVecCD, lineVecDA;

    private Vector3 StartPoint, EndPoint; //记录云影和terrainBounds相交的起点和终点

    private Camera mainCamera;

    private Collider theTerrainCollider; //Terrain上的collider

    private float OriginalYPos, OriginalYScale; //记录box原本的Y轴坐标和缩放

    private Color OriginalCloudColor;   //云影材质球的颜色

    private bool ifInSideMainCamera; //是否在主摄像机内

    
    [SerializeField] float updateInterval = 1f;     //时间间隔
    float curTime = 0f;
    //private 
    // Start is called before the first frame update
    void Start()
    {
        //初始化一些数据
        thisMeshRenderer = GetComponent<MeshRenderer>();
        meshBound = thisMeshRenderer.bounds;

        thisBoxCollider = GetComponent<BoxCollider>();
        //找到主相机

      /*  GameObject mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (!Object.ReferenceEquals(mainCamera, null))
        {
            mainCamera = mainCameraObj.GetComponent<Camera>(); //没有找到主相机云不移动

        }*/
        mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
        Transform parent = this.transform.parent;
        // while (parent!=null)
        // {
        //     parent.localRotation = quaternion.Euler(Vector3.zero);
        //     parent.localScale = Vector3.one;
        //     parent = parent.parent;
        // }

        if (targetTerrainObj != null)
        {
            if (targetTerrainObj.TryGetComponent<Terrain>(out var t))
            {
                Terrain terrain = targetTerrainObj.GetComponent<Terrain>();
                //TerrainCollider terrainCollider = targetTerrain.GetComponent<TerrainCollider>();
                //theTerrainCollider = terrainCollider;
                //terrainBounds = terrain.terrainData.bounds;
                terrainBounds = Box.GetComponent<MeshRenderer>().bounds;
                Vector3 terrainBoundsCenter = Vector3.zero;
                terrainBoundsCenter.Set(terrainBounds.center.x, 0, terrainBounds.center.z);
                Vector3 centerOffset = Vector3.zero;
                centerOffset.Set(terrainBounds.extents.x, 0, terrainBounds.extents.z);
                TerrainBoundsVertexA = terrainBoundsCenter + centerOffset;
                centerOffset.Set(-terrainBounds.extents.x, 0, terrainBounds.extents.z);
                TerrainBoundsVertexB = terrainBoundsCenter + centerOffset;
                centerOffset.Set(-terrainBounds.extents.x, 0, -terrainBounds.extents.z);
                TerrainBoundsVertexC = terrainBoundsCenter + centerOffset;
                centerOffset.Set(terrainBounds.extents.x, 0, -terrainBounds.extents.z);
                TerrainBoundsVertexD = terrainBoundsCenter + centerOffset;
                lineVecBA = Vector3.Normalize(TerrainBoundsVertexA - TerrainBoundsVertexB);
                lineVecBC = Vector3.Normalize(TerrainBoundsVertexC - TerrainBoundsVertexB);
                lineVecCD = Vector3.Normalize(TerrainBoundsVertexD - TerrainBoundsVertexC);
                lineVecDA = Vector3.Normalize(TerrainBoundsVertexA - TerrainBoundsVertexD);
            }
            else
            {
                //theTerrainCollider = targetTerrain.GetComponent<MeshCollider>();
                //terrainBounds = targetTerrain.GetComponent<MeshRenderer>().bounds;
                terrainBounds = Box.GetComponent<MeshRenderer>().bounds;
                Vector3 terrainBoundsCenter =  Vector3.zero;
                terrainBoundsCenter.Set(terrainBounds.center.x, 0, terrainBounds.center.z);
                Vector3 centerOffset = Vector3.zero;
                centerOffset.Set(terrainBounds.extents.x, 0, terrainBounds.extents.z);
                TerrainBoundsVertexA = terrainBoundsCenter + centerOffset;
                centerOffset.Set(-terrainBounds.extents.x, 0, terrainBounds.extents.z);
                TerrainBoundsVertexB = terrainBoundsCenter + centerOffset;
                centerOffset.Set(-terrainBounds.extents.x, 0, -terrainBounds.extents.z);
                TerrainBoundsVertexC = terrainBoundsCenter + centerOffset;
                centerOffset.Set(terrainBounds.extents.x, 0, -terrainBounds.extents.z);
                TerrainBoundsVertexD = terrainBoundsCenter + centerOffset;
                lineVecBA = Vector3.Normalize(TerrainBoundsVertexA - TerrainBoundsVertexB);
                lineVecBC = Vector3.Normalize(TerrainBoundsVertexC - TerrainBoundsVertexB);
                lineVecCD = Vector3.Normalize(TerrainBoundsVertexD - TerrainBoundsVertexC);
                lineVecDA = Vector3.Normalize(TerrainBoundsVertexA - TerrainBoundsVertexD);
            }

        }
        else
        {
            //Debug.Log("未给云影控制器设置地表Terrain");
        }

        if (Object.ReferenceEquals(direction, null))
        {
            direction = new Vector3(1, 0, 1);
            direction.Normalize();
        }

        if (Object.ReferenceEquals(speed, null))
        {
            speed = 1;
        }

        //计算云影box在运动范围内的起点和终点
        CalculatetheStartEndBox(this.transform.position, this.transform.localScale, direction, speed);

        //为该物体设置一个新的Materail instance 方便我们单独控制这个box的材质属性
        thisMeshRenderer.material = new Material(thisMeshRenderer.material);
        //记录云影box的初始颜色
        OriginalCloudColor = this.thisMeshRenderer.material.GetColor("_Color");

        OriginalYPos = thisMeshRenderer.transform.localPosition.y;
        OriginalYScale = thisMeshRenderer.transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;

        //移动box的xz轴
        float xPosition;
        float zPosition;
        if (this.transform.parent != null)
        {
            //Transform oldParent = this.transform.parent;
            //Vector3 parentLocalScale = this.transform.parent.localScale;
            //this.transform.parent = null;
            CalculateTheXZPosition(this.transform.position, this.transform.localScale, direction, speed, out xPosition, out zPosition);
            this.transform.SetX(xPosition);
            this.transform.SetZ(zPosition);
            //this.transform.parent = oldParent;
        }
        else
        {
            CalculateTheXZPosition(this.transform.position, this.transform.localScale, direction, speed, out xPosition, out zPosition);
            this.transform.SetX(xPosition);
            this.transform.SetZ(zPosition);
        }

       /* bool ifFindMainCamera = false;*/
    /*    if (Object.ReferenceEquals(mainCamera, null))
        {
            GameObject mainCameraObj = Camera.main.gameObject;
            if (!Object.ReferenceEquals(mainCameraObj, null))
            {
                ifFindMainCamera = true; //找到主相机
                mainCamera = mainCameraObj.GetComponent<Camera>();
            }
        }
        else
        {
            ifFindMainCamera = true;
        }*/

        if (/*ifFindMainCamera && */mainCamera.enabled && curTime >= updateInterval)
        {
            curTime = 0f;
            // bool ifInSideMainCamera = false;
            //         
            //获取bounds的四个竖直边长的顶点.
            Vector3 boxCenter = Vector3.zero;//thisBoxCollider.center;
            float boxLength = 1;//thisBoxCollider.size.x;
            float boxWidth = 1;//thisBoxCollider.size.y;
            float boxHeight = 1;//thisBoxCollider.size.z;

            Vector3 centerOffset = Vector3.zero;
            
            //上平面顶点ABCD
            centerOffset.Set((boxLength / 2), (boxWidth / 2), (boxHeight / 2));
            //Vector3 VertexA= thisMeshRenderer.bounds.center + new Vector3(boundLength / 2, boundWidth / 2, boundWidth / 2);
            Vector3 VertexA = transform.TransformPoint(boxCenter + centerOffset);
            centerOffset.Set(-(boxLength / 2), (boxWidth / 2), (boxHeight / 2));
            Vector3 VertexB = transform.TransformPoint(boxCenter + centerOffset);
            centerOffset.Set(-(boxLength / 2), (boxWidth / 2), -(boxHeight / 2));
            Vector3 VertexC = transform.TransformPoint(boxCenter + centerOffset);
            centerOffset.Set((boxLength / 2), (boxWidth / 2), -(boxHeight / 2));
            Vector3 VertexD = transform.TransformPoint(boxCenter + centerOffset);
            
            //如果离摄像机过远,则既不渲染也不进行碰撞检测
            Vector3 thisRenderPos = this.thisMeshRenderer.transform.position;
            Vector3 CameraPosition = mainCamera.transform.position;

            float MaxDistance = 45;
            bool IfInsideRenderingArea = false;
            if (Vector3.Distance(thisRenderPos, CameraPosition) <= MaxDistance)
            {
                IfInsideRenderingArea = true;
            }
            else
            {
                IfInsideRenderingArea = false;
            }

            // Debug.Log("IfVisible "+ this.transform.name + ifInSideMainCamera);
            if (ifInSideMainCamera)
            {
                // Debug.Log("Upadet "+ this.transform.name + "entering the camera！");
            }
            //如果box出现在摄像机以内,且距离摄像机不过远
            if (!Object.ReferenceEquals(targetTerrainObj, null) && thisMeshRenderer.isVisible && ifInSideMainCamera && IfInsideRenderingArea)
            {
                /////////////////////////////////////////////////////////////////////////////
                //使用TerrainData中的getHeight获取地形的高度
                //保持流动一直在地面上面 切随着地面起伏他也欺负 并且保持2000米之类的是么?
                float HeightA, HeightB, HeightC, HeightD;
                HeightA = targetTerrainObj.SampleHeight(VertexA);
                HeightB = targetTerrainObj.SampleHeight(VertexB);
                HeightC = targetTerrainObj.SampleHeight(VertexC);
                HeightD = targetTerrainObj.SampleHeight(VertexD);
                float yAxisMax = -20000f;
                yAxisMax = math.max(math.max(math.max(math.max(yAxisMax, HeightA), HeightB), HeightC), HeightD);
                yAxisMax = math.min(yAxisMax, CameraPosition.y - 2f);
                
                float yAxisMin = 20000f;
                yAxisMin = math.min(math.min(math.min(math.min(yAxisMin, HeightA), HeightB), HeightC), HeightD);
                /////////////////////////////////////////////////////////////////////////////
                
                float yPostion = (yAxisMax + yAxisMin) / 2;
                float yScale = (yAxisMax - yAxisMin)/* + 0.3f*/;
                yScale = math.max(5, yScale + 1f);
                Vector3 scale = Vector3.zero;
                //设置物体的YScale和Yposition
                if (this.transform.parent != null)
                {
                    Transform oldParent = this.transform.parent;
                    Vector3 parentLocalScale = this.transform.parent.localScale;
                    this.transform.parent = null;
                    this.transform.SetLocalY(yPostion);
                    scale.Set(this.transform.localScale.x, yScale, this.transform.localScale.z);
                    this.transform.SetLocalScale(scale);
                    this.transform.parent = oldParent;
                }
                else
                {
                    this.transform.SetLocalY(yPostion);
                    scale.Set(this.transform.localScale.x, yScale, this.transform.localScale.z);
                    this.transform.SetLocalScale(scale);
                }


            }
            else
            {
                thisMeshRenderer.transform.SetLocalY(OriginalYPos);
                Vector3 scale = Vector3.zero;
                scale.Set(thisMeshRenderer.transform.localScale.x, OriginalYScale, thisMeshRenderer.transform.localScale.z);
                thisMeshRenderer.transform.SetLocalScale(scale);
            }


        }
        ifInSideMainCamera = false;
        List<Material> mats = new List<Material>();
        this.transform.GetComponent<MeshRenderer>().GetSharedMaterials(mats);
        Color colors = Color.blue;
        colors.a = 0.2f;
        mats[mats.Count - 1].SetColor("_BaseColor", colors);

    }

    //当该物体被摄像机渲染的时候.
    // private void OnBecameVisible()
    // {
    //     ifInSideMainCamera = true;
    //     if (Camera.current== null)
    //     {
    //         Debug.Log(this.transform.name + "进入摄像机范围" + Camera.current);
    //         Material[] mats;
    //         mats = this.transform.GetComponent<MeshRenderer>().materials;
    //         Color colors = Color.red;
    //         colors.a = 0.5f;
    //         mats[mats.Length-1].SetColor("_BaseColor",colors);
    //     }
    //
    //     //(throw new NotImplementedException();)
    // }
    //
    private void OnWillRenderObject()
    {

        if (Camera.current == null)
        {
            ifInSideMainCamera = true;
            //Debug.Log(this.transform.name + "进入摄像机范围" + Camera.current);
            Material[] mats;
            mats = this.transform.GetComponent<MeshRenderer>().materials;
            Color colors = Color.red;
            colors.a = 0.2f;
            mats[mats.Length - 1].SetColor("_BaseColor", colors);
        }

        //(throw new NotImplementedException();)
    }
    //当该物体不再被摄像机渲染的时候
    // private void OnBecameInvisible()
    // {
    //     ifInSideMainCamera = false;
    //     if (Camera.current == null)
    //     {
    //         //Debug.Log(this.transform.name + "离开摄像机范围"+ Camera.current);
    //         Material[] mats;
    //         mats = this.transform.GetComponent<MeshRenderer>().materials;
    //         Color colors = Color.blue;
    //         colors.a = 0.5f;
    //         mats[mats.Length-1].SetColor("_BaseColor",colors);
    //     }
    //     
    //     //(throw new NotImplementedException();)
    // }

    //协程 用来控制云影材质球的浓淡

    //计算云影Box的在xz平面上和TerrainBox相交的起始点和终点
    void CalculatetheStartEndBox(Vector3 BoxPosition, Vector3 BoxSize, Vector3 moveDir, float moveSpeed)
    {
        // B--------------A
        // |              |
        // |              |
        // |              |
        // |              |
        // C--------------D
        //BA

        // Vector3 lineVec
        //计算和BA,BC,CD,DA边界的相交点
        Vector3 IntersectionBA, IntersectionBC, IntersectionCD, IntersectionDA;
        moveDir = Vector3.Normalize(moveDir);
        Vector3 BoxPositionFlat = Vector3.zero;
        BoxPositionFlat.Set(BoxPosition.x, 0, BoxPosition.z);
        LineLineIntersection(out IntersectionBA, BoxPositionFlat, moveDir, TerrainBoundsVertexB, lineVecBA);//和BA的交点
        LineLineIntersection(out IntersectionBC, BoxPositionFlat, moveDir, TerrainBoundsVertexB, lineVecBC);//和BC的交点
        LineLineIntersection(out IntersectionCD, BoxPositionFlat, moveDir, TerrainBoundsVertexC, lineVecCD);//和CD的交点
        LineLineIntersection(out IntersectionDA, BoxPositionFlat, moveDir, TerrainBoundsVertexD, lineVecDA);//和DA的交点
        
        
        List<Vector3> distancePosList = new List<Vector3>
        {
            new Vector3(IntersectionBA.x, IntersectionBA.z, Vector3.Distance(BoxPosition, IntersectionBA)),
            new Vector3(IntersectionBC.x, IntersectionBC.z, Vector3.Distance(BoxPosition, IntersectionBC)),
            new Vector3(IntersectionCD.x, IntersectionCD.z, Vector3.Distance(BoxPosition, IntersectionCD)),
            new Vector3(IntersectionDA.x, IntersectionDA.z, Vector3.Distance(BoxPosition, IntersectionDA))
        };

        //  List<Vector3>lists = distancePosList = distancePosList.OrderBy(t=>t.z).ToList();
        //
        // // Vector3 StartPoint, EndPoint;  //云的起点和终点
        //  Vector3 MinV1, MinV2;   //距离box最小的次小的距离
        //  MinV1 = lists[0];
        //  MinV2 = lists[1];

        float boxToStartDis = float.MaxValue, boxToEndDis = float.MaxValue;
        foreach (Vector3 v in distancePosList)
        {
            Vector3 BoxToV = Vector3.zero;
            BoxToV.Set(v.x - BoxPosition.x, 0, v.y - BoxPosition.z);
            float dot = Vector3.Dot(BoxToV, direction);
            if (dot >= 0)  //点在往终点方向
            {
                if (v.z < boxToEndDis)
                {
                    boxToEndDis = v.z;
                    EndPoint.Set(v.x, 0, v.y);
                }
            }
            else//点在往起点方向
            {
                if (v.z < boxToStartDis)
                {
                    boxToStartDis = v.z;
                    StartPoint.Set(v.x, 0, v.y);
                }
            }
        }
        
    }
    //计算云影Box的xz位置,来模拟云的运动
    void CalculateTheXZPosition(Vector3 BoxPosition, Vector3 BoxSize, Vector3 moveDir, float moveSpeed, out float xPosition, out float zPosition)
    {
        // xPosition = 0;
        // zPosition = 0;
        //
        // // B--------------A
        // // |              |
        // // |              |
        // // |              |
        // // |              |
        // // C--------------D
        // //BA
        //
        // // Vector3 lineVec
        // //计算和BA,BC,CD,DA边界的相交点
        // Vector3 IntersectionBA,IntersectionBC,IntersectionCD,IntersectionDA;
        // moveDir = Vector3.Normalize(moveDir);
        // Vector3 BoxPositionFlat = new Vector3(BoxPosition.x, 0, BoxPosition.z);
        // LineLineIntersection(out IntersectionBA,BoxPositionFlat,moveDir,TerrainBoundsVertexB,lineVecBA);//和BA的交点
        // LineLineIntersection(out IntersectionBC,BoxPositionFlat,moveDir,TerrainBoundsVertexB,lineVecBC);//和BC的交点
        // LineLineIntersection(out IntersectionCD,BoxPositionFlat,moveDir,TerrainBoundsVertexC,lineVecCD);//和CD的交点
        // LineLineIntersection(out IntersectionDA,BoxPositionFlat,moveDir,TerrainBoundsVertexD,lineVecDA);//和DA的交点
        //
        // List<Vector3> distancePosList = new List<Vector3>
        // {
        //     new Vector3(IntersectionBA.x, IntersectionBA.z, Vector3.Distance(BoxPosition, IntersectionBA)),
        //     new Vector3(IntersectionBC.x, IntersectionBC.z, Vector3.Distance(BoxPosition, IntersectionBC)),
        //     new Vector3(IntersectionCD.x, IntersectionCD.z, Vector3.Distance(BoxPosition, IntersectionCD)),
        //     new Vector3(IntersectionDA.x, IntersectionDA.z, Vector3.Distance(BoxPosition, IntersectionDA))
        // };
        //
        // List<Vector3>lists = distancePosList = distancePosList.OrderBy(t=>t.z).ToList();
        //
        // Vector3 StartPoint, EndPoint;  //云的起点和终点
        // Vector3 MinV1, MinV2;   //距离box最小的次小的距离
        // MinV1 = lists[0];
        // MinV2 = lists[1];
        //
        // if ((MinV1.x - MinV2.x) * direction.x >= 0 && (MinV1.y - MinV2.y) * direction.z >= 0)
        // {
        //     EndPoint = new Vector3(MinV1.x, 0, MinV1.z);
        //     StartPoint = new Vector3(MinV2.x, 0, MinV2.z);
        // }
        // else
        // {
        //     StartPoint = new Vector3(MinV1.x, 0, MinV1.z);
        //     EndPoint = new Vector3(MinV2.x, 0, MinV2.z);
        // }
        //
        //计算和方块
        //计算运动位置
        xPosition = BoxPosition.x + moveDir.x * moveSpeed;
        zPosition = BoxPosition.z + moveDir.z * moveSpeed;

        //如果box运动超过了终点 则返回起点
        Vector3 i = Vector3.zero;
        i.Set(BoxPosition.x - EndPoint.x, 0, BoxPosition.z - EndPoint.z);
        if (Vector3.Dot(Vector3.Normalize(i), direction) > 0)
        {
            xPosition = StartPoint.x;
            zPosition = StartPoint.z;
        }
    }
    //计算两条直线的交点
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
        Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
            && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2)
                      / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}
