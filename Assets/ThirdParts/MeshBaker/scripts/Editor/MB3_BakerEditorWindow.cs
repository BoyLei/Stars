#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DigitalOpus.MB.Core;
using UnityEditor;
using UnityEngine;
using static MB3_MeshBakerGrouper;

public class MB3_BakerEditorWindow : EditorWindow
{
    struct BakerInfo
    {
        public MB3_TextureBaker baker;
        public int count;
    }

    string nodeName = "SGAME/SGAME_Scene_01";
    string orgx = "0";
    string orgy = "0";
    string orgz = "0";
    string cellx = "25";
    string celly = "100";
    string cellz = "25";
    string bakePath = "Assets/Resources/MeshBaker/Prefabs";
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        nodeName = GUILayout.TextArea(nodeName);

        if (GUILayout.Button("排序"))
        {
            UnityEditor.Selection.activeObject = GameObject.Find(nodeName);
            SortBakers();
        }

        if (GUILayout.Button("删除"))
        {
            UnityEditor.Selection.activeObject = GameObject.Find(nodeName);
            DestroySubBakers();
        }

        EditorGUILayout.BeginHorizontal();

        orgx = GUILayout.TextArea(orgx);
        orgy = GUILayout.TextArea(orgy);
        orgz = GUILayout.TextArea(orgz);

        cellx = GUILayout.TextArea(cellx);
        celly = GUILayout.TextArea(celly);
        cellz = GUILayout.TextArea(cellz);
        if (GUILayout.Button("生成组"))
        {
            UnityEditor.Selection.activeObject = GameObject.Find(nodeName);
            GenGrouper(new Vector3(float.Parse(orgx), float.Parse(orgy), float.Parse(orgz)), float.Parse(cellx), float.Parse(celly), float.Parse(cellz));
        }

        EditorGUILayout.EndHorizontal();

        bakePath = GUILayout.TextArea(bakePath);
        if (GUILayout.Button("烘培网格"))
        {
            UnityEditor.Selection.activeObject = GameObject.Find(nodeName);
            BakerPrefabs(bakePath);
        }

