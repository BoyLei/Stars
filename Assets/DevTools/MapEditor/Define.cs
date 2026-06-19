///--------------------------------------------------------------------
/// 文件名   :   Define
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 11:43:29
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using UnityEngine;
#if UNITY_EDITOR
using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
namespace MapEditor
{
    public interface IMapElement
    {
        void Load();
        void ExportExcel(int row, ExcelRange excel, int index);
    }



    public enum FreshType
    {
        OnDead = 0,                     //0：死亡间隔刷新
        FirstOpenServer = 1,            //1：开服间隔刷新
        FixTime = 2,                    //2：固定时刻刷新
    }

    public enum AreaType
    {
        BuffZone = 0,                   //0：buff区域
        ReviveZone = 1,                 //1：复活点
        BronPoint = 2,                    //3:出生点
        TriggerZone = 99,                 //99触发区域
        Jurisdiction = 100,               //管辖区域
    }



    public enum ColliderType
    {
        InAndOut = 0,                     //0：可进可出
        OnlyIn = 1,                       //1：可进不可出
        OnlyOut = 2,                      //2：不可进可出
        NoneInAndOut,                   //3：不可进出
    }

    /// <summary>
    /// 阻挡边缘显示类型
    /// </summary>
    public enum BlockShowType
    {
        Always = 0,                     //随时可见
        Near = 1,                         //靠近可见
        Never = 2,                        //永不可可见
    }

    public enum MonsterType
    {
        None = 0,                             //异常
        Normal = 1,                         //1 - 普通
        Elite = 2,                          //2 - 精英
        Boss = 3,                           //3 - BOSS
        NPC = 4,                            //4 - NPC
        Melee = 5,                          //5 - 近战
        Ranged = 6,                         //6 - 远程
        Reward = 7,                         //7 - 奖励怪
    }

    public enum NPCType
    {
        NORMALNPC = 0,                          //0 －普通NPC
        TASKNPC = 1,                            //1 - 任务NPC
        ESCORTNPC = 2,                          //2 - 护送NPC
        DIALOGUENPC = 3,                        //3 - 对话NPC
        TRANSFERNPC = 4,                        //4 - 传送NPC
    }

    public enum MineType
    {
        BaseObject = 0,                     //基础物件
        ReciveTask = 1,                     //接任务 
        Transfer = 2,                       // 传送
        SceneTransfer = 3,                  //跨场景传送
        Clamber = 4,                        // 攀爬
        AddBuff = 5,                        //加buff
    }

    /// <summary>
    /// 效果类型
    /// </summary>
    public enum EffectType
    {
        ReleaseSkill = 1,                   //释放技能
        AddBuff = 2,                        //添加Buff
        ReciveTask = 3,                     //接收任务
        FinishTask = 4,                     //完成任务
        StopTask = 5,                       //终止任务
    }

    public enum WalkableType
    {
        None = -1,                            //闲置
        Stand = 0,                          //站立
        WalkInage = 1,                      //自由徘徊，徘徊范围
        WalkByPath = 2,                     //沿路径行走，路径ID
    }
}
#endif
public enum AreaShape
{
    Circle = 0,                      //0：圆形
    Rectangle = 1,                    //1：矩形
    Polygon = 2,                     //2：多边形
}

public class Define
{
    public static Vector3 LeftUp = new Vector3(-0.5f, 0, 0.5f);
    public static Vector3 RightUp = new Vector3(0.5f, 0, 0.5f);
    public static Vector3 LeftDown = new Vector3(-0.5f, 0, -0.5f);
    public static Vector3 RightDown = new Vector3(0.5f, 0, -0.5f);
}

