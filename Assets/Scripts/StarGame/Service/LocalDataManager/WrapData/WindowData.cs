//来源表弹窗配置表_Window.xlsm.xlsx -> sheet:Window
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class WindowData
	{
		[Key(0)]
		public Dictionary<int, WindowDataCell> StaticWindowDatas = new Dictionary<int, WindowDataCell>();
	}
	[MessagePackObject]
	public class WindowDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//弹窗类型
		[Key(1)]
		public int Type;
		public int GetType()
		{
			return Type;
		}
		//弹窗标题多语言Key
		[Key(2)]
		public string Ttile
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Ttile); }
			set { _Ttile = value; }
        }
		[IgnoreMember]
		private string _Ttile;
		//弹窗文本多语言Key
		[Key(3)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//是否在确认按钮上显示道具数量
		[Key(4)]
		public bool IsShowItemCount;
		public bool GetIsShowItemCount()
		{
			return IsShowItemCount;
		}
		//按钮
		[Key(5)]
		public string Buttons
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Buttons); }
			set { _Buttons = value; }
        }
		[IgnoreMember]
		private string _Buttons;
		//事件
		[Key(6)]
		public List<string> Events = new List<string>();
		//是否今日不再提醒
		[Key(7)]
		public bool Warn;
		public bool GetWarn()
		{
			return Warn;
		}
		//层级类型
		[Key(8)]
		public int RootType;
		public int GetRootType()
		{
			return RootType;
		}
	}
}
