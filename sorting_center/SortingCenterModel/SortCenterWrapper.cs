using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class SortCenterWrapper : FastAbstractWrapper
    {
        private SortingCenterConfig sortConfig;
        Dictionary<int, Dictionary<int, RobotNode>> robotNodes = new Dictionary<int, Dictionary<int, RobotNode>>();
        
        private void AddNode(int row, int v)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v));
        }

        public SortCenterWrapper(SortingCenterConfig sortConfig)
        {
            this.sortConfig = sortConfig;
            TimeSpan startPoint = TimeSpan.Zero;

            int lastColumn = sortConfig.columnNumber * sortConfig.lineNumber + 1;

            for (int row = 0; row < sortConfig.rowNumber + 1; row++)
            {
                AddNode(row, 0);
                for (int column = 0; column < sortConfig.columnNumber; column++)
                {
                    for (int line = 0; line < sortConfig.lineNumber; line++)
                    {
                        AddNode(row, column * sortConfig.lineNumber + line + 1);
                    }
                }
                AddNode(row, lastColumn);
            }


            foreach(var row in robotNodes.Keys)
            {
                foreach(var col in robotNodes[row].Keys)
                {
                    int linked_col = 0;
                    if (row % 2 == 0)
                        linked_col = col - 1;
                    else
                        linked_col = col + 1;

                    if (robotNodes[row].ContainsKey(linked_col))
                        robotNodes[row][col].AddLinkedNode(robotNodes[row][linked_col]);
                }
                if (row % 2 == 0)
                {
                    if (robotNodes.ContainsKey(row + 1))
                    {
                        robotNodes[row][0].AddLinkedNode(robotNodes[row + 1][0]);
                    }
                    if (robotNodes.ContainsKey(row - 1))
                    {
                        robotNodes[row][0].AddLinkedNode(robotNodes[row - 1][0]);
                    }
                }
                else
                {
                    if (robotNodes.ContainsKey(row + 1))
                    {
                        robotNodes[row][lastColumn].AddLinkedNode(robotNodes[row + 1][lastColumn]);
                    }
                    if (robotNodes.ContainsKey(row - 1))
                    {
                        robotNodes[row][lastColumn].AddLinkedNode(robotNodes[row - 1][lastColumn]);
                    }
                }
            }


            Console.WriteLine("SIPPJOP");
            /*
            for(int i=0; i<sortConfig.sorceNumber;i++)
            {
                startPoint = AddEvent(startPoint, new CreateSourcePoint()); 
            }
            */
        }


    }
}
