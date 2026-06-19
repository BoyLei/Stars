//来源表Holiday.xlsx -> sheet:Notice
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class NoticeData
	{
		[Key(0)]
		public Dictionary<int, NoticeDataCell> StaticNoticeDatas = new Dictionary<int, NoticeDataCell>();
	}
	[MessagePackObject]
	public class NoticeDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//服务器地址
		[Key(1)]
		public string SeverAddress;
		//标识符
		[Key(2)]
		public int NoticeSign;
		public int GetNoticeSign()
		{
			return NoticeSign;
		}
	}
}
