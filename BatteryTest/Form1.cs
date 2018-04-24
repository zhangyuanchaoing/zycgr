using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;

namespace BatteryTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static Form1 Instance=null;
        string mac = "";
        string ip = "";
        string ipd = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            Instance = this;
            for (int i = 1; i < 101; i++)
            {
                comboBox1.Items.Add(i);
            }
            List<string> macs = GetMacByNetworkInterface();
            if (macs != null && macs.Count >= 1)
            {

                mac = macs[0].ToString();
            }
            textBox1.Tag = "0";
            ip = GetIpAddress();
            ipd = jisuanIPQZ(ip);
            textBox4.Text = ip;
            textBox5.Text = mac;

        }


        // 返回描述本地计算机上的网络接口的对象(网络接口也称为网络适配器)。
        public static NetworkInterface[] NetCardInfo()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }


        private string GetIpAddress()
        {
            string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry localhost = Dns.GetHostByName(hostName);    //方法已过期，可以获取IPv4的地址

            //IPHostEntry localhost = Dns.GetHostEntry(hostName);   //获取IPv6地址
            int i = 0;
            for (; i < localhost.AddressList.Length; i++)
            {
                if(localhost.AddressList[i].ToString() == "192.168.128.254")
                    break;
            }
            if (i >= localhost.AddressList.Length) i = 0;
            IPAddress localaddr = localhost.AddressList[i];

            return localaddr.ToString();
        }

        ///<summary>
        /// 通过NetworkInterface读取网卡Mac
        ///</summary>
        ///<returns></returns>
        public static List<string> GetMacByNetworkInterface()
        {
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                macs.Add(ni.GetPhysicalAddress().ToString());
            }
            return macs;
        }
        public string jisuanIPQZ(string ip)
        {
            string[] s = ip.Split('.');
            int count=0;
            for (int i = 0; i < s.Length-1; i++)
            {
                count += s[i].Length + 1;
            }
            if (count == 0) return "";
            return ip.Substring(0,count);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int count = Int32.Parse(comboBox1.Text);
                if (count > 250 || count <= 0)
                {
                    MessageBox.Show("请输入合理的值");
                    return;
                }
                treeView1.Nodes.Clear();
                sms = new SlaveMachine[count+1];
                SlaveMachine sm = new SlaveMachine();
                sm.ip = "所有设备";

                sms[0] = sm;
                for (int i = 1; i <= count; i++)
                {
                    SlaveMachine sm2 = new SlaveMachine();
                    sm2.ip = ipd + i;
                    sms[i] = sm2;
                }

                for (int i = 0; i <= count; i++)
                {
                    TreeNode tn = new TreeNode();
                    tn.Text = sms[i].ip;
                    tn.Tag = sms[i];
                    treeView1.Nodes.Add(tn);
                }
               
            }
            catch (Exception err)
            {
                MessageBox.Show("请输入正确的值"); ;
            }
        }

        //StringBuilder sb = new StringBuilder();
        //StringBuilder[] sbs = null;
        SlaveMachine[] sms = null; 
        Thread TD = null;
        private void button3_Click(object sender, EventArgs e)
        {
            TD = new Thread(ml123);
            TD.Start();
        }

        string ml1 = "A5 5A C0 A8 80 00 FF FF FF 00 70 58 12 E1 A1 E4";//A5 5A+192 168 128 0+ 255 255 255 0 +本机MAC 地址
        public void ml123()
        {
            /*
             命令
             1.1 探查从机情况
             各从机返回 5A A5 IP   和从机发送MAC

             1.2主机端口随机变化，从机端口变为1642 （初始化完成后端口就不在变化）
             主机发01 00 00 63（对应的从机IP 为192.168.128.1 及对应MAC。）01表示从机1，63为命令字符，目地是查询从机可测试通道数。
             */
            try
            {
                ml1 = "A55AC0A88000FFFFFF00" + mac;

                UDPCZClass udpcz = new UDPCZClass(30001);
                udpcz.MonitorUDP(ip,30001);
                udpcz.SendUDP(ml1, "255.255.255.255", 1643);
                Thread.Sleep(2000);
                udpcz.SendUDP(ml1, "255.255.255.255", 1643);
                
               


                //IPAddress HostIP = new IPAddress(ipStrToLong("192.168.128.254"));
                //IPEndPoint host = new IPEndPoint(HostIP, Int32.Parse("30001"));
                //UdpClient udp = new UdpClient(Int32.Parse("1643"));
                //byte[] bts = strToToHexByte(textBox3.Text);
                //udp.Send(bts, bts.Length, host);
                //udp.Close();
                Thread.Sleep(2000);
                udpcz.ClostMonitorUDP();
                aisleCount();
            }
            catch (Exception err)
            {
                MessageBox.Show("出错了:" + err.Message);
            }
        }

        //第二段命令 确定各个从机通道
        public void aisleCount()
        {
            int udpdk = 5190;
            int cjxh = 1;
            //获取从机通道数量
            for (int i = 1; i < sms.Length; i++)
            {
                if (sms[i].Online)
                {
                    receive1642s = false;
                    sms[i].SlaveMachineID = ("" + cjxh).PadLeft(2, '0');
                    UDPCZClass udpcz = new UDPCZClass(udpdk);
                    sms[i].CommunicationPort = udpdk;
                    udpcz.AddReceiveData += new UDPCZClass.AddReceiveDataHandler(ReceiveData1642);
                    udpcz.MonitorUDP(ip, udpdk);
                    udpcz.SendUDP(sms[i].SlaveMachineID + "000063", sms[i].ip, 1642);
                    cjxh++;
                    for (int k = 0; k < 1000; k++)
                    {
                        if (receive1642s) break;
                        Thread.Sleep(10);
                    }
                    udpcz.ClostMonitorUDP();

                }
            }


            //辅助通道分配情况

            for (int i = 1; i < sms.Length; i++)
            {
                if (sms[i].Online)
                {
                    receive1642s = false;
                    UDPCZClass udpcz = new UDPCZClass(sms[i].CommunicationPort);
                    udpcz.MonitorUDP(ip, udpdk);
                    for (int k = 1; k <= sms[i].aisle; k++)
                    {
                        receive1642s = false;
                        string td = ("" + k).PadLeft(2, '0');
                        udpcz.SendUDP(sms[i].SlaveMachineID  + td + "006D", sms[i].ip, 1642);
                        for (int kx = 0; kx < 100; kx++)
                        {
                            if (receive1642s) break;
                            Thread.Sleep(10);
                        }
                        receive1642s = false;
                        udpcz.SendUDP(sms[i].SlaveMachineID + td + "0044", sms[i].ip, 1642);
                        for (int kx = 0; kx < 100; kx++)
                        {
                            if (receive1642s) break;
                            Thread.Sleep(10);
                        }
                        receive1642s = false;
                        udpcz.SendUDP(sms[i].SlaveMachineID + td + "4344", sms[i].ip, 1642);
                        for (int kx = 0; kx < 100; kx++)
                        {
                            if (receive1642s) break;
                            Thread.Sleep(10);
                        }
                        receive1642s = false;
                        udpcz.SendUDP(sms[i].SlaveMachineID + td + "0049", sms[i].ip, 1642);
                        for (int kx = 0; kx < 100; kx++)
                        {
                            if (receive1642s) break;
                            Thread.Sleep(10);
                        }
                    }
                    udpcz.ClostMonitorUDP();

                }
            }


            //读取数据



        }
        bool receive1642s = false;

        /// <summary>
        /// 查询指定主机和指定通道 待机状态
        /// </summary>
        /// <param name="zj">主机</param>
        /// <param name="td">通道</param>
        public void cxDJZT(int zj,int td)
        {
            // 主机序号从机序号 01 01 00 49
            int udpdk = 5190;
            int cjxh = 1;
            //获取从机通道数量

            UDPCZClass udpcz = new UDPCZClass(udpdk);
            sms[zj].CommunicationPort = udpdk;
            udpcz.AddReceiveData += new UDPCZClass.AddReceiveDataHandler(ReceiveDataDJZT);
            udpcz.MonitorUDP(ip, udpdk);
            udpcz.SendUDP(zj.ToString()+ td.ToString() + "0049", sms[zj].ip, 1642);
            cjxh++;
            for (int k = 0; k < 1000; k++)
            {
                if (receive1642s) break;
                Thread.Sleep(10);
            }
            udpcz.ClostMonitorUDP();
        }

        public void ReceiveData1642(IPEndPoint ipp,string mesage)
        {
            try
            {
                int select = int.Parse(ipp.Address.ToString().Split('.')[3]);
                if (select < sms.Length)
                {

                    string td = mesage.Substring(8, 2);
                    sms[select].aisle = int.Parse(td);
                }
                receive1642s = true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        /// <summary>
        /// 监听待机状态数据
        /// </summary>
        /// <param name="ipp"></param>
        /// <param name="mesage"></param>
        public void ReceiveDataDJZT(IPEndPoint ipp, string mesage)
        {
            try
            {
                int select = int.Parse(ipp.Address.ToString().Split('.')[3]);
                if (select < sms.Length)
                {

                    string td = mesage.Substring(8, 2);
                    sms[select].aisle = int.Parse(td);
                }
                receive1642s = true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        //public void fs()
        //{
        //    try
        //    {

        //        IPAddress HostIP = new IPAddress(ipStrToLong(textBox1.Text));
        //        //IPAddress HostIP = new IPAddress(new byte[] { 192,168,0,255 });
        //        IPEndPoint host = new IPEndPoint(HostIP, Int32.Parse(textBox2.Text));
        //        UdpClient udp = new UdpClient(Int32.Parse(textBox4.Text));
        //        byte[] bts = strToToHexByte(textBox3.Text);
        //        udp.Send(bts, bts.Length, host);
        //        udp.Close();
        //    }
        //    catch (Exception err)
        //    {
        //        MessageBox.Show("出错了:" + err.Message);
        //    }
        //}

        /** 
             * IP(String) 转 Long 
             * string ip to long 
             * */
        public long ipStrToLong(string ipaddress)
        {
            long[] ip = new long[4];
            int i = 0;
            string[] s = ipaddress.Split('.');
            for (i = 0; i < s.Length; i++)
            {
                ip[i] = long.Parse(s[i]);
            }
            return (ip[3] << 24) + (ip[2] << 16) + (ip[1] << 8) + ip[0];
            // return (ip[0] << 24) + (ip[1] << 16) + (ip[2] << 8) + ip[3];
        }

        /// <summary>
        /// byte转16进制字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string str2HexStr(byte[] bs)
        {
            char[] chars = "0123456789ABCDEF".ToCharArray();
            StringBuilder sb = new StringBuilder("");
            //byte[] bs = str.getBytes();
            int bit;
            for (int i = 0; i < bs.Length; i++)
            {
                bit = (bs[i] & 0x0f0) >> 4;
                sb.Append(chars[bit]);
                bit = bs[i] & 0x0f;
                sb.Append(chars[bit]);
            }
            return sb.ToString();
        }

        /// <summary>
                /// 字符串转16进制字节数组
                /// </summary>
               /// <param name="hexString"></param>
                /// <returns></returns>
        public byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }




        // 向TextBox中添加文本

        delegate void ShowMessageDelegate(TextBox txtbox, string message);

        private void ShowMessage(TextBox txtbox, string message)

        {

            if (txtbox.InvokeRequired)

            {

                ShowMessageDelegate showMessageDelegate = ShowMessage;

                txtbox.Invoke(showMessageDelegate, new object[] { txtbox, message });

            }

            else

            {

                txtbox.Text += message + "\r\n";

            }

        }



        // 清空指定TextBox中的文本

        delegate void ResetTextBoxDelegate(TextBox txtbox);

        private void ResetTextBox(TextBox txtbox)

        {

            if (txtbox.InvokeRequired)

            {

                ResetTextBoxDelegate resetTextBoxDelegate = ResetTextBox;

                txtbox.Invoke(resetTextBoxDelegate, new object[] { txtbox });

            }

            else

            {

                txtbox.Text = "";

            }



        }

        public void treeView1ZT(string ip,int count)
        {
            //count 第0次为刷新ip在线 后面为刷新收到报文
            if (InvokeRequired)
            {
                
                   MethodInvoker mi = () =>
                {
                    treeView1ZT(ip, count);
                };
                Invoke(mi);
            }
            else
            {
                if (count == 0)
                {
                    int i = Int32.Parse(ip.Split('.')[3]);
                    ((SlaveMachine)treeView1.Nodes[i].Tag).Online = true;
                    treeView1.Nodes[i].Text = ((SlaveMachine)treeView1.Nodes[i].Tag).ip + ",True";
                }
            }
        }

        /// <summary>
        /// 刷新指定IP的内容
        /// </summary>

        /// <param name="ipdata">从IP什么端口 发送到 啥IP啥端口</param>
        ///  <param name="ip">IP</param>
        /// <param name="data">数据</param>
        public void RefreshIPData(string ipdata,string ip, string data)
        {
            if (sms == null)
            {
                return;
            }
            if (InvokeRequired)
            {
                MethodInvoker mi = () =>
                {
                    RefreshIPData(ipdata,ip, data);
                };
                Invoke(mi);
            }
            else
            {
                string datas;
                //if (jsfs)
                //{
                //    datas = "接收:" + ip + "--> 内容:" + data + "\r\n";
                //}
                //else
                //{
                //    datas = "发送:" +  "-->" + ip+" 内容:" + data + "\r\n";
                //}
                datas = ipdata+ " 内容:" + data + "\r\n";
                sms[0].addSendReceive(datas);
                if (textBox1.Tag != null)
                {
                    if (textBox1.Tag.ToString() == "0")
                    {
                        textBox1.Text += datas;
                    }
                    else if (textBox1.Tag.ToString() == ip)
                    {
                        textBox1.Text += datas;
                    }
                    int c = int.Parse(ip.Split('.')[3]);
                    if (c < sms.Length)
                    {
                        sms[c].addSendReceive(datas);
                    }
                    textBox1.SelectionStart = textBox1.Text.Length - 1;
                    textBox1.ScrollToCaret();
                }
            }
           
        }


        public class UDPCZClass
        {
            public UDPCZClass(int port)
            {
                bjPort = port;
            }

            private int bjPort;
            /// <summary>

            /// 用于UDP发送的网络服务类

            /// </summary>

            private UdpClient udpcSend;

            /// <summary>

            /// 用于UDP接收的网络服务类

            /// </summary>

            private UdpClient udpcRecv;



            /// <summary>

            /// 开关：在监听UDP报文阶段为true，否则为false

            /// </summary>

            bool IsUdpcRecvStart = false;

            /// <summary>

            /// 线程：不断监听UDP报文

            /// </summary>

            Thread thrRecv = null;

            /// <summary>
            /// 接受数据委托
            /// </summary>
            /// <param name="ipp">包头</param>
            /// <param name="message">内容</param>
            public delegate void AddReceiveDataHandler(IPEndPoint ipp,string message);

            /// <summary>
            /// 接收数据注册
            /// </summary>
            /// <param name="ipp">包头</param>
            /// <param name="message">内容</param>
            public event AddReceiveDataHandler AddReceiveData;

            /// <summary>
            /// 开始监听指定端口数据
            /// </summary>
            /// <param name="port"></param>
            public void MonitorUDP(string ip,int port)
            {
                ClostMonitorUDP();

                IPEndPoint localIpep = new IPEndPoint(IPAddress.Parse(ip), port); // 本机IP和监听端口号



                udpcRecv = new UdpClient(localIpep);



                thrRecv = new Thread(ReceiveMessage);

                thrRecv.Start();



                IsUdpcRecvStart = true;
            }

            /// <summary>
            /// 发送指定端口数据
            /// </summary>
            /// <param name="strdata">发送内容</param>
            /// <param name="ip">发送给指定的ip</param>
            /// <param name="SendPort">端口</param>
            public void SendUDP(string strdata, string ip,int SendPort)
            {
                
                try
                {
                    //IPEndPoint localIpep = new IPEndPoint( IPAddress.Parse("192.168.0.163"), bjPort); // 本机IP，指定的端口号

                    //udpcSend = new UdpClient(localIpep);
                    byte[] bts = Form1.Instance.strToToHexByte(strdata);
                    IPEndPoint host = new IPEndPoint(IPAddress.Parse(ip), SendPort);
                    udpcRecv.Send(bts, bts.Length, host);
                    Form1.Instance.RefreshIPData(((System.Net.IPEndPoint)udpcRecv.Client.LocalEndPoint).Address +" "+ ((System.Net.IPEndPoint)udpcRecv.Client.LocalEndPoint).Port+ "-->" + ip+" "+ SendPort, ip, strdata);
                    //udpcSend.Close();
                }
                catch
                {
                    ;
                }
            }

            /// <summary>
            /// 关闭监听
            /// </summary>
            /// <param name="port"></param>
            public void ClostMonitorUDP()
            {
                try
                {
                    if (thrRecv != null)
                    {
                        udpcRecv.Close();
                        thrRecv.Abort();
                        thrRecv = null;
                    }
                    IsUdpcRecvStart = false;
                }
                catch (Exception err)
                {
                    Console.WriteLine("" + err.Message);
                }
            }


          

            /// <summary>

            /// 接收数据

            /// </summary>

            private void ReceiveMessage()

            {

                IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);

                while (true)

                {

                    try

                    {

                        byte[] bytRecv = udpcRecv.Receive(ref remoteIpep);
                        string message=Form1.Instance.str2HexStr(bytRecv);
                        //string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                        
                        Form1.Instance.RefreshIPData(remoteIpep.Address+" "+remoteIpep.Port+"-->"+ ((System.Net.IPEndPoint)udpcRecv.Client.LocalEndPoint).Address + " " + ((System.Net.IPEndPoint)udpcRecv.Client.LocalEndPoint).Port, string.Format("{0}", remoteIpep.Address), message);

                        if(AddReceiveData!=null) AddReceiveData(remoteIpep,message);

                        if (bjPort == 30001)
                        {
                            Form1.Instance.treeView1ZT(string.Format("{0}", remoteIpep.Address), 0);
                        }
                        if(remoteIpep.Port==1642)
                        {
                            
                        }
                        //ShowMessage(,string.Format("{0}[{1}]", remoteIpep, message));

                    }

                    catch (Exception ex)

                    {

                        Console.WriteLine(ex.Message);

                        break;

                    }

                }

            }

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                
                TreeNode tn= e.Node;
                if (tn != null)
                {
                    SlaveMachine sm = (SlaveMachine)tn.Tag;
                    textBox1.Tag = sm.ip;
                    textBox1.Text = sm.getSendReceive();
                    textBox3.Text = sm.ip;
                    if (sm.Online)
                    {
                        textBox6.Text = ""+sm.CommunicationPort;
                        textBox7.Text = "1642";
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("" + err.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                ip = textBox4.Text;
                mac = textBox5.Text;
                ipd = jisuanIPQZ(ip);
            }
            catch
            {
                ;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int fsdk = Int32.Parse(textBox6.Text);
                int jsdk = Int32.Parse(textBox7.Text);
                receive1642s = false;
                UDPCZClass udpcz = new UDPCZClass(fsdk);
                udpcz.AddReceiveData += new UDPCZClass.AddReceiveDataHandler(ReceiveData1642);
                udpcz.MonitorUDP(ip, fsdk);
                udpcz.SendUDP(textBox2.Text, textBox3.Text, jsdk);
                for (int k = 0; k < 100; k++)
                {
                    if (receive1642s) break;
                    Thread.Sleep(10);
                }
                udpcz.ClostMonitorUDP();
            }
            catch
            {
                ;
            }

        }
    }
 }
