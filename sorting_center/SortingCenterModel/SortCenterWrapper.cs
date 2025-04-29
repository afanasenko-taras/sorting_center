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
        public Dictionary<int, Dictionary<int, RobotNode>> robotNodes = new Dictionary<int, Dictionary<int, RobotNode>>();
        public Dictionary<int, Dictionary<int, Dictionary<int, LineNode>>> lineNodes = new Dictionary<int, Dictionary<int, Dictionary<int, LineNode>>>();


        private void AddNode(int row, ref int v, int column, int line)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v, column, line));
            v++;
        }


        private void AddNode(int row, ref int v)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v));
            v++;
        }

        private void AddLine(int row, int column, int line, int num_sub_row, int x)
        {
            if (!lineNodes.ContainsKey(row))
                lineNodes.Add(row, new Dictionary<int, Dictionary<int, LineNode>>());
            if (!lineNodes[row].ContainsKey(x))
                lineNodes[row].Add(x, new Dictionary<int, LineNode>());
            for (int i=0; i< num_sub_row; i++)
            {
                lineNodes[row][x].Add(i, new LineNode(row, column, line, i, num_sub_row, x));
            }
        }


        public SortCenterWrapper(SortingCenterConfig sortConfig)
        {
            this.sortConfig = sortConfig;
            TimeSpan startPoint = TimeSpan.Zero;
            int lastColumn = 0;
            for (int row = 0; row < sortConfig.rowNumber + 1; row++)
            {
                Console.WriteLine($"row {row}");
                int cur_column = 0;
                AddNode(row, ref cur_column);
                for (int column = 0; column < sortConfig.columnNumber; column++)
                {
                    Console.WriteLine($"column {column}");
                    for (int line = 0; line < sortConfig.lineNumber; line++)
                    {
                        if (row != sortConfig.rowNumber)
                            AddLine(row, column, line, sortConfig.subRowNumber, cur_column);
                        AddNode(row, ref cur_column, column, line);
                    }
                    if (column != sortConfig.columnNumber - 1)
                    {
                        int node_idx = (column + 1)  * sortConfig.lineNumber  + 1;
                        AddNode(row, ref cur_column);
                    }
                }
                lastColumn = cur_column;
                AddNode(row, ref cur_column);
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
