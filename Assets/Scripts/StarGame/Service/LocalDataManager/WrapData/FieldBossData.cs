//来源表野外BOSS表_FieldBoss.xlsm.xlsx -> sheet:FieldBoss
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FieldBossData
	{
		[Key(0)]
		public Dictionary<int, FieldBossDataCell> StaticFieldBossDatas = new Dictionary<int, FieldBossDataCell>();
	}
	[MessagePackObject]
	public class FieldBossDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//开服刷新天数
		[Key(1)]
		public int OpeningDays;
		public int GetOpeningDays()
		{
			return OpeningDays;
		}
		//控制器ID
		[Key(2)]
		public int ControID;
		public int GetControID()
		{
			return ControID;
		}
		//SpawnID
		[Key(3)]
		public int SpawnID;
		public int GetSpawnID()
		{
			return SpawnID;
		}
		//BOSS等级
		[Key(4)]
		public int BOSSLevel;
		public int GetBOSSLevel()
		{
			return BOSSLevel;
		}
		//单次刷新数量
		[Key(5)]
		public int CreateNum;
		public int GetCreateNum()
		{
			return CreateNum;
		}
		//生成场景
		[Key(6)]
		public int Map;
		public int GetMap()
		{
			return Map;
		}
		//怪物ID
		[Key(7)]
		public long MonID;
		public long GetMonID()
		{
			return MonID;
		}
		//获得宝箱伤害量要求
		[Key(8)]
		public long BoxDamage;
		public long GetBoxDamage()
		{
			return BoxDamage;
		}
		//宝箱掉落
		[Key(9)]
		public long BoxAwards;
		public long GetBoxAwards()
		{
			return BoxAwards;
		}
		//尾刀掉落
		[Key(10)]
		public long KnockoutAwards;
		public long GetKnockoutAwards()
		{
			return KnockoutAwards;
		}
		//奖励预览道具
		[Key(11)]
		public List<int> AwardsShow = new List<int>();
		//奖励预览数量
		[Key(12)]
		public List<int> AwardsShowNum = new List<int>();
		//BOSS背景图
		[Key(13)]
		public string BOSSBackDrop;
		//BOSS卡片图
		[Key(14)]
		public string BOSSCard;
		//BOSS头像框
		[Key(15)]
		public string BOSSHead;
		//BOSS模型位置
		[Key(16)]
		public string ModelPos;
		//BOSS模型旋转
		[Key(17)]
		public string ModelRot;
		//BOSS模型缩放
		[Key(18)]
		public string ModelScale;
		//动画名称
		[Key(19)]
		public string ModelAnima;
		//BOSS描述
		[Key(20)]
		public string BOSSDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_BOSSDec); }
			set { _BOSSDec = value; }
        }
		[IgnoreMember]
		private string _BOSSDec;
	}
}
