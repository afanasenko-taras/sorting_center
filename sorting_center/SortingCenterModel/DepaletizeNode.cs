using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class DepaletizeNode : NodeBase
    {
        public int join_row = 0;
        public int join_col = 0;
        public DepaletizeNode(int row, int col, int x, int y, string id, int join_row, int join_col) : base(row, col, x, y, id)
        {
            this.join_row = join_row;
            this.join_col = join_col;
        }
    }
}
