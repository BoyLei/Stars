using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StarProject.Game.Data
{
    public static class TriggleEventUtils
    {
        public static string FromatUITriggleKey(int uiConfig)
        {
            return $"UI_{uiConfig}"; ;
        }

        public static string FromatComplexUITriggleKey(string uiConfig)
        {
            return $"ComplexUI_{uiConfig}";
        }

    }
}