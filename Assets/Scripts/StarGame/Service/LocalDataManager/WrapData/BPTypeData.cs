//来源表BPTypeData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class BPTypeData
	{
		public Dictionary<int, BPTypeDataCell> StaticBPTypeDatas = new Dictionary<int, BPTypeDataCell>();
	}
	public class BPTypeDataCell
	{
		//ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//价格
		public string Price;
		private int _Price = -1;
		public int GetPrice()
		{
			if (_Price == -1 && int.TryParse(Price, out _Price))
			{
			}
			return _Price;
		}
		//直升等级
		public string AddGrade;
		private int _AddGrade = -1;
		public int GetAddGrade()
		{
			if (_AddGrade == -1 && int.TryParse(AddGrade, out _AddGrade))
			{
			}
			return _AddGrade;
		}
		//一次性奖励1
		public string PayAward1;
		private int _PayAward1 = -1;
		public int GetPayAward1()
		{
			if (_PayAward1 == -1 && int.TryParse(PayAward1, out _PayAward1))
			{
			}
			return _PayAward1;
		}
		//一次性奖励数量1
		public string PayAwardAward1;
		private int _PayAwardAward1 = -1;
		public int GetPayAwardAward1()
		{
			if (_PayAwardAward1 == -1 && int.TryParse(PayAwardAward1, out _PayAwardAward1))
			{
			}
			return _PayAwardAward1;
		}
		//一次性奖励2
		public string PayAward2;
		private int _PayAward2 = -1;
		public int GetPayAward2()
		{
			if (_PayAward2 == -1 && int.TryParse(PayAward2, out _PayAward2))
			{
			}
			return _PayAward2;
		}
		//一次性奖励数量2
		public string PayAwardAward2;
		private int _PayAwardAward2 = -1;
		public int GetPayAwardAward2()
		{
			if (_PayAwardAward2 == -1 && int.TryParse(PayAwardAward2, out _PayAwardAward2))
			{
			}
			return _PayAwardAward2;
		}
		//一次性奖励3
		public string PayAward3;
		private int _PayAward3 = -1;
		public int GetPayAward3()
		{
			if (_PayAward3 == -1 && int.TryParse(PayAward3, out _PayAward3))
			{
			}
			return _PayAward3;
		}
		//一次性奖励数量3
		public string PayAwardAward3;
		private int _PayAwardAward3 = -1;
		public int GetPayAwardAward3()
		{
			if (_PayAwardAward3 == -1 && int.TryParse(PayAwardAward3, out _PayAwardAward3))
			{
			}
			return _PayAwardAward3;
		}

	}
}
