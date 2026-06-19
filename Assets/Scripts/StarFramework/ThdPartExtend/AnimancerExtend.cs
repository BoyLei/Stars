using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    public class AnimancerExtend : AnimancerComponent
    {
        public Action<string, string> ActionOnPlayWwise;
        public void PlayWwise(string path)
        {
            var wwisePath = path.Split("|");
            if (wwisePath.Length < 2)
            {
                SGF.Debuger.LogError($"[VitalState] PlayWwise: {path} 路径报错");
                return;
            }
            string soundBank = wwisePath[0];
            string wwiseName = wwisePath[1];

            ActionOnPlayWwise?.Invoke(soundBank, wwiseName);
            /*StarProject.Service.Sound.SoundManager.Instance.PostSoundBankEvent(soundBank, wwiseName, this.gameObject, this.gameObject);*/
        }
    }
}
