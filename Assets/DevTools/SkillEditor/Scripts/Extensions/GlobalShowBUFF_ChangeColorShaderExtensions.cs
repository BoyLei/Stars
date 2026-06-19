using SkillEditor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using Task;

namespace SkillEditor
{
    public partial class GlobalShowBUFF_ChangeColorShader : GlobalShowTypeSerialize
    {
        public Color GetEdgeColor()
        {
            if (edgeColor == Color.clear)
            {
                edgeColor = new Color(r: EdgeColorR / 1000f, g: EdgeColorG / 1000f, b: EdgeColorB / 1000f, a: EdgeColorA / 1000f);
            }
            return edgeColor;
        }
        public Color GetMainColor()
        {
            if (mainColor == Color.clear)
            {
                mainColor = new Color(r: MainColorR / 1000f, g: MainColorG / 1000f, b: MainColorB / 1000f, a: MainColorA / 1000f);
            }
            return mainColor;
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
#if UNITY_EDITOR
            MainColor = GetMainColor();
            EdgeColor = GetEdgeColor();
#endif
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
#if UNITY_EDITOR
            MainColorR = (int)(MainColor.r * 1000);
            MainColorG = (int)(MainColor.g * 1000);
            MainColorB = (int)(MainColor.b * 1000);
            MainColorA = (int)(MainColor.a * 1000);
            EdgeColorR = (int)(EdgeColor.r * 1000);
            EdgeColorG = (int)(EdgeColor.g * 1000);
            EdgeColorB = (int)(EdgeColor.b * 1000);
            EdgeColorA = (int)(EdgeColor.a * 1000);
#endif
        }
    }
}
