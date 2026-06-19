//来源表弹窗配置表_Window.xlsm.xlsx -> sheet:CRetMsg
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CRetMsgData
	{
		[Key(0)]
		public Dictionary<int, CRetMsgDataCell> StaticCRetMsgDatas = new Dictionary<int, CRetMsgDataCell>();
	}
	[MessagePackObject]
	public class CRetMsgDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//消息类型
		[Key(1)]
		public int MsgType;
		public int GetMsgType()
		{
			return MsgType;
		}
		//枚举类型
		[Key(2)]
		public string EnumName;
		//批注名字多语言Key
		[Key(3)]
		public string Note
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Note); }
			set { _Note = value; }
        }
		[IgnoreMember]
		private string _Note;
	}
}
