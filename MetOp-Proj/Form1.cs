using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetOp_Proj
{
    public partial class Form1 : Form
    {
        public Random random;
        public Graphics graphicsObj;
        public SolidBrush mBrush;
        public Color mColor;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            random = new Random(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000);
            graphicsObj = pictureBox1.CreateGraphics();
            mBrush = new SolidBrush(Color.Red);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            //graphicsObj.Clear(Color.White);
            Pen myPen = new Pen(System.Drawing.Color.Red, 5);
            int x, y;
            x = random.Next(1, 499);
            y = random.Next(1, 499);
            mColor = Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
            mBrush.Color = mColor;
            Rectangle rect = new Rectangle(x-1, y-1, 2, 2);
            //graphicsObj.DrawEllipse(myPen, rect);
            //graphicsObj.DrawRectangle(myPen, rect);
            graphicsObj.FillRectangle(mBrush, rect);
        }
    }
}
