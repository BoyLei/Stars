using System;
using SkillEditor;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProject.Service.LocalData;
using StarProject.Service.Sound;
using StarProjectDef;
using UnityEngine;


namespace StarProject.Game.Entity
{
    /// <summary>
    /// 本地模拟的 实体
    /// note:
    ///     本地模拟的实体, 应该只需要有以下几个功能:
    ///         1.模型创建 
    ///         2.特效播放
    ///         3.动作播放
    /// </summary>
    public abstract class LocalSimulateEntity : EntityLocalStatic
    {

    }


}