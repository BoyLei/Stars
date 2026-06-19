using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StylizedWater2;
using UnityEngine;
using UnityEditor;

//[ExecuteInEditMode]
/*
 * 用来控制角色不同状态 比如冰冻, 石化等的脚本
 * 通过加载通用的材质球到角色身上来是实现
 */
public class CharStateController : MonoBehaviour
{
    //[GUIColor(0.8f, 0.8f, 0.8f, 1f)] 
    //public int ColoredInt1;

    //后续有新的状态随时再添加
    public enum CharacterState
    {
        Normal,
        Frozen,
        Fossil,
        Ghost,
        RedPatches,
        Water,
        OrangeRing
    }

    private static string MatPath = "Roles/Material/State/";

    static private Dictionary<string, string> CharStateMatPath = new Dictionary<string, string>()
    {
        //冰冻
        {"Frozen", MatPath + "Common_Char_Frozen" },
        //石化
        {"Fossil",MatPath + "Common_Char_Fossil"},
        //幽灵
        {"Ghost",MatPath + "Common_Char_Ghost"},
        //幽灵-金
        {"Ghost-Gold",MatPath + "Common_Char_Ghost_Gold"},
        //红斑
        {"RedPatches",MatPath + "Common_Char_RedPatches"},
        //深度写入材质球
        {"ZWrite",MatPath + "Common_Char_ZwriteIn"},
        //濡湿材质球
        {"Water",MatPath + "Common_Char_Water"},
        //橙色光圈
        {"OrangeRing",MatPath + "Common_Char_Ghost_Orange"}
    };

    //角色的材质球列表
    private Material[] charMatList;

    //角色Renderer
    public List<Renderer> charRenderers = new();
    private Renderer charRenderer;

    [Button("LoadMeshRenderer", ButtonSizes.Small)]
    private void LoadMashRenderer()
    {
        SetMeshRenderer();
    }


    //角色Renderers的材质球
    struct NormalMat
    {
        public Material outLineMat;
        public Material normalMat;
    }

    private Dictionary<string, NormalMat> originalCharMat = new Dictionary<string, NormalMat>();

    //当前状态
    private CharacterState currentState = CharacterState.Normal;

    //角色的材质球
    private Material CharMat;

    private Material CharMatOutLine;

    private List<Material> cacheMatList;

    private List<Material> normalMaterialList = new();

    public CharacterState TargetState;

    [Button("ChangeState", ButtonSizes.Large)]
    private void ChangeState()
    {
        SetCharState(TargetState);
    }


    // Start is called before the first frame update
    void Start()
    {



        // charRenderer =  this.transform.gameObject.GetComponent<Renderer>();
        // charMatList = charRenderer.sharedMaterials;

        //获取当前物体的描边材质球和本身的材质球
        // foreach (var mat  in charMatList)
        // {
        //     if (mat.name.Contains("Outline"))
        //     {
        //         CharMatOutLine = mat;
        //     }
        //     else
        //     {
        //         CharMat = mat;
        //     }
        // }



        //normalMaterialList = new Material[] {CharMat,CharMatOutLine};
    }
    //加载子物体的所有MeshRenderer
    public void SetMeshRenderer()
    {
        if (init)
        {
            return;
        }
        init = true;

        Renderer[] childrenRenderer = GetComponentsInChildren<Renderer>();
        if (charRenderers != null)
        {
            charRenderers.Clear();
            charRenderers = childrenRenderer.ToList();
        }

        SetNormalMat();

    }

    private bool init = false;

    //设置所有子物体renderer的 normal 状态下的材质球
    public void SetNormalMat()
    {

        foreach (var renderer in charRenderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat != null)
                {
                    if (mat.name.Contains("Outline"))
                    {
                        CharMatOutLine = mat;
                    }
                    else
                    {
                        CharMat = mat;
                    }
                }
            }

