using DG.Tweening;
using ProtoMsg;
using SGF.Module.Framework;
using StarProject;
using StarProject.ArtHelper;
using StarProject.Game;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using StarProject.Service.Input;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using XLua;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.Playables;
using SGF.UI.Framework;
using UnityEngine.Rendering.Universal;
using MessagePack;
using StarProject.Service.Sound;

[LuaCallCSharp]
public class DrawCardUI : MonoBehaviour
{
    //public GameObject detailGo;
    //public GameObject tentimesGo;

    public PlayableDirector director;
    public GameObject tenTimesCardsGo;      //十连抽go
    public Material cardMat;

    public Camera cam;
    public Camera dirCam;


    Action finishAct = null;

    //public Transform parentTrans;

    public static DrawCardUI Instance = null;

    public GameObject reflectionGo;
    public GameObject ckdGo;

    void Awake()
    {
        Instance = this;
        director.stopped += (dir) =>
        {
            director.gameObject.SetActive(false);
            dirCam.gameObject.SetActive(false);
            cam.gameObject.SetActive(true);
            if (finishAct != null)
            {
                finishAct.Invoke();
            }
            ShowTenTimes(false);
            RenderSettings.fog = true;
        };
    }

    public void ShowDrawCardDetail()
    {
        // detailGo.SetActive(true);
        // tentimesGo.SetActive(false);
    }

    public void ShowTenTimesWindow()
    {
        // detailGo.SetActive(false);
        // tentimesGo.SetActive(true);
    }

    public void PlayTimeline(Action cb)
    {
        UIManager.Instance.CloseAllLoadedWindowNWidget();
        finishAct = cb;
        director.gameObject.SetActive(true);
        dirCam.gameObject.SetActive(true);
        cam.gameObject.SetActive(false);
        director.Play();
        ShowTenTimes(true);
        // UI_Click_On_Sound
        SoundManager.Instance.PlayEventName("UI_ChouK_Friend_03", null,null);
        RenderSettings.fog = false;
    }

    public void ShowTenTimes(bool show)
    {
        reflectionGo.SetActive(!show);
        ckdGo.SetActive(!show);

        cam.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = !show;
    }
}