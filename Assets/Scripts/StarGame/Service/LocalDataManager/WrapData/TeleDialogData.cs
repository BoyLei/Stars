//来源表通用对话配置表_Common_Dialog.xlsm.xlsx -> sheet:TeleDialog
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeleDialogData
	{
		[Key(0)]
		public Dictionary<int, TeleDialogDataCell> StaticTeleDialogDatas = new Dictionary<int, TeleDialogDataCell>();
	}
	[MessagePackObject]
	public class TeleDialogDataCell
	{
		//对话id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//对话类型
		[Key(1)]
		public int Dialog_type;
		public int GetDialog_type()
		{
			return Dialog_type;
		}
		//下一句id
		[Key(2)]
		public List<int> Next_dialog_id = new List<int>();
		//对话组ID
		[Key(3)]
		public int Dialog_group;
		public int GetDialog_group()
		{
			return Dialog_group;
		}
		//是否可以中断
		[Key(4)]
		public bool Can_interrupt;
		public bool GetCan_interrupt()
		{
			return Can_interrupt;
		}
		//对话者形象
		[Key(5)]
		public string Spine_id;
		//对话者位置
		[Key(6)]
		public string Site;
		//对话者名字多语言Key
		[Key(7)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//对话内容多语言Key
		[Key(8)]
		public string Dialog_desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Dialog_desc); }
			set { _Dialog_desc = value; }
        }
		[IgnoreMember]
		private string _Dialog_desc;
		//对话选项多语言Key
		[Key(9)]
		public string Dialog_option
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Dialog_option); }
			set { _Dialog_option = value; }
        }
		[IgnoreMember]
		private string _Dialog_option;
		//语音id
		[Key(10)]
		public string Voice_id;
		//发送delay
		[Key(11)]
		public int Send_delay;
		public int GetSend_delay()
		{
			return Send_delay;
		}
		//对话效果
		[Key(12)]
		public List<int> Common_effect = new List<int>();
	}
}
