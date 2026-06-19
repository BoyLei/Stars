//来源表HonourPointData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class HonourPointData
	{
		public Dictionary<int, HonourPointDataCell> StaticHonourPointDatas = new Dictionary<int, HonourPointDataCell>();
	}
	public class HonourPointDataCell
	{
		//分数上限
		public string HonourPoint;
		private int _HonourPoint = -1;
		public int GetHonourPoint()
		{
			if (_HonourPoint == -1 && int.TryParse(HonourPoint, out _HonourPoint))
			{
			}
			return _HonourPoint;
		}
		//匹配时长
		public string MatchWaitTime;
		private int _MatchWaitTime = -1;
		public int GetMatchWaitTime()
		{
			if (_MatchWaitTime == -1 && int.TryParse(MatchWaitTime, out _MatchWaitTime))
			{
			}
			return _MatchWaitTime;
		}
		//匹配描述
		public string HonourDesc;

	}
}
