//来源表TreasureMapPointData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class TreasureMapPointData
	{
		public Dictionary<int, TreasureMapPointDataCell> StaticTreasureMapPointDatas = new Dictionary<int, TreasureMapPointDataCell>();
	}
	public class TreasureMapPointDataCell
	{
		//坐标ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}

	}
}
