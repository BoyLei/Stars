//来源表战场表_BattleField.xlsm.xlsx -> sheet:BattleFieldBox
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattleFieldBoxData
	{
		[Key(0)]
		public Dictionary<int, BattleFieldBoxDataCell> StaticBattleFieldBoxDatas = new Dictionary<int, BattleFieldBoxDataCell>();
	}
	[MessagePackObject]
	public class BattleFieldBoxDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//积分要求
		[Key(1)]
		public int Score;
		public int GetScore()
		{
			return Score;
		}
		//奖励道具
		[Key(2)]
		public List<long> Reward1 = new List<long>();
		//奖励数量
		[Key(3)]
		public List<long> Reward1Num = new List<long>();
	}
}
