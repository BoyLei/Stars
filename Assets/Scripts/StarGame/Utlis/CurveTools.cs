using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace SGF.Utlis
{
    public class CurveTools
    {
        private static float Func(float x, float height)
        {
            return 4 * (-height * x * x + height * x);
        }

        /// <summary>
        /// 抛物线 实现
        /// </summary>
        /// <returns></returns>
        public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
        {

            var mid = Vector3.Lerp(start, end, t);

            return new Vector3(mid.x, Func(t, height) + Mathf.Lerp(start.y, end.y, t), mid.z);
        }

        public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
        {
            var mid = Vector2.Lerp(start, end, t);

            return new Vector2(mid.x, Func(t, height) + Mathf.Lerp(start.y, end.y, t));
        }
        public static Vector2 ParabolaWidth(Vector2 start, Vector2 end, float width, float t)
        {
            var mid = Vector2.Lerp(start, end, t);

            return new Vector2(Func(t, width) + Mathf.Lerp(start.x, end.x, t), mid.y);
        }

    }
}
