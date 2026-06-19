//来源表交互物件_Interact.xlsm.xlsx -> sheet:Interact
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class InteractData
	{
		[Key(0)]
		public Dictionary<long, InteractDataCell> StaticInteractDatas = new Dictionary<long, InteractDataCell>();
	}
	[MessagePackObject]
	public class InteractDataCell
	{
		//交互ID
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//化身ID
		[Key(1)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//关联矿物id
		[Key(2)]
		public long MineID;
		public long GetMineID()
		{
			return MineID;
		}
		//物件名称
		[Key(3)]
		public string ModelName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ModelName); }
			set { _ModelName = value; }
        }
		[IgnoreMember]
		private string _ModelName;
		//物件对话框名字
		[Key(4)]
		public string ModeltalkeName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ModeltalkeName); }
			set { _ModeltalkeName = value; }
        }
		[IgnoreMember]
		private string _ModeltalkeName;
		//称谓
		[Key(5)]
		public string Title
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Title); }
			set { _Title = value; }
        }
		[IgnoreMember]
		private string _Title;
		//物件类型
		[Key(6)]
		public int ObjectType;
		public int GetObjectType()
		{
			return ObjectType;
		}
		//是否显示任务状态
		[Key(7)]
		public bool IsShowTask;
		public bool GetIsShowTask()
		{
			return IsShowTask;
		}
		//触发范围
		[Key(8)]
		public int TriggerRange;
		public int GetTriggerRange()
		{
			return TriggerRange;
		}
		//交互时间
		[Key(9)]
		public int InterTime;
		public int GetInterTime()
		{
			return InterTime;
		}
		//是否可被打断
		[Key(10)]
		public bool CanBreak;
		public bool GetCanBreak()
		{
			return CanBreak;
		}
		//交互类型
		[Key(11)]
		public int InterType;
		public int GetInterType()
		{
			return InterType;
		}
		//交互需要人数
		[Key(12)]
		public int NeedNum;
		public int GetNeedNum()
		{
			return NeedNum;
		}
		//交互次数
		[Key(13)]
		public int Count;
		public int GetCount()
		{
			return Count;
		}
		//条件组ID
		[Key(14)]
		public int ConditionID;
		public int GetConditionID()
		{
			return ConditionID;
		}
		//服务ID
		[Key(15)]
		public int EffectID;
		public int GetEffectID()
		{
			return EffectID;
		}
		//消耗ID
		[Key(16)]
		public int CostID;
		public int GetCostID()
		{
			return CostID;
		}
		//交互后物件留存时间
		[Key(17)]
		public int DelayTime;
		public int GetDelayTime()
		{
			return DelayTime;
		}
		//交互物模型缩放
		[Key(18)]
		public int ModelScaling;
		public int GetModelScaling()
		{
			return ModelScaling;
		}
		//交互物外发光特效
		[Key(19)]
		public int SpecialEffect;
		public int GetSpecialEffect()
		{
			return SpecialEffect;
		}
		//特效地址
		[Key(20)]
		public string EffectAddress;
		//任务特效地址
		[Key(21)]
		public string TaskEffectAddress;
		//玩家交互动作
		[Key(22)]
		public string Action;
		//资源组ID
		[Key(23)]
		public int AssetIndex;
		public int GetAssetIndex()
		{
			return AssetIndex;
		}
		//交互物初始动作
		[Key(24)]
		public string DefaultIdle;
		//交互物结束动作
		[Key(25)]
		public string EndinterIdle;
		//按钮图标id
		[Key(26)]
		public string Icon;
		//是否自动交互
		[Key(27)]
		public int IsAutoInter;
		public int GetIsAutoInter()
		{
			return IsAutoInter;
		}
		//是否显示读条
		[Key(28)]
		public bool IsShowbar;
		public bool GetIsShowbar()
		{
			return IsShowbar;
		}
		//交互提示文字
		[Key(29)]
		public string InterContent
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_InterContent); }
			set { _InterContent = value; }
        }
		[IgnoreMember]
		private string _InterContent;
		//是否弹出消耗提示框界面
		[Key(30)]
		public bool IsShowPanel;
		public bool GetIsShowPanel()
		{
			return IsShowPanel;
		}
		//是否有每日消耗提醒
		[Key(31)]
		public bool IsNotice;
		public bool GetIsNotice()
		{
			return IsNotice;
		}
		//显隐类型
		[Key(32)]
		public int ShowType;
		public int GetShowType()
		{
			return ShowType;
		}
		//显隐条件
		[Key(33)]
		public int ShowCondition;
		public int GetShowCondition()
		{
			return ShowCondition;
		}
		//是否初始显示
		[Key(34)]
		public bool IsShow;
		public bool GetIsShow()
		{
			return IsShow;
		}
		//创建后销毁时间（秒）
		[Key(35)]
		public long CreatedBETime;
		public long GetCreatedBETime()
		{
			return CreatedBETime;
		}
	}
}
