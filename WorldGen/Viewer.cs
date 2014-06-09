// The viewer allows design changes to be easily viewed without loading the map into the game engine. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace WorldGen {
    public partial class Viewer : Form {

        Generator parent;
        public Viewer(Generator parent) {
            this.parent = parent;
            InitializeComponent();
            int posY, posX;
            long filepos;
            FileStream indata = File.OpenRead("level.txt");
            BinaryReader binread = new BinaryReader(indata);
            size = binread.ReadInt32();
            ResizeWin(size);

            colors = new Brush[10];
            colors[0] = Brushes.White;
            colors[1] = Brushes.SaddleBrown;
            colors[2] = Brushes.DimGray;
            colors[3] = Brushes.Green;
            colors[4] = Brushes.BurlyWood;
            colors[5] = Brushes.DarkGreen;
            colors[6] = Brushes.Blue;
            colors[7] = Brushes.Black;
            colors[8] = Brushes.OrangeRed;
            colors[9] = Brushes.Silver;

            grid = new byte[size, size];
            filepos = indata.Position;
            for (posX = 0; posX < size; posX++)
                for (posY = 0; posY < size; posY++)
                    grid[posY, posX] = binread.ReadByte();

            indata.Close();
        }
        private Brush[] colors;
        byte[,] grid;
        int size;

        //provides an easy way to change the drawing size
        int scaling = 1;

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            paint(e.Graphics);

        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            parent.Close();
        }

        public void paint(Graphics g) {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++) {
                    if (grid[x,y] != 0) 
                        g.FillRectangle(colors[grid[x,y]], x*scaling,y*scaling,scaling,scaling);
                }
            g.Dispose();
        }

        // Sizes the window based on the map size
        public void ResizeWin(int size) {
            Width = size * scaling;
            Height = size * scaling;
        }

        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // Viewer
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Viewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Viewer";
            this.ResumeLayout(false);

        }
    }
}
