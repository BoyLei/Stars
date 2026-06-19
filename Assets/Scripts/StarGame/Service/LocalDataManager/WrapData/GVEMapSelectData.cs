//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEMapSelect
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEMapSelectData
	{
		[Key(0)]
		public Dictionary<int, GVEMapSelectDataCell> StaticGVEMapSelectDatas = new Dictionary<int, GVEMapSelectDataCell>();
	}
	[MessagePackObject]
	public class GVEMapSelectDataCell
	{
		//主Key
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//玩法ID
		[Key(1)]
		public int PlayMode;
		public int GetPlayMode()
		{
			return PlayMode;
		}
		//生效开服时间
		[Key(2)]
		public int OpenDay;
		public int GetOpenDay()
		{
			return OpenDay;
		}
		//失效开服时间
		[Key(3)]
		public int FailureDay;
		public int GetFailureDay()
		{
			return FailureDay;
		}
		//地图ID
		[Key(4)]
		public int Map;
		public int GetMap()
		{
			return Map;
		}
		//怪物等级
		[Key(5)]
		public int MonsLv;
		public int GetMonsLv()
		{
			return MonsLv;
		}
		//随机权重
		[Key(6)]
		public int MapWeight;
		public int GetMapWeight()
		{
			return MapWeight;
		}
	}
}
