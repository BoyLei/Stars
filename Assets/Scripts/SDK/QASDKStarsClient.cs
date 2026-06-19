using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QASDK
{
    public class QASDK_StarsClient
    {

        private const string CUR_CLIENT = "galaxy_stars";

        public static QASDK_StarsClient instance;

        public Socket clientSocket;

        public static QASDK_StarsClient GetInstance()
        {
            if (instance == null)
                instance = new QASDK_StarsClient("10.225.136.96", 31470, 0);
            return instance;
        }


        public IPAddress IP;
        public IPEndPoint IPEndPoint;

        public QASDK_StarsClient(string hostName, int port, int i)
        {
            IP = IPAddress.Parse(hostName);
            IPEndPoint = new IPEndPoint(IP, port);

            // 开启一个新的协程来 初始化 qa的sdk socket, 从而 qa的 消息异常不会 暴露在 主线程中
            Thread t2 = new Thread(() => InitClientSocket()); ;
            t2.Start();



            // init msg




        }

        public void InitClientSocket()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(IPEndPoint);


                Dictionary<string, string> initBody = new Dictionary<string, string>();
                initBody["name"] = CUR_CLIENT;
                string initBodyJsonStr = JsonConvert.SerializeObject(initBody);
                byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(initBodyJsonStr);
                byte[] msgLenBytes = BitConverter.GetBytes(bodyBytes.Length);
                byte[] msgTypeBytes = System.Text.Encoding.UTF8.GetBytes("k");
                byte[] initBodyBlockBytes = new byte[5 + bodyBytes.Length];
                Array.Copy(msgTypeBytes, 0, initBodyBlockBytes, 0, msgTypeBytes.Length);
                Array.Copy(msgLenBytes, 0, initBodyBlockBytes, msgTypeBytes.Length, msgLenBytes.Length);
                Array.Copy(bodyBytes, 0, initBodyBlockBytes, msgTypeBytes.Length + msgLenBytes.Length, bodyBytes.Length);
                this.clientSocket.Send(initBodyBlockBytes);

                Thread t = new Thread(() => RecvData(clientSocket));
                t.Start();


                while (true)
                {
                    while (msgActionQueue.Count > 0)
                    {
                        var msgAction = msgActionQueue.Dequeue();
                        if (msgAction != null)
                        {
                            msgAction.Invoke();
                        }
                    }

                }
            }
            catch (System.Exception e)
            {
                SGF.Debuger.Log($"[QASDK] connect 异常: {e.Message}");
            }
        }



        Queue<Action> msgActionQueue = new Queue<Action>();
        public void PushQAProtoData(string msgType, int opcode, string msg, string protoName)
        {
            msgActionQueue.Enqueue(() =>
            {
                sendMsg(msgType, opcode, msg, protoName);
            });
        }

        public void sendMsg(string msgType, int opcode, string msg, string protoName)
        {
            try
            {
                JObject jobj = JsonConvert.DeserializeObject<JObject>(msg);
                jobj["protoName"] = protoName;
                msg = JsonConvert.SerializeObject(jobj);
                if (!msgType.Equals("m") && !msgType.Equals("n"))
                {
                    Console.WriteLine("Unsupported message types! message type only support 'm' or 'n'!");
                    return;
                }

                if (String.IsNullOrEmpty(msg))
                {
                    Console.WriteLine("Empty msg! opcode: " + opcode);
                    return;
                }

                if (String.IsNullOrEmpty(protoName))
                {
                    Console.WriteLine("Invalid protoName! opcode: " + opcode);
                    return;
                }
                // 发送 sendData 1 type + 4 len + 4 opcode + msg
                byte[] msgTypeBytes = Encoding.UTF8.GetBytes(msgType);
                byte[] bodyBytes = Encoding.UTF8.GetBytes(msg);
                byte[] msgLenBytes = BitConverter.GetBytes(bodyBytes.Length);
                byte[] opcodeBytes = BitConverter.GetBytes(opcode);
                byte[] blockBytes = new byte[1 + 4 + 4 + bodyBytes.Length];
                Array.Copy(msgTypeBytes, 0, blockBytes, 0, msgTypeBytes.Length);
                Array.Copy(msgLenBytes, 0, blockBytes, msgTypeBytes.Length, msgLenBytes.Length);
                Array.Copy(opcodeBytes, 0, blockBytes, msgTypeBytes.Length + msgLenBytes.Length, opcodeBytes.Length);
                Array.Copy(bodyBytes, 0, blockBytes, msgTypeBytes.Length + msgLenBytes.Length + opcodeBytes.Length, bodyBytes.Length);
                this.clientSocket.Send(blockBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! send msg err! ex msg: " + e.Message);
            }
        }


        private void RecvData(Socket clientSocket)
        {
            while (true)
            {
                byte[] msgTypeBytes = new byte[1];
                int length = clientSocket.Receive(msgTypeBytes);
                string msgType = Encoding.UTF8.GetString(msgTypeBytes);
                if (msgType.Equals("p"))
                {
                    Console.WriteLine("ping msg");
                }
                else if (msgType.Equals("d"))
                {
                    // proto editor msg
                    byte[] msgLenBytes = new byte[4];
                    clientSocket.Receive(msgLenBytes);
                    byte[] opcodeBytes = new byte[4];
                    clientSocket.Receive(opcodeBytes);
                    int msgLen = BitConverter.ToInt32(msgLenBytes, 0);
                    if (msgLen > 0)
                    {
                        byte[] sendDataBytes = new byte[msgLen];
                        clientSocket.Receive(sendDataBytes);
                        string sendData = Encoding.UTF8.GetString(sendDataBytes);
                        // Console.WriteLine("send data: " + sendData);
                        // TODO 此处调用开发发送协议方法
                        // 方法示例 public static void sendProto(int opcode, string jsonStr)
                        QASDKMsgUtils.SendQAMsg2Server(sendData);

                    }
                    else
                    {
                        Console.WriteLine("WARN! empty msg. opcode: " + BitConverter.ToInt32(opcodeBytes, 0));
                    }
                }
            }
        }

    }
}