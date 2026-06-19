//来源表鸣器养成表_Etch.xlsm.xlsx -> sheet:Etch
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EtchData
	{
		[Key(0)]
		public Dictionary<int, EtchDataCell> StaticEtchDatas = new Dictionary<int, EtchDataCell>();
	}
	[MessagePackObject]
	public class EtchDataCell
	{
		//鸣器ID
		[Key(0)]
		public int EtchID;
		public int GetEtchID()
		{
			return EtchID;
		}
		//强化道具ID
		[Key(1)]
		public List<long> ItemID = new List<long>();
		//战技ID
		[Key(2)]
		public int BattleSkillID;
		public int GetBattleSkillID()
		{
			return BattleSkillID;
		}
		//鸣器名字多语言Key
		[Key(3)]
		public string EtchName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_EtchName); }
			set { _EtchName = value; }
        }
		[IgnoreMember]
		private string _EtchName;
		//鸣器描述多语言Key
		[Key(4)]
		public string EtchDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_EtchDec); }
			set { _EtchDec = value; }
        }
		[IgnoreMember]
		private string _EtchDec;
		//鸣器图标
		[Key(5)]
		public string EtchIcon;
		//鸣器解锁条件
		[Key(6)]
		public int UnlockCondition;
		public int GetUnlockCondition()
		{
			return UnlockCondition;
		}
	}
}
