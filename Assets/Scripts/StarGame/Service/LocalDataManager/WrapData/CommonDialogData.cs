//来源表通用对话配置表_Common_Dialog.xlsm.xlsx -> sheet:CommonDialog
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CommonDialogData
	{
		[Key(0)]
		public Dictionary<int, CommonDialogDataCell> StaticCommonDialogDatas = new Dictionary<int, CommonDialogDataCell>();
	}
	[MessagePackObject]
	public class CommonDialogDataCell
	{
		//对话id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//对话类型
		[Key(1)]
		public int Dialog_type;
		public int GetDialog_type()
		{
			return Dialog_type;
		}
		//下一句id
		[Key(2)]
		public List<int> Next_dialog_id = new List<int>();
		//选项显隐条件
		[Key(3)]
		public List<int> Select_condition = new List<int>();
		//拦截
		[Key(4)]
		public int Loop;
		public int GetLoop()
		{
			return Loop;
		}
		//选项类型
		[Key(5)]
		public int Select_type;
		public int GetSelect_type()
		{
			return Select_type;
		}
		//对话者形象
		[Key(6)]
		public string Spine_id;
		//表情
		[Key(7)]
		public int Face_id;
		public int GetFace_id()
		{
			return Face_id;
		}
		//对话者位置
		[Key(8)]
		public string Site;
		//坐标偏移
		[Key(9)]
		public string Offset;
		//对话者名字多语言Key
		[Key(10)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//对话内容多语言Key
		[Key(11)]
		public string Dialog_desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Dialog_desc); }
			set { _Dialog_desc = value; }
        }
		[IgnoreMember]
		private string _Dialog_desc;
		//对话选项多语言Key
		[Key(12)]
		public string Dialog_option
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Dialog_option); }
			set { _Dialog_option = value; }
        }
		[IgnoreMember]
		private string _Dialog_option;
		//语音id
		[Key(13)]
		public string Voice_id;
		//插入图片
		[Key(14)]
		public string Image;
		//是否跳过
		[Key(15)]
		public int Is_skip;
		public int GetIs_skip()
		{
			return Is_skip;
		}
		//显示时间
		[Key(16)]
		public int Show_time;
		public int GetShow_time()
		{
			return Show_time;
		}
		//对话效果
		[Key(17)]
		public List<int> Common_effect = new List<int>();
		//背景图片
		[Key(18)]
		public string Dialog_background;
		//对话延迟显示时间
		[Key(19)]
		public int Delay_time;
		public int GetDelay_time()
		{
			return Delay_time;
		}
	}
}
