//来源表剧情配置表_Plot.xlsm.xlsx -> sheet:LevelEnterWindow
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LevelEnterWindowData
	{
		[Key(0)]
		public Dictionary<int, LevelEnterWindowDataCell> StaticLevelEnterWindowDatas = new Dictionary<int, LevelEnterWindowDataCell>();
	}
	[MessagePackObject]
	public class LevelEnterWindowDataCell
	{
		//主key
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//插图
		[Key(1)]
		public string Image;
		//主标题
		[Key(2)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//描述
		[Key(3)]
		public string Description
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Description); }
			set { _Description = value; }
        }
		[IgnoreMember]
		private string _Description;
		//进入副本ID
		[Key(4)]
		public int EnterLevel;
		public int GetEnterLevel()
		{
			return EnterLevel;
		}
		//预览奖励1ID
		[Key(5)]
		public List<long> ShowReward = new List<long>();
		//预览奖励1数量
		[Key(6)]
		public List<long> ShowRewardNum = new List<long>();
	}
}
