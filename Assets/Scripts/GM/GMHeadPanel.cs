using SGF.UI.Framework;
using StarProject.Service.LocalData;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMHeadPanel : MonoBehaviour
{
    private GMHeadItem selectItem;

    public GameObject gmHeadItemCopy;

    public Transform content;

    public RawImage rawImage;

    public GameObject roleOb1;
    public GameObject roleOb2;

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        if (roleOb1 != null)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(UIDef.DisplayModelPath, roleOb1);
        }
        roleOb1 = null;

        if (roleOb2 != null)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.PushGameObject(UIDef.DisplayModelPath, roleOb2);
        }
        roleOb2 = null;

        UIManager.Instance.SetRawCamrea(false);
    }

    private void Init()
    {
        LoadModelRoot();

        content.DestroyChildrenImmediate();
        AvatarData avatarData = LocalDataManager.Instance.M_AvatarData;

        Dictionary<int, AvatarDataCell> datas = avatarData.StaticAvatarDatas;

        foreach (KeyValuePair<int, AvatarDataCell> item in datas)
        {
            CreateHeadItem(item.Value, item.Key);
        }

        rawImage.texture = StarProject.Service.Resource.ResourceFormalManager.Instance.RenderTexture;
    }

    private void LoadModelRoot()
    {
        StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(UIDef.DisplayModelPath, (go) =>
        {
            if (go != null)
            {
                roleOb1 = go;
            }
        });

        StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(UIDef.DisplayModelPath, (go) =>
        {
            if (go != null)
            {
                roleOb2 = go;
            }
        });
    }

    private void CreateHeadItem(AvatarDataCell avatarDataCell, int id)
    {
        GameObject go = Instantiate(gmHeadItemCopy, content);
        go.SetActive(true);

        GMHeadItem gMHeadItem = go.GetComponent<GMHeadItem>();
        gMHeadItem.Init(avatarDataCell, id);

        gMHeadItem.ActionOnToggleSelect = OnToggleSelect;
    }

    private void OnToggleSelect(GMHeadItem gMHeadItem, bool isOn)
    {
        if (isOn)
        {
            if (selectItem != null)
            {
                selectItem.SelectToggle.isOn = false;
            }
            selectItem = gMHeadItem;
        }
        else
        {
            //如果 取消的是 当前的 选择的 头像，那就清除这个
            if (selectItem == gMHeadItem)
            {
                selectItem = null;
            }
            else
            {
                // 如果取消的是 其它的,那就不管
                return;
            }
        }

        RefreshModel(selectItem);
    }

    private void RefreshModel(GMHeadItem gMHeadItem)
    {
        if (gMHeadItem == null)
        {
            UIManager.Instance.SetRawCamrea(false);
            if (roleOb1 != null)
            {
                roleOb1.SetActive(false);
            }
            if (roleOb2 != null)
            {
                roleOb2.SetActive(false);
            }
            return;
        }

        bool isShowHigh = SetHighModel(gMHeadItem.avatarDataCfg);
        if (!isShowHigh)
        {
            if (roleOb1 != null)
            {
                roleOb1.SetActive(false);
            }
        }
        bool isShow = SetModel(gMHeadItem.avatarDataCfg);
        if (!isShow)
        {
            if (roleOb2 != null)
            {
                roleOb2.SetActive(false);
            }
        }
        rawImage.texture = StarProject.Service.Resource.ResourceFormalManager.Instance.RenderTexture;

        UIManager.Instance.SetRawCamrea(isShowHigh || isShow);
    }

    private bool SetHighModel(AvatarDataCell avatarDataCell)
    {
        if (roleOb1 != null)
        {
            int modelID = avatarDataCell.GetHighModelId();
            ModelDataCell modelDataCell = LocalDataManager.Instance.GetModelDataCell(modelID);
            if (modelDataCell != null)
            {
                var v3 = new Vector3(-1.1f, 0, 1.5f);
                ShowModel(roleOb1, avatarDataCell, v3, true);
                return true;
            }
        }
        return false;
    }

    private bool SetModel(AvatarDataCell avatarDataCell)
    {
        if (roleOb2 != null)
        {
            int modelID = avatarDataCell.GetModelId();
            ModelDataCell modelDataCell = LocalDataManager.Instance.GetModelDataCell(modelID);
            if (modelDataCell != null)
            {
                var v3 = new Vector3(-0.25f, 0, 1.5f);
                ShowModel(roleOb2, avatarDataCell, v3, false);
                return true;
            }
        }
        return false;
    }

    private void ShowModel(GameObject modelViewDisplay, AvatarDataCell avatarDataCell, Vector3 localPos, bool isHighModel = true)
    {
        UIManager.Instance.SetModel2UI2(
            modelViewDisplay,
            avatarDataCell,
            ThreeDModelPos.Raw,
            scale: Vector3.one,
            WorleRotate: Vector3.zero,
            localRotate: Vector3.zero,
            localPos: localPos,
            "idle_01",
            isHighModel: isHighModel
        );

        modelViewDisplay.SetActive(true);
    }

}
