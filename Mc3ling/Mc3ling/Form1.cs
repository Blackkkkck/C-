using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication.Profinet.Melsec;


namespace Mc3ling
{
    public partial class Form1 : Form
    {
      Hslcommcation MyHslcommcation;
        //三菱PLC ASCLL
        private MelsecMcNet client;
      

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MyHslcommcation = new Hslcommcation(IPtextBox1.Text, (int)portnumericUpDown1.Value);

            MyHslcommcation.MessageReceived += _MC_MessageReceived;
        }

        private void Savebutton1_Click(object sender, EventArgs e)
        {
            try
            {
                string Filename = Path.Combine(MyHslcommcation.startDir, "1.xml");
                //按顺序保存 读取地址一，二，发送地址一，二，ip，端口，要发送的数据一、二
                Hslcommcation.BinarySerializer.Serialize(Filename,gettextBox1.Text,
                    gettextBox2.Text,settextBox1.Text,settextBox2.Text,IPtextBox1.Text
                    ,portnumericUpDown1.Value.ToString(), sendtextBox1.Text, sendtextBox2.Text
                    );

                Hslcommcation.WriteLog("保存成功");
            }
            catch
            {
                Hslcommcation.WriteLog("保存失败");
            }
        }
        bool Mybool=false;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

         
            if (!Mybool)
            {


                MyHslcommcation.SendPlc(Convert.ToByte(sendtextBox1.Text));
                Mybool = true;
                string str0 = string.Format("发送数据" +sendtextBox1.Text);
                AddTextToRichTextBox(richTextBox1, str0, true);
            }
            else { Mybool = false; MyHslcommcation.SendPlc(Convert.ToByte(sendtextBox2.Text));
                string str0 = string.Format("发送数据" + sendtextBox2.Text);
                AddTextToRichTextBox(richTextBox1, str0, true);
            }
            }catch { }
        

        }

        public static void AddTextToRichTextBox(RichTextBox rtbox, string text, bool addTime)
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


        void _MC_MessageReceived(object sender, Hslcommcation.MessageEventArgs e)
        {
            string stra=e.Message.ToString();
            AddTextToRichTextBox(richTextBox2, stra,true);


        }


        }
}
