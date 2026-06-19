//来源表TaskData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class TaskData
	{
		public Dictionary<int, TaskDataCell> StaticTaskDatas = new Dictionary<int, TaskDataCell>();
	}
	public class TaskDataCell
	{
		//任务id
		public string TaskId;
		private int _TaskId = -1;
		public int GetTaskId()
		{
			if (_TaskId == -1 && int.TryParse(TaskId, out _TaskId))
			{
			}
			return _TaskId;
		}
		//任务类型
		public string TaskType;
		private int _TaskType = -1;
		public int GetTaskType()
		{
			if (_TaskType == -1 && int.TryParse(TaskType, out _TaskType))
			{
			}
			return _TaskType;
		}
		//章节ID
		public string Chapter;
		private int _Chapter = -1;
		public int GetChapter()
		{
			if (_Chapter == -1 && int.TryParse(Chapter, out _Chapter))
			{
			}
			return _Chapter;
		}
		//章节名
		public string ChapterName;
		//任务周期
		public string Cycle;
		private int _Cycle = -1;
		public int GetCycle()
		{
			if (_Cycle == -1 && int.TryParse(Cycle, out _Cycle))
			{
			}
			return _Cycle;
		}
		//前置任务id
		public List<int> Pre_id = new List<int>();
		//等级要求
		public string PlayerLv;
		private int _PlayerLv = -1;
		public int GetPlayerLv()
		{
			if (_PlayerLv == -1 && int.TryParse(PlayerLv, out _PlayerLv))
			{
			}
			return _PlayerLv;
		}
		//接取条件
		public string Contion;
		private int _Contion = -1;
		public int GetContion()
		{
			if (_Contion == -1 && int.TryParse(Contion, out _Contion))
			{
			}
			return _Contion;
		}
		//承接NPC
		public List<int> Pick_Spawner = new List<int>();
		//交付NPC
		public List<int> Finish_Spawner = new List<int>();
		//任务标题
		public string Title;
		//任务描述
		public string Desc;
		//目标事件
		public List<int> Process_Event = new List<int>();
		//是否自动承接
		public string CanPick;
		public bool GetCanPick()
		{
			bool _CanPick;
			if (bool.TryParse(CanPick, out _CanPick))
			{
			}
			return _CanPick;
		}
		//是否自动提交
		public string CanFinish;
		public bool GetCanFinish()
		{
			bool _CanFinish;
			if (bool.TryParse(CanFinish, out _CanFinish))
			{
			}
			return _CanFinish;
		}
        //是否点击提交
        public string ClickFinish;
        public bool GetClickFinish()
        {
            bool _ClickFinish;
            if (bool.TryParse(ClickFinish, out _ClickFinish))
            {
            }
            return _ClickFinish;
        }
        //是否可放弃
        public string CanDelete ;
		public bool GetCanDelete ()
		{
			bool _CanDelete ;
			if (bool.TryParse(CanDelete , out _CanDelete ))
			{
			}
			return _CanDelete ;
		}
		//失败是否可重新接取
		public string CanAgain ;
		public bool GetCanAgain ()
		{
			bool _CanAgain ;
			if (bool.TryParse(CanAgain , out _CanAgain ))
			{
			}
			return _CanAgain ;
		}
		//是否有接取提示
		public string Get_Tips;
		public bool GetGet_Tips()
		{
			bool _Get_Tips;
			if (bool.TryParse(Get_Tips, out _Get_Tips))
			{
			}
			return _Get_Tips;
		}
		//是否有完成提示
		public string Finish_Tips;
		public bool GetFinish_Tips()
		{
			bool _Finish_Tips;
			if (bool.TryParse(Finish_Tips, out _Finish_Tips))
			{
			}
			return _Finish_Tips;
		}
		//任务限时
		public string Time;
		private int _Time = -1;
		public int GetTime()
		{
			if (_Time == -1 && int.TryParse(Time, out _Time))
			{
			}
			return _Time;
		}
		//临时获得
		public string Temporary;
		//奖励标签
		public string Tag;
		//任务奖励
		public string Drop;
		private int _Drop = -1;
		public int GetDrop()
		{
			if (_Drop == -1 && int.TryParse(Drop, out _Drop))
			{
			}
			return _Drop;
		}
		//即将获得
		public string Item;
		private int _Item = -1;
		public int GetItem()
		{
			if (_Item == -1 && int.TryParse(Item, out _Item))
			{
			}
			return _Item;
		}

	}
}
