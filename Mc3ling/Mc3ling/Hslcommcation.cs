using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.Profinet.Melsec;

namespace Mc3ling
{
    public class Hslcommcation
    {
        public string startDir = Application.StartupPath;
        public string receiveMess;
        //三菱PLC ASCLL
        private MelsecMcNet client;

        public delegate void ConnectResultEventHandler(object sender, ResultEventArgs e);

        public delegate void MessageReceivedEventHandler(object sender, MessageEventArgs e);

        public event MessageReceivedEventHandler MessageReceived;//接受到数据事件

        public event ConnectResultEventHandler ConnectedResult;//连接成功事件


        //连接成功事件
        public class ResultEventArgs : EventArgs
        {
            public readonly bool conResult;

            public ResultEventArgs(bool conResult)
            {
                this.conResult = conResult;
            }
        }
        //接受到数据事件
        public class MessageEventArgs : EventArgs
        {
            public readonly string Message;

            public MessageEventArgs(string Message)
            {
                this.Message = Message;
            }
        }

        public Hslcommcation(string Ip, int Port)
        {
            OpenPlc("192.168.0.170", 506);
            Thread Mythread = new Thread(Getsignal);
            Mythread.Start();
            string Filename = Path.Combine(startDir, "1.xml");

            Thread Mythread2 = new Thread(Getsignal2);
            Mythread2.Start();
         
            if (!File.Exists(Filename))
            {
                MessageBox.Show("没有默认配置文件");
                WriteLog("没有默认配置文件");
            }
            WriteLog("初始化成功，接收线程已开启");
        }


        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsItPLCConnect { get; set; } = false;

        /// <summary>
        /// 打开plc
        /// </summary>
        public void OpenPlc(string Ip, int Port)
        {
            try
            {
                client?.ConnectClose();
                //三菱PLC需开启TCP通信，并且设置端口号
                client = new MelsecMcNet(Ip, Port);
                var result = client.ConnectServer();
                if (result.IsSuccess)
                {
                    IsItPLCConnect = true;
                    WriteLog("PLC初始化连接成功");
                }
                else
                {
                    WriteLog("PLC初始化连接失败");
                    // MessageBox.Show("连接PLC服务器失败！", "系统出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                WriteLog("PLC初始化连接失败" + ex.ToString());
            }
        }
        /// <summary>
        /// 关闭plc
        /// </summary>
        public void ClosePlc()
        {
            IsItPLCConnect = false;
            WriteLog("PLC连接关闭");
            client?.ConnectClose();

        }
 
        /// <summary>
        /// 接收信号1
        /// </summary>
        public void Getsignal()
        {
            while (true)
            {
                string Filename = Path.Combine(startDir, "1.xml");
                object[] Myobj = BinarySerializer.Deserialize(Filename);
                ushort bytes = 0;
                try
                {
                    if (IsItPLCConnect)
                    {
                        //读指定地址的值
                        client.Read("D4700", bytes);

                        if (bytes == 1)
                        {
                            receiveMess = bytes.ToString();

                            MessageEventArgs e = new MessageEventArgs(receiveMess);
                            if (MessageReceived != null)
                            {
                                MessageReceived(this, e);
                            }
                            WriteLog("读取地址的值成功，当前值为："+ bytes.ToString());
                        }


                    }
                }
                catch (Exception ex)
                {
                    WriteLog("读取地址的值时报错" + ex.ToString());
                }
            }
        }



        /// <summary>
        /// 接收信号2
        /// </summary>
        public void Getsignal2()
        {
            while (true)
            {
                string Filename = Path.Combine(startDir, "1.xml");
                object[] Myobj = BinarySerializer.Deserialize(Filename);
                ushort bytes = 0;
                try
                {
                    if (IsItPLCConnect)
                    {
                        //读指定地址的值
                        client.Read(Myobj[1].ToString(), bytes);

                        if (bytes == 1)
                        {
                            receiveMess = bytes.ToString();

                            MessageEventArgs e = new MessageEventArgs(receiveMess);
                            if (MessageReceived != null)
                            {
                                MessageReceived(this, e);
                            }
                            WriteLog("读取地址的值成功，当前值为：" + bytes.ToString());
                        }


                    }
                }
                catch (Exception ex)
                {
                    WriteLog("读取地址的值时报错" + ex.ToString());
                }
            }
        }
        public void SendPlc( byte mymess)
        {
            string Filename = Path.Combine(startDir, "1.xml");

            
            //string address = BinarySerializer.Deserialize("D4702", mymess.ToString());
            try
            {
                 

                if (IsItPLCConnect)
                {
                    client.Write("D4702", mymess);

                }
            }
            catch
            {
                WriteLog("发送失败");
            }

        }


        /// <summary>
        /// 对多个变量进行操作
        /// </summary>
        public static class BinarySerializer
        {/// <summary>
         /// 序列化保存
         /// </summary>
         /// <param name="filename"></param>
         /// <param name="objects"></param>
            public static void Serialize(string filename, params object[] objects)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    foreach (object obj in objects)
                    {
                        formatter.Serialize(stream, obj);
                    }
                }
            }
            /// <summary>
            /// 序列化读取
            /// </summary>
            /// <param name="filename"></param>
            /// <returns></returns>
            public static object[] Deserialize(string filename)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    List<object> objects = new List<object>();

                    while (stream.Position < stream.Length)
                    {
                        object obj = formatter.Deserialize(stream);
                        objects.Add(obj);
                    }

                    return objects.ToArray();
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
                lock (logMessage)
                {
                    string logDir = Path.Combine(Directory.GetCurrentDirectory(), "log");
                    string dateDir = Path.Combine(logDir, DateTime.Now.ToString("yyyy-MM-dd"));
                    string logFile = Path.Combine(dateDir, "logHslcommcation.txt");

                    if (!Directory.Exists(dateDir))
                        Directory.CreateDirectory(dateDir);

                    // 日期目录定义

                    string time = DateTime.Now.ToString("HH:mm:ss");
                    using (StreamWriter writer = File.AppendText(logFile))
                    {
                        writer.WriteLine($"{time}: {logMessage}");
                        writer.Close();
                    }
                    }
                });
            }
            catch
            {

                }
            
        }
    }
}