        EditorGUILayout.EndVertical();
    }

    [MenuItem("gopal/MeshBaker/MeshBaker")]
    public static void MeshBaker()
    {
        // MB3_BakerEditorWindow window = (MB3_BakerEditorWindow)EditorWindow.GetWindow(typeof(MB3_BakerEditorWindow));
        // window.Show();


        MB3_MeshBakerEditorWindowInterface mmWin = (MB3_MeshBakerEditorWindowInterface)EditorWindow.GetWindow(typeof(MB3_MeshBakerEditorWindow));
        //mmWin.target = (MB3_MeshBakerCommon) target;
    }

    [MenuItem("gopal/MeshBaker/SortBakers")]
    public static void SortBakers()
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        foreach (var item in bakers)
        {
            bakerInfos.Add(new BakerInfo { baker = item, count = item.GetObjectsToCombine().Count });
        }
        bakerInfos.Sort((x, y) =>
        {
            if (x.count == y.count)
            {
                return 0;
            }
            else if (x.count > y.count)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        foreach (var item in bakerInfos)
        {
            item.baker.transform.SetAsFirstSibling();
        }
    }

    [MenuItem("gopal/MeshBaker/FindAllStones")]
    public static void FindAllStones()
    {
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        var root = GameObject.Find("BatchPrefabBaker-Stone").transform;
        foreach (var item in bakers)
        {
            if (item.GetObjectsToCombine()[0].name.Contains("Rock"))
            {
                item.transform.parent = root;
            }
        }
    }

    [MenuItem("gopal/MeshBaker/CombineBaker")]
    public static void CombineBaker()
    {
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        var root = GameObject.Find("Combiner-Stone").transform;
        var combineBaker = root.GetComponent<MB3_TextureBaker>();
        for (int i = 0; i < 4; i++)
        {
            var objs = bakers[i].GetObjectsToCombine();
            combineBaker.objsToMesh.AddRange(objs);
        }
    }

    [MenuItem("gopal/MeshBaker/PrintMat")]
    public static void PrintMat()
    {
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        for (int i = 0; i < 5; i++)
        {
            var go = bakers[i].GetObjectsToCombine()[0];
            var mr = go.GetComponent<MeshRenderer>();
            var tex1 = mr.sharedMaterial.GetTexture("_MainTex");
            var tex2 = mr.sharedMaterial.GetTexture("_NormalTex");
            var tex3 = mr.sharedMaterial.GetTexture("_MRAETex");
            var tex4 = mr.sharedMaterial.GetTexture("_DetailTex");

            //法线强度
            float _NormalScale = mr.sharedMaterial.GetFloat("_NormalScale");
            //光滑度强度
            float _RoughnessScale = mr.sharedMaterial.GetFloat("_RoughnessScale");
            //自发光强度
            float _EmissionScale = mr.sharedMaterial.GetFloat("_EmissionScale");
            //自发光颜色
            Color _EmissionColor = mr.sharedMaterial.GetColor("_EmissionColor");
            //ao强度
            float _AoScale = mr.sharedMaterial.GetFloat("_AoScale");
            //草的范围
            float _GrassRange = mr.sharedMaterial.GetFloat("_GrassRange");
            //草出现的阈值
            float _GrassDownRang = mr.sharedMaterial.GetFloat("_GrassDownRang");
            //细节法线强度
            float _DetailNormalStrength = mr.sharedMaterial.GetFloat("_DetailNormalStrength");
            //细节贴图的光滑度强度
            float _DetailSmoothStrength = mr.sharedMaterial.GetFloat("_DetailSmoothStrength");
            //是否启动菲涅尔
            float _FesColorFlag = mr.sharedMaterial.GetFloat("_FesColorFlag");
            //offset
            Vector4 _Offset = mr.sharedMaterial.GetVector("_Offset");
            //clipdis
            float _ClipDis = mr.sharedMaterial.GetFloat("_ClipDis");
            //dithermax
            float _DitherMax = mr.sharedMaterial.GetFloat("_DitherMax");


            Debug.Log($"需要的纹理:{tex1.name},{tex2?.name},{tex3?.name},{tex4?.name},{_NormalScale},{_RoughnessScale},{_EmissionScale},{_AoScale},{_GrassRange},{_GrassDownRang},{_DetailNormalStrength},{_DetailSmoothStrength},{_ClipDis},{_DitherMax}");
        }
    }

    [MenuItem("gopal/MeshBaker/DestroySubBakers")]
    public static void DestroySubBakers()
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        foreach (var item in bakers)
        {
            if (item.transform.childCount > 0)
            {
                foreach (var child in item.transform.GetChildren())
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            if (item.GetObjectsToCombine().Count == 1)
            {
                //GameObject.DestroyImmediate(item.gameObject);
            }
        }
    }

    [MenuItem("gopal/MeshBaker/GenGrouper")]
    public static void GenGrouper(Vector3 org, float x, float y, float z)
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        foreach (var item in bakers)
        {
            var g = item.GetComponent<MB3_MeshBakerGrouper>();
            g.clusterType = ClusterType.grid;
            g.data.origin = org;
            g.data.cellSize = new Vector3(x, y, z);

            if (item.GetObjectsToCombine().Count == 0)
            {
                Debug.LogError("The MB3_MeshBakerGrouper creates clusters based on the objects to combine in the MB3_TextureBaker component. There were no objects in this list.");
                return;
            }
            if (item.transform.childCount > 0)
            {
                Debug.LogWarning("This MB3_TextureBaker had some existing child objects. You may want to delete these before 'Generating Mesh Bakers' since your source objects may be included in the List Of Objects To Combine of multiple MeshBaker objects.");
            }
            if (item != null)
            {
                if (item.GetObjectsToCombine()[0].name == "Common_well_Pfb (1)")
                {
                    Debug.Log("aaa");
                }
                g.grouper.DoClustering(item, new Bounds());
            }
            else
            {
                Debug.LogError("MB3_MeshBakerGrouper needs to be attached to an MB3_TextureBaker");
            }
        }
    }


    [MenuItem("gopal/MeshBaker/BakerPrefabs")]
    public static void BakerPrefabs(string path)
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        int index = 0;

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);

        foreach (var item in bakers)
        {
            item.CreateAtlases(null, true, new MB3_EditorMethods());
            if (item.textureBakeResults != null) EditorUtility.SetDirty(item.textureBakeResults);

            var mbs = item.GetComponentsInChildren<MB3_MeshBaker>();
            var mbms = item.GetComponentsInChildren<MB3_MultiMeshBaker>();
            if (mbs != null && mbs.Length > 0)
            {
                int j = 0;
                foreach (var mb in mbs)
                {
                    mb.meshCombiner.outputOption = MB2_OutputOptions.bakeIntoPrefab;

                    var pre = new GameObject(index.ToString());
                    var dir = $"{path}/{index}";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    PrefabUtility.SaveAsPrefabAsset(pre, $"{path}/{index}/{j}.prefab");
                    var preObj = AssetDatabase.LoadAssetAtPath<GameObject>($"{path}/{index}/{j}.prefab");
                    mb.resultPrefab = preObj;
                    MB3_MeshBakerEditorInternal.bake(mb);

                    GameObject.DestroyImmediate(pre);
                    j++;
                }
            }
            if (mbms != null && mbms.Length > 0)
            {
                int j = 0;
                foreach (var mbm in mbms)
                {
                    mbm.meshCombiner.outputOption = MB2_OutputOptions.bakeIntoPrefab;

                    var pre = new GameObject(index.ToString());
                    var dir = $"{path}/{index}";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    PrefabUtility.SaveAsPrefabAsset(pre, $"{path}/{index}/{j}.prefab");
                    var preObj = AssetDatabase.LoadAssetAtPath<GameObject>($"{path}/{index}/{j}.prefab");
                    mbm.resultPrefab = preObj;
                    MB3_MeshBakerEditorInternal.bake(mbm);

                    GameObject.DestroyImmediate(pre);
                    j++;
                }
            }

            index++;
        }
    }

    [MenuItem("gopal/MeshBaker/HideOrg")]
    public static void HideOrg()
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        foreach (var item in bakers)
        {
            if (item.transform.childCount > 0)
            {
                var cobjs = item.GetObjectsToCombine();
                foreach (var org in cobjs)
                {
                    if (org.name == "Common_TreeTrunk03_Stc")
                    {
                        Debug.Log("sss");
                    }
                    org.SetActive(false);
                }
            }
            else
            {
                Debug.Log("sss");
            }
        }
    }


    [MenuItem("gopal/MeshBaker/ShowOrg")]
    public static void ShowOrg()
    {
        List<BakerInfo> bakerInfos = new List<BakerInfo>();
        var obj = UnityEditor.Selection.activeObject as GameObject;
        var bakers = obj.GetComponentsInChildren<MB3_TextureBaker>();
        foreach (var item in bakers)
        {
            var cobjs = item.GetObjectsToCombine();
            foreach (var org in cobjs)
            {
                if (org.name == "Common_TreeTrunk03_Stc")
                {
                    UnityEditor.Selection.activeObject = item;
                }
                org.SetActive(true);
            }
        }
    }
}
#endif