using Animancer.FSM;
using DG.Tweening;
using StarProject.Game.Entity.Factory;
using StarProject.Game.Entity.View.VitalSign.State;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game.Entity.View.LocalDynamic
{
    public class TreasureBoxView : ViewInterctive, I_VVitalAnim
    {
        /////---------------- 宝箱有个自己的客户端模拟抛物线的掉落动画
        protected override void SetBornPosition()
        {
            /*
            //SGF.Debuger.LogError($"设置宝箱坐标 -------------- id={m_entity.EntityKey}");
            // 先模拟动画
            Vector3 startPos = m_entity.StartPosition();
            Vector3 endPos = GetBirthPos();
            float randomHeight = UnityEngine.Random.Range(2.0f, 4.0f);
            float percentDistance = UnityEngine.Random.Range(0.1f, 0.5f);
            Vector3 midPos = Vector3.Lerp(startPos, endPos, percentDistance) + Vector3.up * randomHeight;
            float time = UnityEngine.Random.Range(0.2f, 0.5f);
            //SGF.Debuger.LogError($"设置宝箱坐标 --------------startPos={startPos},midPos={midPos},endPos={endPos},tiem={time},randomHeight={randomHeight},percentDistance={percentDistance}");
            Vector3[] posArr = Reign.MathUtilities.SampleBezierCurve(startPos, midPos, endPos, 4);
            transform.localPosition = startPos;
            transform.DOPath(posArr, time, PathType.Linear).SetEase(Ease.InCirc);   // 曲线 由慢到快
            //SGF.Debuger.LogWarning($"设置宝箱坐标 -------------- id={m_entity.EntityKey}");
            */
            transform.localPosition = GetBirthPos();
        }
    }
}
