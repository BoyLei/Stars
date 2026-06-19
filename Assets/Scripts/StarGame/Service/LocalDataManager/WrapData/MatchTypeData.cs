//来源表MatchTypeData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class MatchTypeData
	{
		public Dictionary<int, MatchTypeDataCell> StaticMatchTypeDatas = new Dictionary<int, MatchTypeDataCell>();
	}
	public class MatchTypeDataCell
	{
		//类型序号
		public string TypeID;
		private int _TypeID = -1;
		public int GetTypeID()
		{
			if (_TypeID == -1 && int.TryParse(TypeID, out _TypeID))
			{
			}
			return _TypeID;
		}
		//类型名称
		public string TypeName;
		//多人匹配
		public string MultiMatch;
		private int _MultiMatch = -1;
		public int GetMultiMatch()
		{
			if (_MultiMatch == -1 && int.TryParse(MultiMatch, out _MultiMatch))
			{
			}
			return _MultiMatch;
		}
		//队伍序号
		public List<int> TeamID = new List<int>();
		//队伍人数
		public List<int> TeamMemberNum = new List<int>();
		//是否使用段位匹配
		public string IsMatchWithRank;
		public bool GetIsMatchWithRank()
		{
			bool _IsMatchWithRank;
			if (bool.TryParse(IsMatchWithRank, out _IsMatchWithRank))
			{
			}
			return _IsMatchWithRank;
		}
		//是否使用隐藏分匹配
		public string IsMatchWithPoint;
		public bool GetIsMatchWithPoint()
		{
			bool _IsMatchWithPoint;
			if (bool.TryParse(IsMatchWithPoint, out _IsMatchWithPoint))
			{
			}
			return _IsMatchWithPoint;
		}

	}
}
