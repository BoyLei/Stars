//来源表组队表_Team.xlsm.xlsx -> sheet:TeamPlay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamPlayData
	{
		[Key(0)]
		public Dictionary<int, TeamPlayDataCell> StaticTeamPlayDatas = new Dictionary<int, TeamPlayDataCell>();
	}
	[MessagePackObject]
	public class TeamPlayDataCell
	{
		//玩法ID
		[Key(0)]
		public int PlayID;
		public int GetPlayID()
		{
			return PlayID;
		}
		//玩法分类
		[Key(1)]
		public int PlayType;
		public int GetPlayType()
		{
			return PlayType;
		}
		//玩法名称-KEY
		[Key(2)]
		public string PlayName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PlayName); }
			set { _PlayName = value; }
        }
		[IgnoreMember]
		private string _PlayName;
		//参与人数描述-KEY
		[Key(3)]
		public string NumberDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_NumberDec); }
			set { _NumberDec = value; }
        }
		[IgnoreMember]
		private string _NumberDec;
		//参与等级
		[Key(4)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//玩法时间描述-KEY
		[Key(5)]
		public string TimeDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_TimeDec); }
			set { _TimeDec = value; }
        }
		[IgnoreMember]
		private string _TimeDec;
		//是否跨服
		[Key(6)]
		public bool CrossServer;
		public bool GetCrossServer()
		{
			return CrossServer;
		}
		//参与人数下限
		[Key(7)]
		public int NumLowerLimit;
		public int GetNumLowerLimit()
		{
			return NumLowerLimit;
		}
		//参与人数上限
		[Key(8)]
		public int NumTopLimit;
		public int GetNumTopLimit()
		{
			return NumTopLimit;
		}
	}
}
