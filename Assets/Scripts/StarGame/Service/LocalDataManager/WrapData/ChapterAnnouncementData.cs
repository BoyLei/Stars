//来源表剧情配置表_Plot.xlsx -> sheet:ChapterAnnouncement
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ChapterAnnouncementData
	{
		[Key(0)]
		public Dictionary<int, ChapterAnnouncementDataCell> StaticChapterAnnouncementDatas = new Dictionary<int, ChapterAnnouncementDataCell>();
	}
	[MessagePackObject]
	public class ChapterAnnouncementDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//文本内容
		[Key(1)]
		public string Desc;
		//持续时间
		[Key(2)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
	}
}
