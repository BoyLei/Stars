//来源表T_TimelineDialog.xlsm.xlsx -> sheet:TimelineDialog
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TimelineDialogData
	{
		[Key(0)]
		public Dictionary<int, TimelineDialogDataCell> StaticTimelineDialogDatas = new Dictionary<int, TimelineDialogDataCell>();
	}
	[MessagePackObject]
	public class TimelineDialogDataCell
	{
		//ID（唯一不重复）
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//TL名称
		[Key(1)]
		public string TimelineName;
		//人物名称
		[Key(2)]
		public string PlayerName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PlayerName); }
			set { _PlayerName = value; }
        }
		[IgnoreMember]
		private string _PlayerName;
		//对话内容
		[Key(3)]
		public string Content
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Content); }
			set { _Content = value; }
        }
		[IgnoreMember]
		private string _Content;
		//语音路径
		[Key(4)]
		public string Audio;
		//对话时间
		[Key(5)]
		public int DialogTime;
		public int GetDialogTime()
		{
			return DialogTime;
		}
		//对话类型（1为过场2为无镜头表演）
		[Key(6)]
		public int DialogType;
		public int GetDialogType()
		{
			return DialogType;
		}
	}
}
