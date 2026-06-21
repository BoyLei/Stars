using SGF.Module.Framework;
using StarProject;
using StarProjectDef;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AKRTPCHelper: MonoBehaviour
{
    //音乐的ArtHelper
    //他属于对一个mixer管线操作的器,管线自己选择，但是定义个一个管线是可以对应很多音频的操作
    //这个可以放定义多个他是一个key对应的自己脚本
    public AK.Wwise.RTPC RTPC_controller;
    // 使用此函数进行初始化。
    private void Awake()
    {
        //怪物仇恨server战斗状态，客户端放过技能6秒，现在混合状态通知C+S有一个为战斗则为战斗；
        //如果是战斗，接受主角战斗信息状态
        //不要mix了，服务器再改变吧，客户端不参与
        GlobalEvent.onMainPlayerChanageBattleState.AddListener(onMainPlayerChanageBattleState);
    }
    private void OnDestroy()
    {
        GlobalEvent.onMainPlayerChanageBattleState.RemoveListener(onMainPlayerChanageBattleState);
    }
    
    //管线选择要是受到战斗管线影响的，管线
    //音频组控制对应管线参数影响这条线的所有设定
    //如果需要有新的管线mixer那就新启一个
    private void onMainPlayerChanageBattleState(E_PlayerStateForMusic arg0)
    {
        RTPC_controller.SetGlobalValue((int)arg0);
    }

    /*void Update()
    {
     
    }*/
}