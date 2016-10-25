using CSaptcha;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Captcha cptch;

        public Form1()
        {
            InitializeComponent();
            cptch = new Captcha();
            pictureBox1.Image = cptch.GenerateBitmap(Color.White, Color.Black);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = cptch.GenerateBitmap(Color.White, Color.Black);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cptch.ReGenerate();
            pictureBox1.Image = cptch.GenerateBitmap(Color.White, Color.Black);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                cptch.ReGenerate();
                cptch.GenerateBitmap(Color.White, Color.Black);
            }
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show("Total time for 10000: " + (elapsedMs).ToString() + "ms");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(cptch.GenerateBase64Png(Color.White, Color.Black));
        }
    }
}
