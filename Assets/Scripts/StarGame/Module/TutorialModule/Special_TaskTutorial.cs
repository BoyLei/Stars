///--------------------------------------------------------------------
/// 文件名   :   Special_TaskTutorial.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/05/21 09:53:41
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using ProtoMsg;
using SGF.Unity;
using StarProject;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using Task;
using UnityEngine;

public class Special_TaskTutorial 
{
    /// <summary>
    /// 是否需要检测
    /// </summary>
    public bool NeedCheck { get; private set; }
    
    /// <summary>
    /// 是否触发
    /// </summary>
    public  bool IsTrigger { get; private set; }

    private float m_MainTaskFinishTime;
    
    public void Init()
    {
        NeedCheck = false;
        IsTrigger = false;
        m_MainTaskFinishTime = Time.time;
        if (!TaskHelper.IsTaskFinsh(SystemConstConfigs.Guidance_NoInputHLEndTask))
        {
            NeedCheck = true;
            MonoHelper.AddUpdateListener(OnUpdate);
        }
        
        GlobalEvent.TaskStageChange.AddListener(OnTaskStatechange);
    }



    private void OnTaskStatechange(int type, int id)
    {
        //任务完成
        if (type == 3)
        {
            var  taskConfig = TaskHelper.GetTaskConfig((uint)id);
            if (taskConfig != null)
            {
                //完成主线任务
                if (taskConfig.Base.TaskType == TaskClassifyType.MainLine)
                {
                    m_MainTaskFinishTime = Time.time;
                    IsTrigger = false;
                }
            }
            if (id == SystemConstConfigs.Guidance_NoInputHLEndTask)
            {
                NeedCheck = false;
            }
        }
    }

    private void OnUpdate()
    {
        if (NeedCheck)
        {
            if (!IsTrigger)
            {
                if (Time.time - m_MainTaskFinishTime >= SystemConstConfigs.Guidance_NoCompTaskHLInterval)
                {
                    GlobalEvent.OnShowGuidanceMainTask.Invoke(1);
                    IsTrigger = true;
                }
            }
        }
    }


    public void OnRelease()
    {
        MonoHelper.RemoveUpdateListener(OnUpdate);
        GlobalEvent.TaskStageChange.RemoveListener(OnTaskStatechange);
    }
}
