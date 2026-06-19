using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StarProject.Game
{
    /// <summary>
    /// Feel 震屏参数，暂时不用，参数有点麻烦，但好歹现在知道具体意义了
    /// note: 一般选择噪声模式，震动才有比较顺滑的左右上下对称性，否则只会但方向震动
    /// </summary>
    public struct ShakeFeelCfg
    {
        public float duration;
        public float amplitude;     //振幅
        public float frequency;     //频率
        public float amplitudeX;    //x轴振幅
        public float amplitudeY;    //y轴振幅
        public float amplitudeZ;
        public bool infinite;       //是否一直
        public int channel;
        public bool useUnscaledTime;    //是否考虑缩放时间

        public ShakeFeelCfg(float _duration, float _amplitude, float _frequency, float _amplitudeX, float _amplitudeY, float _amplitudeZ, bool _infinite = false, int _channel = 0, bool _useUnscaledTime = false)
        {
            duration = _duration;
            amplitude = _amplitude;
            frequency = _frequency;
            amplitudeX = _amplitudeX;
            amplitudeY = _amplitudeY;
            amplitudeZ = _amplitudeZ;
            infinite = _infinite;
            channel = _channel;
            useUnscaledTime = _useUnscaledTime;
        }
    }

    public struct ShakeTweenCfg
    {
        public float duration;
        public Vector3 amplitude;   //振幅，一个方向向量    通过配置 (1,0,0) ;(1,1,0) 可以达到单方向，多方向的效果
        public int vibrato;         //震动的次数 ，默认是10，

        public bool fadeOut;        //震动是否衰退，默认是true，false 的话振幅不变，直到结束

        public ShakeTweenCfg(float _duration, Vector3 _amplitude, int _vibrato = 10, bool _fadeOut = true)
        {
            duration = _duration;
            amplitude = _amplitude;
            vibrato = _vibrato;
            fadeOut = _fadeOut;
        }
    }

    public static class ShakeConfig
    {


        // public static ShakeCfg DefaultShakeCfg = new ShakeCfg(0.3f, 4, 5, 4, 4, 0, false, 0, false);

        // public static Dictionary<int, ShakeCfg> ShakeCfgDictionary = new Dictionary<int, ShakeCfg>();


        public static ShakeTweenCfg DefaultShakeCfg = new ShakeTweenCfg(2, new Vector3(1, 1, 0));

        public static Dictionary<int, ShakeTweenCfg> ShakeCfgDictionary = new Dictionary<int, ShakeTweenCfg>();

        /// <summary>
        /// //TODO: dl
        ///  要根据配置来生成，目前先搞个默认的
        ///  得到对应的震屏配置参数
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static ShakeTweenCfg GetShakeCfg(int level)
        {
            if (!ShakeCfgDictionary.ContainsKey(level))
            {
                return DefaultShakeCfg;
            }
            return ShakeCfgDictionary[level];
        }

    }
}
