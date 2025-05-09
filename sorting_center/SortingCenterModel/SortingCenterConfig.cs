using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class JoinPoint
    {
        public int row;
        public int col;
        public int join_row;
        public int join_col;

        public JoinPoint()
        {
            row = 0;
            col = 0;
            join_row = 0;
            join_col = 0;
        }

        public JoinPoint(int row, int col, int join_row, int join_col)
        {
            this.row = row;
            this.col = col;
            this.join_row = join_row; 
            this.join_col = join_col;
        }
    }

    public class SortingCenterConfig
    {
        public int rowNumber = 2;
        public int columnNumber = 2;
        public int lineNumber = 10;
        public int subRowNumber = 10;
        public bool isDoubleLine = false;

        public int palleteSize = 2000;
        public int skuSize = 1;
        public int minNumberBox = 20;


        public double pickBoxFromDisassemblyChannelTime = 3.0; //Time to pick a box from the pallet disassembly channel.
        public double placeBoxIntoPalletAssemblyTime = 3.0; //Time to place a box into the pallet assembly channel.
        public double placeBoxIntoChannelTime = 3.0; //Time to place a box into a channel.
        public double pickBoxFromChannelTime = 3.0; //Time to pick a box from a channel.
        public double robotSpeedMs = 1.0; //Robot movement speed in meters per second (e.g., 0.5 for 0.5 m/s).



        public List<JoinPoint> robotSpawnPoints = new List<JoinPoint>();
        public List<JoinPoint> depaletizePoints = new List<JoinPoint>();
        public List<JoinPoint> paletizePoint = new List<JoinPoint>();

    }
}
