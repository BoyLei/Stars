using System;
using System.Collections;
using System.Collections.Generic;
using DeadMosquito.AndroidGoodies;
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.LocalData;
using StarProjectDef;
using Unity.Mathematics;
using UnityEngine;

public class AdvancedTreasure : MonoBehaviour
{
    public Transform mPointer;

    public JButton mClose;

    public GameObject mNotice;
    
    [SerializeField] private Vector3 mTargetPosition;

    public float Angle;

    private bool IsVibrating = false;

    public float LessDistance = 3;

    public Vector3 RolePosition
    {
        get
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase != null)
            {
                return GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position();
            }

            return Vector3.zero;
        }
    }

    private void Awake()
    {
        var config= LocalDataManager.Instance.GetInteractDataCell(1000002);
       if (config != null)
       {
           LessDistance = config.GetTriggerRange() * 0.01f;
       }
        mClose.OnClick += OnCloseHandler;
    }
    private void OnCloseHandler(GameObject arg0)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TreasureModule,"OnBreakDigTreasureReq");
        gameObject.SetActive(false);
    }

    public void SetTarget(Vector3 target)
    {
        mTargetPosition = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (mPointer == null)
        {
            return;
        }

        if (GameManager.Instance.M_MainPlayerCtrlBase == null)
        {
            return;
        }

        var dir = mTargetPosition - RolePosition;
        Angle = (float)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360;
        mPointer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Angle - 90));

        if (Vector3.Distance(mTargetPosition, RolePosition) <= LessDistance)
        {
            mNotice.gameObject.SetActive(true);
            mPointer.gameObject.SetActive(false);
            Vibrator();
        }
        else
        {
            mNotice.gameObject.SetActive(false);
            mPointer.gameObject.SetActive(true);
        }
            
    }

    private void Vibrator()
    {
        if (IsVibrating)
        {
            return;
        }

        if (GameManager.Instance.M_MainPlayerCtrlBase != null)
        {
            var player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (player != null)
            {
                if (player.IsIntering())
                {
                    AGVibrator.Cancel();
                    IsVibrating = false;
                    return;
                }
            }
        }

        if (!AGVibrator.HasVibrator())
        {
            Debug.LogWarning("This device does not have vibrator");
        }

        if (!AGVibrator.AreVibrationEffectsSupported)
        {
            Debug.LogWarning("This device does not support vibration effects API!");
            return;
        }

        long[] mVibratePattern = { 0, 400, 1000, 600, 1000, 800, 1000, 1000 };
        int[] mAmplitudes = { 0, 255, 0, 255, 0, 255, 0, 255 };
        //Create a waveform vibration with different vibration amplitudes
        AGVibrator.Cancel();
        AGVibrator.Vibrate(VibrationEffect.CreateWaveForm(mVibratePattern, mAmplitudes, 0));
        IsVibrating = true;
        DelayInvoker.DelayInvoke(5.8f, (arg) => { IsVibrating = false; }, null);
    }
}