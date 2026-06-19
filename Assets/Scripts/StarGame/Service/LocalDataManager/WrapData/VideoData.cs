//来源表FramEvent.xlsx -> sheet:Video
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class VideoData
	{
		[Key(0)]
		public Dictionary<int, VideoDataCell> StaticVideoDatas = new Dictionary<int, VideoDataCell>();
	}
	[MessagePackObject]
	public class VideoDataCell
	{
		//id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//视频资源路径
		[Key(1)]
		public string Path;
	}
}
