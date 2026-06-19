//来源表聊天配置表_Chat.xlsm.xlsx -> sheet:Chat
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ChatData
	{
		[Key(0)]
		public Dictionary<int, ChatDataCell> StaticChatDatas = new Dictionary<int, ChatDataCell>();
	}
	[MessagePackObject]
	public class ChatDataCell
	{
		//编号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//频道名-KEY
		[Key(1)]
		public string ChannelName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ChannelName); }
			set { _ChannelName = value; }
        }
		[IgnoreMember]
		private string _ChannelName;
		//发言条件
		[Key(2)]
		public int Condition;
		public int GetCondition()
		{
			return Condition;
		}
		//是否支持输入
		[Key(3)]
		public bool AllowInput;
		public bool GetAllowInput()
		{
			return AllowInput;
		}
		//是否支持语音
		[Key(4)]
		public bool AllowVoice;
		public bool GetAllowVoice()
		{
			return AllowVoice;
		}
		//是否支持弹幕
		[Key(5)]
		public bool AllowBarrage;
		public bool GetAllowBarrage()
		{
			return AllowBarrage;
		}
		//发言间隔（毫秒）
		[Key(6)]
		public int SendInterval;
		public int GetSendInterval()
		{
			return SendInterval;
		}
		//记录上限
		[Key(7)]
		public int MaxCount;
		public int GetMaxCount()
		{
			return MaxCount;
		}
	}
}
