//来源表MatchData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class MatchData
	{
		public Dictionary<int, MatchDataCell> StaticMatchDatas = new Dictionary<int, MatchDataCell>();
	}
	public class MatchDataCell
	{
		//匹配规则序号
		public string MatchID;
		private int _MatchID = -1;
		public int GetMatchID()
		{
			if (_MatchID == -1 && int.TryParse(MatchID, out _MatchID))
			{
			}
			return _MatchID;
		}
		//等待时长
		public string LastTime;
		private int _LastTime = -1;
		public int GetLastTime()
		{
			if (_LastTime == -1 && int.TryParse(LastTime, out _LastTime))
			{
			}
			return _LastTime;
		}
		//段位下限范围
		public string Rankmin;
		private int _Rankmin = -1;
		public int GetRankmin()
		{
			if (_Rankmin == -1 && int.TryParse(Rankmin, out _Rankmin))
			{
			}
			return _Rankmin;
		}
		//段位上限范围
		public string Rankmax;
		private int _Rankmax = -1;
		public int GetRankmax()
		{
			if (_Rankmax == -1 && int.TryParse(Rankmax, out _Rankmax))
			{
			}
			return _Rankmax;
		}
		//是否填充机器人
		public string IsRobot;
		public bool GetIsRobot()
		{
			bool _IsRobot;
			if (bool.TryParse(IsRobot, out _IsRobot))
			{
			}
			return _IsRobot;
		}

	}
}
