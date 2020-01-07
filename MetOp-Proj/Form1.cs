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
        public Graphics graphicsObj;
        public Graphics graphics;
        Graphics go1, go2;
        Robot mRobot;
        Field mFiled;
        int bf;
        int ibots;
        public System.Collections.ArrayList mBots;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mRobot = new Robot();
            mFiled = new Field();
            ibots = 1000;
            graphicsObj = pictureBox1.CreateGraphics();
            graphics = pictureBox2.CreateGraphics();
            mBots = new System.Collections.ArrayList();
            for (int i = 0; i < ibots; ++i)
                mBots.Add(new Robot());
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            int gens = 0;
            mRobot = new Robot();
            xy bad;
            //while (mRobot.x < 0.99 || mRobot.y < 0.99)
            //    mRobot.Simulate(ref mFiled);
            double bestdist;
            double besttime;
            double botdist;
            bool ended = false;
            int besti;
            while (mRobot.fdist > 0.01 && gens < 500)
            {
                bad.x = 0.0; bad.y = 0.0;
                ++gens;
                label1.Text = gens.ToString();
                bestdist = 10.0;
                besti = 0;
                besttime = 0.0;
                for (int i = 0; i < ibots; ++i) 
                {
                    Robot tmp = new Robot(mRobot);
                    tmp.Randomize();
                    tmp.Simulate(ref mFiled);
                    bad.x += tmp.x / 1000.0;
                    bad.y += tmp.y / 1000.0; // получаем усреднённую плохую точку
                    mBots[i] = new Robot(tmp);
                    if (tmp.fdist <= 0.01)
                    {
                        besttime = tmp.time;
                        besti = i;
                        ended = true;
                    }
                }
                for (int i = 0; i < ibots; ++i) 
                {
                    Robot tmp = (Robot)mBots[i];
                    botdist = ((tmp.x - bad.x) * (tmp.x - bad.x) + (tmp.y - bad.y) * (tmp.y - bad.x)) * 0.7;
                    botdist = tmp.fdist - botdist * botdist;
                    if (ended)
                    { 
                        if (besttime > tmp.time && tmp.fdist <= 0.01)
                        {
                            besttime = tmp.time;
                            besti = i;
                        }
                    }
                    else
                    {
                        if (bestdist > botdist)
                        {
                            besttime = tmp.time;
                            bestdist = botdist;
                            besti = i;
                        }
                    }
                }
                mRobot = new Robot((Robot) mBots[besti]);
                label2.Text = besttime.ToString();

                graphicsObj.Clear(Color.White);
                graphics.Clear(Color.White);
                mRobot.Draw(ref graphics);
                mFiled.Draw(ref graphicsObj);
                mRobot.Simulate(ref mFiled, graphicsObj, false);
            }
            gens = 0;
            mBots.Add(new Robot(mRobot)); // #101 - наш найденный идеал до ускорения
            while (gens < 10)
            {
                ++gens;
                label6.Text = gens.ToString();
                besttime = mRobot.time;
                label8.Text = besttime.ToString();

                for (int i = 0; i < ibots; ++i)
                {
                    Robot tmp = new Robot(mRobot);
                    tmp.AccRand();
                    tmp.Simulate(ref mFiled);
                    mBots[i] = new Robot(tmp);
                    if (tmp.fdist <= 0.01 && besttime > tmp.time)
                    {
                        besttime = tmp.time;
                        mBots[101] = new Robot(tmp);
                        mRobot = new Robot(tmp);
                        graphicsObj.Clear(Color.White);
                        mFiled.Draw(ref graphicsObj);
                        mRobot.Simulate(ref mFiled, graphicsObj, false);
                        label8.Text = besttime.ToString();
                    }
                }

            }
            graphics.Clear(Color.White);
            mRobot.Draw(ref graphics);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            graphicsObj.Clear(Color.White);
            mFiled = new Field();
            mFiled.Draw(ref graphicsObj);
        }
    }


    public class Zone
    {
        public double x, y, r;
        public Zone(double x_, double y_, double r_)
        {
            x = x_;
            y = y_;
            r = r_;
        }
    }

    public class Field
    {
        public Random mRandom;
        public System.Collections.ArrayList mZones;
        public SolidBrush mBrush;

        public Field()
        {
            mRandom = new Random(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000);
            mZones = new System.Collections.ArrayList();
            for (int z = mRandom.Next(10, 20); z > 0; --z)
            {
                mZones.Add(new Zone(mRandom.NextDouble(), mRandom.NextDouble(), 0.02+mRandom.NextDouble()*0.1));
            }

            mBrush = new SolidBrush(Color.Red);
        }

        public void Draw(ref Graphics graphicsObj)
        {
            foreach (Zone z in mZones)
            {
                double x = z.x;
                double y = z.y;
                double r = z.r;
                graphicsObj.FillEllipse(mBrush, new Rectangle((int)((x - r) * 500), (int)((1.0 - y - r ) * 500), (int)(r * 2.0 * 500), (int)(r * 2.0 * 500)));
            }
        }
    }


    public class Robot
    {
        public QuadTree qt;
        public double time;
        public double x, y;
        double vx, vy;
        double fmax;
        double dt;
        public double fdist;
        int n;
        bool stopped;
        double nx, ny;

        public Robot(Robot r)
        {
            this.dt = r.dt;
            this.fdist = r.fdist;
            this.fmax = r.fmax;
            this.n = r.n;
            this.qt = new QuadTree(r.qt);
            this.stopped = r.stopped;
            this.time = r.time;
            this.vx = r.vx;
            this.vy = r.vy;
            this.x = r.x;
            this.y = r.y;
        }

        public void Simulate(ref Field f, Graphics graphicsObj = null, bool spl = true)
        {
            stopped = false;
            x = 0.0;
            y = 0.0;
            vx = 0.0;
            vy = 0.0;
            time = dt;
            n = 0;
            do
            {
                if (n > 5000)
                {
                    break;
                }
                nx = x + vx * dt;
                ny = y + vy * dt;

                if (nx < 0.0 || nx > 1.0 || ny < 0.0 || ny > 1.0) // коллизия стен
                {

                    stopped = true;
                }

                foreach (Zone z in f.mZones) // 
                {
                    if ((x - z.x) * (x - z.x) + (y - z.y) * (y - z.y) <= z.r * z.r)
                        stopped = true;
                }

                x = nx;
                y = ny;

                xy a = qt.GetA(x, y); // изменение скорости
                vx += a.x*dt;
                vy += a.y*dt;




                if (stopped && spl)
                    qt.SplitHere(x, y);

                if (graphicsObj != null)
                {
                    graphicsObj.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)(x * 500), (int)((1.0 - y) * 500), 2, 2));
                }
                ++n;
                time += dt;
                qt.SetTime(x, y, time);
            } while (!stopped);
            fdist = Math.Sqrt((1.0 - x) * (1.0 - x) + (1.0 - y) * (1.0 - y));
        }

        public Robot()
        {
            qt = new QuadTree();
            qt.Split();
            qt.child00.Split();
            qt.child01.Split();
            qt.child10.Split();
            qt.child11.Split();
            time = 0.0;
            fmax = 1.0;
            dt = 0.01; // TODO:
            vx = 0.0;
            vy = 0.0;
            x = 0.0;
            y = 0.0;
            n = 0;
            fdist = 10.0;
            // Инициализируем рандом и графику
        }
        public void Draw(ref Graphics graphics)
        {
            qt.Draw(ref graphics, time);
        }
        public void Randomize()
        {
            qt.Randomize(time);
        }
        public void AccRand()
        {
            qt.AccRandomize();
        }
        public void SplitHere(double x_, double y_)
        {
            qt.SplitHere(x_, y_);
        }
    }

    public struct xy
    {
       public double x, y;
    }

    public class QuadTree
    {
        public Random mRandom;
        public SolidBrush mBrush;
        public Color mColor;
        public double x, y, w; // расположения квадрата на оси координат и его размер. это для Draw удобно
        public double ax, ay; // у каждого [-1; 1]
        public double time;
        int depth;

        public QuadTree(QuadTree q)
        {
            this.ax = q.ax;
            this.ay = q.ay;
            this.depth = q.depth;
            this.mBrush = q.mBrush;
            this.mColor = q.mColor;
            this.mRandom = q.mRandom;
            this.time = q.time;
            this.w = q.w;
            this.x = q.x;
            this.y = q.y;

            if (q.child00 != null)
            {
                this.child00 = new QuadTree(q.child00);
                this.child01 = new QuadTree(q.child01);
                this.child10 = new QuadTree(q.child10);
                this.child11 = new QuadTree(q.child11);
            }
        }

        public void SetTime(double x_, double y_, double t)
        {

            if (child00 != null)
            {
                if (x_ < x + w / 2.0)
                {
                    if (y_ < y + w / 2.0)
                    {
                        child00.SetTime(x_, y_, t);
                    }
                    else
                    {
                        child01.SetTime(x_, y_, t);
                    }
                }
                else
                {
                    if (y_ < y + w / 2.0)
                    {
                        child10.SetTime(x_, y_, t);
                    }
                    else
                    {
                        child11.SetTime(x_, y_, t);
                    }
                }
            }
            else
            {
                time = t;
            }
        }

        public xy GetA(double x_, double y_)
        {
            xy a;
            if (child00 != null)
            {
                if (x_ < x + w / 2.0)
                {
                    if (y_ < y + w / 2.0)
                    {
                        a = child00.GetA(x_, y_);
                    }
                    else
                    {
                        a = child01.GetA(x_, y_);
                    }
                }
                else
                {
                    if (y_ < y + w / 2.0)
                    {
                        a = child10.GetA(x_, y_);
                    }
                    else
                    {
                        a = child11.GetA(x_, y_);
                    }
                }
            }
            else
            {
                a.x = ax;
                a.y = ay;
            }
            
            return a;
        }

        public void SplitHere(double x_, double y_)
        {
            if (child00 != null)
            {
                if (x_ < x + w / 2.0)
                {
                    if (y_ < y + w / 2.0)
                    {
                        child00.SplitHere(x_, y_);
                    }
                    else
                    {
                        child01.SplitHere(x_, y_);
                    }
                }
                else
                {
                    if (y_ < y + w / 2.0)
                    {
                        child10.SplitHere(x_, y_);
                    }
                    else
                    {
                        child11.SplitHere(x_, y_);
                    }
                }
            }
            else 
            {
                Split();
                //if (child00 != null)
                //{
                //    child00.ax = -ax;
                //    child00.ay = -ay;
                //    child01.ax = -ax;
                //    child10.ay = -ay;
                //}
            }
        }

        public QuadTree // дочерние ветки
            child00, // левая нижняя
            child01, // левая верхняя
            child10, // правая нижняя
            child11; // правая верхняя

        public void Randomize(double tmax)
        {
            double tk = time / tmax;
            if (tmax > 0.0001)
            {
                if (child00 == null) // если у нас нет детей, то рисуем цвета
                {
                    ///*mRandom.Next(6, 8))*/
                    ax += (Math.Pow(tk, 4.0)) * ((mRandom.NextDouble() - 0.5) * 2.0) + (mRandom.NextDouble() - 0.5) * 0.01 * depth;
                    ay += (Math.Pow(tk, 4.0)) * ((mRandom.NextDouble() - 0.5) * 2.0) + (mRandom.NextDouble() - 0.5) * 0.01 * depth;
                    if (ax > 1.0) ax = 1.0;
                    if (ay > 1.0) ay = 1.0;
                    if (ax < -1.0) ax = -1.0;
                    if (ay < -1.0) ay = -1.0;
                    double n = Math.Sqrt(ax * ax + ay * ay);
                    ax /= n;
                    ay /= n;
                    double acmax = (mRandom.NextDouble() + 0.5) / 1.5; // [0.1 - 1]
                    ax *= acmax;
                    ay *= acmax;
                    time = 0.0;
                }
                else
                {
                    child00.Randomize(tmax);
                    child01.Randomize(tmax);
                    child10.Randomize(tmax);
                    child11.Randomize(tmax);
                }
            }
        }
        public void AccRandomize()
        {
            if (child00 == null) // если у нас нет детей, то рисуем цвета
            {
                double n = Math.Sqrt(ax * ax + ay * ay);
                ax /= n;
                ay /= n;
                double acmax = (mRandom.NextDouble() + 0.75) / 1.75; // [0.1 - 1]
                ax *= acmax;
                ay *= acmax;
            }
            else
            {
                child00.AccRandomize();
                child01.AccRandomize();
                child10.AccRandomize();
                child11.AccRandomize();
            }
        }

        public void Split()
        {
            if (depth < 7)
            {
                child00 = new QuadTree(x, y, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth, time);
                child01 = new QuadTree(x, y + w / 2.0, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth, time);
                child10 = new QuadTree(x + w / 2.0, y, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth, time);
                child11 = new QuadTree(x + w / 2.0, y + w / 2.0, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth, time);
            }
        }

        public QuadTree(double x_, double y_, double w_, double ax_, double ay_, SolidBrush mb, Color c, Random mr, int d, double ttt)
        {
            x = x_;
            y = y_;
            w = w_;
            ax = ax_;
            ay = ay_;
            mBrush = mb;
            mColor = c;
            child00 = null;
            child01 = null;
            child10 = null;
            child11 = null;
            mRandom = mr;
            depth = d + 1;
            time = ttt;
        }
        public void Draw(ref Graphics graphicsObj, double timemax)
        {
            if (child00 == null) // если у нас нет детей, то рисуем цвета
            {
                mColor = Color.FromArgb((int)((ax + 1.0) / 2.0 * 255.0), (int)((ay + 1.0) / 2.0 * 255.0), Math.Min((int)(time/timemax * 255.0),255));
                mBrush.Color = mColor;
                graphicsObj.FillRectangle(mBrush, new Rectangle((int)(x * 500), (int)((1.0 - y - w) * 500), (int)(w * 500), (int)(w * 500)));
            }
            else // иначе рекурсивно рисуем
            {
                child00.Draw(ref graphicsObj, timemax);
                child01.Draw(ref graphicsObj, timemax);
                child10.Draw(ref graphicsObj, timemax);
                child11.Draw(ref graphicsObj, timemax);
            }
        }
        public QuadTree() // конструктор, который вызывается в самом начале на чистом поле
        {
            child00 = null; // вас не должно быть в начале
            child01 = null;
            child10 = null;
            child11 = null;
            x = 0.0;
            y = 0.0;
            w = 1.0;
            ax = Math.Sqrt(0.5); // всё сначала направлено максимально в сторону финиша
            ay = Math.Sqrt(0.5);
            time = 0.0001;
            depth = 1;

            mBrush = new SolidBrush(Color.Transparent);
            mRandom = new Random(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000);

        }
    }
}
