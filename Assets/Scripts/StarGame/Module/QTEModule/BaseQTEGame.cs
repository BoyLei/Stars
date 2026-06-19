using ProtoMsg;

namespace StarProject.Module.QTE
{
    public abstract class BaseQTEGame
    {
        /// <summary>
        /// 对应配置表ID
        /// </summary>
        private int GameID{ get; set; }


        /// <summary>
        /// 服务器下发token用于验证游戏，结束时发给服务器
        /// </summary>
        private string Token{ get; set; }

        /// <summary>
        /// 高级产出道具信息 可能有多个;如果为0 则没有QTE产出
        /// </summary>
        private BaseData RareItem{ get; set; }

        /// <summary>
        /// 最迟结束时间
        /// </summary>
        private long EndTime { get; set; }

        /// <summary>
        /// 是否需要tick
        /// </summary>
        public virtual bool NeedTick
        {
            get;protected set;
        }

        /// <summary>
        /// 游戏是否胜利
        /// </summary>
        public bool IsWin { get; protected set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="info"></param>
        public virtual void OnGameInit(LifeSkillQTEItemInfo info)
        {
            GameID = info.QTEId;
            RareItem = info.RareItem;
            Token = info.QTEToken;
            EndTime = info.EndTime;
            IsWin = false;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="gameId">游戏ID</param>
        /// <param name="token">Token串</param>
        /// <param name="rareItem">奖励产出</param>
        /// <param name="endTime">结束时间</param>
        public virtual void OnGameInit(int gameId, string token, BaseData rareItem, long endTime)
        {
            GameID = gameId;
            RareItem = rareItem;
            Token = token;
            EndTime = endTime;
            IsWin = false;
        }
        
        /// <summary>
        /// 游戏开始
        /// </summary>
        public abstract void OnGamePlay();

        /// <summary>
        /// 游戏结束
        /// </summary>
        public abstract void OnGameEnd();
        
        /// <summary>
        /// 游戏每帧Tick
        /// </summary>
        public virtual void OnGameTick()
        {
            
        }
    }

}
