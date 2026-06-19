//来源表邮件表_Mail.xlsm.xlsx -> sheet:Mail
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MailData
	{
		[Key(0)]
		public Dictionary<int, MailDataCell> StaticMailDatas = new Dictionary<int, MailDataCell>();
	}
	[MessagePackObject]
	public class MailDataCell
	{
		//邮件ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//唯一标识码
		[Key(1)]
		public string Ustring;
		//服务器所需备注
		[Key(2)]
		public string Note;
		//邮件标题-KEY
		[Key(3)]
		public string MailTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_MailTitle); }
			set { _MailTitle = value; }
        }
		[IgnoreMember]
		private string _MailTitle;
		//服务器所需备注
		[Key(4)]
		public string MailTitleComment;
		//邮件正文-KEY
		[Key(5)]
		public string MailTxt
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_MailTxt); }
			set { _MailTxt = value; }
        }
		[IgnoreMember]
		private string _MailTxt;
		//服务器所需备注
		[Key(6)]
		public string MailTxtComment;
	}
}
