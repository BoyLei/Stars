//来源表分包下载_Download.xlsx -> sheet:ResourcePack
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ResourcePackData
	{
		[Key(0)]
		public Dictionary<int, ResourcePackDataCell> StaticResourcePackDatas = new Dictionary<int, ResourcePackDataCell>();
	}
	[MessagePackObject]
	public class ResourcePackDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//资源包名-KEY
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//对应资源
		[Key(2)]
		public string ResPack;
		//下载排序
		[Key(3)]
		public int DownloadOrder;
		public int GetDownloadOrder()
		{
			return DownloadOrder;
		}
		//玩法ID
		[Key(4)]
		public int GamePlayID;
		public int GetGamePlayID()
		{
			return GamePlayID;
		}
		//地图ID
		[Key(5)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
	}
}
