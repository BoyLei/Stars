using UnityEngine;
using ProtoMsg;
using SGF.Network;
public class TestSocket : MonoBehaviour
{
    // Start is called before the first frame update

    public CustomMsg customMsg = CustomMsg.Instance;





    void Start()
    {
        // DataType dt = new DataType();
        // byte[] buf = new byte[1024];

        // System.Type type = dt.GetType();



        // MessageParser parse = DataType.Parser;

        // // parse.ParseFrom(buf);

        // IPAddress iPAddress = IPAddress.Parse("10.191.72.37");
        // IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8080);



        NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.ClientVerifySucceedRet, OnClientVerifySucceedRetMsgID, this);

        NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.ClientVerifyFailedRet, OnClientVerifyFailedRetMsgID, this);

        NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.HeartBeat, onHeartBeatMsgID, this);

        NetworkManager.Instance.OnMessageCmd((int)CustomMsgID.BattleClientVerifyRet, OnBattleClientVerifyRetID, this);

        //NetworkManager.Instance.OnMessageEnum(MsgIDEnum.BattleServerInfoAckID, onBattleServerInfoAck, this);

        DontDestroyOnLoad(this);

    }

    void OnClientVerifySucceedRetMsgID(MessageHandleData data)
    {
        ClientVerifySucceedRet clientVerifySucceedRet = (ClientVerifySucceedRet)data.data;

        Debug.Log($"gameSocket  OnClientVerifySucceedRetMsgID to struct {clientVerifySucceedRet.PID}");

    }

    void OnClientVerifyFailedRetMsgID(MessageHandleData data)
    {

        ClientVerifyFailedRet clientVerifyFailedRet = (ClientVerifyFailedRet)data.data;

        Debug.Log($"gameSocket  OnClientVerifyFailedRetMsgID ");
    }

    void onHeartBeatMsgID(MessageHandleData data)
    {
        HeartBeat heartBeat = (HeartBeat)data.data;

        Debug.Log($"gameSocket  onHeartBeatMsgID ");
    }

    void OnBattleClientVerifyRetID(MessageHandleData data)
    {
        BattleClientVerifyRet battleClientVerifyRet = (BattleClientVerifyRet)data.data;


        Debug.Log($"gameSocket  OnBattleClientVerifyRetID to struct {battleClientVerifyRet.IsNew}");


        SocketBase battleSocket = NetworkManager.Instance.gameSocket;

        MoveMsg2 moveMsg2 = new MoveMsg2();
        ProtoMsg.Vector3 vector3 = new ProtoMsg.Vector3();
        vector3.X = 1;
        vector3.Y = 2;
        vector3.Z = 3;
        moveMsg2.IsStart = true;
        moveMsg2.Pos = vector3;
        moveMsg2.Rot = 126;
        moveMsg2.TimeStamp = 1111111111;
        // Debug.Log($"gameSocket  ======> SendEnumPbMsg MoveMsg2 ");

        // battleSocket.SendEnumPbMsg(MsgIDEnum.MoveMsg2ID, moveMsg2, false);



    }

    void onBattleServerInfoAck(MessageHandleData data)
    {
        //BattleServerInfoAck battleServerInfo = (BattleServerInfoAck)data.data;
        //Debug.Log($"gameSocket  onBattleServerInfoAck  MessageHandleData  socketName {data.socketName} battleServerInfo : {battleServerInfo.BattleAddr}");


        ////测试tcp 发送协议       

        //MatchBeginReq matchBeginReq = new MatchBeginReq();
        //matchBeginReq.LevelID = 11111;
        //matchBeginReq.MatchType = 22222;
        //matchBeginReq.IsAuto = true;
        //Debug.Log("=================");
        // NetworkManager.Instance.gameSocket.SendEnumPbMsg(MsgIDEnum.MatchBeginReqID, matchBeginReq, false);
        // NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.xxx, matchBeginReq, false);



        //测试两条tcp
        // SocketBase battleSocket = NetworkManager.Instance.battleSocket;
        // battleSocket.Connect(battleServerInfo.BattleAddr);

        // BattleClientVerifyReq battleClientVerifyReqID = new BattleClientVerifyReq();
        // battleClientVerifyReqID.PID = GameLoginInfo.M_ChooseHeroReq.PID;
        // battleClientVerifyReqID.Token = battleServerInfo.Token;
        // battleClientVerifyReqID.SessState = 1;  // SessState: 客户端登录时需要传送当前状态 1 新链接 2 断线重连

        // battleSocket.SendCustomMsg(CustomMsg.Instance.BattleClientVerifyReqID, battleClientVerifyReqID, false);

    }

    void Update()
    {
        //NetworkManager.Instance.update();
    }

}


