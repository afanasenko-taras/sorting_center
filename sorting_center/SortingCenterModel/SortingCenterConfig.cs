using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{

    public class SortingCenterConfig
    {
        public int rowNumber = 2;
        public int columnNumber = 2;
        public int lineNumber = 10;
        public int subRowNumber = 10;
        public bool isDoubleLine = false;


        public List<(int row, int col, int join_row, int join_col)> robotSpawnPoints = new List<(int row, int col, int join_row, int join_col)>();
        public List<(int row, int col, int join_row, int join_col)> depaletizePoints = new List<(int row, int col, int join_row, int join_col)>();
        public List<(int row, int col, int join_row, int join_col)> paletizePoint = new List<(int row, int col, int join_row, int join_col)>();

    }
}
