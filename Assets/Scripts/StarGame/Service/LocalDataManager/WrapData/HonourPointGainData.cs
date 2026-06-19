//来源表HonourPointGainData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class HonourPointGainData
	{
		public Dictionary<int, HonourPointGainDataCell> StaticHonourPointGainDatas = new Dictionary<int, HonourPointGainDataCell>();
	}
	public class HonourPointGainDataCell
	{
		//荣誉行为类型
		public string ActionType;
		private int _ActionType = -1;
		public int GetActionType()
		{
			if (_ActionType == -1 && int.TryParse(ActionType, out _ActionType))
			{
			}
			return _ActionType;
		}
		//获得荣誉值
		public string Point;
		private int _Point = -1;
		public int GetPoint()
		{
			if (_Point == -1 && int.TryParse(Point, out _Point))
			{
			}
			return _Point;
		}

	}
}
