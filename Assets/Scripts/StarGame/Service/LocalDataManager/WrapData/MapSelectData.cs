//来源表MapSelectData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class MapSelectData
	{
		public Dictionary<int, MapSelectDataCell> StaticMapSelectDatas = new Dictionary<int, MapSelectDataCell>();
	}
	public class MapSelectDataCell
	{
		//主Key
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//玩法ID
		public string PlayMode;
		private int _PlayMode = -1;
		public int GetPlayMode()
		{
			if (_PlayMode == -1 && int.TryParse(PlayMode, out _PlayMode))
			{
			}
			return _PlayMode;
		}
		//生效开服时间
		public string OpenDay;
		private int _OpenDay = -1;
		public int GetOpenDay()
		{
			if (_OpenDay == -1 && int.TryParse(OpenDay, out _OpenDay))
			{
			}
			return _OpenDay;
		}
		//失效开服时间
		public string FailureDay;
		private int _FailureDay = -1;
		public int GetFailureDay()
		{
			if (_FailureDay == -1 && int.TryParse(FailureDay, out _FailureDay))
			{
			}
			return _FailureDay;
		}
		//地图ID
		public string Map;
		private int _Map = -1;
		public int GetMap()
		{
			if (_Map == -1 && int.TryParse(Map, out _Map))
			{
			}
			return _Map;
		}
		//怪物等级
		public string MonsLv;
		private int _MonsLv = -1;
		public int GetMonsLv()
		{
			if (_MonsLv == -1 && int.TryParse(MonsLv, out _MonsLv))
			{
			}
			return _MonsLv;
		}
		//随机权重
		public string MapWeight;
		private int _MapWeight = -1;
		public int GetMapWeight()
		{
			if (_MapWeight == -1 && int.TryParse(MapWeight, out _MapWeight))
			{
			}
			return _MapWeight;
		}

	}
}
