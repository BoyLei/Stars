//来源表RankPointData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class RankPointData
	{
		public Dictionary<int, RankPointDataCell> StaticRankPointDatas = new Dictionary<int, RankPointDataCell>();
	}
	public class RankPointDataCell
	{
		//行为类型
		public string ActType;
		private int _ActType = -1;
		public int GetActType()
		{
			if (_ActType == -1 && int.TryParse(ActType, out _ActType))
			{
			}
			return _ActType;
		}
		//行为数据
		public List<int> Value = new List<int>();
		//获得分数
		public List<int> PointValue = new List<int>();

	}
}
