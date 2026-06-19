//来源表ModelAnimancer.xlsx -> sheet:ModelAnimancer
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ModelAnimancerData
	{
		[Key(0)]
		public Dictionary<int, ModelAnimancerDataCell> StaticModelAnimancerDatas = new Dictionary<int, ModelAnimancerDataCell>();
	}
	[MessagePackObject]
	public class ModelAnimancerDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//休闲待机
		[Key(1)]
		public string Idle;
		//战斗待机
		[Key(2)]
		public string BattleIdle;
		//漫步移动
		[Key(3)]
		public string WanderMoving;
		//休闲移动
		[Key(4)]
		public string SingleMoving;
		//战斗移动
		[Key(5)]
		public string BattleMoving;
		//受击
		[Key(6)]
		public string Hurt;
		//击倒
		[Key(7)]
		public string Deading;
		//待机展示
		[Key(8)]
		public string Stand;
		//待机收刀
		[Key(9)]
		public string WeaponRetractionIdle;
		//移动收刀
		[Key(10)]
		public string WeaponRetractionMoving;
		//抽卡待机
		[Key(11)]
		public string DrawCardIdle;
		//抽卡出场动作
		[Key(12)]
		public string DrawCardEnter;
		//抽卡展示
		[Key(13)]
		public string DrawCardShow;
	}
}
