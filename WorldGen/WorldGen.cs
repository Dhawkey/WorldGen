//Doug Hawkey
// Procedurally generate terrain in a 2D array of bytes and write to a file.

// Air      = 0
// Dirt     = 1
// Stone    = 2
// Grass    = 3
// Wood     = 4
// Leaves   = 5
// Water    = 6
// Coal     = 7
// Copper   = 8
// Iron     = 9
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WorldGen
{

    public partial class Generator : Form
    {

        static int size;
        static Random rand;
        static byte[,] grid;

        static bool[] lakeSpots;

        public Generator()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            FileStream indata = File.OpenWrite("level.txt");
            BinaryWriter binwrite = new BinaryWriter(indata);
            size = Convert.ToInt32(nudSize.Value);
            rand = new Random();
            grid = new byte[size, size];

             //Increased hillWidth will lead to more gradual hills and longer lakes assuming fixed map size.
            // The size must be a multiple of hillWidth
            int hillWidth = 100;
           
            FindLakeSpots(hillWidth, .3f);
            FillDirtandStone(hillWidth, 50);
            AddMinerals(8, 140);
            AddMinerals(9, 160);
            GenerateCaves();
            MakeLakes(hillWidth);
            AddGrass();
            AddTrees();
            
            binwrite.Write(size);

             // Writes the 2d array of bytes to a file.
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    indata.WriteByte(grid[y, x]);

            indata.Close();

            if (checkBox1.Checked)
            {
                this.Visible = false;
                Viewer viewer = new Viewer(this);
                viewer.Visible = true;
            }
            else
                this.Close();
        }
        //Uses a Perlin noise function to generate smooth hills.
        static void FillDirtandStone(int hillWidth, int hillHeight) 
        {
            int lakeIndex = 0;
            float newGradient = ((float)rand.NextDouble() - 0.5f) * 2f;
            if (lakeSpots[0]) newGradient += 3f;
            float lastGradient = newGradient;

            for (int blocktype = 1; blocktype < 3; blocktype++)
                for (int x = 0; x < size; x++)
                {
                    float fx = (float)x / (hillWidth / blocktype);
                    float distanceFromLeftPoint = fx - (int)fx;
                    float distanceFromRightPoint = distanceFromLeftPoint - 1f;

                    // Checks if a new gradient needs to be made.
                    if (x % (hillWidth / blocktype) == 0) 
                    {
                        lastGradient = newGradient;
                        newGradient = ((float)rand.NextDouble() - 0.5f) * 2f;
                        if (blocktype == 1 && lakeSpots[++lakeIndex])
                            newGradient += 3f;
                    }
                    float leftInfluence = lastGradient * distanceFromLeftPoint;
                    float rightInfluence = newGradient * distanceFromRightPoint;
                    fx = Smooth(distanceFromLeftPoint);
                    float yValue = (leftInfluence + fx * (rightInfluence - leftInfluence)) * hillHeight / blocktype + 50f + 50f * blocktype;
                    for (int y = (int)yValue; y < size; y++)
                        grid[y, x] = (byte)blocktype;
                }
        }

        static float Smooth(float x) 
        {
            return x * x * (3f - 2f * x);
        }

        // Creates a list with bool values to determine how many lakes will be made.
        static void FindLakeSpots(int hillWidth, float chance)
        
        {
            lakeSpots = new bool[(int)Math.Ceiling((double)(size / hillWidth)) + 1];
            for (int i = 0; i < lakeSpots.Length; i++) 
                if (chance >= rand.NextDouble()) lakeSpots[i] = true;
                    
        
        }
        // Finds the first yVal value of a given blocktype at a given x value.
        static int FindY(int xVal, int blocktype)
        {
            for (int y = 0; y < size; y++)
                if (grid[y, xVal] == blocktype || (blocktype == -1 && grid[y, xVal] != 0)) return y;       
            return 0;
        }

        // Creates caves by generating a random position, length, and direction.
        static void GenerateCaves()
        {
            for (int y = 120; y < size; y += 40) 
                for (int x = 0; x < size; x += 40) 
                {
                    int xStart = rand.Next(50);
                    int yStart = rand.Next(50);
                    int length = rand.Next(2,30);
                    int direction = rand.NextDouble() < 0.5 ? -1 : 1;
                    int holeRadius = rand.Next(1, 5);
                    double curve = rand.NextDouble() * 4;
                    for (int k = 0; k < length; k++)
                        MakeHole(y + yStart + direction * (int)(Math.Sqrt(k) * curve), x + xStart + k, holeRadius, 0);

                }

        }
        // This function is used to generate a circle and was used for caves and treetops.
        static void MakeHole(int yPosition, int xPosition, int radius, byte blocktype)
        {
            for (int x = -radius; x <= radius; x++) 
                for (int y = -radius; y <= radius; y++) 
                    if (y * y + x * x <= radius * radius) 
                        if (y + yPosition >= 0 && y + yPosition < size && x + xPosition >= 0 && x + xPosition < size)
                            grid[y + yPosition, x + xPosition] = blocktype;
        }

        // Fills in water to make lakes.
        static void MakeLakes(int hillWidth) 
        {
            for (int lakeLocation = 0; lakeLocation < lakeSpots.Length; lakeLocation++) 
                if (lakeSpots[lakeLocation]) 
                {
                    int lakeStart = lakeLocation * hillWidth;
                    if (lakeStart >= size) return;
                    int lakeEnd = lakeStart;
                    int yVal = FindY(lakeStart, -1);
                    while (++lakeEnd < size && grid[yVal, lakeEnd] == 0);
                    for (int x = lakeStart + 1; x < lakeEnd; x++) 
                    {
                        if (x >= size) return;
                        int y = yVal;
                        while (grid[y, x] == 0)
                        {
                            grid[y, x] = 6;
                            y++;
                        }
                    }
                }
        }

        // Adds a layer of grass to the top dirt layer.
        static void AddGrass()
        {
            int y, x;
            for (x = 0; x < size; x++)
            {
                y = FindY(x, 1);
                if (y != 0 && grid[y - 1, x] == 0)
                    grid[y, x] = 3;
            }
        }
        // Finds locations for trees to be added.
        static void AddTrees()
        {
            int y, x, treeOrNot, treeHeight, leafRadius, lastBigTreeLocation = 0;
            for (x = 0; x < size; x++)
            { 
                y = FindY(x, 3);
                while (grid[y, x] != 3) 
                {
                    x++;
                    y = FindY(x, 3);
                }
                treeOrNot = rand.Next(100);
                if (treeOrNot > 97 && lastBigTreeLocation < x - 8) 
                {
                    treeHeight = rand.Next(20, 30);
                    leafRadius = rand.Next(8, 12);
                    GrowTree(treeHeight, leafRadius, y, x);
                    lastBigTreeLocation = x;
                }
                else if (treeOrNot > 90) 
                {
                    treeHeight = rand.Next(6, 9);
                    leafRadius = rand.Next(2, 4);
                    GrowTree(treeHeight, leafRadius, y, x);
                }
                
            }
        }
        // Creates the tree.
        static void GrowTree(int height, int leafRadius, int y, int x)
        {
            int currentHeight;
            for (currentHeight = 0; currentHeight < height; currentHeight++)
                grid[y - currentHeight, x] = 4;
            MakeHole(y - currentHeight - 1, x, leafRadius, 5);
        }

        // Adds minerals throughout the map.
        static void AddMinerals(byte oreType, int yVal)
        {
            int x, y, yMax, xMax, oreChance, oreCount, orePlaced;
            for (y = yVal; y < size; y++)
                for (x = 0; x < size; x++)
                {
                    oreChance = rand.Next(1000);
                    orePlaced = 0;
                    if (oreChance > 998)
                    {
                        if (grid[y, x] != 0) 
                            grid[y, x] = oreType;
                        oreCount = rand.Next(1, 5);
                        yMax = y;
                        xMax = x;
                        while (orePlaced < oreCount && x > 5)
                            for (y -= 3; y < yMax; y++)
                                for (x -= 3; x < xMax; x++)
                                {
                                    oreChance = rand.Next(1, 15);
                                    if (oreChance == 1)
                                    {
                                        grid[y, x] = oreType;
                                        orePlaced++;
                                    }
                                }
                    }
                }       
        }

    }
}

