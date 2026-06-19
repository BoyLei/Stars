//来源表组队日常本表_TeamDailyCopy.xlsm.xlsx -> sheet:CopyBOSS
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CopyBOSSData
	{
		[Key(0)]
		public Dictionary<long, CopyBOSSDataCell> StaticCopyBOSSDatas = new Dictionary<long, CopyBOSSDataCell>();
	}
	[MessagePackObject]
	public class CopyBOSSDataCell
	{
		//BOSSID
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//所属BOSS库
		[Key(1)]
		public int BOSSGroupID;
		public int GetBOSSGroupID()
		{
			return BOSSGroupID;
		}
		//BOSS模型缩放（副本入口）
		[Key(2)]
		public int BOSS;
		public int GetBOSS()
		{
			return BOSS;
		}
	}
}
