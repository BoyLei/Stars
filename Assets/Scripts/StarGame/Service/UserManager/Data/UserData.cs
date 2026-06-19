using ProtoBuf;

namespace StarProject.Service.UserManager.Data
{
    /// <summary>
    /// 用户数据
    /// 使用ProtoBuf进行序列化
    /// </summary>
    [ProtoContract]
    public class UserData
    {
        public ulong uuid;//通用唯一识别码（Universally Unique Identifier
        public ulong uid;//SDK
        public ulong accountID;//账号id
        [ProtoMember(1)]
        public ulong playerRoleId;//角色id。实体ld不在这里是battle里面的 （DBID）  // 这个是PID
        [ProtoMember(2)]
        public string name;//用户名字



        public uint groupID; // 小区ID

        public string groupName;    // 小区名字
        //[ProtoMember(3)]
        //public int level;//用户等级
        //[ProtoMember(4)]
        //public int defaultAvatarId;//用户的Avatar的ID

    }
}
