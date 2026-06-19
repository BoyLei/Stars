//来源表Npc.xlsm.xlsx -> sheet:Npc
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class NpcData
	{
		[Key(0)]
		public Dictionary<long, NpcDataCell> StaticNpcDatas = new Dictionary<long, NpcDataCell>();
	}
	[MessagePackObject]
	public class NpcDataCell
	{
		//序号
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//名称
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//称谓
		[Key(2)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//触发范围
		[Key(3)]
		public int TriggerRange;
		public int GetTriggerRange()
		{
			return TriggerRange;
		}
		//类型
		[Key(4)]
		public int NpcType;
		public int GetNpcType()
		{
			return NpcType;
		}
		//等级
		[Key(5)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//是否可交互
		[Key(6)]
		public bool IsInteractive;
		public bool GetIsInteractive()
		{
			return IsInteractive;
		}
		//化身ID
		[Key(7)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//初始动作
		[Key(8)]
		public string StartIdle;
		//立绘
		[Key(9)]
		public int Drawing;
		public int GetDrawing()
		{
			return Drawing;
		}
		//头像
		[Key(10)]
		public int Head;
		public int GetHead()
		{
			return Head;
		}
		//地图显示图标
		[Key(11)]
		public string MapLogo;
		//开场白
		[Key(12)]
		public List<int> Dialog = new List<int>();
		//随机开场白库
		[Key(13)]
		public List<int> DialogGroup = new List<int>();
		//服务条件
		[Key(14)]
		public List<int> CommonCondition = new List<int>();
		//服务效果
		[Key(15)]
		public List<int> CommonEffect = new List<int>();
		//显隐类型
		[Key(16)]
		public int ShowType;
		public int GetShowType()
		{
			return ShowType;
		}
		//显隐条件
		[Key(17)]
		public int ShowCondition;
		public int GetShowCondition()
		{
			return ShowCondition;
		}
		//慢速移动
		[Key(18)]
		public int WalkSpeed;
		public int GetWalkSpeed()
		{
			return WalkSpeed;
		}
		//快速移动
		[Key(19)]
		public int RunSpeed;
		public int GetRunSpeed()
		{
			return RunSpeed;
		}
		//是否初始显示
		[Key(20)]
		public bool IsShow;
		public bool GetIsShow()
		{
			return IsShow;
		}
		//初始shader
		[Key(21)]
		public int StartShader;
		public int GetStartShader()
		{
			return StartShader;
		}
		//是否对话可转向
		[Key(22)]
		public int IsVeer;
		public int GetIsVeer()
		{
			return IsVeer;
		}
		//消失表现
		[Key(23)]
		public int DisappearShow;
		public int GetDisappearShow()
		{
			return DisappearShow;
		}
	}
}
