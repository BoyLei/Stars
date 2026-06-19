//来源表TutorialData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class TutorialData
	{
		public Dictionary<int, TutorialDataCell> StaticTutorialDatas = new Dictionary<int, TutorialDataCell>();
	}
	public class TutorialDataCell
	{
		//编号
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//引导名
		public string Name;
		//引导组
		public string GroupID;
		private int _GroupID = -1;
		public int GetGroupID()
		{
			if (_GroupID == -1 && int.TryParse(GroupID, out _GroupID))
			{
			}
			return _GroupID;
		}
		//触发条件1类型
		public string TriggerType1;
		private int _TriggerType1 = -1;
		public int GetTriggerType1()
		{
			if (_TriggerType1 == -1 && int.TryParse(TriggerType1, out _TriggerType1))
			{
			}
			return _TriggerType1;
		}
		//触发条件1参数1
		public string TriggerParam1_1;
		//触发条件1参数2
		public string TriggerParam1_2;
		//触发条件2类型
		public string TriggerType2;
		private int _TriggerType2 = -1;
		public int GetTriggerType2()
		{
			if (_TriggerType2 == -1 && int.TryParse(TriggerType2, out _TriggerType2))
			{
			}
			return _TriggerType2;
		}
		//触发条件2参数1
		public string TriggerParam2_1;
		//触发条件2参数2
		public string TriggerParam2_2;
		//触发条件3类型
		public string TriggerType3;
		private int _TriggerType3 = -1;
		public int GetTriggerType3()
		{
			if (_TriggerType3 == -1 && int.TryParse(TriggerType3, out _TriggerType3))
			{
			}
			return _TriggerType3;
		}
		//触发条件3参数1
		public string TriggerParam3_1;
		//触发条件3参数2
		public string TriggerParam3_2;
		//触发条件4类型
		public string TriggerType4;
		private int _TriggerType4 = -1;
		public int GetTriggerType4()
		{
			if (_TriggerType4 == -1 && int.TryParse(TriggerType4, out _TriggerType4))
			{
			}
			return _TriggerType4;
		}
		//触发条件4参数1
		public string TriggerParam4_1;
		//触发条件4参数2
		public string TriggerParam4_2;
		//引导行为类型
		public string BehaviorType;
		private int _BehaviorType = -1;
		public int GetBehaviorType()
		{
			if (_BehaviorType == -1 && int.TryParse(BehaviorType, out _BehaviorType))
			{
			}
			return _BehaviorType;
		}
		//引导参数
		public string BehaviorParam;
		//手势动作
		public string FingerType;
		private int _FingerType = -1;
		public int GetFingerType()
		{
			if (_FingerType == -1 && int.TryParse(FingerType, out _FingerType))
			{
			}
			return _FingerType;
		}
		//手指旋转偏移量
		public string FingerRotateOffset;
		//引导对象类型1
		public string ObjectType1;
		private int _ObjectType1 = -1;
		public int GetObjectType1()
		{
			if (_ObjectType1 == -1 && int.TryParse(ObjectType1, out _ObjectType1))
			{
			}
			return _ObjectType1;
		}
		//引导对象1参数1
		public string Object1Param1;
		//引导对象1参数2
		public string Object1Param2;
		//手指1偏移量
		public string Finger1Offset;
		//引导对象类型2
		public string ObjectType2;
		private int _ObjectType2 = -1;
		public int GetObjectType2()
		{
			if (_ObjectType2 == -1 && int.TryParse(ObjectType2, out _ObjectType2))
			{
			}
			return _ObjectType2;
		}
		//引导对象2参数1
		public string Object2Param1;
		//引导对象2参数2
		public string Object2Param2;
		//手指2偏移量
		public string Finger2Offset;
		//手指拖动时长
		public string FingerSiderTime;
		private int _FingerSiderTime = -1;
		public int GetFingerSiderTime()
		{
			if (_FingerSiderTime == -1 && int.TryParse(FingerSiderTime, out _FingerSiderTime))
			{
			}
			return _FingerSiderTime;
		}
		//是否显示黑底
		public string ShowMask;
		public bool GetShowMask()
		{
			bool _ShowMask;
			if (bool.TryParse(ShowMask, out _ShowMask))
			{
			}
			return _ShowMask;
		}
		//黑底透明度
		public string MaskAlpha;
		private int _MaskAlpha = -1;
		public int GetMaskAlpha()
		{
			if (_MaskAlpha == -1 && int.TryParse(MaskAlpha, out _MaskAlpha))
			{
			}
			return _MaskAlpha;
		}
		//黑底长宽
		public List<int> MaskArea = new List<int>();
		//是否缩圈
		public string Cycle_Fx;
		public bool GetCycle_Fx()
		{
			bool _Cycle_Fx;
			if (bool.TryParse(Cycle_Fx, out _Cycle_Fx))
			{
			}
			return _Cycle_Fx;
		}
		//文本框偏移量
		public List<int> TextOffset = new List<int>();
		//文本头像id
		public string TextRoleID;
		private int _TextRoleID = -1;
		public int GetTextRoleID()
		{
			if (_TextRoleID == -1 && int.TryParse(TextRoleID, out _TextRoleID))
			{
			}
			return _TextRoleID;
		}
		//文字
		public string TipText;
		//语音
		public string Voice;
		private int _Voice = -1;
		public int GetVoice()
		{
			if (_Voice == -1 && int.TryParse(Voice, out _Voice))
			{
			}
			return _Voice;
		}
		//循环时间
		public string CyclicTime;
		private int _CyclicTime = -1;
		public int GetCyclicTime()
		{
			if (_CyclicTime == -1 && int.TryParse(CyclicTime, out _CyclicTime))
			{
			}
			return _CyclicTime;
		}
		//中断是否从头引导
		public string IsReenter;
		public bool GetIsReenter()
		{
			bool _IsReenter;
			if (bool.TryParse(IsReenter, out _IsReenter))
			{
			}
			return _IsReenter;
		}
		//完成条件类型
		public string CompleteType;
		private int _CompleteType = -1;
		public int GetCompleteType()
		{
			if (_CompleteType == -1 && int.TryParse(CompleteType, out _CompleteType))
			{
			}
			return _CompleteType;
		}
		//完成条件参数1
		public string CompleteParam1;
		//完成条件参数2
		public string CompleteParam2;
		//完成条件参数3
		public string CompleteParam3;

	}
}
