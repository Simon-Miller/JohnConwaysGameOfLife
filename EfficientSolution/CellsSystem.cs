using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnConwaysGameOfLife.EfficientSolution
{
    public class CellsSystem
    {
        private const int LIVING_NEIGHBOURS_TO_CREATE_LIFE = 3; // gotcha!  each cell WITH EXACTLY this value comes to life.  So overpopulation won't fill this.

        public CellsSystem(List<Cell> startingCells)
        {
            this.livingCells = startingCells;
        }

        /// <summary>
        /// represent the cells at the end of an iteration that need to be rendered to the screen.
        /// </summary>
        private List<Cell> livingCells;

        public void Iterate()
        {
            var candidatesForNewLife = this.getCandidatesForNewLife(this.livingCells);

            var cellsToKill = this.getCellsToKill(this.livingCells);

            var newIterationLivingCells = this.getCellsThatSurviveIteration(this.livingCells, cellsToKill);

            this.addNewCellsForThisIteration(candidatesForNewLife, ref newIterationLivingCells);

            // BOOM!
            this.livingCells = newIterationLivingCells;
        }

        public List<Cell> GetVisibleCells(int screenX, int screenY, int screenXMax, int screenYMax) =>
            this.livingCells.Where(cell => cell.X >= screenX && cell.X <= screenXMax && cell.Y >= screenY && cell.Y <= screenYMax).ToList();

        private List<(int x, int y)> getCandidatesForNewLife(List<Cell> livingCells)
        {
            var candidatesForNewLife = new List<(int x, int y)>();

            // for each living cell, we get their neighbours.
            foreach (var cell in livingCells)
            {
                var neighbours = this.getCellNeighboursLocations(cell);

                var candidatesNotAlreadySelected = neighbours.Where(n =>
                        candidatesForNewLife.Any(c => c.x == n.x && c.y == n.y)
                                                == false) // against the ANY because we only want to return TRUE when there are NO matches.
                                            .ToList();

                var candidatesNotMatchForLivingCells = candidatesNotAlreadySelected.Where(c => 
                        livingCells.Any(l => l.X == c.x && l.Y == c.y) == false)
                                   .ToList();

                candidatesForNewLife.AddRange(candidatesNotMatchForLivingCells);
            }

            return candidatesForNewLife;
        }

        private List<Cell> getCellsToKill(List<Cell> livingCells)
        {
            var cellsToKill = new List<Cell>();

            foreach (var cell in livingCells)
            {
                var neighbours = this.getCellNeighboursLocations(cell);

                // get living neighbours of current cell.
                var livingNeighbours = this.livingCells.Where(l => neighbours.Any(n => n.x == l.X && n.y == l.Y)).ToList();

                // process rules for living cells.
                if (cell.DecideIfAlive(livingNeighbours) == false)
                    cellsToKill.Add(cell);
            }

            return cellsToKill;
        }

        private List<Cell> getCellsThatSurviveIteration(List<Cell> livingCells, List<Cell> cellsToKill)
        {
            var newIterationLivingCells = new List<Cell>();

            // just add cells that made it through the life cycle.            
            foreach (var cell in livingCells)
            {
                if (cellsToKill.Contains(cell) == false)
                    newIterationLivingCells.Add(cell);
            }

            return newIterationLivingCells;
        }

        private void addNewCellsForThisIteration(List<(int x, int y)> candidatesForNewLife, ref List<Cell> newIterationLivingCells)
        {
            // iterate over candidates for new life to see if we create any!
            foreach (var candidate in candidatesForNewLife)
            {
                var cell = new Cell(candidate.x, candidate.y);
                var livingNeighbours = cell.GetNeighbours(this.livingCells).Count;

                if (livingNeighbours == LIVING_NEIGHBOURS_TO_CREATE_LIFE)  // overpopulated areas won't breed into this.)
                    newIterationLivingCells.Add(cell);
            }
        }

        private List<(int x, int y)> getCellNeighboursLocations(Cell cell)
        {
            return new List<(int, int)>
            {
                (cell.X-1, cell.Y-1),
                (cell.X, cell.Y-1),
                (cell.X+1, cell.Y-1),
                (cell.X-1, cell.Y),
                (cell.X+1, cell.Y),
                (cell.X-1, cell.Y+1),
                (cell.X, cell.Y+1),
                (cell.X+1, cell.Y+1),
            };
        }

    }
}
