//来源表战力表_Power.xlsm.xlsx -> sheet:RankRes
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RankResData
	{
		[Key(0)]
		public Dictionary<int, RankResDataCell> StaticRankResDatas = new Dictionary<int, RankResDataCell>();
	}
	[MessagePackObject]
	public class RankResDataCell
	{
		//大段位ID
		[Key(0)]
		public int ParentRank;
		public int GetParentRank()
		{
			return ParentRank;
		}
		//段位名称
		[Key(1)]
		public string RankName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RankName); }
			set { _RankName = value; }
        }
		[IgnoreMember]
		private string _RankName;
		//序列帧资源
		[Key(2)]
		public string SEQRes;
		//显示图标
		[Key(3)]
		public string Icon;
	}
}
