using ProtoMsg;

namespace StarProject.Game.Data
{
    //[ProtoContract]
    public class MapData
    {
        /// <summary>
        /// 地图的ID，通过ID 可以找到地图的资源
        /// </summary>
        //[ProtoMember(1)] 
        public ulong SpaceId = 0;
        /// <summary>
        /// 地图ID
        /// </summary>
        public int MapID;
        /// <summary>
        /// 分线ID
        /// </summary>
        public ulong ServerID = 0;
        /// <summary>
        /// 分线ID[显示用的]
        /// </summary>
        public int ServerIDShow = 0;
        /// <summary>
        /// 场景类型
        /// </summary>
        public SpaceType SpaceType = SpaceType.SpaceDefault;

        /*/// <summary>
        /// 地图的名字，用于在UI中显示
        /// </summary>
		//[ProtoMember(2)] 
        public string name = "";*/

    }
}
