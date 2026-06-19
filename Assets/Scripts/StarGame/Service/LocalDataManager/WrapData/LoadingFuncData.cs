//来源表loading.xlsm.xlsx -> sheet:LoadingFunc
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LoadingFuncData
	{
		[Key(0)]
		public Dictionary<int, LoadingFuncDataCell> StaticLoadingFuncDatas = new Dictionary<int, LoadingFuncDataCell>();
	}
	[MessagePackObject]
	public class LoadingFuncDataCell
	{
		//FuncID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//玩法ID
		[Key(1)]
		public int GamePlayID;
		public int GetGamePlayID()
		{
			return GamePlayID;
		}
		//地图ID
		[Key(2)]
		public int MapID;
		public int GetMapID()
		{
			return MapID;
		}
		//图库ID
		[Key(3)]
		public int GalleryID;
		public int GetGalleryID()
		{
			return GalleryID;
		}
		//tips库ID
		[Key(4)]
		public int TipsID;
		public int GetTipsID()
		{
			return TipsID;
		}
	}
}
