using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatteryTest
{
    /// <summary>
    /// 从机类
    /// </summary>
    class SlaveMachine
    {
        /// <summary>
        /// 从机IP
        /// </summary>
        public string ip;
        /// <summary>
        /// 从机是否在线
        /// </summary>
        public bool Online;
        /// <summary>
        /// 从机通道数量
        /// </summary>
        public int aisle;

        /// <summary>
        /// 从机序号
        /// </summary>
        public string SlaveMachineID;

        /// <summary>
        /// 发送接受到的数据
        /// </summary>
        private StringBuilder SendReceive;

        /// <summary>
        /// MAC地址
        /// </summary>
        private string Mac;

        /// <summary>
        /// 通信端口
        /// </summary>
        public int CommunicationPort;
        public SlaveMachine()
        {
            SendReceive = new StringBuilder();
            aisle = 0;
            Online = false;
        }

        /// <summary>
        /// 添加发送接受到的数据
        /// </summary>
        /// <param name="data"></param>
        public void addSendReceive(string data)
        {
            SendReceive.Append(data);

        }
        /// <summary>
        /// 返回发送接受到的数据
        /// </summary>
        /// <returns></returns>
        public string getSendReceive()
        {
            return SendReceive.ToString();
        }
    }
}
