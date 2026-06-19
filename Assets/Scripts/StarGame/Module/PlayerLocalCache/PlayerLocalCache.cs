///--------------------------------------------------------------------
/// 文件名   :   PlayerLocalCache.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/02/24 11:16:57
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.Module.Framework;
using SGF.Time;
using StarProject;
using StarProject.Service.User;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Module
{
    public class PlayerLocalCache : BusinessModule
    {
        /// <summary>
        /// 今日提醒
        /// </summary>
        private string NOTICE = "";

        public int last_notice = 0;

        private ulong playerRoleId;

        public TodayNoticeInfo todayNotice;
        public override void Create(object args = null)
        {
            GlobalEvent.OnRoleCreateComplete.AddListener(OnRoleCreateComplete);
        }

        private void OnRoleCreateComplete(object arg0)
        {
            playerRoleId = UserManager.Instance.MainUserData.playerRoleId;
            NOTICE = string.Format("today_notice_{0}", playerRoleId);
            if (KeyExists("today_notice", NOTICE))
            {
                todayNotice = Load<TodayNoticeInfo>("today_notice", NOTICE);
            }
        }

        /// <summary>
        /// 更新每日提醒
        /// </summary>
        public void SaveNotice(string key)
        {
            if (todayNotice == null)
            {
                todayNotice = new TodayNoticeInfo();
            }

            if (todayNotice.Maps.ContainsKey(key))
            {
                int time = todayNotice.Maps[key];
                if (time != TimeUtils.ServerNow.DayOfYear)
                {
                    time = TimeUtils.ServerNow.DayOfYear;
                    todayNotice.Maps[key] = time;
                    Save("today_notice", todayNotice, NOTICE);
                }
            }
            else
            {
                todayNotice.Maps.Add(key, TimeUtils.ServerNow.DayOfYear);
                Save("today_notice", todayNotice, NOTICE);
            }

        }

        /// <summary>
        /// 是否存在每日提醒
        /// </summary>
        /// <returns></returns>
        public bool HasNotice(string key)
        {
            if (todayNotice!=null && todayNotice.Maps.ContainsKey(key))
            {
                int time = todayNotice.Maps[key];

                if (time == TimeUtils.ServerNow.DayOfYear)
                {
                    return false;
                }
            }

            return true;
        }


        public override void Release()
        {

        }
    }


    public class TodayNoticeInfo
    {
        public Dictionary<string, int> Maps = new Dictionary<string, int>();
    }
}