            bool ifContain = originalCharMat.ContainsKey(renderer.name);
            if (!ifContain)
            {
                originalCharMat.Add(renderer.name, new NormalMat() { outLineMat = CharMatOutLine, normalMat = CharMat });
            }

        }
    }

    //设置角色的状态
    public void SetCharState(CharacterState charState)
    {
        if (charRenderers == null || charRenderers.Count == 0)
        {
            SetMeshRenderer();
        }

        if (charRenderers == null || charRenderers.Count == 0)
        {
            return;
        }
        else
        {
            //如果是非正常状态就先设置为正常状态
            if (currentState != CharacterState.Normal)
            {
                ReturnNormal();
                currentState = CharacterState.Normal;
            }
            switch (charState)
            {
                case CharacterState.Frozen:
                    ToFrozen();
                    currentState = CharacterState.Frozen;
                    break;
                case CharacterState.Fossil:
                    ToFossil();
                    currentState = CharacterState.Fossil;
                    break;
                case CharacterState.Ghost:
                    ToGhost();
                    currentState = CharacterState.Ghost;
                    break;
                case CharacterState.RedPatches:
                    ToRedPatches();
                    currentState = CharacterState.RedPatches;
                    break;
                case CharacterState.Water:
                    ToWater();
                    currentState = CharacterState.Water;
                    break;
                case CharacterState.OrangeRing:
                    ToOrangeRing();
                    currentState = CharacterState.OrangeRing;
                    break;
            }
        }


    }
    //进入冰冻状态
    void ToFrozen()
    {
        string matPath = CharStateMatPath["Frozen"];
        //加载冰冻材质球
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    foreach (var charRenderer in charRenderers)
                    {
                        //去除描边材质球
                        cacheMatList = charRenderer.sharedMaterials.ToList();
                        for (int i = 0; i < cacheMatList.Count; i++)
                        {
                            if (cacheMatList[i].name.Contains("Outline"))
                            {
                                cacheMatList.Remove(cacheMatList[i]);
                                break;
                            }
                        }


                        //Debug.Log("Material name: " + mat.name);
                        cacheMatList.Add(mat);
                        charRenderer.sharedMaterials = cacheMatList.ToArray();
                    }
                    //callback?.Invoke(atlas);
                }

            });

    }
    //进入石化状态
    void ToFossil()
    {
        // //去除描边材质球
        // cacheMatList = charRenderer.materials.ToList();
        // foreach (var VARIABLE in cacheMatList)
        // {
        //     if (VARIABLE.name.Contains("Outline"))
        //     {
        //         cacheMatList.Remove(VARIABLE);
        //     }
        // }
        // //加载石化材质球
        // cacheMatList.Add(Resources.Load<Material>(CharStateMatPath["Fossil"]));
        // charRenderer.materials = cacheMatList.ToArray();

        string matPath = CharStateMatPath["Fossil"];
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    foreach (var charRenderer in charRenderers)
                    {
                        //原神的石化效果保持描边 可以去除角色本身的材质球
                        cacheMatList = charRenderer.sharedMaterials.ToList();
                        for (int i = 0; i < cacheMatList.Count; i++)
                        {
                            if ((!cacheMatList[i].name.Contains("Outline")) && cacheMatList[i].name.Contains("M_DP"))
                            {
                                cacheMatList.Remove(cacheMatList[i]);
                                break;
                            }
                        }

                        //加载石化材质球
                        cacheMatList.Add(mat);
                        charRenderer.sharedMaterials = cacheMatList.ToArray();

                        //callback?.Invoke(atlas);'
                    }
                }
            });
    }
    //进入幽灵状态
    void ToGhost()
    {
        // //去除描边材质球
        // cacheMatList = charRenderer.materials.ToList();
        // foreach (var VARIABLE in cacheMatList)
        // {
        //     if (VARIABLE.name.Contains("Outline"))
        //     {
        //         cacheMatList.Remove(VARIABLE);
        //     }
        // }
        // //加载幽灵材质球
        // cacheMatList.Add(Resources.Load<Material>(CharStateMatPath["Ghost"]));
        // charRenderer.materials = cacheMatList.ToArray();

        string matPath = CharStateMatPath["Ghost"];
        string matPath2 = CharStateMatPath["ZWrite"];
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath2,
                        (Material mat2) =>
                        {
                            if (mat2 != null && this != null)
                            {
                                foreach (var charRenderer in charRenderers)
                                {
                                    //幽灵状态 把角色本身的材质球都去除 
                                    cacheMatList = charRenderer.sharedMaterials.ToList();
                                    cacheMatList.Clear();

                                    //加载幽灵材质球和Zwrite材质球
                                    cacheMatList.Add(mat2);
                                    cacheMatList.Add(mat);
                                    charRenderer.sharedMaterials = cacheMatList.ToArray();

                                    //callback?.Invoke(atlas);'
                                }
                            }
                        });
                }
            });
    }
    //进入红色斑点状态
    void ToRedPatches()
    {
        // foreach (var charRenderer in charRenderers)
        // {
        //     //去除描边材质球
        //         cacheMatList = charRenderer.sharedMaterials.ToList();
        //         for (int i = 0; i < cacheMatList.Count; i++)
        //         {
        //             if (cacheMatList[i].name.Contains("Outline"))
        //             {
        //                 cacheMatList.Remove(cacheMatList[i]);
        //                 break;
        //             }
        //         }
        //         //加载红色斑点材质球
        //         //cacheMatList.Add(Resources.Load<Material>(CharStateMatPath["RedPatches"]));
        //         string matPath = "Roles/Material/State/Common_Char_RedPatches";
        //         StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
        //             (Material mat) =>
        //             {
        //                 if (mat != null)
        //                 {
        //                     Debug.Log("Material name: " + mat.name);
        //                     cacheMatList.Add(mat);
        //                     charRenderer.sharedMaterials = cacheMatList.ToArray();
        //                 }
        //                 //callback?.Invoke(atlas);
        //             });
        //         //
        //         //cacheMatList.Add(AssetDatabase.LoadAssetAtPath<Material>(CharStateMatPath["RedPatches"]));
        //        // charRenderer.materials = cacheMatList.ToArray();
        // }

        string matPath = CharStateMatPath["RedPatches"];
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    foreach (var charRenderer in charRenderers)
                    {
                        //去除描边材质球
                        cacheMatList = charRenderer.sharedMaterials.ToList();
                        for (int i = 0; i < cacheMatList.Count; i++)
                        {
                            if (cacheMatList[i].name.Contains("Outline"))
                            {
                                cacheMatList.Remove(cacheMatList[i]);
                                break;
                            }
                        }


                        //Debug.Log("Material name: " + mat.name);
                        cacheMatList.Add(mat);
                        charRenderer.sharedMaterials = cacheMatList.ToArray();

                        //callback?.Invoke(atlas);'
                    }
                }
            });

    }

    void ToWater()
    {
        string matPath = CharStateMatPath["Water"];
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    foreach (var charRenderer in charRenderers)
                    {
                        //原神的石化效果保持描边 可以去除角色本身的材质球
                        cacheMatList = charRenderer.sharedMaterials.ToList();
                        for (int i = 0; i < cacheMatList.Count; i++)
                        {
                            if ((cacheMatList[i].name.Contains("Outline")))
                            {
                                cacheMatList.Remove(cacheMatList[i]);
                                break;
                            }
                        }

                        //加载濡湿材质球
                        cacheMatList.Add(mat);
                        charRenderer.sharedMaterials = cacheMatList.ToArray();

                        //callback?.Invoke(atlas);'
                    }
                }
            });
    }

    //进入橙色光圈
    void ToOrangeRing()
    {
        string matPath = CharStateMatPath["OrangeRing"];
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>(matPath,
            (Material mat) =>
            {
                if (mat != null && this != null)
                {
                    foreach (var charRenderer in charRenderers)
                    {
                        //原神的石化效果保持描边 可以去除角色本身的材质球
                        cacheMatList = charRenderer.sharedMaterials.ToList();
                        for (int i = 0; i < cacheMatList.Count; i++)
                        {
                            if ((cacheMatList[i].name.Contains("Outline")))
                            {
                                cacheMatList.Remove(cacheMatList[i]);
                                break;
                            }
                        }

                        //加载濡湿材质球
                        cacheMatList.Add(mat);
                        charRenderer.sharedMaterials = cacheMatList.ToArray();

                        //callback?.Invoke(atlas);'
                    }
                }
            });
    }
    //返回正常模式
    public void ReturnNormal()
    {
        // charRenderer.materials = normalMaterialList;
        foreach (var renderer in charRenderers)
        {
            normalMaterialList.Clear();
            var mat = originalCharMat[renderer.name];
            if (mat.normalMat != null)
            {
                normalMaterialList.Add(mat.normalMat);
            }
            if (mat.outLineMat != null)
            {
                normalMaterialList.Add(mat.outLineMat);
            }
            // normalMaterialList[0] = originalCharMat[renderer.name].normalMat;

            // normalMaterialList[1] = originalCharMat[renderer.name].outLineMat;

            renderer.materials = normalMaterialList.ToArray();
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        //StarProject.Service.Resource.ResourceManager.Instance.Init();
        //StarProject.Service.Resource.ResourceFormalManager.Instance.Init();


        // foreach (var renderer in charRenderers)
        // {
        //     foreach (var mat in renderer.sharedMaterials)
        //     {
        //         if (mat.name.Contains("Outline"))
        //         {
        //             CharMatOutLine = mat;
        //         }
        //         else
        //         {
        //             CharMat = mat;
        //         }
        //     }
        //

        // SetNormalMat();
        //     bool ifContain = originalCharMat.ContainsKey(renderer.name);
        //     if (!ifContain)
        //     {
        //         originalCharMat.Add(renderer.name, new NormalMat() { outLineMat = CharMatOutLine, normalMat = CharMat });
        //     }

        // }


#if UNITY_EDITOR

#endif
        //throw new NotImplementedException();
    }
}