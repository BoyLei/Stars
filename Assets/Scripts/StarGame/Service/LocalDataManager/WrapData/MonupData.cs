//来源表Monster.xlsm.xlsx -> sheet:Monup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MonupData
	{
		[Key(0)]
		public Dictionary<int, MonupDataCell> StaticMonupDatas = new Dictionary<int, MonupDataCell>();
	}
	[MessagePackObject]
	public class MonupDataCell
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
		//展示等级
		[Key(2)]
		public int ShowLevel;
		public int GetShowLevel()
		{
			return ShowLevel;
		}
		//玩法
		[Key(3)]
		public int Model;
		public int GetModel()
		{
			return Model;
		}
		//小怪曲线
		[Key(4)]
		public List<int> Mob = new List<int>();
		//精英曲线
		[Key(5)]
		public List<int> Elite = new List<int>();
		//BOSS曲线
		[Key(6)]
		public List<int> Boss = new List<int>();
	}
}
