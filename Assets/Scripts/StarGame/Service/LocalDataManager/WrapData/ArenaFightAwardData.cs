//来源表ArenaFightAwardData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class ArenaFightAwardData
	{
		public Dictionary<int, ArenaFightAwardDataCell> StaticArenaFightAwardDatas = new Dictionary<int, ArenaFightAwardDataCell>();
	}
	public class ArenaFightAwardDataCell
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
		//挑战次数
		public string FightNum;
		private int _FightNum = -1;
		public int GetFightNum()
		{
			if (_FightNum == -1 && int.TryParse(FightNum, out _FightNum))
			{
			}
			return _FightNum;
		}
		//胜负
		public string WinOrLose;
		private int _WinOrLose = -1;
		public int GetWinOrLose()
		{
			if (_WinOrLose == -1 && int.TryParse(WinOrLose, out _WinOrLose))
			{
			}
			return _WinOrLose;
		}
		//积分数量
		public string PointNum;
		private int _PointNum = -1;
		public int GetPointNum()
		{
			if (_PointNum == -1 && int.TryParse(PointNum, out _PointNum))
			{
			}
			return _PointNum;
		}
		//宝箱奖励1
		public string BoxAward1;
		private int _BoxAward1 = -1;
		public int GetBoxAward1()
		{
			if (_BoxAward1 == -1 && int.TryParse(BoxAward1, out _BoxAward1))
			{
			}
			return _BoxAward1;
		}
		//宝箱奖励数量1
		public string BoxAwardNum1;
		private int _BoxAwardNum1 = -1;
		public int GetBoxAwardNum1()
		{
			if (_BoxAwardNum1 == -1 && int.TryParse(BoxAwardNum1, out _BoxAwardNum1))
			{
			}
			return _BoxAwardNum1;
		}
		//宝箱奖励2
		public string BoxAward2;
		private int _BoxAward2 = -1;
		public int GetBoxAward2()
		{
			if (_BoxAward2 == -1 && int.TryParse(BoxAward2, out _BoxAward2))
			{
			}
			return _BoxAward2;
		}
		//宝箱奖励数量2
		public string BoxAwardNum2;
		private int _BoxAwardNum2 = -1;
		public int GetBoxAwardNum2()
		{
			if (_BoxAwardNum2 == -1 && int.TryParse(BoxAwardNum2, out _BoxAwardNum2))
			{
			}
			return _BoxAwardNum2;
		}

	}
}
