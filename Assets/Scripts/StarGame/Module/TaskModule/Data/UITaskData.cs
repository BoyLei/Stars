using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class UITaskData 
{
    public bool Fold;
    public string TypeName;
    public Dictionary<int, List<TaskInfo>> Maps = new Dictionary<int, List<TaskInfo>>();

    public void AddTask(TaskInfo taskInfo)
    {
        if(Maps.ContainsKey(taskInfo.Chapter))
        {
            Maps[taskInfo.Chapter].Add(taskInfo);
        }
        else
        {
            List<TaskInfo> list = new List<TaskInfo>();
            list.Add(taskInfo);
            Maps.Add(taskInfo.Chapter, list);
        }
    }

    public void Sort()
    {
        foreach (var item in Maps)
        {
            item.Value.Sort(SortTask);
        }
    }

    private int SortTask(TaskInfo task1, TaskInfo task2)
    {

        if (task1.Chapter < task2.Chapter)
        {
            return 1;
        }
        else if (task1.Chapter > task2.Chapter)
        {
            return -1;
        }
        else
        {
            if (task1.TaskID < task2.TaskID)
            {
                return -1;
            }
            else if (task1.TaskID > task2.TaskID)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

}
