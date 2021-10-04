using JohnConwaysGameOfLife.EfficientSolution;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace JohnConwaysGameOfLife
{
    // see: https://playgameoflife.com/info

    public partial class Form2 : Form
    {
        private readonly byte ZOOM = 8;

        private const int RND_MAX = 1000000;
        private const float RND_BALANCE = 0.2F; // make more or less TRUE values.

        private const int LIVING_NEIGHBOURS_TO_CREATE_LIFE = 3; // gotcha!  each cell WITH EXACTLY this value comes to life.  So overpopulation won't fill this.
        private const int SOLITUDE_DEATH_COUNT = 1; // less than or equal to this number.
        private const int DEATH_BY_OVER_POPULATION = 4; // greater than or equal to this number.

        private int maxX;
        private int maxY;

        private readonly Bitmap gameScreen;
        private readonly int GAME_WIDTH = 120;
        private readonly int GAME_HEIGHT = 120;

        private const PixelFormat PIXEL_FORMAT = PixelFormat.Format32bppArgb;

        private CellsSystem cellsSystem;
        private int ViewX = 0;
        private int ViewY = 0;

        public Form2()
        {
            InitializeComponent();
            this.gameScreen = new Bitmap(GAME_WIDTH, GAME_HEIGHT, PIXEL_FORMAT);

            this.maxX = this.GAME_WIDTH - 1;
            this.maxY = this.GAME_HEIGHT - 1;

            this.setup();

            this.butIterate.Visible = !this.timer1.Enabled;
        }

        private void setup()
        {
            var xx = 12;
            var yy = 10;

            var s = "            " +
                    "            " +
                    "  *         " +
                    "* *         " +
                    " **         " +
                    "           *" +
                    "         ** " +
                    "          **" +
                    "            " +
                    "            ";

            var offsetX = 30;
            var offsetY = 5;

            // setup;

            var startCells = new List<Cell>();

            for (int y = 0; y < yy; y++)
                for (int x = 0; x < xx; x++)
                    if (s[(y * xx) + x] == '*')
                        startCells.Add(new Cell(x + offsetX,y + offsetY));

            this.cellsSystem = new CellsSystem(startCells);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var cellsToPaint = this.cellsSystem.GetVisibleCells(this.ViewX, this.ViewY, this.ViewX + this.maxX, this.ViewY + this.maxY);

            // paint the game.
            var locker = this.gameScreen.LockBits(new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT), ImageLockMode.ReadWrite, PIXEL_FORMAT);

            unsafe
            {
                // unsafe cast from a void pointer to a typed pointer.
                var pixelPtr = (UInt32*)locker.Scan0.ToPointer();

                // clear screen
                var lastByte = GAME_WIDTH * GAME_HEIGHT;
                for (int i = 0; i < lastByte; i++)
                    pixelPtr[i] = 0xFF000000;

                foreach(var cell in cellsToPaint)
                {
                    var ptrOffset = (cell.Y - this.ViewY) * gameScreen.Width;
                    pixelPtr[ptrOffset + (cell.X - this.ViewX)] = 0xFFffffff;
                }
            }

            gameScreen.UnlockBits(locker);

            var destRect = new Rectangle(0, 0, GAME_WIDTH * ZOOM, GAME_HEIGHT * ZOOM);
            var sourceRect = new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT);

            //e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(this.gameScreen, destRect, sourceRect, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// process every 'cell' in the game of life.  One iteration of the 'game'.
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.cellsSystem.Iterate();

            this.Invalidate();
            this.Refresh();
        }

        private void butIterate_Click(object sender, EventArgs e)
        {
            this.timer1_Tick(sender, e);
        }

        private void butReset_Click(object sender, EventArgs e)
        {
            this.setup();
            this.Invalidate();
            this.Refresh();
        }
    }
}
