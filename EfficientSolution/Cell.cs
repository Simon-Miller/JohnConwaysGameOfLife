using System.Collections.Generic;
using System.Linq;

namespace JohnConwaysGameOfLife.EfficientSolution
{
    public class Cell
    {
        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        private const int LIVING_NEIGHBOURS_TO_CREATE_LIFE = 3; // gotcha!  each cell WITH EXACTLY this value comes to life.  So overpopulation won't fill this.
        private const int SOLITUDE_DEATH_COUNT = 1; // less than or equal to this number.
        private const int DEATH_BY_OVER_POPULATION = 4; // greater than or equal to this number.

        public int X { get; private set; }
        public int Y { get; private set; }

        public List<Cell> GetNeighbours(List<Cell> allLivingCells) =>
            allLivingCells.Where(cell => cell.X >= X - 1 && cell.X <= X + 1 && cell.Y >= Y - 1 && cell.Y <= Y + 1 && cell != this).ToList();


        // Easy to call this for every cell, but we also need to create a list of cells adjacent to living cells to then decide if they can become alive.


        /// <summary>
        /// only call this method on a living cell.  A call to this method returning false should immediately be deleted.
        /// </summary>
        /// <param name="livingNeighbourCells"></param>
        /// <returns></returns>
        public bool DecideIfAlive(List<Cell> livingNeighbourCells)
        {
            var livingNeightbours = livingNeighbourCells.Count;

            // rule: Each cell with ONE or NO neighbors dies, as if by solitude.
            if (livingNeightbours <= SOLITUDE_DEATH_COUNT) return false; // death by solitude

            // rule: Each cell with four or more neighbors dies, as if by overpopulation.
            if (livingNeightbours >= DEATH_BY_OVER_POPULATION)
                return false; // death by over-population

            // each cell with neighbours > solitude && < overpopulation continues to live!
            return true;
        }
    }
}
