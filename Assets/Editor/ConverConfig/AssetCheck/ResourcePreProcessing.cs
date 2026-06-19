/// <summary>
/// 资源预处理
/// 做成离线数据
/// </summary>
#if UNITY_EDITOR
using AmplifyImpostors;
using Animancer;
using Koenigz.PerfectCulling.SamplingProviders;
using SGF.Utlis;
using StarProject.OffLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class ResourcePreProcessing : EditorWindow
{
    private static string Hurt_D = "Hurt_D";                        //胸部受击伤害挂点
    private static string BackWeapon_D = "BackWeapon_D";            //后背部武器挂点
    private static string Wing_D = "Wing_D";                        //后背部翅膀挂点
    private static string HandWeapon_D_L = "HandWeapon_D_L";        //左手武器手部挂点
    private static string HandWeapon_D_R = "HandWeapon_D_R";        //右手武器手部挂点
    private static string WeaponRoot_D_L = "WeaponRoot_D_L";        //左手武器根部挂点
    private static string WeaponRoot_D_R = "WeaponRoot_D_R";        //右手武器根部挂点
    private static string WeaponHurt_D_R = "WeaponHurt_D_R";        //右手或者双手武器特效挂点
    private static string WeaponHurt_D_L = "WeaponHurt_D_L";        //左手武器特效挂点
    private static string Root_D = "Root_D";                        //Root下（脚下）加特效挂点
    private static string Top_D = "Top_D";                          //头顶眩晕挂点
    private static string Foot_D_R = "Foot_D_R";                    //右脚脚底特效挂点
    private static string Foot_D_L = "Foot_D_L";                    //左脚脚底特效挂点
    private static string UnitInfo_D = "UnitInfo_D";                //头顶信息（气泡，公会，名字等）挂点
    private static string Bip001 = "Bip001";                        //尾巴骨位置挂点（用于主角模型的摄像机偏移）
    private static string Mouth_D = "Mouth_D";                      //嘴巴挂点
    private static string Mouth_D02 = "Mouth_D02";                      //嘴巴2挂点
    private static string Mouth_D03 = "Mouth_D03";                      //嘴巴2挂点


    private static List<string> CheckBindPointStringNames = new List<string>() {
    Hurt_D,BackWeapon_D,Wing_D,HandWeapon_D_L,HandWeapon_D_R,WeaponRoot_D_L,
    WeaponRoot_D_R,WeaponHurt_D_R,WeaponHurt_D_L,Root_D,Top_D,Foot_D_R,
    Foot_D_L,UnitInfo_D,Bip001,Mouth_D,Mouth_D02,Mouth_D03
    };


    [MenuItem("自动化工具/修复模型描边丢失")]
    static void ReimportModelAssets()
    {
        // 搜索包含 R_WD 的模型资源
        string[] wdGuids = AssetDatabase.FindAssets("t:Model R_WD");

        // 搜索包含 R_DP 的模型资源
        string[] dpGuids = AssetDatabase.FindAssets("t:Model R_DP");

        // 合并两次搜索结果的资源 guid
        string[] allGuids = wdGuids.Concat(dpGuids).Distinct().ToArray();

        foreach (string guid in allGuids)
        {
            // 获取资源路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 获取资源名称
            string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            // 打印资源名称
            Debug.Log("Found Model Asset: " + assetName);

            // 重新导入资源
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("Model Assets Reimported Successfully");
    }



    [MenuItem("gopal/DebugMipmap")]
    public static void VariantCollector()
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParts/StarOpt/Console/VisualizationMode/TestMipMap.prefab");
        GameObject.Instantiate(go);
    }


    [MenuItem("gopal/DebugSrp")]
    public static void DebugSRP()
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParts/StarOpt/Console/VisualizationMode/Srp.prefab");
        GameObject.Instantiate(go);
    }
    //// % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
    //名字有大写，有小写
    /// <summary>
    /// 功能，选中的物品名称，在父级节点进行搜索;要有instance奇怪问题的
    /// </summary>
    [MenuItem("自动化工具/角色处理相关/绑定挂点 &B", false, 10)]
    public static void BindDummy()
    {
        ///选择目标就要选择模型:小曲找模型
        /*Transform ModelOffset = UnityEditor.Selection.gameObjects[0].transform.Find("ModelOffset");

        if (ModelOffset.GetComponent<ModelOffLineData>() == null)
        {
            ModelOffset.gameObject.AddComponent<ModelOffLineData>();
        }*/
        Transform ArtModel = UnityEditor.Selection.gameObjects[0].transform;
        /*Dictionary<string, Transform> BindDummyPos = ModelOffset.GetComponent<ModelOffLineData>().BindDummyPos;*/
        if (ArtModel.GetComponent<AnimancerComponent>() == null)
        {
            Debug.LogError("请选择角色节点");
            return;
        }
        if (ArtModel.GetComponent<ModelOffLineData>() == null)
        {
            ArtModel.gameObject.AddComponent<ModelOffLineData>();
        }


        Dictionary<string, Transform> BindDummyPos = ArtModel.GetComponent<ModelOffLineData>().BindDummyPos;
        BindDummyPos.Clear();


        /*Transform Root = ModelOffset.GetChild(0).Find("Root");*/
        Transform Root = ArtModel.Find("Root");
        if (Root != null)
        {
            BindDummyPos.Add("Root", Root);
            string findName = "";

            for (int i = 0; i < CheckBindPointStringNames.Count; i++)
            {
                findName = CheckBindPointStringNames[i];

                for (int j = 0; j < Root.childCount; j++)
                {
                    //目前单状态，所以在结果的itemlist里面全部遍历，就一个就好【0】
                    Transform child = Root.GetChild(j);
                    if (child.name == findName)
                    {
                        BindDummyPos.Add(findName, child);
                    }
                    else
                    {
                        List<Transform> bindPart = Root.GetChild(j).DeepFirstTransList(findName);
                        if (bindPart != null && bindPart.Count != 0 && bindPart[0] != null)
                        {
                            //ModelOffLineData
                            //bindPart = tranRoot.GetChild(i).DeepFirstTransList("item");

                            BindDummyPos.Add(findName, bindPart[0]);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("模型没有Root根节点！！！");
        }
        // 设置值为脏数据，，编辑器会自己检测一下
        UnityEditor.EditorUtility.SetDirty(ArtModel);
    }
    [MenuItem("自动化工具/界面处理相关/绑定离线数据 #&w", false, 10)]
    public static void BindPanelOffLineData()
    {
        Transform ArtModel = UnityEditor.Selection.gameObjects[0].transform;
        if (ArtModel.GetComponent<PanelOffLineData>() == null)
        {
            ArtModel.gameObject.AddComponent<PanelOffLineData>();
        }
        ArtModel.GetComponent<PanelOffLineData>().GenerateNodesData();

        // 设置值为脏数据，，编辑器会自己检测一下
        UnityEditor.EditorUtility.SetDirty(ArtModel);
    }


    [MenuItem("自动化工具/界面处理相关/绑定离线数据对应路径表现 %#&w", false, 10)]
    public static void DebugBindPanelOffLineData()
    {
        Transform select = UnityEditor.Selection.gameObjects[0].transform;
        if (select == null)
        {
            Debug.LogError("点空了");
        }
        Stack<string> names = new Stack<string>();

        Transform current = select;

        // check if current object contains PanelOffLineData component
        while (current != null && current.GetComponent<PanelOffLineData>() == null)
        {
            names.Push(current.name); // save current parent's name
            current = current.parent;
        }

        // check if we have reached the root node with PanelOffLineData
        if (current != null && current.GetComponent<PanelOffLineData>() != null)
        {
            // constructing path
            string path = "";
            while (names.Count > 1)  //修改了循环条件
            {
                path += names.Pop() + "/"; //修改了拼接字符串的位置
            }
            path += names.Pop();  // 在循环结束时添加最后一个节点名

            // at this point, we have path to our new button with all its data
            Debug.Log("Button path: " + path);
        }

        // 设置值为脏数据，，编辑器会自己检测一下
        UnityEditor.EditorUtility.SetDirty(select);
    }

    [MenuItem("自动化工具/特效LOD处理相关/绑定特效离线数据 #&T", false, 10)]
    public static void BindFXOffLineData()
    {
        Transform ArtModel = UnityEditor.Selection.gameObjects[0].transform;
        if (ArtModel.GetComponent<FXLODOffLineData>() == null)
        {
            ArtModel.gameObject.AddComponent<FXLODOffLineData>();
        }
        ArtModel.GetComponent<FXLODOffLineData>().GenerateNodesData();

        // 设置值为脏数据，，编辑器会自己检测一下
        UnityEditor.EditorUtility.SetDirty(ArtModel);
    }



    //名字有大写，有小写
    /// <summary>
    /// 功能，选中的物品名称，在父级节点进行搜索;要有instance奇怪问题的
    /// </summary>
    [MenuItem("自动化工具/浮点数点高管/处理浮点数Scale #&C", false, 10)]
    public static void ClearTransformFloat()
    {
        Vector3 localP;
        Vector3 localR;
        Vector3 localS;
        GameObject[] ArtModels = UnityEditor.Selection.gameObjects;
        for (int i = 0; i < ArtModels.Length; i++)
        {
            localS = ArtModels[i].transform.localScale;
            localS.x = (float)Math.Round((double)localS.x, 3);
            localS.y = (float)Math.Round((double)localS.y, 3);
            localS.z = (float)Math.Round((double)localS.z, 3);
            ArtModels[i].transform.localScale = localS;
        }
        // 设置值为脏数据，，编辑器会自己检测一下
        //UnityEditor.EditorUtility.SetDirty(ArtModel);
    }

    //名字有大写，有小写
    /// <summary>
    /// 功能，选中的物品名称，在父级节点进行搜索;要有instance奇怪问题的
    /// </summary>
    [MenuItem("自动化工具/[石头]_3添加Culling给石头 #&O", false, 10)]
    public static void AutoAdd()
    {
        GameObject[] currentSameNameReplaced = UnityEditor.Selection.gameObjects;
        Transform RocksRoot = currentSameNameReplaced[0].transform;
        //选择RockRoot
        for (int i = 0; i < RocksRoot.childCount; i++)
        {
            /*LOD[] lods;
            LODGroup lODGroup;*/
            PerfectCullingRebounder PerfectCullingRebounder;
            for (int j = 0; j < RocksRoot.GetChild(i).transform.childCount; j++)
            { //而且不应该剔除已经呗Lod的情况，没必要，增加脚本力度了
                if (j == 0)
                {
                    Transform eachMeshRenderLod = RocksRoot.GetChild(i).transform.GetChild(j);
                    if (eachMeshRenderLod.GetComponent<MeshRenderer>() != null)
                    {
                        PerfectCullingRebounder = eachMeshRenderLod.AddComp<PerfectCullingRebounder>();
                        PerfectCullingRebounder.setPer(0.5f);
                        //主要是竖屏场景，所以竖向剔除力度要很大
                       
                    }
                }
             
            }
          


        }
        Debug.Log("处理完成！成功！");
    }


    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[石头]_1含预先处理非Lod的层级结构 #&Z", false, 10)]
    public static void AutoToLodLayer()
    {
        //直接选择RockGroup节点哦
        //执行过程中顺序会发生改变多执行几次
        GameObject[] OnlySelectRockGroupRoot = UnityEditor.Selection.gameObjects;
        for (int i = 0; i < OnlySelectRockGroupRoot[0].transform.childCount; i++)
        {
            Transform LodLayer = OnlySelectRockGroupRoot[0].transform.GetChild(i);
            GameObject gob = null;
            //补充创建
            if (LodLayer.GetComponent<MeshRenderer>() != null)
            {
               
                gob = new GameObject();
                gob.transform.SetParent(OnlySelectRockGroupRoot[0].transform);
                gob.transform.localPosition = LodLayer.localPosition;
                gob.transform.eulerAngles = LodLayer.eulerAngles;
                gob.transform.localScale = LodLayer.localScale;
                gob.name = LodLayer.name + "_Pfb";
                LodLayer.SetParent(gob.transform);
                //这一句是因为换Gob为Lod层级了嘛。
                LodLayer = gob.transform;
            }
            //1都没有Mesh外层--2都有里面内容——3perfab--4错误的层级对--5transform设置外面承担里面不承担---6外层里层位置对
            //13
            //正式处理Lod
            if (LodLayer.GetComponent<MeshRenderer>() == null)
            {
                LODGroup lODGroup = LodLayer.AddComp<LODGroup>();
                LOD[] lods;
                lods = lODGroup.GetLODs();
                lODGroup.RecalculateBounds();//根据自己实际大小来定义
                for (int j = 0; j < lods.Length; j++)
                {
                    try
                    {
                        //不知道+是什么意识，new一个数组，但是人家是public被unity管，是基于手操作的
                        //N级Lod:通过Debug看其实不是null，而是Length == 0
                        if (lods[j].renderers.Length == 0)
                        {
                            lods[j].renderers = new Renderer[1];

                        }
                        if (j == 0)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.11f;//22
                            lods[j].renderers[0] = LodLayer.transform.GetChild( 0 ).GetComponent<Renderer>();
                        }
                        else if (j == 1)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.10f; //2.75= 0.0275f  0.5098959 = 0.005098959
                            lods[j].renderers[0] = LodLayer.transform.GetChild( 0 ).GetComponent<Renderer>();
                            //这一层是等他给我低模的
                        }
                        else if (j == 2)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.0051f;//2.75= 0.0275f  0.5098959 = 0.005098959
                            if (LodLayer.transform.GetChild( 1 ) != null)
                            {
                                lods[j].renderers[0] = LodLayer.transform.GetChild( 1 ).GetComponent<Renderer>();
                            }
                            else
                            {
                                lods[j].renderers[0] = null;
                            }
                         
                        }
                        //SetLOD：试图在屏幕相对大小大于或等于更高细节LOD级别的情况下设置LOD。
                        //lods[j].renderers[0] = null;

                     
                        //设计层级，设置值，赋赋空
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                lODGroup.SetLODs(lods);
                //lODGroup.size = 1;//只有草地可以，这个是对应模型大小设定的理论上模型大小不同都不no同

            }
            //处理AMP
            //设置AMP、
            //删除AMP

/*
            AmplifyImpostor amplifyImpostor = LodLayer.AddComp<AmplifyImpostor>();//没选中的话，只是代码加入AmplifyImpostor，系统不会帮我add AmplifyImpostorInspector
            //系统只有一个AmplifyImpostorInspector，点击类似切换target逻辑
            AmplifyImpostorInspector amplifyImpostorInspector = Editor.CreateEditor(amplifyImpostor) as AmplifyImpostorInspector;
            amplifyImpostorInspector.OnCustomenable();
            amplifyImpostorInspector.DelayedBake();*/


            //Editor.Destroy(amplifyImpostorInspector,0.1f);
            //DestroyImmediate(amplifyImpostorInspector);
            //amplifyImpostor.Do();
            /* if (LodLayer.GetComponent<AmplifyImpostorInspector>() != null)
             {
                 AmplifyImpostorInspector amplifyImpostorInspector = LodLayer.GetComponent<AmplifyImpostorInspector>();
                 amplifyImpostorInspector.DelayedBake();
             }*/

            //LodGroup就默认都会做到自己
        }
        //下一步一定要开始创建了
    }


  


    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[石头]_2进行同名替换 #&p", false, 10)]
    public static void AutoReplacedSamename()
    {
        //直接选取对应的石头
        //不必关心后面几个多余的名字，和多余的”（9）“
        GameObject[] currentSameNameReplaced = UnityEditor.Selection.gameObjects;
        Transform RocksRoot = currentSameNameReplaced[0].transform.parent;
        Transform RocksRootAdd = currentSameNameReplaced[0].transform.parent.parent.Find("Rock+");
        string checkName = currentSameNameReplaced[0].name;
        int checkNameLength = checkName.Length;
        for (int i = 0; i < RocksRoot.transform.childCount; i++)
        {
            Transform LodLayer = RocksRoot.transform.GetChild(i);
            GameObject gob = null;
            //创建到Rock+
            if (LodLayer.name.Contains(checkName, StringComparison.OrdinalIgnoreCase))
            {
                GameObject gob1 = Instantiate(currentSameNameReplaced[0]);
                gob1.transform.SetParent(RocksRootAdd);
                gob1.name = checkName;

                gob1.transform.localPosition = LodLayer.localPosition;
                gob1.transform.eulerAngles = LodLayer.eulerAngles;
                gob1.transform.localScale = LodLayer.localScale;

                LodLayer.name += "Done___";
                //自己删除，Done___然后新的挪过来
            }
 /*           当前文化 = 0，

当前文化忽略案例 = 1，

不变文化 = 2，

不变文化忽略事例 = 3，

序号＝4，

普通忽略大小写 = 5*/
        }
     
    }


    //名字有大写，有小写
    /// <summary>
    /// 功能，选中的物品名称，在父级节点进行搜索;要有instance奇怪问题的
    /// </summary>
    [MenuItem("自动化工具/[草地]_关联lod清无效资源Scale #&M", false, 10)]
    public static void ClearTransformGrass()
    {
        //请选择草地跟节点
      
        GameObject[] eachGrassGroup = UnityEditor.Selection.gameObjects;
        for (int i = 0; i < eachGrassGroup[0].transform.childCount; i++)
        {
            Transform LodLayer = eachGrassGroup[0].transform.GetChild(i);
            LOD[] lods;
            LODGroup lODGroup;
            if (LodLayer.gameObject.GetComponent<LODGroup>() != null)
            {
                DestroyImmediate(LodLayer.gameObject.GetComponent<MeshRenderer>());//Lod层不应该有mr
                DestroyImmediate(LodLayer.gameObject.GetComponent<MeshFilter>());//Lod层不应该有mf
                lODGroup = LodLayer.gameObject.GetComponent<LODGroup>();//这里不做确保有问题要告诉我
                lods = lODGroup.GetLODs();
                for (int j = 0; j < lods.Length; j++)
                {
                    //lods[j].renderers[0] = null;
                    try
                    {
                        //不知道+是什么意识，new一个数组，但是人家是public被unity管，是基于手操作的
                        //N级Lod:通过Debug看其实不是null，而是Length == 0
                        if (lods[j].renderers.Length == 0)
                        {
                            lods[j].renderers = new Renderer[1];

                        }
                        if (j == 0)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.03f;
                        }
                        else if (j == 1)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.0275f;
                        }
                        else if (j == 2)
                        {
                            lods[j].screenRelativeTransitionHeight = 0.0265f;
                        }

                        //lods[j].renderers[0] = null;
                        lods[j].renderers[0] = LodLayer.transform.GetChild(j).GetComponent<Renderer>();

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                lODGroup.SetLODs(lods);
                lODGroup.size = 1;


            }
            
        }
        Debug.Log("处理完成！成功！");
        // 设置值为脏数据，，编辑器会自己检测一下
        //UnityEditor.EditorUtility.SetDirty(ArtModel);
    }


    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[树木]_1含预先处理非Lod的层级结构 #&u", false, 10)]
    public static void AutoToLodLayer4Tree()
    {
        
        //直接选择RockGroup节点哦
        //执行过程中顺序会发生改变多执行几次

        //先解开perfab关联
        ///处理Lod内容的先后关系
        GameObject[] OnlySelectRockGroupRoot = UnityEditor.Selection.gameObjects;
        //2点击不是根节点
        if (!OnlySelectRockGroupRoot[0].name.Contains("Tree-"))
        {
        
            if (    !     OnlySelectRockGroupRoot[0].transform.parent.name.Contains("-"))
            {
                Debug.LogError("要点击Tree-");
                return;
            }
            else if (OnlySelectRockGroupRoot[0].transform.parent.name.Contains("Tree-"))
            {
                //父节点是Tree-
                OnlySelectRockGroupRoot[0] = OnlySelectRockGroupRoot[0].transform.parent.gameObject;
                Debug.LogError("要点击Tree-，不对");
                return;
            }
            else
            {
                Debug.LogError("要点击Tree-");
                return;
            }      
      
        }
        //1如果是根节点，处理过根节点变更也能继续处理
       
        for (int u = 0; u < 10; u++)//transfrom东西再变更有时候弄不准，先来10次
        {
            for (int i = 0; i < OnlySelectRockGroupRoot[0].transform.childCount; i++)
            {
                Transform LodLayer = OnlySelectRockGroupRoot[0].transform.GetChild(i);
                GameObject gob = null;
                //补充创建
                if (LodLayer.GetComponent<MeshRenderer>() != null)
                {

                    gob = new GameObject();
                    gob.transform.SetParent(OnlySelectRockGroupRoot[0].transform);
                    gob.transform.localPosition = LodLayer.localPosition;
                    gob.transform.eulerAngles = LodLayer.eulerAngles;
                    gob.transform.localScale = LodLayer.localScale;
                    gob.name = LodLayer.name + "_Pfb";
                    LodLayer.SetParent(gob.transform);
                    //这一句是因为换Gob为Lod层级了嘛。
                    LodLayer = gob.transform;
                }
            }
        }
    


        //下一步一定要开始创建了
        for (int i = 0; i < OnlySelectRockGroupRoot[0].transform.childCount; i++)
        {
            Transform LodLayer = OnlySelectRockGroupRoot[0].transform.GetChild(i);
            GameObject gob = null;
            //补充创建
            if (LodLayer.GetComponent<MeshRenderer>() != null)
            {

                gob = new GameObject();
                gob.transform.SetParent(OnlySelectRockGroupRoot[0].transform);
                gob.transform.localPosition = LodLayer.localPosition;
                gob.transform.eulerAngles = LodLayer.eulerAngles;
                gob.transform.localScale = LodLayer.localScale;
                gob.name = LodLayer.name + "_Pfb";
                LodLayer.SetParent(gob.transform);
                //这一句是因为换Gob为Lod层级了嘛。
                LodLayer = gob.transform;
            }
            //1都没有Mesh外层--2都有里面内容——3perfab--4错误的层级对--5transform设置外面承担里面不承担---6外层里层位置对
            //13
            //正式处理Lod
            if (LodLayer.GetComponent<MeshRenderer>() == null)
            {
                LODGroup lODGroup = LodLayer.AddComp<LODGroup>();
                LOD[] lods;
                lods = lODGroup.GetLODs();
                lODGroup.RecalculateBounds();//根据自己实际大小来定义
                for (int j = 0; j < lods.Length; j++)
                {
                    //不知道+是什么意识，new一个数组，但是人家是public被unity管，是基于手操作的
                    //N级Lod:通过Debug看其实不是null，而是Length == 0
                    if (lods[j].renderers.Length == 0)
                    {
                        lods[j].renderers = new Renderer[2];

                    }
                    if (j == 0)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.20f;//12
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {
                            if (LodLayer.transform.GetChild(w).name != "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }

                    }
                    else if (j == 1)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.19f; //2.75= 0.0275f  0.5098959 = 0.005098959
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {
                            if (LodLayer.transform.GetChild(w).name != "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }
                        //这一层是等他给我低模的
                    }
                    else if (j == 2)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.0051f;//2.75= 0.0275f  0.5098959 = 0.005098959
                        lods[j].renderers[0] = null;
                        lods[j].renderers[1] = null;
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {
                          
                            if (LodLayer.transform.GetChild(w).name == "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }
                    }
                    //SetLOD：试图在屏幕相对大小大于或等于更高细节LOD级别的情况下设置LOD。
                    //lods[j].renderers[0] = null;


                    //设计层级，设置值，赋赋空
                }
                lODGroup.SetLODs(lods);
                //lODGroup.size = 1;//只有草地可以，这个是对应模型大小设定的理论上模型大小不同都不同

            }


            //LodGroup就默认都会做到自己
        }
    }




    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[树木]_2进行同名替换 #&[", false, 10)]
    public static void AutoReplacedSamename4Tree()
    {
        //直接选取对应的石头
        //不必关心后面几个多余的名字，和多余的”（9）“
        //是在MeshRender上欺骗
        GameObject[] currentSameNameReplaced = UnityEditor.Selection.gameObjects;
        Transform RocksRoot = currentSameNameReplaced[0].transform.parent;
        Transform RocksRootAdd = currentSameNameReplaced[0].transform.parent.parent.Find("Tree+");
        string checkName = currentSameNameReplaced[0].name;
        int checkNameLength = checkName.Length;

        List<GameObject> listForRemove = new List<GameObject>();
        for (int i = 0; i < RocksRoot.transform.childCount; i++)
        {
            Transform LodLayer = RocksRoot.transform.GetChild(i);
            GameObject gob = null;
            //创建到Rock+
            if (LodLayer.name.Contains(checkName, StringComparison.OrdinalIgnoreCase))
            {
                GameObject gob1 = Instantiate(currentSameNameReplaced[0]);
                gob1.transform.SetParent(RocksRootAdd);
                gob1.name = checkName;

                gob1.transform.localPosition = LodLayer.localPosition;
                gob1.transform.eulerAngles = LodLayer.eulerAngles;
                gob1.transform.localScale = LodLayer.localScale;

                LodLayer.name += "Done___";
                listForRemove.Add(LodLayer.gameObject);
                //自己删除，Done___然后新的挪过来
            }
            /*           当前文化 = 0，

           当前文化忽略案例 = 1，

           不变文化 = 2，

           不变文化忽略事例 = 3，

           序号＝4，

           普通忽略大小写 = 5*/
          
        }
        for (int i = listForRemove.Count - 1; i >= 0; i--)
        {
            if (listForRemove[i].name.Contains("Done"))
            {
                DestroyImmediate(listForRemove[i]);
            }

        }
    }



    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[房子]_1含预先处理非Lod的层级结构 #&q", false, 10)]
    public static void AutoToLodLayer4House()
    {
        Debug.Log("大量重复的房子才能用啊");
        //直接选择RockGroup节点哦
        //执行过程中顺序会发生改变多执行几次

        //先解开perfab关联
        ///处理Lod内容的先后关系
        GameObject[] OnlySelectRockGroupRoot = UnityEditor.Selection.gameObjects;
        GameObject select = OnlySelectRockGroupRoot[0];
        //2点击不是根节点
        if (!select.name.Contains("House-"))
        {

            if (!select.transform.parent.name.Contains("-"))
            {
                Debug.LogError("House-");
                return;
            }
            else if (OnlySelectRockGroupRoot[0].transform.parent.name.Contains("House-"))
            {
                //父节点是Tree-
                select = OnlySelectRockGroupRoot[0].transform.parent.gameObject;
                Debug.LogError("要点击House-，不对");
                
            }
            else
            {
                Debug.LogError("House-");
                return;
            }

        }
        //1如果是根节点，处理过根节点变更也能继续处理

        for (int u = 0; u < 10; u++)//transfrom东西再变更有时候弄不准，先来10次
        {
            for (int i = 0; i < select.transform.childCount; i++)
            {
                Transform LodLayer = select.transform.GetChild(i);
                GameObject gob = null;
                //补充创建
                if (LodLayer.GetComponent<MeshRenderer>() != null)
                {

                    gob = new GameObject();
                    gob.transform.SetParent(select.transform);
                    gob.transform.localPosition = LodLayer.localPosition;
                    gob.transform.eulerAngles = LodLayer.eulerAngles;
                    gob.transform.localScale = LodLayer.localScale;
                    gob.name = LodLayer.name + "_Pfb";
                    LodLayer.SetParent(gob.transform);
                    //这一句是因为换Gob为Lod层级了嘛。
                    LodLayer = gob.transform;
                }
            }
        }



        //下一步一定要开始创建了
        for (int i = 0; i < select.transform.childCount; i++)
        {
            Transform LodLayer = select.transform.GetChild(i);
            GameObject gob = null;
            //补充创建
            if (LodLayer.GetComponent<MeshRenderer>() != null)
            {

                gob = new GameObject();
                gob.transform.SetParent(select.transform);
                gob.transform.localPosition = LodLayer.localPosition;
                gob.transform.eulerAngles = LodLayer.eulerAngles;
                gob.transform.localScale = LodLayer.localScale;
                gob.name = LodLayer.name + "_Pfb";
                LodLayer.SetParent(gob.transform);
                //这一句是因为换Gob为Lod层级了嘛。
                LodLayer = gob.transform;
            }
            //1都没有Mesh外层--2都有里面内容——3perfab--4错误的层级对--5transform设置外面承担里面不承担---6外层里层位置对
            //13
            //正式处理Lod
            if (LodLayer.GetComponent<MeshRenderer>() == null)
            {
                LODGroup lODGroup = LodLayer.AddComp<LODGroup>();
                LOD[] lods;
                lods = lODGroup.GetLODs();
                lODGroup.RecalculateBounds();//根据自己实际大小来定义
                for (int j = 0; j < lods.Length; j++)
                {
                    //不知道+是什么意识，new一个数组，但是人家是public被unity管，是基于手操作的
                    //N级Lod:通过Debug看其实不是null，而是Length == 0
                    if (lods[j].renderers.Length == 0)
                    {
                        lods[j].renderers = new Renderer[2];

                    }
                    if (j == 0)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.11f;//12
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {
                            if (LodLayer.transform.GetChild(w).name != "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }

                    }
                    else if (j == 1)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.10f; //2.75= 0.0275f  0.5098959 = 0.005098959
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {
                            if (LodLayer.transform.GetChild(w).name != "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }
                        //这一层是等他给我低模的
                    }
                    else if (j == 2)
                    {
                        lods[j].screenRelativeTransitionHeight = 0.0051f;//2.75= 0.0275f  0.5098959 = 0.005098959
                        lods[j].renderers[0] = null;
                        lods[j].renderers[1] = null;
                        int renderIndex = 0;
                        for (int w = 0; w < LodLayer.transform.childCount; w++)
                        {

                            if (LodLayer.transform.GetChild(w).name == "Impostor")
                            {
                                lods[j].renderers[renderIndex] = LodLayer.transform.GetChild(w).GetComponent<Renderer>();
                                renderIndex++;
                            }
                        }
                    }
                    //SetLOD：试图在屏幕相对大小大于或等于更高细节LOD级别的情况下设置LOD。
                    //lods[j].renderers[0] = null;


                    //设计层级，设置值，赋赋空
                }
                lODGroup.SetLODs(lods);
                //lODGroup.size = 1;//只有草地可以，这个是对应模型大小设定的理论上模型大小不同都不同

            }


            //LodGroup就默认都会做到自己
        }
    }



    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[房子]_2进行同名替换 #&[", false, 10)]
    public static void AutoReplacedSamename4House()
    {
        Debug.Log("大量重复的房子才能用啊");
        //直接选取对应的石头
        //不必关心后面几个多余的名字，和多余的”（9）“
        //是在MeshRender上欺骗
        GameObject[] currentSameNameReplaced = UnityEditor.Selection.gameObjects;
        Transform RocksRoot = currentSameNameReplaced[0].transform.parent;
        Transform RocksRootAdd = currentSameNameReplaced[0].transform.parent.parent.Find("House+");
        string checkName = currentSameNameReplaced[0].name;
        int checkNameLength = checkName.Length;

        List<GameObject> listForRemove = new List<GameObject>();
        for (int i = 0; i < RocksRoot.transform.childCount; i++)
        {
            Transform LodLayer = RocksRoot.transform.GetChild(i);
            GameObject gob = null;
            //创建到Rock+
            if (LodLayer.name.Contains(checkName, StringComparison.OrdinalIgnoreCase))
            {
                GameObject gob1 = Instantiate(currentSameNameReplaced[0]);
                gob1.transform.SetParent(RocksRootAdd);
                gob1.name = checkName;

                gob1.transform.localPosition = LodLayer.localPosition;
                gob1.transform.eulerAngles = LodLayer.eulerAngles;
                gob1.transform.localScale = LodLayer.localScale;

                LodLayer.name += "Done___";
                listForRemove.Add(LodLayer.gameObject);
                //自己删除，Done___然后新的挪过来
            }
            /*           当前文化 = 0，

           当前文化忽略案例 = 1，

           不变文化 = 2，

           不变文化忽略事例 = 3，

           序号＝4，

           普通忽略大小写 = 5*/

        }
        for (int i = listForRemove.Count - 1; i >= 0; i--)
        {
            if (listForRemove[i].name.Contains("Done"))
            {
                DestroyImmediate(listForRemove[i]);
            }

        }
    }


    //private const int resolutionRatio = GameConfig.RESOLUTION_RATIO;//128
    //private const int mapImageSize = GameConfig.MAP_IMAGE_SIZE;//1024
    //private const float halfImageSize = mapImageSize / (resolutionRatio * 2);
    //private const float halfMaskSize = GameConfig.MAP_MASK_SIZE / (resolutionRatio * 2);
    //private const float partOfImageAndMask = halfImageSize / halfMaskSize;
    //public static int Rows = 1;
    //public static int Cols = 1;
    //// % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
    ////TODO：!自动添加图片；自动生成perfab，自动转换图片分辨率,MapGeneratorTools自我组件添加，自我组件删除，添加物品
    //[MenuItem("ShinChanTools/CreateMap &R", false, 10)]
    //public static void CreateMapSprite()
    //{
    //    string gobName = string.Empty;
    //    string spritePath = string.Empty;
    //    int stageIndex;
    //    int maxCount = Rows * Cols;
    //    GameObject currentSelectGob = UnityEditor.Selection.gameObjects[0];
    //    MapGeneratorTools mgt = currentSelectGob.GetComponent<MapGeneratorTools>();
    //    Rows = mgt.Rows;
    //    Cols = mgt.Cols;
    //    stageIndex = mgt.StageIndex;
    //    GameObject spriteGob;
    //    float firstRowValue = (-1 + ((Rows / 2) * 2)) * halfImageSize;
    //    float firstColValue = (-1 + ((Cols / 2) * 2)) * -1 * halfImageSize;
    //    float leftPart = ((Cols / 2) * 2) * -1 * halfImageSize - halfMaskSize;
    //    float rightPart = ((Cols / 2) * 2) * 1 * halfImageSize + halfMaskSize;
    //    float topPart = ((Rows / 2) * 2) * halfImageSize + halfMaskSize;
    //    float bottomPart = ((Rows / 2) * 2) * -1 * halfImageSize - halfMaskSize;
    //    int artNameDef = 1;//美术喜欢1开始
    //    int space = 2;
    //    for (int i = 0; i < Rows; i++)
    //    {
    //        for (int j = 0; j < Cols; j++)
    //        {
    //            gobName = "Map_" + i + "_" + j;
    //            spriteGob = Instantiate(Resources.Load<GameObject>("Perfab/Map/MapTmpl"));
    //            spriteGob.transform.SetParent(currentSelectGob.transform);
    //            spriteGob.transform.SetAsLastSibling();
    //            spriteGob.name = gobName;
    //            spriteGob.transform.localPosition = new Vector3(firstColValue + space * j * halfImageSize, firstRowValue - space * i * halfImageSize, 0f);
    //            spriteGob.transform.localScale = Vector3.one;
    //            //spritePath = Path.Combine(Application.dataPath, "Resources/Image/Map/", stageIndex.ToString() + "/", (j + (i * Cols) + artNameDef).ToString());
    //            spritePath = "Image/Map/" + stageIndex.ToString() + "/" + (j + (i * Cols) + artNameDef).ToString();
    //            spriteGob.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spritePath);
    //        }
    //    }
    //    //CreateMapMask
    //    GameObject mapMaskRootClone = new GameObject("MapMask");
    //    GameObject mapMaskRoot = Instantiate(mapMaskRootClone);
    //    mapMaskRoot.transform.SetParent(currentSelectGob.transform);
    //    mapMaskRoot.transform.SetAsLastSibling();
    //    mapMaskRoot.name = "MapMask";
    //    mapMaskRoot.transform.localPosition = Vector3.zero;
    //    mapMaskRoot.transform.localScale = Vector3.one;
    //    DestroyImmediate(mapMaskRootClone);

    //    GameObject cellMaskGob;

    //    gobName = "LeftMask";
    //    cellMaskGob = Instantiate(Resources.Load<GameObject>("Perfab/Map/MaskWall"));
    //    cellMaskGob.transform.SetParent(mapMaskRoot.transform);
    //    cellMaskGob.transform.SetAsLastSibling();
    //    cellMaskGob.name = gobName;
    //    cellMaskGob.transform.localPosition = new Vector3(leftPart, 0f, 0f);
    //    cellMaskGob.transform.localScale = new Vector3(1, partOfImageAndMask * Rows, 1);

    //    gobName = "RightMask";
    //    cellMaskGob = Instantiate(Resources.Load<GameObject>("Perfab/Map/MaskWall"));
    //    cellMaskGob.transform.SetParent(mapMaskRoot.transform);
    //    cellMaskGob.transform.SetAsLastSibling();
    //    cellMaskGob.name = gobName;
    //    cellMaskGob.transform.localPosition = new Vector3(rightPart, 0f, 0f);
    //    cellMaskGob.transform.localScale = new Vector3(1, partOfImageAndMask * Rows, 1);

    //    gobName = "TopMask";
    //    cellMaskGob = Instantiate(Resources.Load<GameObject>("Perfab/Map/MaskWall"));
    //    cellMaskGob.transform.SetParent(mapMaskRoot.transform);
    //    cellMaskGob.transform.SetAsLastSibling();
    //    cellMaskGob.name = gobName;
    //    cellMaskGob.transform.localPosition = new Vector3(0f, topPart, 0f);
    //    cellMaskGob.transform.localScale = new Vector3(partOfImageAndMask * Cols, 1, 1);

    //    gobName = "BottomMask";
    //    cellMaskGob = Instantiate(Resources.Load<GameObject>("Perfab/Map/MaskWall"));
    //    cellMaskGob.transform.SetParent(mapMaskRoot.transform);
    //    cellMaskGob.transform.SetAsLastSibling();
    //    cellMaskGob.name = gobName;
    //    cellMaskGob.transform.localPosition = new Vector3(0f, bottomPart, 0f);
    //    cellMaskGob.transform.localScale = new Vector3(partOfImageAndMask * Cols, 1, 1);

    //    //camera范围！！
    //    mgt.X_left = ((Cols / 2) * 2) * -1 * halfImageSize * currentSelectGob.transform.localScale.x;
    //    mgt.X_right = ((Cols / 2) * 2) * 1 * halfImageSize * currentSelectGob.transform.localScale.x;
    //    mgt.Y_top = ((Rows / 2) * 2) * halfImageSize * currentSelectGob.transform.localScale.y;
    //    mgt.Y_bottom = ((Rows / 2) * 2) * -1 * halfImageSize * currentSelectGob.transform.localScale.y;

    //    /* string pathMidiSourceFolder = Path.Combine(Application.dataPath, "Resources/aaa/bbb/1");
    //     string pathTargetFolder = Path.Combine(Application.dataPath, "Resources/aaa/bbb/ccc");*/
    //    Debug.Log("CreateImageMaps");
    //}



    //[MenuItem("ShinChanTools/CreateCloud &W", false, 10)]
    //public static void CreateCloud()
    //{
    //    GameObject currentSelectGob = UnityEditor.Selection.gameObjects[0];
    //    MapCloudTools mct = currentSelectGob.GetComponent<MapCloudTools>();
    //    int x = mct.X_MAX;
    //    int y = mct.Y_MAX;
    //    int Xplace = mct.XPLACE;
    //    int Yplace = mct.YPLACE;
    //    for (int i = 0; i < x; i += Xplace)
    //    {
    //        for (int j = 0; j < y; j += Yplace)
    //        {
    //            float RandomValue = UnityEngine.Random.RandomRange(-mct.OffsetMaxValue, mct.OffsetMaxValue);
    //            string gobName = "Cloud_" + i + "_" + j;
    //            GameObject cloud = Instantiate(Resources.Load<GameObject>("Perfab/StageSet3D/Stage1/EvnItem/Cloud/Cloud"));
    //            cloud.transform.SetParent(currentSelectGob.transform);
    //            cloud.transform.SetAsLastSibling();
    //            cloud.name = gobName;
    //            cloud.transform.localEulerAngles = new Vector3(90, 0, 90);
    //            cloud.transform.localPosition = new Vector3(i + RandomValue, 4, j + RandomValue);
    //            cloud.transform.localScale = new Vector3(3, 1.5f, 1);
    //            cloud.gameObject.layer = LayerMask.NameToLayer("CloudLayer");
    //        }
    //    }
    //}


    //[MenuItem("ShinChanTools/CreateEnvItemData &O", false, 10)]
    //public static void CreateOffLineEnvData()
    //{
    //    GameObject currentSelectGob = UnityEditor.Selection.gameObjects[0];
    //    //GameEventsManager.Instance.OFFLINE_StaticEnvItems.Clear();
    //    EventItemCollider[] eic = currentSelectGob.transform.GetComponentsInChildren<EventItemCollider>(true);
    //    foreach (EventItemCollider item in eic) //eic只一层,Transform item in currentSelectGob.transform
    //    {
    //        ///所有策划采集物件节点下，commonEventGob下的，去掉meshfilter
    //        item.gameObject.GetComponent<EventItemCollider>().OFfLineAddReg();
    //        if (item.gameObject.GetComponent<MeshFilter>() != null)
    //        {
    //            DestroyImmediate(item.gameObject.GetComponent<MeshFilter>());
    //        }
    //    }
    //}





    //[MenuItem("ShinChanTools/RevertPerfabName &O", false, 10)]
    //public static void RevertPerfabName()
    //{
    //    GameObject currentSelectGob = UnityEditor.Selection.gameObjects[0];
    //    //GameEventsManager.Instance.OFFLINE_StaticEnvItems.Clear();
    //    EventItemCollider[] eic = currentSelectGob.transform.GetComponentsInChildren<EventItemCollider>(true);
    //    foreach (EventItemCollider item in eic) //eic只一层,Transform item in currentSelectGob.transform
    //    {
    //        ///所有策划采集物件节点下，commonEventGob下的，去掉meshfilter
    //        item.gameObject.GetComponent<EventItemCollider>().RevertName();

    //    }
    //}


    ////名字有大写，有小写
    ///// <summary>
    ///// 功能，选中的物品名称，在父级节点进行搜索;要有instance奇怪问题的
    ///// </summary>
    //[MenuItem("ShinChanTools/FindSelectSameNameItem &p", false, 10)]
    //public static void FindSelectSameNameItem()
    //{


    //    int point = UnityEditor.Selection.gameObjects[0].name.IndexOf("(");
    //    string trueName = UnityEditor.Selection.gameObjects[0].name.Substring(0, point - 1);
    //    Transform tranRoot = UnityEditor.Selection.gameObjects[0].transform.parent;
    //    List<Transform> ItemTransNeedSelect = new List<Transform>();
    //    for (int i = 0; i < tranRoot.childCount; i++)
    //    {
    //        if (tranRoot.GetChild(i).name.Contains(trueName))
    //        {
    //            //目前单状态，所以在结果的itemlist里面全部遍历，就一个就好【0】
    //            List<Transform> transItem = tranRoot.GetChild(i).DeepFirstTransList("Item");
    //            if (transItem == null || transItem.Count == 0 || transItem[0] == null)
    //            {
    //                transItem = tranRoot.GetChild(i).DeepFirstTransList("item");
    //            }
    //            if (transItem != null && transItem.Count != 0 && transItem[0] != null)
    //            {
    //                ItemTransNeedSelect.Add(transItem[0]);
    //            }

    //        }

    //    }


    //    for (int i = 0; i < ItemTransNeedSelect.Count; i++)
    //    {
    //        ItemTransNeedSelect[i].name = "NeedFixedItem1111111";
    //    }
    //    //先还原名字 1
    //    //选中名字有inst的进行查找FindSelectSameNameItem 2
    //    //NeedFixedItem1111111 查找3
    //    //修好一个4
    //    //pase Comp5
    //    //改名item6

    //    //中途修改数据
    //    //还原名字
    //    //再查找inst


    //    //UnityEditor.Selection.objects = null;
    //    //UnityEngine.Object[] select = ItemTransNeedSelect.ToArray();
    //    //Debug.Log(select);
    //    //UnityEditor.Selection.objects = select;


    //}



    //[MenuItem("ShinChanTools/Count &c", false, 10)]
    //public static void ShowCount()
    //{
    //    Debug.Log(UnityEditor.Selection.gameObjects.Length);

    //}





    //[MenuItem("ShinChanTools/CreateCoinOffLineData &f", false, 10)]
    //public static void CreateCoinOffLineData()
    //{
    //    GameObject currentSelectGob = UnityEditor.Selection.gameObjects[0];
    //    //GameEventsManager.Instance.OFFLINE_StaticEnvItems.Clear();
    //    CoinCollider[] eic = currentSelectGob.transform.GetComponentsInChildren<CoinCollider>(true);
    //    foreach (CoinCollider item in eic) //eic只一层,Transform item in currentSelectGob.transform
    //    {
    //        ///所有策划采集物件节点下，commonEventGob下的，去掉meshfilter
    //        item.gameObject.GetComponent<CoinCollider>().OFfLineAddReg();
    //        if (item.gameObject.GetComponent<MeshFilter>() != null)
    //        {
    //            DestroyImmediate(item.gameObject.GetComponent<MeshFilter>());
    //        }
    //    }

    //}


    //[MenuItem("ShinChanTools/ScaleBoxColliderHalf_XZ ", false, 10)]
    //public static void ScaleBoxColliderHalf_XZ()
    //{
    //    GameObject[] currentSelectGobs = UnityEditor.Selection.gameObjects;

    //    for (int i = 0; i < currentSelectGobs.Length; i++)
    //    {
    //        Vector3 v3 = currentSelectGobs[i].GetComponent<BoxCollider>().size;
    //        v3.x *= 0.5f;
    //        v3.y *= 0.5f;//z
    //        //v3.z *= 2f;
    //        currentSelectGobs[i].GetComponent<BoxCollider>().size = v3;
    //    }
    //}

    //[MenuItem("ShinChanTools/ScaleBoxColliderDouble_XZ ", false, 10)]
    //public static void ScaleBoxColliderDouble_XZ()
    //{
    //    GameObject[] currentSelectGobs = UnityEditor.Selection.gameObjects;

    //    for (int i = 0; i < currentSelectGobs.Length; i++)
    //    {
    //        Vector3 v3 = currentSelectGobs[i].GetComponent<BoxCollider>().size;
    //        v3.x *= 2f;
    //        v3.y *= 2f;//z
    //        //v3.z *= 2f;
    //        currentSelectGobs[i].GetComponent<BoxCollider>().size = v3;
    //    }
    //}
    //名字有大写，有小写
    /// <summary>
    /// 每一个
    /// </summary>
    [MenuItem("自动化工具/[临时处理]", false, 10)]
    public static void DeleteGlass()
    {
        List<Transform> deleteList = new List<Transform>();

        GameObject[] parents = UnityEditor.Selection.gameObjects;



            //10个--- 9 开始
            for (int i = parents[0].transform.childCount - 1; i >= 0; i--)
        {
            if (i % 3 == 0)//排除 2/3
            {
                Debug.Log("Recoder:" + i);
              /*  UnityEditor.PrefabUtility.UnpackPrefabInstance(parents[0].transform.GetChild(i).gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);*/
                deleteList.Add(parents[0].transform.GetChild(i));
            }
          
        }
        //方式顺序影响删除，变更顺序索引和物件都在变化，另外也防止%的判断删除一直删除全部
        for (int j = 0; j < deleteList.Count; j++)
        {
            Debug.Log("delete");
            DestroyImmediate(deleteList[j].gameObject);
            //存储-手动拖拽
        }
        Debug.Log("complete");
    }

 /*   [MenuItem("自动化工具/FixChecker/Find Standard Shader")]
    private static void ReplaceStandardToDiffuse()
    {
        Shader sd = Shader.Find("Standard");

        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.EndsWith(".mat"))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader == sd)
                {
                    Debug.LogError(path);
                }
            }
        }
    }

    [MenuItem("自动化工具/FixChecker/Find FBX Shader")]
    private static void SearchStandardShader()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".fbx"))
            {
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in objs)
                {
                    var gameObj = obj as GameObject;
                    if (gameObj != null)
                    {
                        var render = gameObj.GetComponent<Renderer>();
                        if (render != null && render.sharedMaterials != null)
                        {
                            foreach (var item in render.sharedMaterials)
                            {
                                if (item != null && item.shader != null)
                                {
                                    if (item.shader.name == "Standard")
                                        Debug.LogError(item.shader);
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    [MenuItem("自动化工具/FixChecker/Find Shader Text")]
    private static void SearchStandardShaderText()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".shader"))
            {
                var fullPath = Path.GetFullPath(path);
                var lines = File.ReadAllLines(fullPath);
                foreach (var line in lines)
                {
                    if (line.Contains("Standard"))
                    {
                        Debug.LogError(line);
                        Debug.LogError(fullPath);
                        break;
                    }
                }
            }
        }
    }

    [MenuItem("自动化工具/FixChecker/Find Default Material")]
    private static void SearchDefaultMaterial()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.ToLower().EndsWith(".prefab"))
            {
                var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (gameObj != null)
                {
                    var renders = gameObj.GetComponentsInChildren<Renderer>(true);
                    if (renders != null)
                    {
                        foreach (var renderer in renders)
                        {
                            foreach (var mate in renderer.sharedMaterials)
                            {
                                if (mate != null && mate.shader != null)
                                {
                                    if (mate.shader.name == "Standard")
                                    {
                                        Debug.LogError(mate);
                                        Debug.LogError(path);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [MenuItem("自动化工具/FixChecker/Replace Default Material")]
    private static void ReplaceDefaultMaterial()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        var newDefaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Res/NewDefault-Material.mat");
        if (newDefaultMat != null)
        {
            foreach (var path in paths)
            {
                if (path.ToLower().EndsWith(".prefab"))
                {
                    var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (gameObj != null)
                    {
                        var renders = gameObj.GetComponentsInChildren<Renderer>(true);
                        if (renders != null)
                        {
                            foreach (var renderer in renders)
                            {
                                var isDirty = false;
                                var materials = new Material[renderer.sharedMaterials.Length];
                                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                                {
                                    var mate = renderer.sharedMaterials[i];
                                    if (mate != null && mate.shader != null)
                                    {
                                        if (mate.shader.name == "Standard")
                                        {
                                            isDirty = true;
                                            materials[i] = newDefaultMat;
                                        }
                                        else
                                        {
                                            materials[i] = mate;
                                        }
                                    }
                                }
                                if (isDirty)
                                {
                                    renderer.sharedMaterials = materials;
                                    EditorUtility.SetDirty(renderer);
                                }
                            }
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
    }*/



    ////1先吧这个东西做成perfab手动
    ////2缓存所有同名
    ////3创建一个吧吧缓存的值给他
    ////4删除缓存
    ///// <summary>
    ///// 每一个
    ///// </summary>
    //[MenuItem("自动化工具/[资源处理工具]_进行同名替换 #&。", false, 10)]
    //public static void ReplaceSameName()
    //{
    //    //直接选取对应的石头
    //    //不必关心后面几个多余的名字，和多余的”（9）“
    //    //是在MeshRender上欺骗
    //    GameObject[] currentSameNameReplaced = UnityEditor.Selection.gameObjects;
    //    Transform thisPerfab = currentSameNameReplaced[0].transform;

    //    Transform parentsRoot = thisPerfab.parent;

    //    string path =AssetDatabase.GetAssetPath(thisPerfab.gameObject);
    //    string checkName = currentSameNameReplaced[0].name;
    //    int checkNameLength = checkName.Length;

    //    List<GameObject> listForRemove = new List<GameObject>();


    //    for (int i = 0,iMax = parentsRoot.childCount; i < iMax; i++)
    //    {
    //        Transform checker = parentsRoot.transform.GetChild(i);
    //        GameObject gob = null;

    //        if (checker.name.Contains(checkName, StringComparison.OrdinalIgnoreCase))
    //        {

    //            GameObject perfab = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(thisPerfab);//Instantiate(thisPerfab.gameObject);
    //            //创建新的物件
    //            //gob1 = /*Instantiate*/
    //            if (perfab != null)
    //            {
    //                perfab.transform.SetParent(parentsRoot);
    //                perfab.name = checkName;
    //                //设置新的物件
    //                perfab.transform.localPosition = checker.localPosition;
    //                perfab.transform.eulerAngles = checker.eulerAngles;
    //                perfab.transform.localScale = checker.localScale;
    //                //设置
    //                checker.name += "Done___";
    //                listForRemove.Add(checker.gameObject);
    //                //自己删除，Done___然后新的挪过来

    //              /*  PrefabUtility.SaveAsPrefabAssetAndConnect(perfab, path, InteractionMode.AutomatedAction);*/
    //            }

    //        }


    //    }
    //    for (int i = listForRemove.Count - 1; i >= 0; i--)
    //    {
    //        if (listForRemove[i].name.Contains("Done"))
    //        {
    //            DestroyImmediate(listForRemove[i]);
    //        }

    //    }


    //    UnityEditor.EditorGUIUtility.PingObject
    //}
}
#endif