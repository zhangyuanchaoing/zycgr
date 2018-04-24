using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BatteryTest
{ 
    // 定义 UdpState类
    public class UdpState
    {
        public UdpClient udpClient = null;
        public IPEndPoint ipEndPoint = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public int counter = 0;
    }

    /// <summary>
    /// udp操作类
    /// </summary>
    class AsyncUdpClient
    {
        /// <summary>定义本机端口</summary>
        public int listenPort = 1101;
        /// <summary>定义远程端口</summary>
        public int remotePort = 1100;
        /// <summary>定义节点</summary> 
        private IPEndPoint ipEndPoint = null;
        private IPEndPoint remoteEP = null;
        /// <summary>定义UDP发送和接收</summary> 
        private UdpClient udpReceive = null;
        /// <summary>定义UDP发送和接收</summary> 
        private UdpClient udpSend = null;
        private UdpState udpSendState = null;
        private UdpState udpReceiveState = null;
        private int counter = 0;
        /// <summary> 异步状态同步</summary> 
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        /// <summary> 异步状态同步</summary> 
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        // 定义套接字
        //private Socket receiveSocket;
        //private Socket sendSocket;

        public AsyncUdpClient()
        {
            // 本机节点
            ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            // 远程节点
            remoteEP = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[0], remotePort);
            // 实例化
            udpReceive = new UdpClient(ipEndPoint);
            udpSend = new UdpClient();

            // 分别实例化udpSendState、udpReceiveState
            udpReceiveState = new UdpState();
            udpReceiveState.udpClient = udpReceive;
            udpReceiveState.ipEndPoint = ipEndPoint;

            udpSendState = new UdpState();
            udpSendState.udpClient = udpSend;
            udpSendState.ipEndPoint = remoteEP;
        }
        public void ReceiveMsg()
        {
            Console.WriteLine("listening for messages");
            while (true)
            {
                lock (this)
                {
                    // 调用接收回调函数
                    IAsyncResult iar = udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
                    receiveDone.WaitOne();
                    Thread.Sleep(100);
                }
            }
        }
        // 接收回调函数
        private void ReceiveCallback(IAsyncResult iar)
        {
            UdpState udpReceiveState = iar.AsyncState as UdpState;
            if (iar.IsCompleted)
            {
                Byte[] receiveBytes = udpReceiveState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);
                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("Received: {0}", receiveString);
                //Thread.Sleep(100);
                receiveDone.Set();
                //SendMsg();
            }
        }
        // 发送函数
        private void SendMsg()
        {
            udpSend.Connect(udpSendState.ipEndPoint);
            udpSendState.udpClient = udpSend;
            udpSendState.counter++;

            string message = string.Format("第{0}个UDP请求处理完成！", udpSendState.counter);
            Byte[] sendBytes = Encoding.Unicode.GetBytes(message);
            udpSend.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallback), udpSendState);
            sendDone.WaitOne();
        }
        // 发送回调函数
        private void SendCallback(IAsyncResult iar)
        {
            UdpState udpState = iar.AsyncState as UdpState;
            //Console.WriteLine("第{0}个请求处理完毕！", udpState.counter);
            //Console.WriteLine("number of bytes sent: {0}", udpState.udpClient.EndSend(iar));
            udpState.udpClient.EndSend(iar);
            sendDone.Set();
        }
        //使用方法
        /*
         AsyncUdpSever aus = new AsyncUdpSever();
            Thread t = new Thread(new ThreadStart(aus.ReceiveMsg));
            t.Start();
            Console.Read();

         */
    }
}
