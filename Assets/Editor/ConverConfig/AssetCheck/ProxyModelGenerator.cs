////垂直灯光不能生成或者放在上方
////灯光不能变化
////灯光产生的阴影不能逆推所以代理模型不用那么合乎物理法则（光源，物件，地面地面其实他不知道信息其实可以给他嘛），但是要求一定是垂直地面的，光源和物件其实是渲染物件的一个切面，
//代码里面生成的模型不用那么多顶点只需要外轮廓就行了，被渲染的模型一定不透明的
//XZ是平面，Y是向上的
//中间没有镂空的情况才行
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


//public class ProxyModelGenerator : EditorWindow
//{
//    private Light sceneLight; // 场景灯光
//    private string outputFolder; // 存储生成的代理模型的fbx文件的目标文件夹路径

//    [MenuItem("自动化工具/ProxyModelGenerator")] // 在菜单栏中创建一个名为"自动化工具"的主菜单，并添加"ProxyModelGenerator"的子菜单项
//    static void Init()
//    {
//        // 创建并显示窗口
//        ProxyModelGenerator window = GetWindow<ProxyModelGenerator>();
//        window.Show();
//    }

//    void OnGUI()
//    {
//        GUILayout.Label("代理模型生成器", EditorStyles.boldLabel);

//        sceneLight = EditorGUILayout.ObjectField("场景灯光", sceneLight, typeof(Light), true) as Light;

//        if (GUILayout.Button("生成代理模型"))
//        {
//            GenerateProxyModels();
//        }
//    }

//    private void GenerateProxyModels()
//    {
//        // 检测选择的所有对象
//        GameObject[] selectedObjects = Selection.gameObjects;

//        foreach (GameObject obj in selectedObjects)
//        {
//            // 从对象的网格组件获取网格
//            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
//            if (meshFilter == null)
//                continue;

//            Mesh originalMesh = meshFilter.sharedMesh;

//            // 根据灯光角度和投影生成代理模型的2D mesh
//            Mesh proxyMesh = GenerateProxyMesh(originalMesh);

//            // 创建代理模型的实例
//            GameObject proxyModel = Instantiate(obj, obj.transform.position, obj.transform.rotation);
//            proxyModel.transform.SetParent(obj.transform.parent);
//            proxyModel.layer = obj.layer;

//            // 获取光线的方向向量
//            Vector3 lightDirection = -sceneLight.transform.forward;

//            // 使用光线的方向向量计算旋转
//            Quaternion proxyRotation = Quaternion.LookRotation(lightDirection);

//            // 设置代理模型的旋转
//            proxyModel.transform.rotation = proxyRotation;

//            proxyModel.GetComponent<MeshFilter>().sharedMesh = proxyMesh;
//            proxyModel.name += "_2D";

//            // 将代理模型放置在原始物体的位置和旋转下
//            proxyModel.transform.position = obj.transform.position;

//            SaveMeshAsFbx(proxyMesh, obj.name + "_proxy");
//        }
//    }


//    private void SaveMeshAsFbx(Mesh mesh, string fileName)
//    {/*
//        string filePath = Path.Combine(outputFolder, fileName + ".fbx");
//        File.Copy(sourceFbxFilePath, filePath); //将源 'fbx' 文件复制到生成的位置

//        AssetDatabase.ImportAsset(filePath); //导入 'fbx' 文件

//        Debug.Log("Generated proxy model saved as fbx: " + filePath);*/
//    }




//    // 根据灯光角度和投影生成2D代理模型
//    private Mesh GenerateProxyMesh(Mesh originalMesh)
//    {

//        // 在这里根据灯光角度和投影等因素生成代理模型的2D mesh
//        Vector3 lightDirection = -sceneLight.transform.forward;
//        Quaternion lightRotation = Quaternion.LookRotation(lightDirection, Vector3.up);

//        // 获取灯光的转换矩阵
//        Matrix4x4 lightMatrix = Matrix4x4.TRS(sceneLight.transform.position, sceneLight.transform.rotation, Vector3.one);

//        // 计算模型和灯光切面
//        Plane lightPlane = new Plane(lightDirection, Vector3.zero);
//        float verticalAngle = Vector3.Angle(Vector3.up, lightDirection);
//        float horizontalAngle = 180 - verticalAngle;

//        // 创建新的代理模型的顶点和三角形数组
//        Vector3[] originalVertices = originalMesh.vertices;
//        Vector3[] proxyVertices = new Vector3[originalVertices.Length];
//        int[] proxyTriangles = originalMesh.triangles;

//        for (int i = 0; i < originalVertices.Length; i++)
//        {
//            Vector3 vertex = originalVertices[i];

//            // 根据顶点和光照角度计算投影点的位置
//            Vector3 projectedVertex = lightRotation * vertex;

//            // 将投影点的y坐标设置为0，即将其投影到平面上
//            projectedVertex.y = 0;

//            // 使用灯光的转换矩阵将投影点变换到世界空间
//            projectedVertex = lightMatrix.MultiplyPoint(projectedVertex);

//            // 将投影点的位置作为代理模型的顶点位置
//            proxyVertices[i] = projectedVertex;
//        }

//        // 创建代理模型的网格，并设置顶点和三角形
//        Mesh proxyMesh = new Mesh();
//        proxyMesh.vertices = proxyVertices;
//        proxyMesh.triangles = proxyTriangles;

//        // 重新计算法线和边界
//        proxyMesh.RecalculateNormals();
//        proxyMesh.RecalculateBounds();

//        if (verticalAngle > 0)
//        {
//            float scale = Mathf.Tan(Mathf.Deg2Rad * horizontalAngle) / Mathf.Tan(Mathf.Deg2Rad * verticalAngle);
//            Vector3[] scaledVertices = new Vector3[originalVertices.Length];

//            for (int i = 0; i < originalVertices.Length; i++)
//            {
//                Vector3 vertex = proxyVertices[i];
//                vertex *= scale;
//                scaledVertices[i] = vertex;
//            }

//            proxyMesh.vertices = scaledVertices;

//            // 重新计算法线和边界
//            proxyMesh.RecalculateNormals();
//            proxyMesh.RecalculateBounds();
//        }

//        // 返回生成的代理模型的mesh
//        return proxyMesh;

//        /*Vector3 lightDirection = -sceneLight.transform.forward;
//      Plane lightPlane = new Plane(lightDirection, Vector3.zero);

//      Vector3[] originalVertices = originalMesh.vertices;
//      List<Vector3> proxyVerticesList = new List<Vector3>();
//      List<int> proxyTrianglesList = new List<int>();

//      for (int i = 0; i < originalVertices.Length; i++)
//      {
//          Vector3 vertex1 = originalVertices[i % originalVertices.Length];
//          Vector3 vertex2 = originalVertices[(i + 1) % originalVertices.Length];

//          float distance1 = lightPlane.GetDistanceToPoint(vertex1);
//          float distance2 = lightPlane.GetDistanceToPoint(vertex2);

//          if (distance1 >= 0 && distance2 >= 0)
//          {
//              // 两个顶点均在光线的同一侧，直接添加到代理模型
//              proxyVerticesList.Add(vertex1);
//              proxyVerticesList.Add(vertex2);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 2);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 1);
//          }
//          else if (distance1 >= 0 && distance2 < 0)
//          {
//              // vertex1在光线的同一侧，vertex2在光线相反的一侧，需要计算交点并添加到代理模型
//              Vector3 intersectionPoint = lightPlane.ClosestPointOnPlane(vertex1);
//              proxyVerticesList.Add(vertex1);
//              proxyVerticesList.Add(intersectionPoint);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 2);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 1);
//          }
//          else if (distance1 < 0 && distance2 >= 0)
//          {
//              // vertex2在光线的同一侧，vertex1在光线相反的一侧，需要计算交点并添加到代理模型
//              Vector3 intersectionPoint = lightPlane.ClosestPointOnPlane(vertex1);
//              proxyVerticesList.Add(intersectionPoint);
//              proxyVerticesList.Add(vertex2);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 2);
//              proxyTrianglesList.Add(proxyVerticesList.Count - 1);
//          }
//      }

//      // 创建代理模型的Mesh
//      Mesh proxyMesh = new Mesh();
//      proxyMesh.vertices = proxyVerticesList.ToArray();
//      proxyMesh.triangles = proxyTrianglesList.ToArray();
//      proxyMesh.RecalculateNormals();
//      proxyMesh.RecalculateBounds();

//      return proxyMesh;*/
//    }

//}

