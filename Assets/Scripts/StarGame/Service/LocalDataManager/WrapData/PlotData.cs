//来源表剧情配置表_Plot.xlsm.xlsx -> sheet:Plot
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PlotData
	{
		[Key(0)]
		public Dictionary<int, PlotDataCell> StaticPlotDatas = new Dictionary<int, PlotDataCell>();
	}
	[MessagePackObject]
	public class PlotDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//章节名
		[Key(1)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//幕序号
		[Key(2)]
		public string ActNum
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ActNum); }
			set { _ActNum = value; }
        }
		[IgnoreMember]
		private string _ActNum;
		//报幕文本内容
		[Key(3)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//持续时间
		[Key(4)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
		//报幕类型（1：主线；2：支线）
		[Key(5)]
		public int Type;
		public int GetType()
		{
			return Type;
		}
		//世界线ICON链接
		[Key(6)]
		public string Icon;
		//世界线章节ID
		[Key(7)]
		public int ActID;
		public int GetActID()
		{
			return ActID;
		}
	}
}
