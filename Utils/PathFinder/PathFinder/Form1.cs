using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Utopia.Server.AStar;

namespace PathFinder
{
    public partial class Form1 : Form
    {
        private Size _cellSize = new Size(32, 32);
        public static int[,] Map = new int[64, 64];
        private Bitmap _image;

        private Point _start = new Point();
        private Point _goal = new Point(5,5);
        private Font _font = new Font("Lucida Console", 7);

        private AStarNode2D _selectedNode = null;

        private AStar<AStarNode2D> _pathFinder = new AStar<AStarNode2D>();

        public Form1()
        {
            InitializeComponent();

            _image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = _image;

            for (int x = 0; x < Map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < Map.GetUpperBound(1); y++)
                {
                    Map[x, y] = 1;
                }
            }



            DrawMap();
        }

        void _pathFinder_NodeSelected(object sender, AStar.AStarDebugEventArgs<AStarNode2D> e)
        {
            _selectedNode = e.Node;
            DrawMap();
            Application.DoEvents();
            Thread.Sleep(1000);
        }

        void _pathFinder_NextNode(object sender, EventArgs e)
        {
            
            DrawMap();
            Application.DoEvents();
            Thread.Sleep(400);
        }

        public void DrawMap()
        {
            using (var g = Graphics.FromImage(_image))
            {
                g.Clear(Color.White);

                for (int x = 0; x < Map.GetLength(0); x++)
                {
                    for (int y = 0; y < Map.GetLength(1); y++)
                    {
                        var p = new Point(x * _cellSize.Height, y * _cellSize.Width);

                        if (Map[x, y] == -1)
                            g.FillRectangle(Brushes.Black, new Rectangle(p, _cellSize));

                        if (Map[x, y] == 5)
                            g.FillRectangle(Brushes.Gray, new Rectangle(p, _cellSize));

                        if (p == _start)
                            g.FillRectangle(Brushes.Green, new Rectangle(p, _cellSize));

                        if (x == _goal.X && y == _goal.Y)
                            g.FillRectangle(Brushes.Red, new Rectangle(p, _cellSize));
                    }
                }

                foreach (var node in _pathFinder.EnumerateOpen())
                {
                    var move = 2;
                    g.DrawEllipse(Pens.LightBlue, new Rectangle(_cellSize.Width * node.X + move, _cellSize.Height * node.Y + move, _cellSize.Width - move * 2, _cellSize.Height - move * 2));
                    g.DrawString(node.GoalEstimate.ToString(), _font, Brushes.Black, _cellSize.Width * node.X, _cellSize.Height * node.Y);
                    g.DrawString(node.Cost.ToString(), _font, Brushes.Black, _cellSize.Width * node.X, _cellSize.Height * node.Y + _cellSize.Height/2);
                }

                foreach (var node in _pathFinder.EnumerateClosed())
                {
                    var move = 3;
                    g.FillEllipse(Brushes.Blue, new Rectangle(_cellSize.Width * node.X + move, _cellSize.Height * node.Y + move, _cellSize.Width - move * 2, _cellSize.Height - move * 2));
                    g.DrawString(node.GoalEstimate.ToString(), _font, Brushes.Black, _cellSize.Width * node.X, _cellSize.Height * node.Y);
                    g.DrawString(node.Cost.ToString(), _font, Brushes.Black, _cellSize.Width * node.X, _cellSize.Height * node.Y + _cellSize.Height / 2);
                }

                foreach (var aStarNode2D in _pathFinder.Solution)
                {
                    var p = new Point(aStarNode2D.X * _cellSize.Height, aStarNode2D.Y * _cellSize.Width);
                    g.FillRectangle(Brushes.Blue, new Rectangle(p, _cellSize));
                }

                if (_selectedNode != null)
                {
                    var p = new Point(_selectedNode.X * _cellSize.Height, _selectedNode.Y * _cellSize.Width);
                    g.DrawRectangle(Pens.Red, new Rectangle(p, _cellSize));
                    g.DrawString(_selectedNode.GoalEstimate.ToString(), _font, Brushes.Black, _cellSize.Width * _selectedNode.X, _cellSize.Height * _selectedNode.Y);
                    g.DrawString(_selectedNode.Cost.ToString(), _font, Brushes.Black, _cellSize.Width * _selectedNode.X, _cellSize.Height * _selectedNode.Y + _cellSize.Height / 2);
                }

            }

            pictureBox1.Image = _image;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X / _cellSize.Width;
            int y = e.Y / _cellSize.Height;

            if (e.Button == MouseButtons.Left)
            {
                if (Map[x, y] == 1)
                    Map[x, y] = -1;
                else Map[x, y] = 1;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                Map[x, y] = 5;
            }

            if (e.Button == MouseButtons.Right)
            {
                _goal = new Point(x, y);
            }


            DrawMap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var goal = new AStarNode2D(null, null, 1, _goal.X, _goal.Y);
            _pathFinder = new AStar<AStarNode2D>();
            _pathFinder.NextNode += _pathFinder_NextNode;
            _pathFinder.NodeSelected += _pathFinder_NodeSelected;
            _pathFinder.FindPath(new AStarNode2D(null, goal, 1, _start.X, _start.Y));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _pathFinder = new AStar<AStarNode2D>();
            DrawMap();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _pathFinder = new AStar<AStarNode2D>();


            var goal = new AStarNode2D(null, null, 1, _goal.X, _goal.Y);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                _pathFinder.FindPath(new AStarNode2D(null, goal, 1, _start.X, _start.Y));
            }
            sw.Stop();
            DrawMap();
            MessageBox.Show((sw.Elapsed.TotalMilliseconds / (double)numericUpDown1.Value).ToString() + "ms");


        }
    }
}
