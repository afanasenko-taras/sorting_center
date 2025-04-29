using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class RobotNode
    {
        public int x;
        public int y;
        public List<RobotNode> nextNodes = new List<RobotNode>();
        public List<LineNode> dropNodes = new List<LineNode>();
        public List<LineNode> getNodes = new List<LineNode>();
        public int row;
        public int col;
        public string Id;

        public RobotNode(int row, int col, int column, int line)
        {
            Console.WriteLine($"Add {row} {col}");
            this.row = row;
            this.col = col;
            this.x = col;
            this.y = (row - 1) * 11;
            this.Id = $"Node_{row}_{column}_{line}_{col}";
        }

        public RobotNode(int row, int col)
        {
            Console.WriteLine($"Add {row} {col}");
            this.row = row;
            this.col = col;
            this.x = col;
            this.y = (row - 1) * 11;
            this.Id = $"Node_{row}_{col}";
        }

        internal void AddLinkedNode(RobotNode robotNode)
        {
            nextNodes.Add(robotNode);
        }
    }
}
