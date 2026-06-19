//来源表通用说明栏_Common_Info.xlsm.xlsx -> sheet:CommonInfo
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CommonInfoData
	{
		[Key(0)]
		public Dictionary<int, CommonInfoDataCell> StaticCommonInfoDatas = new Dictionary<int, CommonInfoDataCell>();
	}
	[MessagePackObject]
	public class CommonInfoDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//组ID
		[Key(1)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
		//标题多语言Key
		[Key(2)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//提示描述多语言Key
		[Key(3)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//插入图片
		[Key(4)]
		public string Image;
		//发送事件
		[Key(5)]
		public string FireEventName;
	}
}
