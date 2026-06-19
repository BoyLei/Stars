using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarProject.Game.Data
{
    /// <summary>
    /// 定义游戏单局中可能的玩家操作
    /// </summary>
    public static class GameVKey
    {
        //////////////////  摇杆按键
        /// <summary>
        /// X方向移动
        /// </summary>
        public const int MoveX_CMD = 11;

        /// <summary>
        /// Y方向移动
        /// </summary>
        public const int MoveZ_CMD = 12;

        /// <summary>
        /// 技能
        /// </summary>
        public const int Skill = 999;

        /// <summary>
        /// 技能
        /// </summary>
        public const int Test = 9999;
    }


}
