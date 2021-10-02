using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace JohnConwaysGameOfLife
{
    // see: https://playgameoflife.com/info

    public partial class Form1 : Form
    {
        private readonly byte ZOOM = 5;

        private const int RND_MAX = 1000000;
        private const float RND_BALANCE = 0.2F; // make more or less TRUE values.

        private const int LIVING_NEIGHBOURS_TO_CREATE_LIFE = 3; // gotcha!  each cell WITH EXACTLY this value comes to life.  So overpopulation won't fill this.
        private const int SOLITUDE_DEATH_COUNT = 1; // less than or equal to this number.
        private const int DEATH_BY_OVER_POPULATION = 4; // greater than or equal to this number.

        private int maxX;
        private int maxY;

        private readonly Bitmap gameScreen;
        private readonly int GAME_WIDTH = 100;
        private readonly int GAME_HEIGHT = 100;

        private const PixelFormat PIXEL_FORMAT = PixelFormat.Format32bppArgb;

        public Form1()
        {
            InitializeComponent();
            this.gameScreen = new Bitmap(GAME_WIDTH, GAME_HEIGHT, PIXEL_FORMAT);

            this.cells = new bool[this.GAME_WIDTH, this.GAME_HEIGHT];

            this.maxX = this.GAME_WIDTH - 1;
            this.maxY = this.GAME_HEIGHT - 1;

            //this.randomSetup();
            this.setup();

            this.button1.Visible = !this.timer1.Enabled;
        }

        // the easiest way, until we can remove the grid (2 indexes, I think) would be a 2 dimensional array
        private bool[,] cells;

        private bool[,] iterationCells;

        private void randomSetup()
        {
            var rnd = new System.Random((int)DateTime.Now.Ticks); // random starting seed.

            var threshold = RND_MAX * RND_BALANCE;

            for (var y = 0; y < GAME_HEIGHT; y++)
            {
                for (int x = 0; x < GAME_WIDTH; x++)
                {
                    var r = rnd.Next(RND_MAX);
                    this.cells[x, y] = (r < threshold);
                }
            }
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

            var offsetX = 20;
            var offsetY = 10;

            // setup;
            for (int y = 0; y < GAME_HEIGHT; y++)
                for (int x = 0; x < GAME_WIDTH; x++)
                    this.cells[x, y] = false;

            for (int y = 0; y < yy; y++)
                for (int x = 0; x < xx; x++)
                    this.cells[x + offsetX, y + offsetY] = s[(y * xx) + x] == '*'; // if a *, then place a live cell in the matrix.
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // paint the game.
            var locker = this.gameScreen.LockBits(new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT), ImageLockMode.ReadWrite, PIXEL_FORMAT);

            unsafe
            {
                // unsafe cast from a void pointer to a typed pointer.
                var pixelPtr = (UInt32*)locker.Scan0.ToPointer();

                // given we know this.width would be one stride...
                for (int y = 0; y < gameScreen.Height; y++)
                {
                    var ptrOffset = y * gameScreen.Width;

                    for (int x = 0; x < gameScreen.Width; x++)
                    {
                        // based on true/false, we write a pixel colour.
                        pixelPtr[ptrOffset + x] = cells[x, y] ? 0xffffffff : 0xff000000;
                    }
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
            // game time!
            var w = this.GAME_WIDTH;
            var h = this.GAME_HEIGHT;

            // put all results into here! We don't want iteraction calculations that write to the array to affect further calculations in the same iteration (tick)
            this.iterationCells = new bool[this.GAME_WIDTH, this.GAME_HEIGHT];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // this is inefficient, as we could write code checking if we're on an edge, and run specialized code for that case.

                    var livingNeightbours = 0;

                    if (isAlive(x - 1, y - 1)) livingNeightbours++;
                    if (isAlive(x, y - 1)) livingNeightbours++;
                    if (isAlive(x + 1, y - 1)) livingNeightbours++;
                    if (isAlive(x - 1, y)) livingNeightbours++;
                    if (isAlive(x + 1, y)) livingNeightbours++;
                    if (isAlive(x - 1, y + 1)) livingNeightbours++;
                    if (isAlive(x, y + 1)) livingNeightbours++;
                    if (isAlive(x + 1, y + 1)) livingNeightbours++;

                    if (cells[x, y])
                    {
                        // currently alive

                        // rule 1: Each cell with ONE or NO neighbors dies, as if by solitude.
                        if (livingNeightbours <= SOLITUDE_DEATH_COUNT)
                        {
                            this.iterationCells[x, y] = false; // death by solitude
                            continue;
                        }

                        // rule 2: Each cell with four or more neighbors dies, as if by overpopulation.
                        if (livingNeightbours >= DEATH_BY_OVER_POPULATION)
                        {
                            this.iterationCells[x, y] = false; // death by over-population
                            continue;
                        }

                        // each cell with neighbours > solitude && < overpopulation continues to live!
                        this.iterationCells[x, y] = true;
                    }
                    else
                    {
                        // currently dead (or empty)
                        this.iterationCells[x, y] = (livingNeightbours == LIVING_NEIGHBOURS_TO_CREATE_LIFE); // overpopulated areas won't breed into this.
                    }
                }
            }

            // swap built response to render area.
            this.cells = this.iterationCells;

            this.Invalidate();
            this.Refresh();

            bool isAlive(int x, int y)
            {
                if (x < 0) return false;
                if (x >= maxX) return false;

                if (y < 0) return false;
                if (y >= maxY) return false;

                return cells[x, y];
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.timer1_Tick(sender, e);
        }
    }





}
