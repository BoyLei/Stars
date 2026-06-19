using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class LimitData
	{
		public Dictionary<int, LimitDataCell> StaticLimitDatas = new Dictionary<int, LimitDataCell>();
	}
	public class LimitDataCell
	{
		//编号id
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//解锁条件
		public string Condition;
		private int _Condition = -1;
		public int GetCondition()
		{
			if (_Condition == -1 && int.TryParse(Condition, out _Condition))
			{
			}
			return _Condition;
		}
		//强化上限
		public string Max;
		private int _Max = -1;
		public int GetMax()
		{
			if (_Max == -1 && int.TryParse(Max, out _Max))
			{
			}
			return _Max;
		}

	}
}
