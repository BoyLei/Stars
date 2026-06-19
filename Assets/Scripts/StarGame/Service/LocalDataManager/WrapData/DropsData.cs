//来源表掉落配置表_Drops.xlsx -> sheet:Drops
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DropsData
	{
		[Key(0)]
		public Dictionary<long, DropsDataCell> StaticDropsDatas = new Dictionary<long, DropsDataCell>();
	}
	[MessagePackObject]
	public class DropsDataCell
	{
		//id
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//掉落包id
		[Key(1)]
		public long DropID;
		public long GetDropID()
		{
			return DropID;
		}
		//掉落次数
		[Key(2)]
		public int DropNum;
		public int GetDropNum()
		{
			return DropNum;
		}
		//索引id
		[Key(3)]
		public long IndexID;
		public long GetIndexID()
		{
			return IndexID;
		}
		//概率
		[Key(4)]
		public int Rate;
		public int GetRate()
		{
			return Rate;
		}
		//掉落表现
		[Key(5)]
		public int ShowType;
		public int GetShowType()
		{
			return ShowType;
		}
		//是否弹窗显示
		[Key(6)]
		public int WindowType;
		public int GetWindowType()
		{
			return WindowType;
		}
		//是否任务奖励
		[Key(7)]
		public int TaskAward;
		public int GetTaskAward()
		{
			return TaskAward;
		}
	}
}
