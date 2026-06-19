//来源表剧情配置表_Plot.xlsm.xlsx -> sheet:Image
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ImageData
	{
		[Key(0)]
		public Dictionary<int, ImageDataCell> StaticImageDatas = new Dictionary<int, ImageDataCell>();
	}
	[MessagePackObject]
	public class ImageDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//图片路径
		[Key(1)]
		public string Path;
	}
}
