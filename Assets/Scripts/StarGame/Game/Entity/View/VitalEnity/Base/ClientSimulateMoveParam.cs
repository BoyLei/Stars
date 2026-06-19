using UnityEngine;
namespace StarProject.Game.Entity.View.VitalSign
{
    enum ClientSimulateMoveType
    {
        None,
        /// <summary>
        /// 抛物线
        /// </summary>
        Parabola
    }

    /// <summary>
    /// 客户端 模拟运动 采用的通用参数
    /// </summary>
    class ClientSimulateMoveParam
    {
        public ClientSimulateMoveType moveTyp = ClientSimulateMoveType.None;


    }

    /// <summary>
    /// 抛物线运动，目前 策划要求的 抛物线 确定了 3个点
    /// 起点 A(0,h1) , 最高点 B(x,h2) 和 落点 C(s,0).
    /// 同时 水平方向速度 Vx 确定, 即 事件 t = s/Vx 确定.
    /// 以上 只有最高点的 x 未知。
    /// 策划 需要 模拟一条抛物线运动。
    /// 
    /// 综上,由于Vx 和 时间 t 确定, 那么就只需要计算出 Y 方向上运动的初始速度和 加速度 a 即可。
    /// 即解方程 h=Vy*t - 0.5*a*t*t + Hy;
    /// </summary>
    class TParabolaMove
    {
        /// <summary>
        /// 抛物线 起点,
        /// </summary>
        public Vector2 startPoint = Vector2.zero;
        public Vector2 maxHightPoint = Vector2.zero;
        public Vector2 endPoint = Vector2.zero;

        // public float 

        public void Init(Vector2 start, Vector2 hight, Vector2 end)
        {
            startPoint = start;
            maxHightPoint = hight;
            endPoint = end;
        }

        public void Start()
        {

        }


    }
}