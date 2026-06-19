//来源表通用提示配置表_RetMsg.xlsm.xlsx -> sheet:RetMsg
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RetMsgData
	{
		[Key(0)]
		public Dictionary<int, RetMsgDataCell> StaticRetMsgDatas = new Dictionary<int, RetMsgDataCell>();
	}
	[MessagePackObject]
	public class RetMsgDataCell
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
		//服务器所需备注
		[Key(3)]
		public string Note;
		//服务器提示
		[Key(4)]
		public string ErrorStr;
		//参数多语言
		[Key(5)]
		public string ParamsKey;
		//客户端提示多语言
		[Key(6)]
		public string ClientStr
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ClientStr); }
			set { _ClientStr = value; }
        }
		[IgnoreMember]
		private string _ClientStr;
	}
}