//===============================================================
/*这两种产生阴影的方式各有优劣，并且性能的消耗也取决于具体的实现方式和场景的需求。

使用图片通道的方式，可以利用图片的特定通道来绘制图形投影，这种方式相对简单且易于实现。它不需要额外的几何体，只需在一个平面上进行渲染。但是，它的主要局限是依赖于图片本身的视觉信息。如果图片的质量、分辨率或内容不够准确，可能会影响投影的质量。

另一种方式是使用一个扁平的mesh来绘制图形投影。这个方式可能需要更多的计算和几何处理，以便将mesh正确地投射到面板上。相比于使用图片通道的方式，这种方式提供了更多的控制和自定义性。您可以根据需要创建不同形状和样式的投影。但是，在复杂的场景中，这种方式可能会导致性能开销较高，特别是当需要实时更新投影时。

因此，对于选择哪种方式来生成阴影，您需要考虑到您的具体需求和场景。如果您需要简单而快速的投影效果，并且图片通道中的视觉信息足够准确，则使用图片通道的方式可能更合适。如果您需要更多的自定义性和复杂的投影效果，并且可以承受更高的性能开销，则使用扁平mesh的方式可能更适合。
*/
/*首先，我们需要计算一个与物体和光源相关的有效投影平面。可以使用物体的包围盒（Bounding Box）来近似表示物体的有效形状。
Bounds bounds = obj.GetComponent<Renderer>().bounds;
接下来，我们需要计算投影平面的位置和方向。可以通过将光源的位置向物体平移得到投影平面的位置，方向则是光源的负方向。
Vector3 projectionPosition = bounds.center - sceneLight.transform.forward * bounds.extents.magnitude;
Vector3 projectionNormal = -sceneLight.transform.forward;
然后，我们将生成一个表示投影的模型，这可以通过在投影平面上产生光线和物体的交点，并以交点为顶点构建模型。
Mesh projectionMesh = GenerateProjectionMesh(bounds, projectionPosition, projectionNormal);
最后，我们可以将生成的投影模型应用到场景中。
GameObject projectionObject = new GameObject("Projection");
projectionObject.AddComponent<MeshFilter>().mesh = projectionMesh;
projectionObject.AddComponent<MeshRenderer>();

// 将投影模型放置在投影平面上
projectionObject.transform.position = projectionPosition;
projectionObject.transform.rotation = Quaternion.LookRotation(projectionNormal);*/

using System.Collections.Generic;
using System;
using Autodesk.Fbx;
using UnityEditor.Formats.Fbx.Exporter;
//===============================================================
//这里要做一下数据映射
[CustomEditor(typeof(ProxyModelGeneratorData))]
[assembly: InternalsVisibleTo("Unity.Formats.Fbx.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.ProBuilder.AddOns.Editor")]
public class ProxyModelGenerator : EditorWindow
{
    private Light sceneLight; // 场景灯光

    private string outputFolder; // 存储生成的代理模型的fbx文件的目标文件夹路径

    public static ProxyModelGenerator Window;

    string oldMeshPath;

    string assetPath = "a";
    string oldMeshDicPath = "a";

    [MenuItem("自动化工具/ProxyModelGenerator")] // 在菜单栏中创建一个名为"自动化工具"的主菜单，并添加"ProxyModelGenerator"的子菜单项
    static void Init()
    {
        // 创建并显示窗口
        Window = GetWindow<ProxyModelGenerator>();
     
        Window.Show();
    }


