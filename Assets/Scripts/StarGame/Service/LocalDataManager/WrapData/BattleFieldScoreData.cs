//来源表战场表_BattleField.xlsm.xlsx -> sheet:BattleFieldScore
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BattleFieldScoreData
	{
		[Key(0)]
		public Dictionary<int, BattleFieldScoreDataCell> StaticBattleFieldScoreDatas = new Dictionary<int, BattleFieldScoreDataCell>();
	}
	[MessagePackObject]
	public class BattleFieldScoreDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//得分类型
		[Key(1)]
		public int ScoreType;
		public int GetScoreType()
		{
			return ScoreType;
		}
		//参数
		[Key(2)]
		public int Param;
		public int GetParam()
		{
			return Param;
		}
		//备注
		[Key(3)]
		public string Comment
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Comment); }
			set { _Comment = value; }
        }
		[IgnoreMember]
		private string _Comment;
		//获得积分
		[Key(4)]
		public int Score;
		public int GetScore()
		{
			return Score;
		}
	}
}
