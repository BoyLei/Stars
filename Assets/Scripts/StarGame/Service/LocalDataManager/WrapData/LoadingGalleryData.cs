//来源表loading.xlsm.xlsx -> sheet:LoadingGallery
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LoadingGalleryData
	{
		[Key(0)]
		public Dictionary<int, LoadingGalleryDataCell> StaticLoadingGalleryDatas = new Dictionary<int, LoadingGalleryDataCell>();
	}
	[MessagePackObject]
	public class LoadingGalleryDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//背景图
		[Key(1)]
		public string LoadingPic;
		//loading图库ID
		[Key(2)]
		public int GalleryID;
		public int GetGalleryID()
		{
			return GalleryID;
		}
	}
}
