using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THT_Fuzzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string capital_letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string lower_letters = capital_letters.ToLower();
        private static string digits = "0123456789";
        private string gen_string(int len)
        {

            /* 
             
             Aa0Aa1Aa2Aa3Aa4Aa5Aa6Aa7Aa8Aa9
             Ab0Ab1Ab2Ab3Ab4Ab5Ab6Ab7Ab8Ab9
             
             */

            string tmp = "";
            for (int k = 0; k < capital_letters.Length; k++)
            {
                string cap_letter = capital_letters[k].ToString();
                for (int l = 0; l < lower_letters.Length; l++)
                {
                    string lower_letter = lower_letters[l].ToString();
                    for (int m = 0; m < digits.Length; m++)
                    {
                        string digit = digits[m].ToString();
                        tmp += (cap_letter + lower_letter + digit);
                    }
                }
            }

            return tmp.Substring(0, len);  
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            comboBox2.SelectedIndex = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox1.Text = trackBar1.Value.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out _))
            {
                textBox1.Text = "100";
                trackBar1.Value = 100;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
                return;
            }

            if (Convert.ToInt32(textBox1.Text) == 0)
            {
                trackBar1.Value = 1;
                textBox1.Text = "1";
                return;
            }

            if (Convert.ToInt32(textBox1.Text) > 20280)
            {
                textBox1.Text = "20280";
                trackBar1.Value = Convert.ToInt32(textBox1.Text);
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
                return;
            }

            trackBar1.Value = Convert.ToInt32(textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            richTextBox1.Text = gen_string(Convert.ToInt32(textBox1.Text));
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox2.Text == " ")
            {
                MessageBox.Show("EIP değeri boş olamaz!");
                return;
            }

            if (textBox2.Text.Length != 8)
            {
                MessageBox.Show("EIP değeri doğru bir değer değil!");
                return;
            }

            string search_pattern = "";
            try
            {
                search_pattern += Convert.ToString((char)Int16.Parse(textBox2.Text.Substring(0,2), NumberStyles.AllowHexSpecifier));
                search_pattern += Convert.ToString((char)Int16.Parse(textBox2.Text.Substring(2, 2), NumberStyles.AllowHexSpecifier));
                search_pattern += Convert.ToString((char)Int16.Parse(textBox2.Text.Substring(4, 2), NumberStyles.AllowHexSpecifier));
                search_pattern += Convert.ToString((char)Int16.Parse(textBox2.Text.Substring(6, 2), NumberStyles.AllowHexSpecifier));
            }
            catch
            {
                MessageBox.Show("EIP değeri doğru bir değer değil!");
                return;
            }

            textBox3.Text = search_pattern;
            string pattern = gen_string(20280);

            textBox4.Text = pattern.IndexOf(search_pattern).ToString();
            richTextBox2.ResetText();
            richTextBox2.SelectionColor = Color.Yellow;
            richTextBox2.AppendText(pattern.Substring(0, pattern.IndexOf(search_pattern)));
            richTextBox2.SelectionColor = Color.Lime;
            richTextBox2.AppendText(pattern.Substring(pattern.IndexOf(search_pattern), pattern.Length - pattern.IndexOf(search_pattern)));
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {
            richTextBox3.Text = richTextBox3.Text.Replace(" ", "").Replace("\n", "");
            richTextBox3.SelectionStart = richTextBox3.Text.Length;
            richTextBox3.SelectionLength = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    richTextBox4.Text = "USER anonymous\nPASS {{PATTERN}}";
                    break;

                case 1:
                    richTextBox4.Text = "{{PATTERN}}";
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Burası sokete nasıl bir komut girmek istediğinizi gösterir. {{PATTERN}} yazdığınız yere program otomatik olarak patterni yerleştirir ve gönderir. Komutlar ASCII'ye uygun olmalıdır.");
        }

        private void tcp_connect()
        {
            if (textBox5.Text.Trim() == "" || textBox6.Text.Trim() == "")
            {
                MessageBox.Show("Lütfen host ve port değerlerini kontrol edin!");
                return;
            }

            if (!int.TryParse(textBox6.Text, out _))
            {
                MessageBox.Show("Port değeri sayı olmalıdır!");
                return;
            }

            if (richTextBox3.Text.Trim() == "")
            {
                MessageBox.Show("Pattern değeri boş olamaz!");
                return;
            }

            if (richTextBox4.Text.Trim(' ', '\n') == "")
            {
                MessageBox.Show("Gönderilecek bir komut yazın!");
                return;
            }

            try
            {
                using (TcpClient clientSocket = new TcpClient())
                {
                    richTextBox5.AppendText(Environment.NewLine + Environment.NewLine + "Başlatılıyor!");
                    clientSocket.Connect(textBox5.Text.Trim(), Convert.ToInt32(textBox6.Text.Trim()));
                    richTextBox5.AppendText(Environment.NewLine + "Bağlantı sağlandı!");

                    using (NetworkStream networkStream = clientSocket.GetStream())
                    {
                        foreach (string line in richTextBox4.Text.Split('\n'))
                        {
                            networkStream.Write(Encoding.ASCII.GetBytes(line.Replace("{{PATTERN}}", richTextBox3.Text.Trim())), 0, Encoding.ASCII.GetBytes(line.Replace("{{PATTERN}}", richTextBox3.Text.Trim())).Length);
                            networkStream.Flush();

                            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                            networkStream.Read(buffer, 0, clientSocket.ReceiveBufferSize);
                            string data = Encoding.ASCII.GetString(buffer);
                            richTextBox5.AppendText(Environment.NewLine + "Yanıt: " + data);
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                richTextBox5.AppendText(Environment.NewLine + "HATA: " + ex.Message);
            }
        }

        private void udp_connect()
        {
            if (textBox5.Text.Trim() == "" || textBox6.Text.Trim() == "")
            {
                MessageBox.Show("Lütfen host ve port değerlerini kontrol edin!");
                return;
            }

            if (!int.TryParse(textBox6.Text, out _))
            {
                MessageBox.Show("Port değeri sayı olmalıdır!");
                return;
            }

            if (richTextBox3.Text.Trim() == "")
            {
                MessageBox.Show("Pattern değeri boş olamaz!");
                return;
            }

            if (richTextBox4.Text.Trim(' ', '\n') == "")
            {
                MessageBox.Show("Gönderilecek bir komut yazın!");
                return;
            }

            try
            {
                using (UdpClient clientSocket = new UdpClient())
                {
                    richTextBox5.AppendText(Environment.NewLine + Environment.NewLine + "Başlatılıyor!");
                    clientSocket.Connect(textBox5.Text.Trim(), Convert.ToInt32(textBox6.Text.Trim()));
                    richTextBox5.AppendText(Environment.NewLine + "Bağlantı sağlandı!");

                    foreach (string line in richTextBox4.Text.Split('\n'))
                    {
                        var tmp = Encoding.ASCII.GetBytes(line.Replace("{{PATTERN}}", richTextBox3.Text.Trim()));
                        clientSocket.Send(tmp, tmp.Length);
                    }
                }
                richTextBox5.AppendText(Environment.NewLine + "Gönderildi!");
            }
            catch (SocketException ex)
            {
                richTextBox5.AppendText(Environment.NewLine + "HATA: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                tcp_connect();
            } else
            {
                udp_connect();
            }
        }
    }
}
