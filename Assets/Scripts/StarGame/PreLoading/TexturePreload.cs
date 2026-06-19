using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SGF.Unity;
using XLua;
public class TexturePreload
{
    static string[] PreloadAtlas = new string[] {
        "ui/icons/atlas/iconrole",
        "ui/common/atlas/commonitem",
    };
    static string[] PreloadIcons = new string[] {
        "IconRole_S_91801.png",
        "Bag_Grid_sq5.png",
    };
    bool isloadfinish = true;
    int loadindex = 0;
    public TexturePreload()
    {
        MonoHelper.AddSecTimeUpdateListener(Loading);
    }

    public void Loading()
    {
        if (loadindex < PreloadIcons.Length && isloadfinish)
        {
            isloadfinish = false;
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Object>(
               PreloadAtlas[loadindex], loadfinished, PreloadIcons[loadindex]
            );
        }
        else
        {
            if(loadindex >= PreloadIcons.Length)
            {
                MonoHelper.RemoveSecTimeUpdateListener(Loading);
            }
        }
    }

    void loadfinished(UnityEngine.Object obj)
    {
        isloadfinish = true;
        loadindex++;
    }

}