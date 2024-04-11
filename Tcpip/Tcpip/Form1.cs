using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Tcpip.TcpServer;

namespace Tcpip
{
    public partial class Form1 : Form
    {
        string TCP3message;

        TcpServer MytcpServer;

        bool Zhuce = true;



        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void ShwoText(object sender, ResultEventArgs e)
        {
            if (MytcpServer.IsTcpip)
            {
                TCP3message = "已有设备连接";
                AddTextToRichTextBox(richTextBox1, TCP3message, true);
            }


        }


        public void ShwoText(object sender, TcpServer.MessageEventArgs e)
        {
            if (MytcpServer.IsTcpip)
            {
                TCP3message = null;
                TCP3message = MytcpServer.receiveMess;
                AddTextToRichTextBox(richTextBox1, "接收数据为：" + TCP3message, true);
            }
        }
        /// <summary>
        /// 将内容添加到控件中
        /// </summary>
        /// <param name="rtbox"></param>
        /// <param name="text"></param>
        /// <param name="addTime"></param>
        public static void AddTextToRichTextBox(RichTextBox rtbox, string text, bool addTime)
        {
            try
            {
                if (rtbox.InvokeRequired)
                {
                    rtbox.Invoke(new Action<RichTextBox, string, bool>((rtb, str, addtime) => AddTextToRichTextBox(rtb, str, addtime)), rtbox, text, addTime);
                    return;
                }
                if (addTime)
                {
                    text = string.Format("【{0}】--{1}", DateTime.Now.ToString("yyyy") + "年"
                        + DateTime.Now.ToString("MM") + "月" + DateTime.Now.ToString("dd") + "天"
                        + DateTime.Now.ToString("HH") + "时" + DateTime.Now.ToString("mm") + "分"
                        + DateTime.Now.ToString("ss") + "秒", text);
                }
                rtbox.AppendText(text + "\r\n");
                rtbox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {


            //正常情况下应该先实例化直接注册事件
            MytcpServer = new TcpServer(TcpServer.NetBehavior.Server, textBox1.Text, (int)numericUpDown1.Value);

            if (Zhuce && MytcpServer.IsTcpip)
            {
                //事件注册，当接收到信息时，会触发该方法
                MytcpServer.MessageReceived += ShwoText;

                MytcpServer.ConnectedResult += ShwoText;
                Zhuce = false;
            }




        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MytcpServer.IsTcpip)
            {
                MytcpServer.SendMessage(richTextBox2.Text);
            }
            else
            {
                MessageBox.Show("未连接");
            }
        }
    }
}
