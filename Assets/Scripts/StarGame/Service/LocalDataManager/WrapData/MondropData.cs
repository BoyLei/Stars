//来源表Monster.xlsm.xlsx -> sheet:Mondrop
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MondropData
	{
		[Key(0)]
		public Dictionary<int, MondropDataCell> StaticMondropDatas = new Dictionary<int, MondropDataCell>();
	}
	[MessagePackObject]
	public class MondropDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//等级
		[Key(1)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//玩法
		[Key(2)]
		public int Model;
		public int GetModel()
		{
			return Model;
		}
		//小怪掉落
		[Key(3)]
		public int MobDrop;
		public int GetMobDrop()
		{
			return MobDrop;
		}
		//掉落归属
		[Key(4)]
		public int DropBelong1;
		public int GetDropBelong1()
		{
			return DropBelong1;
		}
		//精英掉落
		[Key(5)]
		public int EliteDrop;
		public int GetEliteDrop()
		{
			return EliteDrop;
		}
		//掉落归属
		[Key(6)]
		public int DropBelong2;
		public int GetDropBelong2()
		{
			return DropBelong2;
		}
		//BOSS掉落
		[Key(7)]
		public int BossDrop;
		public int GetBossDrop()
		{
			return BossDrop;
		}
		//掉落归属
		[Key(8)]
		public int DropBelong3;
		public int GetDropBelong3()
		{
			return DropBelong3;
		}
	}
}
