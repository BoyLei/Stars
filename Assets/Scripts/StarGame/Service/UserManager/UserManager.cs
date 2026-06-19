using SGF.Module.Framework;
using StarProject.Service.UserManager.Data;
using StarProjectDef;

namespace StarProject.Service.User
{
    public class UserManager : ServiceModule<UserManager>
    {
        private UserData m_mainUserData;
        public UserData MainUserData { get { return m_mainUserData; } }

        public void Init()
        {
            CheckSingleton();
        }

        public void Clear()
        {
            m_mainUserData = null;
        }


        /// <summary>
        /// 通过登录等逻辑来更新用户数据
        /// </summary>
        /// <param name="data"></param>
        public void UpdateMainUserData(UserData data)
        {
            m_mainUserData = data;
            LocalCache.RoleCreate(data.playerRoleId);
        }

    }
}
