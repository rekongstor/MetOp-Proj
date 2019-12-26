﻿using System;
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
        Graphics go1, go2;
        Robot mRobot;
        Field mFiled;
        int bf;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mRobot = new Robot();
            mFiled = new Field();
            graphicsObj = pictureBox1.CreateGraphics();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //while (mRobot.x < 0.99 || mRobot.y < 0.99)
            //    mRobot.Simulate(ref mFiled);

            while (mRobot.x < 0.99 || mRobot.y < 0.99)
            {
                graphicsObj.Clear(Color.White);
                mRobot.Randomize();
                mRobot.Draw(ref graphicsObj);
                mFiled.Draw(ref graphicsObj);
                mRobot.Simulate(ref mFiled, graphicsObj);
            }
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
            for (int z = mRandom.Next(3, 7); z > 0; --z)
            {
                mZones.Add(new Zone(mRandom.NextDouble(), mRandom.NextDouble(), mRandom.NextDouble()*0.2));
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
        int n;
        bool stopped;

        public void Simulate(ref Field f, Graphics graphicsObj = null)
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
                x += vx * dt;
                y += vy * dt;
                xy a = qt.GetA(x, y);
                vx += a.x;
                vy += a.y;
                double nr = vx * vx + vy * vy;
                if (n > 1.0)
                {
                    vx /= nr;
                    vy /= nr;
                }

                if (n > 5000)
                {
                    break;
                }

                if (x < 0.0 || x > 1.0 || y < 0.0 || y > 1.0)
                    stopped = true;

                foreach (Zone z in f.mZones)
                {
                    if ((x - z.x) * (x - z.x) + (y - z.y) * (y - z.y) <= z.r * z.r)
                        stopped = true;
                }

                if (stopped && x > 0.001 && y > 0.001 && x < 0.99 && y < 0.99)
                    qt.SplitHere(x, y);

                if (graphicsObj != null)
                {
                    graphicsObj.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)(x * 500), (int)((1.0 - y) * 500), 2, 2));
                }
                ++n;
                time += dt;
                qt.SetTime(x, y, time);
            } while (!stopped);
        }

        public Robot()
        {
            qt = new QuadTree();
            time = 0.0;
            fmax = 1.0;
            dt = 0.001; // TODO:
            vx = 0.0;
            vy = 0.0;
            x = 0.0;
            y = 0.0;
            n = 0;
            // Инициализируем рандом и графику
        }
        public void Draw(ref Graphics graphics)
        {
            qt.Draw(ref graphics);
        }
        public void Randomize()
        {
            qt.Randomize(time);
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
            if (tmax > 0.0001)
            {
                if (child00 == null) // если у нас нет детей, то рисуем цвета
                {
                    ax += ((time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax)) * ((mRandom.NextDouble() - 0.45) * 1.2);
                    ay += ((time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax) * (time / tmax)) * ((mRandom.NextDouble() - 0.45) * 1.2);
                    if (ax > 1.0) ax = 1.0;
                    if (ay > 1.0) ay = 1.0;
                    if (ax < -1.0) ax = -1.0;
                    if (ax < -1.0) ax = -1.0;
                    double n = Math.Sqrt(ax * ax + ay * ay);
                    ax /= n * 1.1;
                    ay /= n * 1.1;
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

        public void Split()
        {
            if (depth < 10)
            {
                child00 = new QuadTree(x, y, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth);
                child01 = new QuadTree(x, y + w / 2.0, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth);
                child10 = new QuadTree(x + w / 2.0, y, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth);
                child11 = new QuadTree(x + w / 2.0, y + w / 2.0, w / 2.0, ax, ay, mBrush, mColor, mRandom, depth);
            }
        }

        public QuadTree(double x_, double y_, double w_, double ax_, double ay_, SolidBrush mb, Color c, Random mr, int d)
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
        }
        public void Draw(ref Graphics graphicsObj)
        {
            if (child00 == null) // если у нас нет детей, то рисуем цвета
            {
                mColor = Color.FromArgb((int)((ax + 1.0) / 2.0 * 255.0), (int)((ay + 1.0) / 2.0 * 255.0), 255);
                mBrush.Color = mColor;
                graphicsObj.FillRectangle(mBrush, new Rectangle((int)(x * 500), (int)((1.0 - y - w) * 500), (int)(w * 500), (int)(w * 500)));
            }
            else // иначе рекурсивно рисуем
            {
                child00.Draw(ref graphicsObj);
                child01.Draw(ref graphicsObj);
                child10.Draw(ref graphicsObj);
                child11.Draw(ref graphicsObj);
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
