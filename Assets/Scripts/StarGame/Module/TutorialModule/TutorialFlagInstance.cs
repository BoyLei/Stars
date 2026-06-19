using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class TutorialFlagInstance
    {
        public int Flag { get; private set; }

        public uint TaskID{ get; private set; }

        public  uint MapID{ get; private set; }
        public List<int> Ids = new List<int>();
        
        public TutorialFlagInstance(int flag, uint taskID,uint mapid)
        {
            Ids.Clear();
            Flag=flag;
            TaskID=taskID;
            MapID=mapid;
        }

        public void AppendTutorialID(int id)
        {
            if (!Ids.Contains(id))
            {
                Ids.Add(id);
            }

        }
        public void OnTaskFinish(uint taskID,System.Action<int,int> finishAction)
        {
            if (taskID == TaskID)
            {
                //查询当前进行的引导是否完成，如若未完成则结束引导，在将标记置为1
                foreach (var item in Ids)
                {
                    finishAction?.Invoke(item,Flag);
                }
            }
        }

        public void OnClickTaskItem(uint taskID, System.Action<int> beginAction)
        {
            if (taskID == TaskID)
            {
                //查询当前进行的引导是否完成，如若未完成则结束引导，在将标记置为1
                foreach (var item in Ids)
                {
                    beginAction?.Invoke(item);
                }
               
            }
        }

        public void OnExitScene(uint SceneID,System.Action<int,int> finishAction)
        {
            if (MapID == SceneID)
            {
                foreach (var item in Ids)
                {
                    finishAction?.Invoke(item,Flag);
                }
            }
        }
    }
}