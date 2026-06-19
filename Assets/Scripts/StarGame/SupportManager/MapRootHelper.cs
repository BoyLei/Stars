using GPUInstancer;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MapRootHelper : MonoBehaviour
{
    public static MapRootHelper MRH;
    ///但是做Perfab的时候需要手动人为去加
    ///1如果还需要其他处理，例如美术自动变成程序用的
    ///2变成批量化脚本：1加组件，2生成属性，3挪位置
    ///Editor 换 RunTime
    /// <summary>
    /// 静态配置，省去Find的时间，都挂载，如之后不挂载都要在GameMap.mapPrefab中处理find，
    /// </summary>
    //public Terrain Terrain;
    public Vector2 MapSize = Vector2.zero;
    public float MimiMapScale = 1.0f;
    public AK.Wwise.Switch @switch;
    public Transform DynamicColliderRoot { get; private set; }

    public Terrain terrain;

    public CharAdditionLightController charAdditionLightController;


    public List<Volume> volumes;

    public GPUInstancerTreeManager GPUITree;


    private void Awake()
    {
        MRH = this;

        var RomoteEnityPools = transform.Find("RomoteEnityPools");
        if (RomoteEnityPools != null)
        {
            RomoteEnityPools.transform.position = Vector3.zero;
            RomoteEnityPools.transform.rotation = Quaternion.identity;
            RomoteEnityPools.transform.localScale = Vector3.one;

            DynamicColliderRoot = RomoteEnityPools.Find("DynamicRoot");
            if (DynamicColliderRoot != null)
            {
                DynamicColliderRoot.transform.position = Vector3.zero;
                DynamicColliderRoot.transform.rotation = Quaternion.identity;
                DynamicColliderRoot.transform.localScale = Vector3.one;
            }

            var StaticRoot = RomoteEnityPools.Find("StaticRoot");
            if (StaticRoot != null)
            {
                StaticRoot.transform.position = Vector3.zero;
                StaticRoot.transform.rotation = Quaternion.identity;
                StaticRoot.transform.localScale = Vector3.one;
            }
        }




        Scene currentScene = SceneManager.GetActiveScene();

        // 获取当前场景中的所有根对象（直接子对象）
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name.Contains("GPUI Tree Manager"))
            {
                GPUITree = rootObject.GetComponent<GPUInstancerTreeManager>();
            }
        }

        Transform lightRoot = MRH.transform.Find("Light");
        if (lightRoot != null)
        {
            lights = FindActiveLightsInChildren<Light>(lightRoot);

            volumes = FindActiveLightsInChildren<Volume>(lightRoot);
        }


        GameConfig.FleshMachineQualityLevel();
        RefleshVolume();
    }
    List<Light> lights;//非单一光源
    List<T> FindActiveLightsInChildren<T>(Transform parent)
    {
        List<T> lights = new List<T>();

        // 遍历 parent 下的所有子节点
        foreach (Transform child in parent)
        {
            // 检查子节点是否有 Light 组件
            T comp = child.GetComponent<T>();
            if (comp != null/* && child.gameObject.activeInHierarchy*/)
            {
                lights.Add(comp);
            }

            // 递归检查子节点的子节点
            lights.AddRange(FindActiveLightsInChildren<T>(child));
        }

        return lights;
    }





    /// <summary>
    /// 1,先设置过的所以这里不存在先后关系。
    /// 2,下次修改也会生效。
    /// 3,设计不用有参数进来目的是约束统一设置。
    /// 4,加载场景gamemap会换，并且find会找到，vol之有有或者没有
    /// </summary>
    public void RefleshVolume()
    {

        if (lights != null)
        {
            foreach (var item in lights)
            {
                if (item != null && item.gameObject.activeInHierarchy)
                {
                    switch (GameConfig.MachineQualityLevel)
                    {
                        case MachineQualityLevel.TopestLevel:

                            item.shadows = LightShadows.Soft;
                            break;
                        case MachineQualityLevel.TopLevel:

                            item.shadows = LightShadows.Soft;
                            break;
                        case MachineQualityLevel.MiddleLevel:

                            item.shadows = LightShadows.Soft;
                            break;
                        case MachineQualityLevel.LowerLevel:

                            item.shadows = LightShadows.Soft;
                            break;
                        case MachineQualityLevel.LowestLevel:

                            item.shadows = LightShadows.None;
                            break;

                        default:
                            break;
                    }
                }
            }
        }



        if (volumes != null)
        {
            foreach (var volume in volumes)
            {
                if (volume != null && volume.gameObject.activeInHierarchy)
                {
                    DepthOfField depthOfField;
                    if (volume.profile.TryGet(out depthOfField))
                    {
                    }
                    switch (GameConfig.MachineQualityLevel)
                    {
                        case MachineQualityLevel.TopestLevel:
                            volume.enabled = true;
                            if (depthOfField != null)
                            {
                                depthOfField.active = true;
                            }

                            if (GPUITree != null)
                            {
                                GPUITree.enabled = true;
                            }

                            break;
                        case MachineQualityLevel.TopLevel:
                            volume.enabled = true;
                            if (depthOfField != null)
                            {
                                depthOfField.active = true;
                            }
                            if (GPUITree != null)
                            {
                                GPUITree.enabled = true;
                            }

                            break;
                        case MachineQualityLevel.MiddleLevel:
                            volume.enabled = true;
                            if (depthOfField != null)
                            {
                                depthOfField.active = false;
                            }
                            if (GPUITree != null)
                            {
                                GPUITree.enabled = true;
                            }

                            break;
                        case MachineQualityLevel.LowerLevel:
                            volume.enabled = false;
                            if (depthOfField != null)
                            {
                                depthOfField.active = false;
                            }
                            if (GPUITree != null)
                            {
                                GPUITree.enabled = true;
                            }

                            break;
                        case MachineQualityLevel.LowestLevel:
                            volume.enabled = false;
                            if (depthOfField != null)
                            {
                                depthOfField.active = false;
                            }
                            if (GPUITree != null)
                            {
                                GPUITree.enabled = false;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }



    private string MapPath = "maps/commoncollider/";
    public void ShowHideCollider(string colliderName, bool setActive)
    {
        //GameObject collider = ResourceManager.Instance.LoadGameObject(MapPath + colliderName);
        //collider.transform.SetParent(DynamicColliderRoot);
        //collider.SetActive(true);
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(MapPath + colliderName,
        (GameObject go) =>
        {
            if (go == null)
            {
                return;
            }

            var gob = GameObject.Instantiate<GameObject>(go);
            if (gob != null)
            {
                gob.transform.SetParent(DynamicColliderRoot);
                gob.SetActive(true);
            }
        });

        //2不用Local归零和Transform归零，3是跟节点是0—下面自带位移—保持这样直接加载即可
    }

    public void ClearDynamicCollider()
    {
        if (DynamicColliderRoot == null)
        {
            return;
        }
        for (int i = DynamicColliderRoot.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(DynamicColliderRoot.transform.GetChild(i).gameObject); //1Gob删除
        }
    }
}