    /*    private void SetUpModelData(Mesh mesh)
        {
            // ...

            // 在需要的地方修改静态变量的值
            ProxyModelGeneratorData.simMesh = mesh;
        }*/
  /*  public bool needFbx = false;*/
    void OnGUI()
    {
        GUILayout.Label("代理模型生成器", EditorStyles.boldLabel);

        sceneLight = EditorGUILayout.ObjectField("场景灯光", sceneLight, typeof(Light), true) as Light;
        GUILayout.Label("虽然不需要考虑uv，但是面片上的所有三角面合并，和删除某些顶点暂时没做");

        GUILayout.Label("第一步：模型简化，调整百分比，然后点击压缩，然后点击处理完成");
        if (GUILayout.Button("模型简化  "))
        {
            SetUpModelData();
        }
      /*  if (ProxyModelGeneratorData.simMesh == null)
        {
            GUILayout.Label("没有拿到优化后的模型");
        }
        else
        {
            GUILayout.Label("已经拿到了优化模型:" + ProxyModelGeneratorData.simMesh.name);
        }*/
     
        if (GUILayout.Button("处理完成  "))
        {
            //DoneModel(1);
            CompleteModelData();
        }
        GUILayout.Label("第二步：两种模式处理");
        //同面合并原则TODO
        //不需要考虑uv原则TODO因为是代理模型

        //1注释掉这个，2之前不知道啥错了就电风扇了，3现在对了
   /*     if (GUILayout.Button("生成代理模型2D  "))
        {
            GenerateProxyModels(1);
        }*/
        if (GUILayout.Button("生成代理模型3D自己删除 生成的自己 scele x ，z  * -1"))
        {
            GenerateProxyModels(2);
        }
        // 使用toggleValue变量作为Toggle的状态，并通过该变量来控制Toggle的值
        //needFbx = GUILayout.Toggle(needFbx, "NeedFBX false那就是asset了");


        GUILayout.Label("第三步：美术记得删掉FBX里面的shadow");

        GUILayout.Label("中途记录：目前需要赶紧做别的工作，以下的标注有空会逐一修改完毕，现在记录下");
        GUILayout.Label("1，需要合并的合并后在使用，2，可以配合自己的工具进行修正，3，需要导出fbx跟程序说");
        GUILayout.Label("1，生成的模型可能X * -1了自己手动改一下 ，2，同一个面的三角面合并没写呢，3，2D不开启程序没法凸包合并没时间写-角度有bug-美术自己修复");
        GUILayout.Label("1，自动模型合并目前没排期时间写，2，2d生成有bug需要修改一下才行，3，面光面互相遮挡删除不确定写没写");


        GUILayout.Label("第三步：美术记得删掉FBX里面的shadow");
        if (GUILayout.Button("新生成的代理模型，导出FBX？"))
        {

            

            GameObject[] selectedGOs = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel);
            Transform Root = selectedGOs[0].transform.parent;//0才是对的

            UnityEngine.Object[] sel_gob =new UnityEngine.Object[] { Root.GetChild(Root.childCount - 1).gameObject };
            Selection.activeObject = sel_gob[0];
            //数据活跃都准备好了
            var toExport = ModelExporter.RemoveRedundantObjects(sel_gob);
            UnityEditor.Formats.Fbx.Exporter.ExportModelEditorWindow.Init(System.Linq.Enumerable.Cast<UnityEngine.Object>(toExport), isTimelineAnim: false);
        }
        GUILayout.Label("TODO1一件批处理   7%");
        GUILayout.Label("TODO2一件批处理，模型合并，代理合并后，单面排除");
        GUILayout.Label("TODO3背面处理，内部抠空处理");
        GUILayout.Label("适合单个不复用超过3000就应该用了，太多的独立模型会增加存储空间换运行压力：如果多的话就要用美术的shadow模型，走第二部就行了");
        GUILayout.Label("map2分大区块合并 抠空 和光面扣除");
        GUILayout.Label("map2分大区块合并 抠空 和光面扣除——重点场---程序合并美术扣空 3D2D算法都复杂");
        GUILayout.Label("小组合4个石头策略组合，防止内部阴影");
        GUILayout.Label("代理和扣面的复用性问题就是抉择，包体和性能互换，包体不重要看物件复用性，减少的面也不占，包体不重要，后面还要减地表");
        GUILayout.Label("包体真的不重要，分包策略，性能和表现都重要，场景最多1万7");
    }
    private Mesh oooooooooooooooooooooooooooooooooooooooooldMes;
    private void SetUpModelData()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length > 1)
        {
            Debug.Log("只能选择一个");
            return;
        }
        if (selectedObjects[0].GetComponent<MeshFilter>() == null)
        {
            Debug.Log("请你选择有meshfilter的gob");
            return;
        }
        if (selectedObjects[0].GetComponent<MeshSimplify>() == null)
        {
            selectedObjects[0].AddComponent<MeshSimplify>().m_fVertexAmount = 0.07f;
        }
        oooooooooooooooooooooooooooooooooooooooooldMes = selectedObjects[0].GetComponent<MeshFilter>().sharedMesh;

        if (oooooooooooooooooooooooooooooooooooooooooldMes != null && !oooooooooooooooooooooooooooooooooooooooooldMes.isReadable)
        {
            oldMeshPath = AssetDatabase.GetAssetPath(oooooooooooooooooooooooooooooooooooooooooldMes);
            ModelImporter importer = AssetImporter.GetAtPath(oldMeshPath) as ModelImporter;
            importer.isReadable = true;
            AssetDatabase.ImportAsset(oldMeshPath);
        }
        //不能放在下面不然导入新资源的时候这个模型被选中，会默认选择他的根节点---不 no 用getp了
        selectedObjects[0].GetComponent<MeshRenderer>().receiveShadows = true;
        selectedObjects[0].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }


    private void CompleteModelData() 
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length > 1)
        {
            Debug.Log("只能选择一个");
            return;
        }

        if (selectedObjects[0].GetComponent<MeshFilter>() == null)
        {
            Debug.Log("请你选择有meshfilter的gob");
            return;
        }

        if (selectedObjects[0].GetComponent<MeshSimplify>() != null)
        {
            DestroyImmediate(selectedObjects[0].GetComponent<MeshSimplify>());
        }
        //老模型处理
        ModelImporter importer1 = AssetImporter.GetAtPath(oldMeshPath) as ModelImporter;
        importer1.isReadable = false;
        AssetDatabase.ImportAsset(oldMeshPath);

     //确认对不对，确认有用，不越位说


        Mesh newMesh = selectedObjects[0].GetComponent<MeshFilter>().sharedMesh;
        /*   string assetPath = AssetDatabase.GetAssetPath(originalMesh);//共享模型节省内存空间，运行时不能调用mesh*/
        oldMeshDicPath = Path.GetDirectoryName(oldMeshPath);


        //if (!needFbx)
        {
            string meshPath = Path.Combine(oldMeshDicPath, newMesh.name + ".asset");
            // 检查文件是否存在
            if (File.Exists(meshPath))
            {
                // 删除已存在的文件
                AssetDatabase.DeleteAsset(meshPath);
            }
            // 创建新的Mesh资源文件
            /*  selectedObjects[0].GetComponent<MeshFilter>().savetoasset*/
            AssetDatabase.CreateAsset(newMesh /*ProxyModelGeneratorData.simMesh 
                                           * 这个不稳妥有时候拿不到，处理有时候不给我，
                                           * 费劲，newMesh简单，我身上的一定对的也不基于别人流程，
                                           * 消耗性能*/, meshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


        }
       /* else
        {
            string meshPath = Path.Combine(oldMeshDicPath, newMesh.name + ".fbx");
            // 检查文件是否存在
            if (File.Exists(meshPath))
            {
                // 删除已存在的文件
                AssetDatabase.DeleteAsset(meshPath);
            }

            GameObject proxyGameObject = new GameObject("ProxyObject");
            MeshFilter meshFilter = proxyGameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = newMesh;
            //==========================================================
            //手配置，商店pkgmgr能看见，代码对
            //FbxExporter.ExportGameObjToFBX(proxyGameObject, meshPath, true, true, true);
            FbxManager fbxManager = FbxManager.Create();

            // 创建 FbxExporter
            FbxExporter exporter = FbxExporter.Create(fbxManager, "Exporter");

            // 设置导出文件路径
            string outputPath = meshPath;

            // 导出之前的准备工作
            FbxIOSettings ioSettings = FbxIOSettings.Create(fbxManager, Globals.FBXSDK_CURVENODE_TRANSFORM);
            //模型网格，骨骼，uv，绑定关系
            fbxManager.SetIOSettings(ioSettings);

            // 导出 Mesh
            FbxMesh fbxMesh = FbxMesh.Create(fbxManager, "Mesh");
            // 设置 Mesh 的数据（顶点坐标、法线、UV 等）
            // ...

            FbxNode meshNode = FbxNode.Create(fbxManager, "MeshNode");
            meshNode.SetNodeAttribute(fbxMesh);


            // 创建 FbxNode 并将 Mesh 添加到 Node 中
            FbxNode rootNode = FbxNode.Create(fbxManager, "RootNode");
            rootNode.AddChild(meshNode);



            // 将 Mesh 导出到 FbxDocument 中
            FbxDocument fbxDoc = FbxDocument.Create(fbxManager, "Scene");
            fbxDoc.ConnectSrcObject(rootNode);
*//*
            FbxDocumentInfo documentInfo = fbxDoc.GetDocumentInfo();
            fbxDoc.SetDocumentInfo(documentInfo);
*//*


            // 导出 FbxDocument 到文件
            bool exportSuccess = exporter.Export(fbxDoc);
            if (exportSuccess)
            {
                Debug.Log("Export completed successfully");
            }
            else
            {
                Debug.LogError("Export failed");
            }

            // 销毁对象
            fbxDoc.Destroy();
            exporter.Destroy();
            ioSettings.Destroy();
            fbxManager.Destroy();


            //==========================================================


            // 销毁临时GameObject
            DestroyImmediate(proxyGameObject);

        }*/







        //TODO新的mesh也没有Read/Write所以不必处理，rw






    }
    //private void GenerateProxyModels()
    //{
    //    // 检测选择的所有对象
    //    GameObject[] selectedObjects = Selection.gameObjects;

    //    foreach (GameObject obj in selectedObjects)
    //    {
    //        // 从对象的网格组件获取网格
    //        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
    //        if (meshFilter == null)
    //            continue;

    //        Mesh originalMesh = meshFilter.sharedMesh;
    //        Vector3[] vertices = originalMesh.vertices;

    //        // 创建一个空的2D纹理
    //        Texture2D shadowTexture = new Texture2D(512, 512, TextureFormat.ARGB32, false);

    //        // 设置纹理的像素为透明
    //        Color32[] pixels = new Color32[shadowTexture.width * shadowTexture.height];
    //        for (int i = 0; i < pixels.Length; i++)
    //        {
    //            pixels[i] = new Color32(0, 0, 0, 0);
    //        }
    //        shadowTexture.SetPixels32(pixels);
    //        shadowTexture.Apply();

    //        // 在阴影纹理上绘制阴影
    //        foreach (Vector3 vertex in vertices)
    //        {
    //            // 将顶点坐标转换为2D纹理坐标
    //            Vector2 textureCoordinate = new Vector2(vertex.x, vertex.z) + new Vector2(0.5f, 0.5f);
    //            textureCoordinate *= shadowTexture.width;

    //            // 计算阴影纹理中的像素位置
    //            int x = Mathf.RoundToInt(textureCoordinate.x);
    //            int y = Mathf.RoundToInt(textureCoordinate.y);

    //            // 设置像素为黑色（阴影）
    //            shadowTexture.SetPixel(x, y, Color.black);
    //        }
    //        shadowTexture.Apply();

    //        // 创建一个新的平面对象作为代理模型
    //        GameObject proxyModel = GameObject.CreatePrimitive(PrimitiveType.Quad);
    //        proxyModel.name = obj.name + "_Proxy";
    //        proxyModel.layer = obj.layer;
    //        Destroy(proxyModel.GetComponent<Collider>());

    //        // 将阴影贴图应用于代理模型的材质
    //        Material proxyMaterial = new Material(Shader.Find("Standard"));
    //        proxyMaterial.mainTexture = shadowTexture;
    //        proxyModel.GetComponent<Renderer>().material = proxyMaterial;

    //        // 设置代理模型的位置和旋转与原始模型相同
    //        proxyModel.transform.position = obj.transform.position;
    //        proxyModel.transform.rotation = obj.transform.rotation;

    //        // 设置代理模型的缩放与原始模型相同（可选）
    //        proxyModel.transform.localScale = obj.transform.localScale;

    //        // 将代理模型放置在原始物体的父对象下
    //        proxyModel.transform.SetParent(obj.transform.parent);

    //        // 存储代理模型为FBX文件（可选）
    //        string outputFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
    //        string filePath = Path.Combine(outputFolder, obj.name + "_Proxy.fbx");
    //        //UnityEditor.FbxExporter.ExportGameObjects(new GameObject[] { proxyModel }, filePath);

    //        // 销毁生成的阴影贴图
    //        Destroy(shadowTexture);
    //    }
    //}
    private void GenerateProxyModels(int type)
    {
        // 检测选择的所有对象
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            // 从对象的网格组件获取网格
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null)
                continue;

            Mesh originalMesh = meshFilter.sharedMesh;//都是第二次加工的了
            string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);//共享模型节省内存空间，运行时不能调用mesh
            string meshDirectory = Path.GetDirectoryName(assetPath);
            Vector3[] vertices = originalMesh.vertices; // 假设mesh是你的三角形网格
            Vector2[] v2vertices = new Vector2[originalMesh.vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                v2vertices[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            /* Vector3 lightDir = (obj.transform.position - sceneLight.transform.position).normalized;//这个他嘛是点光源的根本就不是平行光
             lightDir = new Vector3(lightDir.x, lightDir.y, lightDir.z);*/
            //child是前方， 前方-后方=后方指向爱过你前方，A-b 是b指向A参考 x-0
            Vector3 lightDirection = (sceneLight.transform.GetChild(0).position -sceneLight.transform.position).normalized;
      /*      GameObject ga1meo1bj1e1ct = new GameObject();//inst和new都行
            ga1meo1bj1e1ct.transform.SetParent(sceneLight.transform);
            ga1meo1bj1e1ct.transform.position = sceneLight.transform.position + 5 * lightDirection;//这个是相对原点偏移这么多，世界原点，绝对偏移*/

            int vertexCount = vertices.Length;
     
            Debug.Log("顶点数量：" + vertexCount);
            // 根据灯光角度和投影生成代理模型的2D mesh
            Mesh proxyMesh = new Mesh();

            //模型自动减面:法线平面裁剪
            ///333TODO  uv 颜色 都无需考虑 这里
            ///如果有些面如A 和 B 的法线完全相同且这些顶点 也再同一个模型面上（比如A的3个顶点和B的3个顶点的（X和Y） 或者 （X和Z） 或者 （Y和Z）相同 那就说明A面和B面一定再同一个平面上，那就把A和B 合成一个面
            //进而所有的同平面三角面都可以合并为一个面
            /// 
            ///其实有时候跟你相对也在产生阴影,这个事儿不是这么看的
            ////没有附近合并
            //proxyMesh = ReduceFaceCount(originalMesh);

            if (type == 1)
            {
                proxyMesh = GenerateProxyMesh(originalMesh, obj.transform.rotation, lightDirection);
            }
            else if (type == 2)
            {
                proxyMesh = GenerateProxyMeshWithShadows(originalMesh, lightDirection);

            }
                
                /*GenerateMaskTexture*/
            /*List<Vector2> vector3s = DivideAndConquer(v2vertices);*/
            /*foreach (var item in vector3s)
            {
                Debug.Log(item.x + "_" + item.y);
            }*/





            //Mesh proxyMesh1 = RemoveNonBoundaryVertices(proxyMesh);

            // 获取光源在场景中的世界坐标
            Vector3 lightPosition = sceneLight.transform.position;

           /* // 计算从模型位置到光源位置的照射向量
            Vector3 lightDirection = (lightPosition - obj.transform.position).normalized;*/





            // 创建代理模型的实例
            // 创建代理模型的实例，并设置其垂直于光线方向
            GameObject proxyModel = Instantiate(obj, obj.transform.position, /*Quaternion.LookRotation(lightDirection, Vector3.up)*/
                obj.transform.rotation
                );
            proxyModel.transform.SetParent(obj.transform.parent);
            proxyModel.layer = obj.layer;
            proxyModel.GetComponent<MeshFilter>().sharedMesh = proxyMesh;
            proxyModel.name += "_Shadow";
            proxyModel.GetComponent<MeshRenderer>().receiveShadows = false;
            proxyModel.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            // 根据原始模型的旋转来设置生成的代理模型的旋转
            /*proxyModel.transform.rotation = obj.transform.rotation;*/

    
                /*还原老的东西*/

            //在哪里来的-1不知道我要再乘回来
            proxyModel.transform.localScale = new Vector3(proxyModel.transform.localScale.x * -1,
                proxyModel.transform.localScale.y,
                proxyModel.transform.localScale.z);
            // 根据GUID查找材质
            string guid = "6d04a9b269ccf4d4a921e1aa1b67edff"; // 您的材质GUID
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Material targetMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            // 将目标材质赋给代理模型的所有子渲染器
            MeshRenderer[] renderers = proxyModel.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                //sharedmesh sharedmate才对 不然都是inst 不是内存p了
                /* for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                 {
                     if (j == 0)
                     {
                         renderers[i].sharedMaterials[j] = targetMaterial;
                     }
                     else
                     {
                         renderers[i].sharedMaterials[j] = null;
                     }
                 } */
                //Material[] materials = renderers.sharedMaterials;
                //for (int i = 0; i < materials.Length; i++)
                //{
                //    Destroy(materials[i]);
                //}
                //renderer.sharedMaterials = new Material[0];//立刻删除，不用，我就直接给堆也行因为不是运行时是编辑器
           /*     renderers[i].sharedMaterials = new Material[1];
                renderers[i].sharedMaterials[0] = targetMaterial;*/

                Material[] materials = new Material[1];
                materials[0] = targetMaterial;
                renderers[i].sharedMaterials = materials;
            }
         
            // 获取obj所在的目录
            //string objDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));

            // 设置输出文件夹为obj所在的目录
            //outputFolder = objDirectory;

            // 将代理模型放置在原始物体的位置和旋转下
            proxyModel.transform.position = obj.transform.position;
            proxyModel.transform.rotation = obj.transform.rotation;

           /* if (!needFbx)
            {*/
                string meshPath = Path.Combine(meshDirectory, originalMesh.name + "Proxy.asset");
                // 检查文件是否存在
                if (File.Exists(meshPath))
                {
                    // 删除已存在的文件
                    AssetDatabase.DeleteAsset(meshPath);
                }
                // 创建新的Mesh资源文件
                AssetDatabase.CreateAsset(proxyMesh, meshPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
           /* }
            else
            {
                *//*string meshPath = Path.Combine(meshDirectory, originalMesh.name + "ProxyFBX.fbx");
                // 检查文件是否存在
                if (File.Exists(meshPath))
                {
                    // 删除已存在的文件
                    AssetDatabase.DeleteAsset(meshPath);
                }

                GameObject proxyGameObject1 = new GameObject("ProxyObject1");
                MeshFilter meshFilter1 = proxyGameObject1.AddComponent<MeshFilter>();
                meshFilter1.mesh = proxyMesh;

                FBXExporter.ExportGameObjToFBX(proxyGameObject1, meshPath, true, true, true);
                // 销毁临时GameObject
                DestroyImmediate(proxyGameObject1);*//*
            }*/
          



            //还原原来的东西
            meshFilter.sharedMesh = oooooooooooooooooooooooooooooooooooooooooldMes;
            //SaveMeshAsAsset(proxyMesh, obj.name + "_proxy");
        }
    }

    private bool IsSamePlane(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        bool isSamePlane = false;
        float epsilon = 0.001f;

        // 检查 x 值
        bool sameX = Mathf.Abs(vertex1.x - vertex2.x) < epsilon && Mathf.Abs(vertex1.x - vertex3.x) < epsilon;
        bool differentX = Mathf.Abs(vertex1.x - vertex2.x) > epsilon && Mathf.Abs(vertex1.x - vertex3.x) > epsilon;

        // 检查 y 值
        bool sameY = Mathf.Abs(vertex1.y - vertex2.y) < epsilon && Mathf.Abs(vertex1.y - vertex3.y) < epsilon;
        bool differentY = Mathf.Abs(vertex1.y - vertex2.y) > epsilon && Mathf.Abs(vertex1.y - vertex3.y) > epsilon;

        // 检查 z 值
        bool sameZ = Mathf.Abs(vertex1.z - vertex2.z) < epsilon && Mathf.Abs(vertex1.z - vertex3.z) < epsilon;
        bool differentZ = Mathf.Abs(vertex1.z - vertex2.z) > epsilon && Mathf.Abs(vertex1.z - vertex3.z) > epsilon;

        // 如果顶点在同一个平面上，返回 true
        if ((sameX && (sameY || differentY) && (sameZ || differentZ))
            || (sameY && (sameX || differentX) && (sameZ || differentZ))
            || (sameZ && (sameX || differentX) && (sameY || differentY)))
        {
            isSamePlane = true;
        }

        return isSamePlane;
    }

    private Mesh ReduceFaceCount(Mesh originalMesh)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        List<Vector3> reducedVertices = new List<Vector3>();
        List<int> reducedTriangles = new List<int>();
        Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();

        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int index0 = originalTriangles[i];
            int index1 = originalTriangles[i + 1];
            int index2 = originalTriangles[i + 2];

            Vector3 vertex0 = originalVertices[index0];
            Vector3 vertex1 = originalVertices[index1];
            Vector3 vertex2 = originalVertices[index2];

            // 计算三角面的法线向量
            Vector3 triangleNormal = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0).normalized;

            // 检查当前三角面是否与已处理的三角面重叠
            bool isOverlapped = false;
            for (int j = 0; j < reducedTriangles.Count; j += 3)
            {
                int reducedIndex0 = reducedTriangles[j];
                int reducedIndex1 = reducedTriangles[j + 1];
                int reducedIndex2 = reducedTriangles[j + 2];

                Vector3 reducedVertex0 = reducedVertices[reducedIndex0];
                Vector3 reducedVertex1 = reducedVertices[reducedIndex1];
                Vector3 reducedVertex2 = reducedVertices[reducedIndex2];

                // 计算已处理三角面的法线向量
                Vector3 reducedTriangleNormal = Vector3.Cross(reducedVertex1 - reducedVertex0, reducedVertex2 - reducedVertex0).normalized;

                // 如果法线向量相同，则说明两个三角面可以合并为一个
                if (Vector3.Dot(triangleNormal, reducedTriangleNormal) > 0.99f)
                {
                    isOverlapped = true;
                    index0 = reducedIndex0;
                    index1 = reducedIndex1;
                    index2 = reducedIndex2;
                    break;
                }
            }

            // 如果当前三角面与之前的三角面不重叠，则添加到新的顶点和三角面列表中
            if (!isOverlapped)
            {
                reducedVertices.Add(vertex0);
                reducedVertices.Add(vertex1);
                reducedVertices.Add(vertex2);

                int newIndex0 = reducedVertices.Count - 3;
                int newIndex1 = reducedVertices.Count - 2;
                int newIndex2 = reducedVertices.Count - 1;

                reducedTriangles.Add(newIndex0);
                reducedTriangles.Add(newIndex1);
                reducedTriangles.Add(newIndex2);

                // 将新旧索引的映射关系存储起来
                vertexIndexMap.Add(index0, newIndex0);
                vertexIndexMap.Add(index1, newIndex1);
                vertexIndexMap.Add(index2, newIndex2);
            }
        }

        // 更新旧索引的映射关系
        foreach (KeyValuePair<int, int> kvp in vertexIndexMap)
        {
            for (int i = 0; i < originalTriangles.Length; i++)
            {
                if (originalTriangles[i] == kvp.Key)
                {
                    originalTriangles[i] = kvp.Value;
                }
            }
        }

        // 创建减面后的网格，并设置顶点和三角面
        Mesh reducedMesh = new Mesh();
        reducedMesh.vertices = reducedVertices.ToArray();
        reducedMesh.triangles = reducedTriangles.ToArray();

        // 重新计算法线和边界
        reducedMesh.RecalculateNormals();
        reducedMesh.RecalculateBounds();

        // 返回减面后的网格
        return reducedMesh;
    }

    //凸包Graham Scan
    private List<Vector2> GetConvexHullPoints(Vector2[] points)
    {
        List<Vector2> sortedPoints = new List<Vector2>(points);

        // 根据 x 坐标进行排序
        sortedPoints.Sort((p1, p2) => p1.x.CompareTo(p2.x));

        // 从排序后的点中选择起始点（最小 x 坐标点）
        Vector2 startPoint = sortedPoints[0];
        for (int i = 1; i < sortedPoints.Count; i++)
        {
            if (sortedPoints[i].x > startPoint.x)
            {
                startPoint = sortedPoints[i];
                break;
            }
        }

        // 对其余点按照极角进行排序
        sortedPoints.Sort((p1, p2) => GetAngle(startPoint, p1).CompareTo(GetAngle(startPoint, p2)));

        // 使用栈保存凸包的边界点
        Stack<Vector2> hullPoints = new Stack<Vector2>();
        hullPoints.Push(startPoint);

        // 开始Graham Scan算法
        for (int i = 1; i < sortedPoints.Count; i++)
        {
            Vector2 next = sortedPoints[i];

            while (hullPoints.Count > 1 && Orientation(hullPoints.ElementAt(1), hullPoints.Peek(), next) < 0)
            {
                hullPoints.Pop();
            }

            hullPoints.Push(next);
        }
        List<Vector2> convexHullPoints = hullPoints.ToList();
        convexHullPoints.Reverse();

        return hullPoints.ToList();
    }



    public List<Vector2> DivideAndConquer(Vector2[] points)
    {
        List<Vector2> QuickHull(Vector2[] pts, Vector2 p1, Vector2 p2)
        {
            float Distance(Vector2 p, Vector2 lineP1, Vector2 lineP2)
            {
                float x1 = lineP1.x, y1 = lineP1.y;
                float x2 = lineP2.x, y2 = lineP2.y;
                float x = p.x, y = p.y;
                return Mathf.Abs((y2 - y1) * x - (x2 - x1) * y + x2 * y1 - y2 * x1) / ((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
            }

            Vector2 FindFurthestPoint(Vector2 lineP1, Vector2 lineP2, Vector2[] pts)
            {
                float maxDistance = 0;
                Vector2 furthestPoint = Vector2.zero;
                foreach (Vector2 point in pts)
                {
                    float d = Distance(point, lineP1, lineP2);
                    if (d > maxDistance)
                    {
                        maxDistance = d;
                        furthestPoint = point;
                    }
                }
                return furthestPoint;
            }

            if (pts.Length < 3)
            {
                return new List<Vector2>(pts);
            }

            List<Vector2> hull = new List<Vector2>();
            Vector2 minPoint = pts[0], maxPoint = pts[0];
            foreach (Vector2 point in pts)
            {
                if (point.x < minPoint.x)
                {
                    minPoint = point;
                }
                if (point.x > maxPoint.x)
                {
                    maxPoint = point;
                }
            }
            hull.Add(minPoint);
            hull.Add(maxPoint);
            pts = RemovePoint(pts, minPoint);
            pts = RemovePoint(pts, maxPoint);

            List<Vector2> leftPoints = new List<Vector2>();
            List<Vector2> rightPoints = new List<Vector2>();
            foreach (Vector2 point in pts)
            {
                if (Distance(point, minPoint, maxPoint) > 0)
                {
                    leftPoints.Add(point);
                }
                if (Distance(point, maxPoint, minPoint) > 0)
                {
                    rightPoints.Add(point);
                }
            }

            hull = BuildHull(minPoint, maxPoint, leftPoints, hull);
            hull = BuildHull(maxPoint, minPoint, rightPoints, hull);

            return hull;
        }

        Vector2[] RemovePoint(Vector2[] array, Vector2 point)
        {
            List<Vector2> newList = new List<Vector2>(array);
            newList.Remove(point);
            return newList.ToArray();
        }

        List<Vector2> BuildHull(Vector2 p1, Vector2 p2, List<Vector2> pts, List<Vector2> hull)
        {
            float Distance(Vector2 p, Vector2 lineP1, Vector2 lineP2)
            {
                float x1 = lineP1.x, y1 = lineP1.y;
                float x2 = lineP2.x, y2 = lineP2.y;
                float x = p.x, y = p.y;
                return Mathf.Abs((y2 - y1) * x - (x2 - x1) * y + x2 * y1 - y2 * x1) / ((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
            }
            if (pts.Count == 0)
            {
                return hull;
            }

            Vector2 furthestPoint = FindFurthestPoint(p1, p2, pts.ToArray());
            hull.Add(furthestPoint);
            pts.Remove(furthestPoint);

            List<Vector2> leftPoints = new List<Vector2>();
            List<Vector2> rightPoints = new List<Vector2>();
            foreach (Vector2 point in pts)
            {
                if (Distance(point, p1, furthestPoint) > 0)
                {
                    leftPoints.Add(point);
                }
                if (Distance(point, furthestPoint, p2) > 0)
                {
                    rightPoints.Add(point);
                }
            }

            hull = BuildHull(p1, furthestPoint, leftPoints, hull);
            hull = BuildHull(furthestPoint, p2, rightPoints, hull);

            return hull;
        }

        List<Vector2> outerHull = QuickHull(points, points[0], points[points.Length - 1]);

        return outerHull;
    }
    Vector2 FindFurthestPoint(Vector2 p1, Vector2 p2, Vector2[] pts)
    {
        float maxDistance = 0;
        Vector2 furthestPoint = Vector2.zero;
        foreach (Vector2 point in pts)
        {
            float distance = Vector2.Dot(point - p1, p2 - p1);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPoint = point;
            }
        }
        return furthestPoint;
    }


    //增量凸包法
    public static List<Vector2> GetConvexHullPointsQuickhull(Vector2[] points)
    {
        int n = points.Length;
        if (n < 3)
        {
            throw new Exception("The convex hull requires at least 3 points.");
        }

        Stack<Vector2> convexHull = new Stack<Vector2>();

        // Find the leftmost and rightmost points
        int minIndex = 0;
        int maxIndex = 0;
        for (int i = 1; i < n; i++)
        {
            if (points[i].x < points[minIndex].x)
            {
                minIndex = i;
            }
            if (points[i].x > points[maxIndex].x)
            {
                maxIndex = i;
            }
        }

        // Generate the upper and lower hull
        FindHull(convexHull, points, minIndex, maxIndex, 1);
        FindHull(convexHull, points, minIndex, maxIndex, -1);

        return convexHull.ToList();
    }

    private static void FindHull(Stack<Vector2> convexHull, Vector2[] points, int start, int end, int side)
    {
        int n = points.Length;
        int hullIndex = convexHull.Count;

        int furthestPointIndex = -1;
        float maxDistance = 0;

        for (int i = 0; i < n; i++)
        {
            float distance = GetSignedDistance(points[start], points[end], points[i]);
            if (distance * side > 0 && distance > maxDistance)
            {
                furthestPointIndex = i;
                maxDistance = distance;
            }
        }

        if (furthestPointIndex == -1)
        {
            convexHull.Push(points[start]);
            return;
        }

        FindHull(convexHull, points, furthestPointIndex, start, -GetSide(points[furthestPointIndex], points[start], points[end]));
        convexHull.Push(points[furthestPointIndex]);
        FindHull(convexHull, points, end, furthestPointIndex, -GetSide(points[end], points[furthestPointIndex], points[start]));
    }

    private static float GetSignedDistance(Vector2 p1, Vector2 p2, Vector2 p)
    {
        return (p.x - p1.x) * (p2.y - p1.y) - (p.y - p1.y) * (p2.x - p1.x);
    }

    private static int GetSide(Vector2 v1, Vector2 v2, Vector2 p)
    {
        float side = GetSignedDistance(v1, v2, p);
        return side < 0 ? -1 : (side > 0 ? 1 : 0);
    }

    //Jarvis
    private List<Vector2> GetConvexHullPointsJarvis(Vector2[] points)
    {
        int n = points.Length;
        if (n < 3)
        {
            throw new Exception("The convex hull requires at least 3 points.");
        }

        List<Vector2> convexHull = new List<Vector2>();
        int leftmost = 0;
        int current = leftmost;
        int next;

        do
        {
            convexHull.Add(points[current]);
            next = (current + 1) % n;

            for (int i = 0; i < n; i++)
            {
                if (Orientation1(points[current], points[i], points[next]) < 0)
                {
                    // Found a point that is more counterclockwise
                    next = i;
                }
            }

            current = next;
        } while (current != leftmost);

        return convexHull;
    }

    private float Orientation1(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
    }

    private float GetAngle(Vector2 p1, Vector2 p2)
    {
        return Mathf.Atan2(p2.y - p1.y, p2.x - p1.x);
    }

    private float Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
    }

    private Mesh RemoveNonBoundaryVertices(Mesh proxyMesh)
    {
        // 获取代理模型的顶点和三角形数据
        Vector3[] vertices = proxyMesh.vertices;
        int[] triangles = proxyMesh.triangles;

        // 创建新的边界顶点列表和边界顶点索引映射字典
        List<Vector3> boundaryVertices = new List<Vector3>();
        Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();

        // 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            // 判断边是否在外轮廓上
            bool edge1IsBoundary = IsEdgeOnBoundary(index1, index2, triangles);
            bool edge2IsBoundary = IsEdgeOnBoundary(index2, index3, triangles);
            bool edge3IsBoundary = IsEdgeOnBoundary(index3, index1, triangles);

            // 如果边在外轮廓上，则将顶点添加到边界顶点列表中，并记录索引映射
            if (edge1IsBoundary && !boundaryVertices.Contains(vertices[index1]))
            {
                boundaryVertices.Add(vertices[index1]);
                vertexIndexMap.Add(index1, boundaryVertices.Count - 1);
            }
            if (edge2IsBoundary && !boundaryVertices.Contains(vertices[index2]))
            {
                boundaryVertices.Add(vertices[index2]);
                vertexIndexMap.Add(index2, boundaryVertices.Count - 1);
            }
            if (edge3IsBoundary && !boundaryVertices.Contains(vertices[index3]))
            {
                boundaryVertices.Add(vertices[index3]);
                vertexIndexMap.Add(index3, boundaryVertices.Count - 1);
            }
        }

        // 更新顶点数组和三角形数组
        Vector3[] newVertices = new Vector3[boundaryVertices.Count];
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < boundaryVertices.Count; i++)
        {
            newVertices[i] = boundaryVertices[i];
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            int index = triangles[i];
            if (vertexIndexMap.ContainsKey(index))
            {
                newTriangles[i] = vertexIndexMap[index];
            }
            else
            {
                newTriangles[i] = -1; // 标记需要删除的三角形
            }
        }

        // 去除需要删除的三角形
        List<int> finalTriangles = new List<int>();
        for (int i = 0; i < newTriangles.Length; i += 3)
        {
            if (newTriangles[i] != -1)
            {
                finalTriangles.Add(newTriangles[i]);
                finalTriangles.Add(newTriangles[i + 1]);
                finalTriangles.Add(newTriangles[i + 2]);
            }
        }

        // 创建新的代理模型网格
        Mesh processedProxyMesh = new Mesh();
        processedProxyMesh.vertices = newVertices;
        processedProxyMesh.triangles = finalTriangles.ToArray();
        processedProxyMesh.RecalculateNormals();

        // 创建一个新的网格用于返回结果
        Mesh resultMesh = new Mesh();
        resultMesh.vertices = processedProxyMesh.vertices;
        resultMesh.triangles = processedProxyMesh.triangles;
        resultMesh.RecalculateNormals();

        return resultMesh;
    }

    private List<Vector3> GetBoundaryVertices(Mesh proxyMesh)
    {
        // 获取代理模型的顶点和三角形数据
        Vector3[] vertices = proxyMesh.vertices;
        int[] triangles = proxyMesh.triangles;

        // 创建新的边界顶点列表和边界顶点索引映射字典
        List<Vector3> boundaryVertices = new List<Vector3>();
        Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();

        // 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            // 判断边是否在外轮廓上
            bool edge1IsBoundary = IsEdgeOnBoundary(index1, index2, triangles);
            bool edge2IsBoundary = IsEdgeOnBoundary(index2, index3, triangles);
            bool edge3IsBoundary = IsEdgeOnBoundary(index3, index1, triangles);

            // 如果边在外轮廓上，则将顶点添加到边界顶点列表中，并记录索引映射
            if (edge1IsBoundary && !boundaryVertices.Contains(vertices[index1]))
            {
                boundaryVertices.Add(vertices[index1]);
                vertexIndexMap.Add(index1, boundaryVertices.Count - 1);
            }
            if (edge2IsBoundary && !boundaryVertices.Contains(vertices[index2]))
            {
                boundaryVertices.Add(vertices[index2]);
                vertexIndexMap.Add(index2, boundaryVertices.Count - 1);
            }
            if (edge3IsBoundary && !boundaryVertices.Contains(vertices[index3]))
            {
                boundaryVertices.Add(vertices[index3]);
                vertexIndexMap.Add(index3, boundaryVertices.Count - 1);
            }
        }

        // 打印所有外轮廓顶点
        for (int i = 0; i < boundaryVertices.Count; i++)
        {
            Debug.Log("Boundary Vertex " + i + ": " + boundaryVertices[i]);
        }

        return boundaryVertices;
    }

    /// <summary>
    /// 找到外轮廓顶点
    /// </summary>
    /// <param name="proxyMesh"></param>
    private void FindBoundaryVertices(Mesh proxyMesh)
    {
        // 获取代理模型的顶点和三角形数据
        Vector3[] vertices = proxyMesh.vertices;
        int[] triangles = proxyMesh.triangles;

        // 创建新的边界顶点列表
        List<Vector3> boundaryVertices = new List<Vector3>();

        // 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            // 判断边是否在外轮廓上
            bool edge1IsBoundary = IsEdgeOnBoundary(index1, index2, triangles);
            bool edge2IsBoundary = IsEdgeOnBoundary(index2, index3, triangles);
            bool edge3IsBoundary = IsEdgeOnBoundary(index3, index1, triangles);

            // 如果边在外轮廓上，则将顶点添加到边界顶点列表中
            if (edge1IsBoundary && !boundaryVertices.Contains(vertices[index1]))
            {
                boundaryVertices.Add(vertices[index1]);
            }
            if (edge2IsBoundary && !boundaryVertices.Contains(vertices[index2]))
            {
                boundaryVertices.Add(vertices[index2]);
            }
            if (edge3IsBoundary && !boundaryVertices.Contains(vertices[index3]))
            {
                boundaryVertices.Add(vertices[index3]);
            }
        }

        // 输出边界顶点数量
        Debug.Log("轮廓上的顶点数量: " + boundaryVertices.Count);

        // 在这里你可以将边界顶点列表用于进一步处理或显示
        // ...
    }

    private bool IsEdgeOnBoundary(int index1, int index2, int[] triangles)
    {
        // 统计共享该边的三角形数量
        int sharedTriangleCount = 0;

        // 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // 跳过相同的三角形
            if (i == index1 || i == index2)
            {
                continue;
            }

            int triangleIndex1 = triangles[i];
            int triangleIndex2 = triangles[i + 1];
            int triangleIndex3 = triangles[i + 2];

            // 检查是否有共享该边的三角形
            if ((triangleIndex1 == index1 || triangleIndex1 == index2) &&
                (triangleIndex2 == index1 || triangleIndex2 == index2))
            {
                sharedTriangleCount++;
            }
            else if ((triangleIndex1 == index1 || triangleIndex1 == index2) &&
                (triangleIndex3 == index1 || triangleIndex3 == index2))
            {
                sharedTriangleCount++;
            }
            else if ((triangleIndex2 == index1 || triangleIndex2 == index2) &&
                (triangleIndex3 == index1 || triangleIndex3 == index2))
            {
                sharedTriangleCount++;
            }
        }

        // 如果共享该边的三角形数量为1，则该边在外轮廓上
        return sharedTriangleCount == 1;
    }


    /* 我拿到的这个proxyMesh理论上就是一个平面模型一个planeMesh，现在这个函数理论上应该由我删除无用顶点，重新构建三角面

 外轮廓的顶点都视为有用顶点，理论上这个planeMesh不可能存在内轮廓，所以外轮廓上的任意顶点都是为有用顶点，其他顶点都删除
 然后构建三角面，其实就是有效顶点的遍历加链接，你且不可随意链接整体基于链接附近顶点，这就需要判断链接以后会不会扩大外轮廓如果不会才可以链接，这就叫做内部链接，所以你可能要判断这个顶点附近的两个顶点构成的三角形是凹凸多边形的判断进而找到合理的连接点*/
    private Mesh GenerateProcessedProxyMesh(Mesh proxyMesh)
    {
        // 获取代理模型的顶点和三角形数据
        Vector3[] vertices = proxyMesh.vertices;
        int[] triangles = proxyMesh.triangles;

        // 创建新的顶点列表和边界顶点索引数组
        List<Vector3> processedVertices = new List<Vector3>();
        List<int> boundaryIndices = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int index1 = triangles[i];
            int index2 = triangles[i + 1];
            int index3 = triangles[i + 2];

            // 表示三角形三个顶点是否都是边界上的顶点
            bool isBoundaryTriangle = false;

            // 判断顶点是否在边界上
            if (IsVertexOnBoundary(vertices[index1], vertices[index2], vertices[index3]))
            {
                isBoundaryTriangle = true;
                // 添加边界顶点索引到边界索引列表
                boundaryIndices.Add(index1);
                boundaryIndices.Add(index2);
                boundaryIndices.Add(index3);
                Debug.Log("顶点在边界上");
            }
            else
            {
                Debug.Log("顶点不在边界上");
            }

            // 如果是边界上的顶点，将顶点添加到处理后的顶点列表中
            if (isBoundaryTriangle)
            {
                if (!processedVertices.Contains(vertices[index1]))
                    processedVertices.Add(vertices[index1]);
                if (!processedVertices.Contains(vertices[index2]))
                    processedVertices.Add(vertices[index2]);
                if (!processedVertices.Contains(vertices[index3]))
                    processedVertices.Add(vertices[index3]);
            }
        }

        // 创建新的代理模型网格
        Mesh processedProxyMesh = new Mesh();
        processedProxyMesh.vertices = processedVertices.ToArray();
        processedProxyMesh.triangles = boundaryIndices.ToArray();
        processedProxyMesh.RecalculateNormals();

        return processedProxyMesh;
    }
    private bool IsVertexOnBoundary(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        // 计算顶点之间的边向量
        Vector3 edge1 = vertex2 - vertex1;
        Vector3 edge2 = vertex3 - vertex2;
        Vector3 edge3 = vertex1 - vertex3;

        // 检查每条边向量是否至少与一条相邻边向量平行
        bool isParallel1 = Mathf.Approximately(Vector3.Dot(edge1.normalized, edge2.normalized), 1f);
        bool isParallel2 = Mathf.Approximately(Vector3.Dot(edge2.normalized, edge3.normalized), 1f);
        bool isParallel3 = Mathf.Approximately(Vector3.Dot(edge3.normalized, edge1.normalized), 1f);

        // 判断顶点是否在边界上，如果至少有两组相邻边平行，则顶点在边界上
        return (isParallel1 && (isParallel2 || isParallel3)) || (isParallel2 && isParallel3);
    }

    // 检查顶点是否在边界上的方法
    /*  private bool IsVertexOnBoundary(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
      {
          // 检查顶点是否在边界上的逻辑需要根据您的具体需求来实现
          // 例如，可以根据顶点的法线方向、相邻三角面的关系等进行判断
          // 这里只是一个示例，需要根据具体情况进行修改
          // 假设边界上的顶点法线方向与边界平面垂直（例如，边界平面的法线方向为(0, 1, 0)）
          // 您可以根据实际需求修改该判断条件
          return Mathf.Approximately(normal.y, 1f);
      }*/

    private Mesh GenerateProxyMeshWithShadows(Mesh originalMesh, Vector3 lightDirection)
    {
        // 创建新的代理模型的顶点和三角形数组
        List<Vector3> proxyVertices = new List<Vector3>();
        List<int> proxyTriangles = new List<int>();

        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        // 在XZ平面上计算灯光的投影方向
        Vector3 projectionDirection = lightDirection;
        //222后向面剔除（Backface culling）

        //深度测试（Depth testing）和
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int index0 = originalTriangles[i];//索引不代表顶点,一个点有多个索引
            int index1 = originalTriangles[i + 1];
            int index2 = originalTriangles[i + 2];

            Vector3 vertex0 = originalVertices[index0];
            Vector3 vertex1 = originalVertices[index1];
            Vector3 vertex2 = originalVertices[index2];
            // 计算三角形面法线
            Vector3 triangleNormal = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0).normalized;

            // 判断三角形是否会投影到平行光的XZ平面上
            //1 、 0 、 -1 、 0 ，1
            if (Vector3.Dot(triangleNormal, projectionDirection) > 0)
            {
                continue; // 如果三角形的法线与投影方向相同(相切)，则跳过该三角形,是表面渲染背面都不渲染
            }
            //问题,光反,和 删除逻辑反,是否负负得正;我觉得未必如此
            //都反代表都留着最后一层模型,极其复杂的楼房,最后剩余的就是一个面左,和一个面右.两种都会删除中心着没问题
            //但是都反都是极少保留,只有正才是真正保留的

          


            //1111遮挡查询（Occlusion queries）
            // 通过对比顶点法线和光线方向判断三角形是否被遮挡
            // 遮挡是自身和自身的关系,比如内部还是和光线面对面,但是他是内部的
            bool isOccluded = false;

            for (int j = 0; j < originalTriangles.Length; j += 3)
            {
                if (j == i)
                {
                    continue;
                }

                int otherIndex0 = originalTriangles[j];
                int otherIndex1 = originalTriangles[j + 1];
                int otherIndex2 = originalTriangles[j + 2];

                Vector3 otherVertex0 = originalVertices[otherIndex0];
                Vector3 otherVertex1 = originalVertices[otherIndex1];
                Vector3 otherVertex2 = originalVertices[otherIndex2];

                Vector3 otherTriangleNormal = Vector3.Cross(otherVertex1 - otherVertex0, otherVertex2 - otherVertex0).normalized;

                if (Vector3.Dot(otherTriangleNormal, projectionDirection) > 0)
                {
                    continue;
                }

                // Create a ray from a vertex of the current triangle to the light source position
                Ray ray = new Ray(vertex0, lightDirection);

                // Check if the ray intersects with the other triangle
                if (IsRayIntersectingTriangle(ray, otherVertex0, otherVertex1, otherVertex2))
                {
                    // Calculate the distance from the intersection point to the light source
                    float distanceToLight = Vector3.Distance(ray.origin, sceneLight.transform.position);

                    // Calculate the distance from the current vertex to the light source
                    float vertexToLightDistance = Vector3.Distance(vertex0, sceneLight.transform.position);

                    // If the intersection point is closer to the light source than the current vertex, the triangle is occluded
                    if (distanceToLight < vertexToLightDistance)
                    {
                        isOccluded = true;
                        break;
                    }
                }
            }

            if (!isOccluded)
            {
                // Add vertices and triangles to the proxy mesh arrays
                proxyVertices.Add(vertex0);
                proxyVertices.Add(vertex1);
                proxyVertices.Add(vertex2);

                proxyTriangles.Add(proxyVertices.Count - 3);
                proxyTriangles.Add(proxyVertices.Count - 2);
                proxyTriangles.Add(proxyVertices.Count - 1);
            }
        }

            // 创建代理模型的网格，并设置顶点和三角形
            Mesh proxyMesh = new Mesh();
        proxyMesh.vertices = proxyVertices.ToArray();
        proxyMesh.triangles = proxyTriangles.ToArray();

        // 重新计算法线和边界
        proxyMesh.RecalculateNormals();
        proxyMesh.RecalculateBounds();

        // 返回生成的代理模型的mesh
        return proxyMesh;
    }

    

    private bool IsRayIntersectingTriangle(Ray ray, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        Plane trianglePlane = new Plane(vertex0, vertex1, vertex2);

        float distance;
        if (trianglePlane.Raycast(ray, out distance))
        {
            Vector3 intersectionPoint = ray.GetPoint(distance);

            if (PointInTriangle(intersectionPoint, vertex0, vertex1, vertex2))
            {
                return true;
            }
        }

        return false;
    }

    private bool PointInTriangle(Vector3 point, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        Vector3 edge0 = vertex1 - vertex0;
        Vector3 edge1 = vertex2 - vertex1;
        Vector3 edge2 = vertex0 - vertex2;

        Vector3 c0 = Vector3.Cross(edge0, point - vertex0);
        Vector3 c1 = Vector3.Cross(edge1, point - vertex1);
        Vector3 c2 = Vector3.Cross(edge2, point - vertex2);

        return Vector3.Dot(c0, c1) >= 0 && Vector3.Dot(c1, c2) >= 0;
    }

    private bool IsTriangleOccludedByMesh(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3[] vertices, int[] triangles, int ignoreIndex)
    {
        // 通过光线投射法判断三角形是否被物体的其他面遮挡
        Ray ray = new Ray(vertex0, (vertex1 - vertex0).normalized);
        float maxDistance = Vector3.Distance(vertex0, vertex1);
        RaycastHit hitInfo;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (i == ignoreIndex)
            {
                continue; // 忽略当前三角形
            }

            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            if (Physics.Raycast(ray, out hitInfo, maxDistance))
            {
                // 如果投射到的面不是被忽略的三角形，且投射点在当前三角形内部，则返回被遮挡
                if (hitInfo.triangleIndex != i && IsPointInTriangle(hitInfo.point, v0, v1, v2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsPointInTriangle(Vector3 point, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        // 判断点是否在给定的三角形内部
        Vector3 edge0 = vertex1 - vertex0;
        Vector3 edge1 = vertex2 - vertex0;
        Vector3 edge2 = point - vertex0;

        float dot00 = Vector3.Dot(edge0, edge0);
        float dot01 = Vector3.Dot(edge0, edge1);
        float dot02 = Vector3.Dot(edge0, edge2);
        float dot11 = Vector3.Dot(edge1, edge1);
        float dot12 = Vector3.Dot(edge1, edge2);

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0f) && (v >= 0f) && (u + v < 1f);
    }

    //平行光如果想不碰到人，垂直光就不能找到对焦，就穿出模型，高模型有问题
    //垂直他自己就不准确，他生产的模型透在地面一定不准确
    //找对焦两点做代理面的方式，你不好确定哪些是哪个面

    //如何做深度阴影代理呢
    //删除
    // 根据灯光角度和投影生成2D代理模型
    private Mesh GenerateProxyMesh(Mesh originalMesh, Quaternion objRotation, Vector3 lightDirection)
    {
        // 在这里根据灯光角度和投影等因素生成代理模型的2D mesh
       /* Vector3 lightDirection = lightDirection1*//*-sceneLight.transform.forward*//*;*/
        Quaternion lightRotation = Quaternion.LookRotation(lightDirection, Vector3.up);



        // 计算模型和灯光切面
        Plane lightPlane = new Plane(lightDirection, Vector3.zero);
        float verticalAngle = Vector3.Angle(Vector3.up, lightDirection);
        float horizontalAngle = 180 - verticalAngle;

        // 创建新的代理模型的顶点和三角形数组
        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] proxyVertices = new Vector3[originalVertices.Length];
        int[] proxyTriangles = originalMesh.triangles;

        //for (int i = 0; i < originalVertices.Length; i++)
        //{
        //    Vector3 vertex = originalVertices[i];

        //    // 根据顶点和世界旋转计算投影点的位置
        //    Vector3 projectedVertex = objRotation * vertex;


        //    // 将投影点的y坐标设置为0，即将其投影到平面上
        //    projectedVertex.y = 0;

        //    // 将投影点的位置作为代理模型的顶点位置
        //    proxyVertices[i] = projectedVertex;
        //}

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            // 根据顶点和世界旋转计算投影点的位置
            Vector3 projectedVertex = objRotation * vertex;

            // 考虑灯光的角度，将投影点相对于灯光进行旋转
            projectedVertex = lightRotation * projectedVertex;

            // 将投影点的y坐标设置为0，即将其投影到平面上
            projectedVertex.y = 0;

            // 将投影点的位置作为代理模型的顶点位置
            proxyVertices[i] = projectedVertex;
        }

        // 创建代理模型的网格，并设置顶点和三角形
        Mesh proxyMesh = new Mesh();
        proxyMesh.vertices = proxyVertices;
        proxyMesh.triangles = proxyTriangles;

        // 重新计算法线和边界
        proxyMesh.RecalculateNormals();
        proxyMesh.RecalculateBounds();

        // 如果新模型需要变换到垂直地面来产生相同阴影，则进行缩放
        if (verticalAngle > 0)
        {
            float scale = Mathf.Tan(Mathf.Deg2Rad * horizontalAngle) / Mathf.Tan(Mathf.Deg2Rad * verticalAngle);
            Vector3[] scaledVertices = new Vector3[originalVertices.Length];

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = proxyVertices[i];
                vertex *= scale;
                scaledVertices[i] = vertex;
            }

            proxyMesh.vertices = scaledVertices;
        }

        // 返回生成的代理模型的mesh
        return proxyMesh;
    }
    

    private void SaveMeshAsAsset(Mesh mesh, string fileName)
    {
        GameObject obj = Selection.activeGameObject;

        // 获取目标文件夹路径
        string objDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
        string outputFolder = Path.Combine(objDirectory, "ProxyModels");

        // 创建文件夹如果不存在
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string filePath = Path.Combine(outputFolder, fileName + ".asset");

        // 保存Mesh为Asset文件
        AssetDatabase.CreateAsset(mesh, filePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Generated proxy model saved as asset: " + filePath);
    }

    private struct Edge
    {
        public int Index1 { get; }
        public int Index2 { get; }

        public Edge(int index1, int index2)
        {
            Index1 = index1;
            Index2 = index2;
        }

        public override int GetHashCode()
        {
            return Index1.GetHashCode() ^ Index2.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge))
            {
                return false;
            }

            Edge other = (Edge)obj;
            return (Index1 == other.Index1 && Index2 == other.Index2) ||
                   (Index1 == other.Index2 && Index2 == other.Index1);
        }
    }


    //=====================生成遮罩贴图得了
    private Texture2D GenerateMaskTexture(Mesh originalMesh, Quaternion objRotation, int textureSize)
    {
        // 创建一个新的贴图
        Texture2D maskTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        // 获取原始模型的顶点和三角形数组
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        // 创建一个颜色数组来保存遮罩贴图的像素颜色
        Color[] colors = new Color[textureSize * textureSize];

        // 遍历贴图的每个像素
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // 计算当前像素在原始模型中的位置
                Vector3 pixelPosition = new Vector3((float)x / (textureSize - 1), 0, (float)y / (textureSize - 1));

                // 将像素位置转换为世界空间，并根据物体旋转进行转换
                Vector3 worldPixelPosition = objRotation * Vector3.Scale(pixelPosition - new Vector3(0.5f, 0, 0.5f), originalMesh.bounds.size) + originalMesh.bounds.center;

                // 检查像素位置是否在模型内部
                if (IsPointInsideMesh(originalVertices, originalTriangles, worldPixelPosition))
                {
                    // 设置像素颜色为不透明
                    colors[y * textureSize + x] = Color.white;
                }
                else
                {
                    // 设置像素颜色为透明
                    colors[y * textureSize + x] = Color.clear;
                }
            }
        }

        // 将颜色数组设置到遮罩贴图中
        maskTexture.SetPixels(colors);
        maskTexture.Apply();

        // 返回生成的遮罩贴图
        return maskTexture;
    }

    private bool IsPointInsideMesh(Vector3[] vertices, int[] triangles, Vector3 point)
    {
        // 创建一个射线，从点外部向上发射
        Ray ray = new Ray(point + Vector3.up * 1000, Vector3.down);

        int triangleCount = triangles.Length / 3;

        // 遍历每个三角形
        for (int i = 0; i < triangleCount; i++)
        {
            // 获取三角形的顶点索引
            int vertexIndexA = triangles[i * 3];
            int vertexIndexB = triangles[i * 3 + 1];
            int vertexIndexC = triangles[i * 3 + 2];

            // 获取三角形的顶点位置
            Vector3 vertexA = vertices[vertexIndexA];
            Vector3 vertexB = vertices[vertexIndexB];
            Vector3 vertexC = vertices[vertexIndexC];

            // 检查点是否在三角形内部
            if (IsPointInsideTriangle(vertexA, vertexB, vertexC, point))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointInsideTriangle(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC, Vector3 point)
    {
        // 计算三角形的法线
        Vector3 triangleNormal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;

        // 创建一个射线，从点向法线方向发射
        Ray ray = new Ray(point, triangleNormal);

        // 获取三角形平面的距离
        float distance = Vector3.Dot(triangleNormal, vertexA);

        // 计算射线和三角形平面的交点
        Vector3 intersectionPoint = ray.GetPoint(distance);

        // 检查交点是否在三角形内部
        if (IsPointInTriangleArea(vertexA, vertexB, vertexC, intersectionPoint))
        {
            return true;
        }

        return false;
    }

    private bool IsPointInTriangleArea(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC, Vector3 point)
    {
        // 计算三个子三角形的面积
        float totalArea = CalculateTriangleArea(vertexA, vertexB, vertexC);
        float areaA = CalculateTriangleArea(point, vertexB, vertexC);
        float areaB = CalculateTriangleArea(vertexA, point, vertexC);
        float areaC = CalculateTriangleArea(vertexA, vertexB, point);

        // 检查点是否在三角形内部
        if (Mathf.Approximately(totalArea, areaA + areaB + areaC))
        {
            return true;
        }

        return false;
    }

    private float CalculateTriangleArea(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
    {
        // 计算三个顶点之间的长度
        float sideA = Vector3.Distance(vertexA, vertexB);
        float sideB = Vector3.Distance(vertexB, vertexC);
        float sideC = Vector3.Distance(vertexC, vertexA);

        // 使用海伦公式计算三角形的面积
        float s = (sideA + sideB + sideC) / 2;
        float area = Mathf.Sqrt(s * (s - sideA) * (s - sideB) * (s - sideC));

        return area;
    }
}
//===============================================================
/*
private void GenerateProxyModels()
{
    GameObject[] selectedObjects = Selection.gameObjects;

    foreach (GameObject obj in selectedObjects)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null)
            continue;

        Mesh originalMesh = meshFilter.sharedMesh;

        Mesh proxyMesh = GenerateOutlineMesh(originalMesh);

        Mesh filledMesh = FillOutlineOnPlane(proxyMesh, obj);

        GameObject proxyModel = Instantiate(obj, obj.transform.position, obj.transform.rotation * Quaternion.Euler(90f, 0f, 0f));

        proxyModel.transform.position = obj.transform.position;
        proxyModel.transform.rotation = obj.transform.rotation;
        proxyModel.transform.localScale = obj.transform.localScale;

        MeshRenderer meshRenderer = proxyModel.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter proxyMeshFilter = proxyModel.AddComponent<MeshFilter>();
        proxyMeshFilter.sharedMesh = filledMesh;

        outputFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));

        // 检查输出文件夹是否存在，如果不存在则创建它
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        SaveMeshAsFbx(filledMesh, Path.Combine(outputFolder, obj.name + "_proxy"));
    }
}

private Mesh GenerateOutlineMesh(Mesh originalMesh)
{
    Vector3[] originalVertices = originalMesh.vertices;
    int[] originalTriangles = originalMesh.triangles;

    List<Vector3> outlineVertices = new List<Vector3>();
    List<int> outlineTriangles = new List<int>();

    Vector3 lightDirection = -sceneLight.transform.forward;

    for (int i = 0; i < originalTriangles.Length; i += 3)
    {
        int vertexIndex1 = originalTriangles[i];
        int vertexIndex2 = originalTriangles[i + 1];
        int vertexIndex3 = originalTriangles[i + 2];

        Vector3 vertex1 = originalVertices[vertexIndex1];
        Vector3 vertex2 = originalVertices[vertexIndex2];
        Vector3 vertex3 = originalVertices[vertexIndex3];

        Vector3 triangleNormal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;

        if (Vector3.Dot(triangleNormal, lightDirection) > 0)
        {
            outlineVertices.Add(vertex1);
            outlineVertices.Add(vertex2);
            outlineVertices.Add(vertex3);

            int vertexCount = outlineVertices.Count;

            outlineTriangles.Add(vertexCount - 3);
            outlineTriangles.Add(vertexCount - 2);
            outlineTriangles.Add(vertexCount - 1);
        }
    }

    Mesh outlineMesh = new Mesh();
    outlineMesh.vertices = outlineVertices.ToArray();
    outlineMesh.triangles = outlineTriangles.ToArray();

    outlineMesh.RecalculateNormals();
    outlineMesh.RecalculateBounds();

    return outlineMesh;
}

private Mesh FillOutlineOnPlane(Mesh outlineMesh, GameObject obj)
{
    Vector3 planeNormal = -sceneLight.transform.forward;

    Vector3[] outlineVertices = outlineMesh.vertices;
    Vector3[] planeVertices = new Vector3[outlineVertices.Length];

    for (int i = 0; i < outlineVertices.Length; i++)
    {
        planeVertices[i] = ProjectPointOnPlane(outlineVertices[i], planeNormal, obj.transform.position);
    }

    int[] outlineTriangles = outlineMesh.triangles;

    int[] planeTriangles = new int[outlineTriangles.Length];

    for (int i = 0; i < outlineTriangles.Length; i++)
    {
        int vertexIndex = outlineTriangles[i];

        planeTriangles[i] = vertexIndex;
    }

    Mesh filledMesh = new Mesh();
    filledMesh.vertices = planeVertices;
    filledMesh.triangles = planeTriangles;

    filledMesh.RecalculateNormals();
    filledMesh.RecalculateBounds();

    return filledMesh;
}

private void SaveMeshAsFbx(Mesh mesh, string fileName)
{
    string path = fileName + ".fbx";
    AssetDatabase.CreateAsset(mesh, path);
    AssetDatabase.SaveAssets();
}

private Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeNormal, Vector3 planeOrigin)
{
    Vector3 projection = Vector3.ProjectOnPlane(point - planeOrigin, planeNormal);
    return planeOrigin + projection;
}*/
//============================================================

