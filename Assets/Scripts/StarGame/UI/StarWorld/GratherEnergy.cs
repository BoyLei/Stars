using System;
using System.Collections;
using System.Collections.Generic;
using StarProject;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class GratherEnergy : MonoBehaviour
{
    /// <summary>
    /// 当前体力
    /// </summary>
    public Text EnergyText;

    /// <summary>
    /// 需要消耗的体力
    /// </summary>
    public Text CostText;
    
    private long m_InterID;
    private int Type;

    public bool IsInter
    {
        get { return Type != 0 && m_InterID != 0; }
    }

    private void Awake()
    {
       gameObject.SetActive(false);
       GlobalEvent.OnGratherEnergy.AddListener(OnStopHandler);
       GlobalEvent.OnStopIner.AddListener(OnStopHandler);
       GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateCompleteHandler);
       GlobalEvent.OnBackLogin.AddListener(OnBackLoginHandler);
    }

    private void OnBackLoginHandler(AgainLoginType arg0)
    {
        this.Type = 0;
        this.m_InterID = 0;
        gameObject.SetActive(false);
    }

    private void OnRoleCreateCompleteHandler(object arg0)
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase != null && GameManager.Instance.M_MainPlayerCtrlBase.Data != null)
        {
            GameManager.Instance.M_MainPlayerCtrlBase.Data.RegisterAttribute(AOIAttrDefine.CollectEnergy,
                OnCollectEnergyChange);
        }
    }

    private void OnCollectEnergyChange(string key, object value)
    {
        var energy = GameManager.Instance.GetPlayerCollectEnergy();
        EnergyText.text = energy.ToString();
    }
    
    private void OnStopHandler(int type, long id, bool active)
    {
        if (active)
        {
            if (IsInter)
            {
                StarDebug.LogError($"已经在交互 类型={Type}  ID= {m_InterID}");
                return;
            }

            this.Type = type;
            this.m_InterID = id;
            gameObject.SetActive(true);
            
            var energy = GameManager.Instance.GetPlayerCollectEnergy();
            EnergyText.text = energy.ToString();
            if (Type==1)
            {
               // mStopText.text = "停止采集";
               var entityCtr = GameManager.Instance.GetEntityCtr((ulong)m_InterID) as ObjectCtrlGroup;
               if (entityCtr != null)
               {
                   var  cfg=LocalDataManager.Instance.GetLifeSkillMineDataCell((int)entityCtr.MineID);
                   if (cfg != null)
                   {
                       CostText.text = string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_LimitTime"),cfg.GetEnergyCost());// $"{cfg.GetEnergyCost()}/次";
                   }
               }
            }
            else if (Type==2)
            {
                var  cfg=LocalDataManager.Instance.GetLifeSkillCreateDataCell((int)m_InterID);
                if (cfg != null)
                {
                    CostText.text= string.Format(LanguageManager.Instance.GetLanguageByKey("Local_Str_LimitTime"),cfg.GetEnergyCost());
                }
            }
        }
        else
        {
            if (this.Type == type && this.m_InterID == id)
            {
                this.Type = 0;
                this.m_InterID = 0;
                gameObject.SetActive(false);
            }

        }
    }
}
