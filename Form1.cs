using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections;

namespace GeneticSoduku
{
    public partial class Form1 : Form
    {

        int Crossovers, Mutations;
        int Population, Dimension;
        int Best, cx = 0, cy = 0;
        int[,] Parents, Offsprings, Collision;
        String[,] draw = new String[9, 9];
        bool[,] HasCollision = new bool[9, 9];
        int[] ParentsFitness, OffspringsFitness;
        int[] Constant=new int[81];
        Random random = new Random();
        Thread thread;
        int h = 0, s = 0, Counter=0;
        bool First = false;

        private void Init()
        {
            Population = Convert.ToInt32(textBox1.Text);

            Dimension = 81;
            Parents = new int[Population, Dimension];
            Offsprings = new int[Population, Dimension];
            Collision = new int[Population, Dimension];
            ParentsFitness = new int[Population];
            OffspringsFitness = new int[Population];

            Crossovers = 0;
            Mutations = 0;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            thread = new Thread(ThreadFunction);
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                {
                    draw[x, y] = "";
                    HasCollision[x, y] = false;
                }
            for (int x = 0; x < Dimension; x++)
                Constant[x] = 0;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int c1 = 1, c2 = 1;
            for (int x = 0; x <= 9; x++)
                for (int y = 0; y <= 9; y++)
                {
                    int r;
                    Math.DivRem(x, 3,out r);
                    if (r == 0) c1 = 3;
                    else c1 = 1;
                    Math.DivRem(y, 3, out r);
                    if (r == 0) c2 = 3;
                    else c2 = 1;
                    e.Graphics.DrawLine(new Pen(Color.Black, c1), x * 40, 0, x * 40, 360);
                    e.Graphics.DrawLine(new Pen(Color.Black, c2), 0, y * 40, 360, y * 40);
                    if (x != 9 && y != 9)
                    {
                        if (HasCollision[x,y] == true)
                            e.Graphics.FillRectangle(Brushes.Red, x * 40 + 3, y * 40 + 3, 34, 34);
                        if (Constant[y * 9 + x] == 0) 
                        e.Graphics.DrawString(draw[x, y].ToString(), new Font("Arial", 20), Brushes.Black, x * 40 + 9, y * 40 + 5);
                        else
                            e.Graphics.DrawString(draw[x, y].ToString(), new Font("Arial", 20), Brushes.SkyBlue, x * 40 + 9, y * 40 + 5);

                    }
                }
        }

        private void ShowResult(int n)
        {
            for (int dimension = 0; dimension < Dimension; dimension++)
            {
                int x, y;
                Math.DivRem(dimension,9,out x);
                y = dimension / 9;
                pictureBox1.CreateGraphics().FillRectangle(SystemBrushes.Control,x*40+2,y*40+2,35,35);
                if (Collision[n, dimension] == 1)
                {
                    pictureBox1.CreateGraphics().FillRectangle(Brushes.Red, x * 40 + 3, y * 40 + 3, 34, 34);
                    HasCollision[x, y] = true;
                }
                else
                    HasCollision[x, y] = false;
                if (Constant[dimension] == 0)
                    pictureBox1.CreateGraphics().DrawString(Offsprings[n, dimension].ToString(), new Font("Arial", 20), Brushes.Black, x * 40 + 9, y * 40 + 5);
                else
                    pictureBox1.CreateGraphics().DrawString(Offsprings[n, dimension].ToString(), new Font("Arial", 20), Brushes.SkyBlue, x * 40 + 9, y * 40 + 5);
                draw[x, y] = Offsprings[n, dimension].ToString();
            }
        }


