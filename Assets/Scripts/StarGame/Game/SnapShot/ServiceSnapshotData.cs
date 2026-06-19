using StarProject.Game.Entity.Factory;

namespace StarProject.Game.SnapShot
{

    /// <summary>
    /// 单个的 黑板伤害数据 info
    /// </summary>
    /// 【提前数据】
    /// /
    public class ServiceSnapshotData : DynamicDataObject
    {
        public ulong RuntimeID;
        protected GameContext m_context;


        public virtual bool Init(ulong _EntityID)
        {
            M_EntityID = _EntityID;
            return true;
        }
        protected override void Create(ulong entityId)
        {
            M_EntityID = entityId;
            m_context = GameManager.Instance.Context;
        }

        protected override void Create()
        {
            m_context = GameManager.Instance.Context;
        }

        protected override void Release()
        {
            //data = null;
            //BlackList.Clear();
            base.Release();
        }


        ////黑板数据的拥有者的实体
        ////public ulong EntityID;
        ////被筛选之后的伤害信息黑板数据
        //public List<BlackBoardNode> BlackList = new List<BlackBoardNode>();




        ////TODO：之后要处理成 时间 + 帧数[0,30]
        //public double ExecuteTime = 0;
        ////public double PerEntityID = -1;//前置条件



        //public RuntimeEnumType runtimeEnumType;
        //public EnumBlackBoardType BlackBoardType;
        ///// <summary>
        ///// 原始的数据, 目前通过 参数 传过来的数据,会有不全
        ///// </summary>
        //public object data;


        //public virtual bool Init(ulong _RuntimeID, ulong _EntityID, RepeatedField<BlackBoardNode> _BlackList, RuntimeEnumType _runtimeEnumType, EnumBlackBoardType blackBoardType, object _data)
        //{
        //    RuntimeID = _RuntimeID;
        //    M_EntityID = _EntityID;
        //    runtimeEnumType = _runtimeEnumType;
        //    BlackBoardType = blackBoardType;

        //    data = _data;

        //    BlackList.Clear();
        //    for (int i = 0; i < _BlackList.Count; i++)
        //    {
        //        BlackList.Add(_BlackList[i]);
        //    }

        //    return BlackList.Count > 0;
        //}



    }
}
