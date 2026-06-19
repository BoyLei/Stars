using StarProject.Service.AtlasManager;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMHeadItem : MonoBehaviour
{
    public Text HeadIDText;
    public Text DescText;
    public Toggle SelectToggle;
    public List<Image> HeadImages;

    public AvatarDataCell avatarDataCfg;

    // 头像 cfg
    public ModelHeadDataCell headDataCfg;

    // 高模 cfg
    public ModelDataCell highModelDataCfg;

    // 模型 cfg
    public ModelDataCell modelDataCfg;

    public Action<GMHeadItem, bool> ActionOnToggleSelect;

    private Action<Sprite> loadRoleIcon;

    public void Init(AvatarDataCell avatarDataCell, int ID)
    {
        avatarDataCfg = avatarDataCell;

        HeadIDText.text = ID.ToString();

        //DescText.text = avatarDataCfg.Desc;

        int modelID = avatarDataCell.GetModelId();
        if (modelID != -1)
        {
            modelDataCfg = LocalDataManager.Instance.GetModelDataCell(modelID);
        }

        int highModelID = avatarDataCell.GetHighModelId();
        if (highModelID != -1)
        {
            highModelDataCfg = LocalDataManager.Instance.GetModelDataCell(highModelID);
        }

        int headID = avatarDataCell.GetHeadID();
        if (headID != -1)
        {
            headDataCfg = LocalDataManager.Instance.GetModelHeadDataCell(headID);
        }

        RefreshHeadIcons();
    }



    void RefreshHeadIcons()
    {
        if (headDataCfg == null)
        {
            return;
        }

        RefreshSprite(HeadImages[0], headDataCfg.HalfDrawing);

        string atlasName = headDataCfg.HeadAtlasName;
        RefreshHeadIcon(HeadImages[1], atlasName, headDataCfg.MaxHead);
        RefreshHeadIcon(HeadImages[2], atlasName, headDataCfg.MiddleHead);
        RefreshHeadIcon(HeadImages[3], atlasName, headDataCfg.MinHead);
    }

    void RefreshSprite(Image image, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Sprite>(path,
        (UnityEngine.Sprite img) =>
        {
            if (img == null)
            {
                SGF.Debuger.LogWarning($"头像表中 ID: {avatarDataCfg.GetHeadID()} 不存在 路径: {path}");
                return;
            }
            image.sprite = img;
        });
    }

    void RefreshHeadIcon(Image image, string atlasName, string spName)
    {
        if (string.IsNullOrEmpty(spName))
        {
            return;
        }

        loadRoleIcon = (Sprite sp) =>
        {
            if (image != null && sp != null)
            {
                image.sprite = sp;
            }
            if (sp == null)
            {
                SGF.Debuger.LogWarning($"头像表中 ID: {avatarDataCfg.GetHeadID()} 不存在 头像: {spName}");
            }
        };
        AtlasManager.Instance.GetSpriteAsync(atlasName, spName, loadRoleIcon);
    }

    public void OnToggleSelect(bool isToggleOn)
    {
        ActionOnToggleSelect?.Invoke(this, isToggleOn);
    }

}