        private void GenerateFirstPopulation()
        {
            ArrayList[] all = new ArrayList[3];
            all[0] = new ArrayList();
            all[1] = new ArrayList();
            all[2] = new ArrayList();
            int c=-1;
            int[] tt = { 0, 1, 2, 9, 10, 11, 18, 19, 20 };
            for (int population = 0; population < Population; population++)
            {
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    int rem;
                    Math.DivRem(dimension, 27, out rem);
                    if (rem == 0)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 1; x <= 9; x++)
                                all[y].Add(x);
                            for (int x = 0; x < 9; x++)
                                if (Constant[dimension + tt[x] + y * 3] != 0)
                                    all[y].Remove(Constant[dimension + tt[x] + y * 3]);
                        }
                    }
                    Math.DivRem(dimension, 3, out rem);
                    if (rem == 0)
                        c++;
                    if (c == 3) c = 0;
                    if (Constant[dimension] == 0)
                    {
                        
                        int r = random.Next(all[c].Count);
                        Parents[population, dimension] = Convert.ToInt32(all[c][r]);
                        all[c].RemoveAt(r);
                    }
                    else Parents[population, dimension] = Constant[dimension];
                }
            }
        }
        private void GenerateNewPopulation()
        {
            ArrayList[] all = new ArrayList[3];
            all[0] = new ArrayList();
            all[1] = new ArrayList();
            all[2] = new ArrayList();
            int c = -1;
            int[] tt = { 0, 1, 2, 9, 10, 11, 18, 19, 20 };
            for (int population = Population/2; population < Population; population++)
            {
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    int rem;
                    Math.DivRem(dimension, 27, out rem);
                    if (rem == 0)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 1; x <= 9; x++)
                                all[y].Add(x);
                            for (int x = 0; x < 9; x++)
                                if (Constant[dimension + tt[x] + y * 3] != 0)
                                    all[y].Remove(Constant[dimension + tt[x] + y * 3]);
                        }
                    }
                    Math.DivRem(dimension, 3, out rem);
                    if (rem == 0)
                        c++;
                    if (c == 3) c = 0;
                    if (Constant[dimension] == 0)
                    {
                        int r = random.Next(all[c].Count);
                        Parents[population, dimension] = Convert.ToInt32(all[c][r]);
                        all[c].RemoveAt(r);
                    }
                    else Parents[population, dimension] = Constant[dimension];
                }
            }
        }

        private void Evaluate(int[,] Parameter, int[] FitnessParameter)
        {
            for (int population = 0; population < Population; population++)
            {
                FitnessParameter[population] = 0;
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    Collision[population, dimension] = 0;
                }
            }
            for (int population = 0; population < Population; population++)
            {
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    int r, end = dimension;
                    do
                    {
                        end++;
                        Math.DivRem(end, 9, out r);
                    } while (r != 0);
                    for (int x = dimension + 9; x < Dimension; x += 9)
                        if (Parameter[population, x] == Parameter[population, dimension])
                        {
                            FitnessParameter[population]++;
                            Collision[population, dimension] = 1;
                            Collision[population, x] = 1;
                        }
                    for (int x = dimension + 1; x < end; x ++)
                        if (Parameter[population, x] == Parameter[population, dimension])
                        {
                            FitnessParameter[population]++;
                            Collision[population, dimension] = 1;
                            Collision[population, x] = 1;
                        }
                }
            }
        }

        private void SelectParents(int[,] Parameter, int[] FitnessParameter)
        {
            int _Population = FitnessParameter.Length;
            int[,] TempParents = new int[Population, Dimension];

                int current = 0, top = 0;
                int[] SortIndex = new int[_Population];
                for (int population = 0; population < _Population; population++)
                {
                    current = 0;
                    while (current <= top)
                    {
                        if (FitnessParameter[population] <= FitnessParameter[SortIndex[current]] || current == top)
                        {
                            for (int counter = top - 1; counter >= current; counter--)
                            {
                                SortIndex[counter + 1] = SortIndex[counter];
                            }
                            SortIndex[current] = population;
                            top++;
                            break;
                        }
                        else current++;
                    }
                }
                for (int population = 0; population < Population; population++)
                {
                    for (int dimension = 0; dimension < Dimension; dimension++)
                    {
                        TempParents[population, dimension] = Parameter[SortIndex[population], dimension];
                    }
                }

            for (int population = 0; population < Population; population++)
            {
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    Parents[population, dimension] = TempParents[population, dimension];
                }
            }
        }

        private void Crossover()
        {
            int ParentsCounter = 0, CrossoverPoint;
            while (ParentsCounter < Population / 2)
            {
                double RandomNumber = random.NextDouble();
                if (RandomNumber >= 0.98) CrossoverPoint = Dimension;
                else { CrossoverPoint = random.Next(Dimension - 1) + 1; Crossovers++; }
                for (int dimension = 0; dimension < CrossoverPoint; dimension++)
                {
                    Offsprings[ParentsCounter * 2, dimension] = Parents[ParentsCounter * 2, dimension];
                    Offsprings[ParentsCounter * 2 + 1, dimension] = Parents[ParentsCounter * 2 + 1, dimension];
                }
                for (int dimension = CrossoverPoint; dimension < Dimension; dimension++)
                {
                    Offsprings[ParentsCounter * 2, dimension] = Parents[ParentsCounter * 2 + 1, dimension];
                    Offsprings[ParentsCounter * 2 + 1, dimension] = Parents[ParentsCounter * 2, dimension];
                }
                ParentsCounter++;
            }
        }

        private void Mutate()
        {
            for (int population = 0; population < Population; population++)
            {
                for (int dimension = 0; dimension < Dimension; dimension++)
                {
                    if (Constant[dimension] == 0)
                    {
                        double RandomNumber = random.NextDouble();
                        if (RandomNumber < 0.1)
                        {
                            Offsprings[population, dimension] = random.Next(9) + 1;
                            Mutations++;
                        }
                    }
                }
            }
        }

        private void Fix()
        {
            int[] temp = new int[9];
            int[] t = { 0, 3, 6, 27, 30, 33, 54, 57, 60 };
            int[] tt = { 0, 1, 2, 9, 10, 11, 18, 19, 20 };
            for (int population = 0; population < Population; population++)
            {
                for (int d = 0; d < 9; d++)
                {
                    for (int dd = 0; dd < 9; dd++)
                        temp[dd] = Offsprings[population, t[d] + tt[dd]];
                    ArrayList s = new ArrayList();
                    for (int dd = 1; dd <= 9; dd++) s.Add(dd);
                    for (int dd = 0; dd < 9; dd++)
                        if (s.Contains(temp[dd]))
                            s.Remove(temp[dd]);
                    for (int dd = 0; dd < 9; dd++)
                        for (int ddd = dd + 1; ddd < 9; ddd++)
                            if (temp[dd] == temp[ddd])
                            {
                                int r=new Random().Next(s.Count);
                                int r2 = new Random().Next(2);
                                if(Constant[ t[d] + tt[dd]]!=0)
                                    r2=1;
                                if (Constant[t[d] + tt[ddd]] != 0)
                                    r2=0;
                                if(r2==0)
                                    temp[dd] = Convert.ToInt32(s[r]);
                                else
                                    temp[ddd] = Convert.ToInt32(s[r]);
                                s.Remove(s[r]);
                            }
                    for (int dd = 0; dd < 9; dd++)
                        Offsprings[population, t[d] + tt[dd]] = temp[dd];
                }
            }
        }

        private void SelectNextPopulation()
        {
                int[,] NewGeneration = new int[2 * Population, Dimension];
                int[] NewGenerationFitness = new int[2 * Population];
                for (int population = 0; population < Population; population++)
                {
                    for (int dimension = 0; dimension < Dimension; dimension++)
                    {
                        NewGeneration[population, dimension] = Parents[population, dimension];
                        NewGeneration[population + Population, dimension] = Offsprings[population, dimension];
                    }
                    NewGenerationFitness[population] = ParentsFitness[population];
                    NewGenerationFitness[population + Population] = OffspringsFitness[population];
                }
                SelectParents(NewGeneration, NewGenerationFitness);
        }



        private void ThreadFunction()
        {
            GenerateFirstPopulation();
            int iteration = 0;
            while(true)
            {
                iteration++;
                Evaluate(Parents, ParentsFitness);
                SelectParents(Parents, ParentsFitness);
                Crossover();
                Mutate();
                Fix();
                Evaluate(Offsprings, OffspringsFitness);
                this.Invoke(new update(Update), iteration);
                SelectNextPopulation();
                if (First == true)
                {
                    GenerateFirstPopulation();
                    First = false;
                }
            }
        }

        public delegate void update(int iteration);
        private void Update(int iteration)
        {
            label19.Text = (iteration + 1).ToString();
            Best = OffspringsFitness.Min();
            for (int x = 0; x < OffspringsFitness.Length; x++)
                if (OffspringsFitness[x] == OffspringsFitness.Min())
                {
                    ShowResult(x);
                    break;
                }
            label13.Text = Best.ToString();
            label15.Text = Crossovers.ToString();
            label17.Text = Mutations.ToString();
            if (Best == 0)
            {
                thread.Abort();
                timer1.Enabled = false;
                timer2.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            thread.Abort();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
            textBox6.Left = pictureBox1.Left + (e.X / 40)*40 + 3;
            textBox6.Top = pictureBox1.Top + (e.Y / 40) * 40 + 3;
            cx = e.X / 40;
            cy = e.Y / 40;
            textBox6.Text = "";
            textBox6.Visible = true;
            textBox6.Focus();
        }

        private void textBox6_Leave(object sender, EventArgs e)
        {
            if (textBox6.Visible == true)
            {
                textBox6.Visible = false;
                try
                {
                    if (textBox6.Text != "" && Convert.ToInt32(textBox6.Text) <= 9 && Convert.ToInt32(textBox6.Text) > 0)
                    {
                        draw[cx, cy] = textBox6.Text;
                        Constant[cy * 9 + cx] = Convert.ToInt32(textBox6.Text);
                    }
                    else
                    {
                        draw[cx, cy] = "";
                        Constant[cy * 9 + cx] = 0;
                    }
                }
                catch
                {
                    draw[cx, cy] = "";
                    Constant[cy * 9 + cx] = 0;
                }
            }
        }
        ///////////////////////////////////////////////////////Button Section
        /// <summary>
        /// Start Button
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            h = 0;
            s = 0;
            label2.Text = "00:00";
            timer1.Enabled = true;
            Counter = 0;
            timer2.Enabled = true;
            Init();
            thread = new Thread(ThreadFunction);
            button2.Enabled = true;
            thread.Start();
        }
        /// <summary>
        /// Stop Button
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            thread.Abort();
            timer1.Enabled = false;
            timer2.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }
        /// <summary>
        ///Reset Button 
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                {
                    draw[x, y] = "";
                    HasCollision[x, y] = false;
                }
            for (int x = 0; x < Dimension; x++)
                Constant[x] = 0;
            pictureBox1.Refresh();
            label2.Text = "00:00";
            label19.Text = string.Empty;
            label13.Text = string.Empty;
            label15.Text = string.Empty;
            label17.Text = string.Empty;
        }
        ///////////////////////////////////////////////////////End Button Section
        /// <summary>
        /////////////////////////////////////////////////////// Timer Section
        /// </summary>

        private void timer1_Tick(object sender, EventArgs e)
        {
            s++;
            if (s == 60)
            {
                h++;
                s = 0;
            }
            label2.Text = h.ToString().PadLeft(2, '0') + ":" + s.ToString().PadLeft(2, '0');
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Counter++;
            if (Counter == 60)
            {
                First = true;
                Counter = 0;
            }
        }
    }
}
