using ProtoBuf;
using StarProjectDef;

/// <summary>
/// /// <summary>
/// 所有细胞生物都 MVC都会有
/// 只有非细胞生物，子弹，辅助实体才没有V
/// </summary>
/// </summary>
namespace StarProject.Game.Data
{
    /// <summary>
    /// 单局中人的数据，这里要与人的配置数据区分
    /// 如果我们要做人的成长系统，则需要区分人的配置数据和强化后的数据
    /// </summary>
    [ProtoContract]
    public class VitalSignViewShowData
    {
        /// <summary>
        /// 细胞的ID，表示这是一条什么样的细胞这，可以通过这个ID 找到细胞的资源和配置
        /// 为什么不直接使用蛇的配置数据？
        /// 因为，有可能不同的玩家使用了同细胞，通过不同的强化后，这条蛇的实际参数会不同
        /// 比如初始模型
        /// 比如分类：
        /// 【所以这里只有角色模型的替换】【不包含零部件】
        /// 注意我们游戏设定是：不同职业就是不同的模型：不是魔力宝贝
        /// 1，战士男，2，战士女，3，道士男，4，道士女，5，法师男，6，法师女****职业
        /// 每一个角色都有自己的战斗变身--参考尤迪安（替换模型）****变身
        /// 每一个角色会有时装中--至宝概念---或身心概念--替换模型****时装
        /// 
        /// 每一个角色武器成长，可能会发生模型替换（那是武器数据）
        /// 年龄成长也会换模型？那不可能，一定是在这个模型上长胡子（没有）
        /// </summary>
        [ProtoMember(1)] public int id;

        /// <summary>
        /// 角色外观名称
        /// </summary>
        [ProtoMember(2)] public string name = "";

        /// <summary>
        /// 模型大小默认是1：比如要做缩放，可以服务器做控制，例如征途的三千小世界（其实也可以加入颜色）
        /// </summary>
        [ProtoMember(3)] public int size = 1;

        /// <summary>
        /// 角色的颜色，这个颜色可以通过阵营枚举出来不过，颜色是可以随机的，例如对战游戏的10种颜色，都是随的
        /// </summary>
        [ProtoMember(4)] public string color = "255,255,255";

        /// <summary>
        /// 玩家的摄像机视野：领主战中，任意防守方的视野是服务器可通知的
        /// </summary>
        [ProtoMember(5)] public float viewScale = 1;

        ///// <summary>
        ///// 模型是否呗服务器通知而隐藏，例如特效表现，或能选中，但是模型隐藏，但是确实是有显示数据的，只不过不渲染，可以理解为透明度0.000001
        ///// </summary>
        //[ProtoMember(6)] public bool bodyVisible = true;

        ///优先级如下，0默认基础状态，1有时装显示时装，2有变身显示变身（因为没有项目组给变身在做一套时装那么情怀（LangFeiQian）
        [ProtoMember(7)]
        public E_ModelShowState e_ModelShowState = E_ModelShowState.Normal;
    }
}
