using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class RobotNode
    {
        public int x;
        public int y;
        public List<RobotNode> nextNodes = new List<RobotNode>();
        private int row;
        private int col;

        public RobotNode(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        internal void AddLinkedNode(RobotNode robotNode)
        {
            nextNodes.Add(robotNode);
        }
    }
}
