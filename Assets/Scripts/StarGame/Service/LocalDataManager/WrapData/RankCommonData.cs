//来源表排行榜_Rank.xlsm.xlsx -> sheet:RankCommon
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RankCommonData
	{
		[Key(0)]
		public Dictionary<int, RankCommonDataCell> StaticRankCommonDatas = new Dictionary<int, RankCommonDataCell>();
	}
	[MessagePackObject]
	public class RankCommonDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//排行榜显示用分类
		[Key(1)]
		public int RankShowType;
		public int GetRankShowType()
		{
			return RankShowType;
		}
		//排行榜数据类型
		[Key(2)]
		public int RankDataType;
		public int GetRankDataType()
		{
			return RankDataType;
		}
		//实例参数
		[Key(3)]
		public int RanktypeParam;
		public int GetRanktypeParam()
		{
			return RanktypeParam;
		}
		//排行榜名
		[Key(4)]
		public string RankName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RankName); }
			set { _RankName = value; }
        }
		[IgnoreMember]
		private string _RankName;
		//刷新条件
		[Key(5)]
		public int RefreshType;
		public int GetRefreshType()
		{
			return RefreshType;
		}
		//刷新参数
		[Key(6)]
		public int RefreshParam;
		public int GetRefreshParam()
		{
			return RefreshParam;
		}
		//精确排名顺位
		[Key(7)]
		public int PreciseRank;
		public int GetPreciseRank()
		{
			return PreciseRank;
		}
		//后位排名规则
		[Key(8)]
		public int RoughRank;
		public int GetRoughRank()
		{
			return RoughRank;
		}
		//系统开放
		[Key(9)]
		public int SystemOpen;
		public int GetSystemOpen()
		{
			return SystemOpen;
		}
		//说明栏文本
		[Key(10)]
		public int IntroID;
		public int GetIntroID()
		{
			return IntroID;
		}
	}
}
