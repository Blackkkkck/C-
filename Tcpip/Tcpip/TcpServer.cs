using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static Tcpip.TcpServer;

namespace Tcpip
{
    public class TcpServer
    {
        public Socket mySocket;
        public IPEndPoint myIPEndPoint;
        public byte[] tempByte = new byte[10240];
        public string receiveMess;


        public enum NetBehavior
        {
            Server = 1, Client = 2

        }
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsTcpip { get; set; } = false;

        /// <summary>
        /// 通过传入 客户段还是服务段、IP、端口
        /// </summary>
        /// <param name="netBehavior"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServer(NetBehavior netBehavior, string ip, int port)
        {
            try
            {


                this.myIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                myNetBehavior = netBehavior;

                System.Threading.Thread iniThread = new System.Threading.Thread(Connect);
                iniThread.IsBackground = false;
                iniThread.Start();
                receiveMess = "";

                System.Threading.Thread receiveThread = new System.Threading.Thread(ReceiveMessage);
                receiveThread.IsBackground = false;
                receiveThread.Start();
                receiveMess = "";
                IsTcpip = true;
            }
            catch (Exception e)
            {
                WriteLog(e.ToString()); IsTcpip = false;
            }
        }

        public bool conResult = false;

        public NetBehavior myNetBehavior;   //定义作为服务器运行还是客户端运行

        public delegate void ConnectResultEventHandler(object sender, ResultEventArgs e);
        /// <summary>
        /// 发送事件       
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void MessageReceivedEventHandler(object sender, MessageEventArgs e);
        /// <summary>
        /// 接受到数据事件
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;//接受到数据事件
        /// <summary>
        /// 连接成功事件
        /// </summary>
        public event ConnectResultEventHandler ConnectedResult;//连接成功事件

        public class ResultEventArgs : EventArgs
        {
            public readonly bool conResult;

            public ResultEventArgs(bool conResult)
            {
                this.conResult = conResult;
            }
        }

        public class MessageEventArgs : EventArgs
        {
            public readonly string Message;

            public MessageEventArgs(string Message)
            {
                this.Message = Message;
            }
        }
        /// <summary>
        /// 定义属性--IP地址
        /// </summary>
        public string IpAddress//定义属性--IP地址
        {
            set
            {
                this.myIPEndPoint.Address = IPAddress.Parse(value);
            }
            get
            {
                return this.myIPEndPoint.Address.ToString();
            }

        }

        public int Port//定义属性--端口号
        {
            set
            {
                this.myIPEndPoint.Port = value;
            }
            get
            {
                return myIPEndPoint.Port;
            }

        }
        /// <summary>
        /// 尝试连接服务器或者客户端
        /// </summary>
        public void Connect()//尝试连接服务器或者客户端
        {
            while (true)
            {
                if (myNetBehavior == NetBehavior.Client)
                {

                    try
                    {
                        System.Threading.Thread.Sleep(300);
                        mySocket.Connect(myIPEndPoint);
                        conResult = true;
                        ResultEventArgs e = new ResultEventArgs(conResult);
                        ResultToDo(e);
                        break;
                    }
                    catch
                    {
                        conResult = false;

                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                    try
                    {

                        this.mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        myIPEndPoint.Address = IPAddress.Parse(this.IpAddress);
                        mySocket.Bind(myIPEndPoint);
                        mySocket.Listen(3);
                        mySocket = mySocket.Accept();
                        conResult = true;
                        ResultEventArgs e = new ResultEventArgs(conResult);
                        ResultToDo(e);
                    }
                    catch
                    {
                    }
                    return;
                }

            }

        }

        protected virtual void ResultToDo(ResultEventArgs e)
        {
            if (ConnectedResult != null)
            {
                ConnectedResult(this, e);
            }

        }



        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMessage()//接收数据
        {
            while (true)
            {
                if (mySocket.Connected)
                {
                    try
                    {
                        int numReceive = mySocket.Receive(tempByte, SocketFlags.None);
                        if (numReceive > 0)
                        {
                            // receiveMess = Encoding.ASCII.GetString(tempByte, 0, numReceive);
                            receiveMess = Encoding.Default.GetString(tempByte);

                            MessageEventArgs e = new MessageEventArgs(receiveMess);

                            if (MessageReceived != null)
                            {
                                MessageReceived(this, e);
                            }
                        }
                        else
                        {
                            byte[] temp = Encoding.Default.GetBytes(" ");
                            mySocket.Send(temp);
                        }
                    }
                    catch (Exception a)
                    {
                        conResult = false;
                        ResultEventArgs e = new ResultEventArgs(conResult);
                        ResultToDo(e);
                        if (this.mySocket.Connected)
                        {
                            this.mySocket.Shutdown(SocketShutdown.Both);
                        }
                        if (this.mySocket != null)
                        {
                            this.mySocket.Close();
                            this.mySocket = null;

                        }
                        GC.Collect();
                        this.mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        System.Threading.Thread connectThread = new System.Threading.Thread(Connect);
                        connectThread.IsBackground = true;
                        connectThread.Priority = System.Threading.ThreadPriority.Normal;
                        connectThread.Start();
                    }

                }

            }

        }
        /// <summary>
        /// 日志方法
        /// </summary>
        /// <param name="logMessage"></param>
        public static async void WriteLog(string logMessage)
        {
            try
            {
                await Task.Run(() =>
                {
                    string logDir = Path.Combine(Directory.GetCurrentDirectory(), "log");
                    string dateDir = Path.Combine(logDir, DateTime.Now.ToString("yyyy-MM-dd"));
                    string logFile = Path.Combine(dateDir, "logip.txt");

                    if (!Directory.Exists(dateDir))
                        Directory.CreateDirectory(dateDir);

                    // 日期目录定义

                    string time = DateTime.Now.ToString("HH:mm:ss");
                    using (StreamWriter writer = File.AppendText(logFile))
                    {
                        writer.WriteLine($"{time}: {logMessage}");
                        writer.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="s"></param>
        public void SendMessage(string s)//发送数据
        {
            if (mySocket.Connected)
            {
                byte[] buffer = Encoding.Default.GetBytes(s);
                try
                {
                    mySocket.Send(buffer, buffer.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("网络可能没有成功连接，发送数据时出现错误!");
                    WriteLog("信号发送失败:" + ex.ToString());
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("网络未连接!");
            }
        }


    }
}
