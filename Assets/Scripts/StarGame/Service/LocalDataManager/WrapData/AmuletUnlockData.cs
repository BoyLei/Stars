//来源表AmuletUnlockData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class AmuletUnlockData
	{
		public Dictionary<int, AmuletUnlockDataCell> StaticAmuletUnlockDatas = new Dictionary<int, AmuletUnlockDataCell>();
	}
	public class AmuletUnlockDataCell
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
		//护符类型
		public string AmuletType;
		private int _AmuletType = -1;
		public int GetAmuletType()
		{
			if (_AmuletType == -1 && int.TryParse(AmuletType, out _AmuletType))
			{
			}
			return _AmuletType;
		}
		//冒险等级解锁
		public string AdvGrade;
		private int _AdvGrade = -1;
		public int GetAdvGrade()
		{
			if (_AdvGrade == -1 && int.TryParse(AdvGrade, out _AdvGrade))
			{
			}
			return _AdvGrade;
		}

	}
}
