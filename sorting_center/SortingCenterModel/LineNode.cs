using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class LineNode
    {
        public int x;
        public int y;
        public int row;
        public int col;
        public int line;
        public int row2;
        public string Id;

        public LineNode(int row, int col, int line, int row2, int num_sub_row, int x)
        {
            this.row = row;
            this.col = col;
            this.line = line;
            this.row2 = row2;
            this.x = x;
            this.y = (row - 1) * (num_sub_row + 1) + row2 + 1;
            this.Id = $"LineNode_{row}_{col}_{line}_{x}_{row2}";
        }
    }
}
