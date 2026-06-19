using Google.Protobuf;
using ProtoMsg;
using SGF.Network;

public  interface MDMgrInterface
{
    void Init();

    IMessage GetMsg(string keyname);
    void Notify(FixMessageManager.FixMessageNotifyData datalist);

    IMessage Add(string keyname, DBDataModel data);

    void Update(string keyname);

    void ClearAllData();

    IMessage Del(string keyname);

    void Release();
}