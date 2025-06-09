using AbstractModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace SortingCenterModel
{ 

    public class TeleportLine
    {
        public string uid = Guid.NewGuid().ToString();
        public RobotNode startLine = null;
        public RobotNode endLine = null;
        public Queue<Box> boxes = new Queue<Box>();
    }

    public class SortCenterWrapper : FastAbstractWrapper
    {
        public (int sku, int count) GetMinSkuCount()
        {
            if (skuCount == null || skuCount.Count == 0)
                throw new InvalidOperationException("skuCount is empty or not initialized.");

            var minEntry = skuCount
               // .Where(entry => !allSourcePoint.Any(x=>x.currentSku == entry.Key)) // Исключаем ключи, которые есть в currentSku
                .OrderBy(entry => entry.Value) // Сортируем по значению
                .First();
            return (minEntry.Key, minEntry.Value);
        }



        public SortingCenterConfig sortConfig { get; private set; }
        public Dictionary<int, Dictionary<int, RobotNode>> robotNodes = new Dictionary<int, Dictionary<int, RobotNode>>();
        public Dictionary<int, Dictionary<int, Dictionary<int, LineNode>>> lineNodes = new Dictionary<int, Dictionary<int, Dictionary<int, LineNode>>>();
        public List<PalettizeNode> palettizeNodes = new List<PalettizeNode>();
        public List<DepaletizeNode> depaletizeNodes = new List<DepaletizeNode>();
        public List<RobotSpawnNode> robotSpawnNodes = new List<RobotSpawnNode>();
        TimeSpan lastAdd = TimeSpan.Zero;
        
        public Dictionary<int, int> skuCount = new Dictionary<int, int>();

        private Dictionary<int, List<TeleportLine>> teleportLines = new Dictionary<int, List<TeleportLine>>(); // key - sku, value - teleport lines
        public List<TeleportLine> teleportLinesList { get; private set; } = new List<TeleportLine>();
        public List<TransportRobot> robots = new List<TransportRobot>();
        public List<Reservation> reservations = new List<Reservation>();
        public Dictionary<RobotNode, Reservation> reverceReservation = new Dictionary<RobotNode, Reservation>(); 


        private List<RobotNode> allRobotNodes;
        public List<SourcePoint> allSourcePoint = new List<SourcePoint>();
        public Dictionary<RobotNode, Dictionary<RobotNode, (double distance, List<RobotNode> path)>> shortestPaths;

        public List<EventLog> logs = new List<EventLog>();
        public List<EventLog> logs_2 = new List<EventLog>();

        public void SetupDebug(Action<string> writeDebug) 
        {
            this.writeDebug = writeDebug;
        }

        private void AddNode(int row, ref int v, int column, int line)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v, column, line, sortConfig.lineNumber));
            v++;
        }


        private void AddNode(int row, ref int v)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v, sortConfig.lineNumber));
            v++;
        }

        private void AddNode(int row, ref int v, string id)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            robotNodes[row].Add(v, new RobotNode(row, v, sortConfig.lineNumber, id));
            v++;
        }


        private RobotNode AddNode(int row, int col, int x, int y, string id)
        {
            if (!robotNodes.ContainsKey(row))
                robotNodes.Add(row, new Dictionary<int, RobotNode>());
            RobotNode rNode = new RobotNode(row, col, x, y, id);
            robotNodes[row].Add(col, rNode);
            return rNode;
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

        public void TransportRobotSpawnPointCreate()
        {
            lastAdd = AddEvent(lastAdd, new TransportRobotSpawnPointsCreate(sortConfig.shutleNumber));
        }

        public void palettizeNodesCreate(FileQueue fileQueue)
        {
            foreach (var pNode in palettizeNodes)
            {
                lastAdd = AddEvent(lastAdd, new ConsumerPointCreate(pNode, fileQueue));
            }
        }

        public void depalettizeNodesCreate(List<int> uniqueNumbers)
        {
            foreach (var dNode in depaletizeNodes)
            {
                lastAdd = AddEvent(lastAdd, new SourcePointCreate(dNode, uniqueNumbers));
            }
        }


        public void geterateLight()
        {
            int lastColumn = 0;


            for (int row = 0; row < sortConfig.rowNumber + 1; row++)
            {
                int cur_column = 0;
                AddNode(row, ref cur_column);
                for (int column = 0; column < sortConfig.columnNumber; column++)
                {
                    for (int line = 0; line < sortConfig.lineNumber; line++)
                    {
                        if (row != sortConfig.rowNumber)
                            AddLine(row, column, line, sortConfig.subRowNumber, cur_column);
                        AddNode(row, ref cur_column, column, line);
                    }
                    if (column != sortConfig.columnNumber - 1)
                    {
                        int node_idx = (column + 1) * sortConfig.lineNumber + 1;
                        AddNode(row, ref cur_column);
                    }
                }
                lastColumn = cur_column;
                AddNode(row, ref cur_column);
            }


            foreach (var row in robotNodes.Keys)
            {
                foreach (var col in robotNodes[row].Keys)
                {
                    int linked_col = 0;
                    if (row % 2 == 0)
                        linked_col = col - 1;
                    else
                        linked_col = col + 1;

                    if (robotNodes[row].ContainsKey(linked_col))
                        robotNodes[row][col].AddLinkedNode(robotNodes[row][linked_col]);


                    if (lineNodes.ContainsKey(row - 1))
                    {
                        if (lineNodes[row - 1].ContainsKey(col))
                        {
                            robotNodes[row][col].getNodes.Add(lineNodes[row - 1][col][sortConfig.subRowNumber - 1]);
                            teleportLinesList.Add(new TeleportLine()
                            {
                                startLine = robotNodes[row - 1][col],
                                endLine = robotNodes[row][col],
                            });
                        }
                    }

                    if (lineNodes.ContainsKey(row))
                    {
                        if (lineNodes[row].ContainsKey(col))
                        {
                            robotNodes[row][col].dropNodes.Add(lineNodes[row][col][0]);
                        }
                    }

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


            foreach (var point in sortConfig.robotSpawnPoints)
            {
                robotSpawnNodes.Add(new RobotSpawnNode(point.row, point.col, point.join_row, point.join_col, sortConfig.lineNumber));
                robotSpawnNodes.Last().AddLinkedNode(robotNodes[point.join_row][point.join_col]);
            }

            foreach (var point in sortConfig.depaletizePoints)
            {
                depaletizeNodes.Add(new DepaletizeNode(point.row, point.col,
                    point.row, point.col,
                    $"Depaletize {point.row} {point.col}",
                    point.join_row, point.join_col));
                robotNodes[point.join_row][point.join_col].getNodes.Add(depaletizeNodes.Last());
            }

            foreach (var point in sortConfig.paletizePoint)
            {
                palettizeNodes.Add(new PalettizeNode(point.row, point.col,
                    point.row, point.col,
                    $"Palettize {point.row} {point.col}",
                    point.join_row, point.join_col));
                robotNodes[point.join_row][point.join_col].dropNodes.Add(palettizeNodes.Last());
            }
        }



        public void geterateDoubleLineConfiguration()
        {
            int lastColumn = 0;
            List<string> rowsName = new List<string>(){ "H", "G", "F","E","D", "C", "B", "A" };

            var reserv = new Reservation();
            int current_row = 0;
            for (int column = 0; column < sortConfig.columnNumber + 1; column++)
            {
                reserv = new Reservation();
                reservations.Add(reserv);
                reserv.robotNodes.Add(
                    AddNode(0, column * (sortConfig.lineNumber + 1), 
                        column * (sortConfig.lineNumber + 1), current_row, $"{rowsName[current_row]}.1.{column+1}"));
            }
            current_row = current_row + 1;

            for (int row = 1; row < sortConfig.rowNumber + 2; row++)
            {
                int cur_column = 0;
                if (row > 1)
                    current_row = current_row + sortConfig.subRowNumber + 1;
                reserv = new Reservation();
                reservations.Add(reserv);
                reserv.robotNodes.Add(AddNode(row, cur_column, cur_column, current_row, $"{rowsName[row]}.{1}.{1}"));
                cur_column++;
                for (int column = 1; column < sortConfig.columnNumber+1; column++)
                {
                    reserv = new Reservation();
                    reservations.Add(reserv);
                    for (int line = 0; line < sortConfig.lineNumber; line++)
                    {
                        if (row != sortConfig.rowNumber + 1)
                            AddLine(row, column, line, sortConfig.subRowNumber, cur_column);
                        reserv.robotNodes.Add(
                        AddNode(row, cur_column, cur_column, current_row, $"{rowsName[row]}.{1}.{column + 1}.{line + 1}"));
                        cur_column++;
                    }
                    reserv = new Reservation();
                    reservations.Add(reserv);
                    reserv.robotNodes.Add(AddNode(row, cur_column, cur_column, current_row, $"{rowsName[row]}.{1}.{column + 1}"));
                    cur_column++;
                }
                
                lastColumn = cur_column;
            }

            current_row = current_row + 1;
            for (int column = 0; column < sortConfig.columnNumber + 1; column++)
            {
                reserv = new Reservation();
                reservations.Add(reserv);
                reserv.robotNodes.Add(
                AddNode(sortConfig.rowNumber + 2, column * (sortConfig.lineNumber + 1),
                    column * (sortConfig.lineNumber + 1), current_row, $"{rowsName[sortConfig.rowNumber + 2]}.{1}.{column + 1}"));
            }


            foreach (var row in robotNodes.Keys)
            {
                var keys = robotNodes[row].Keys.ToList();
                for (int i = 0; i < keys.Count; i++)
                {
                    int col = keys[i];
                    int? linked_col = null;

                    if (row % 2 == 0)
                        linked_col = i > 0 ? keys[i - 1] : (int?)null;
                    else
                        linked_col = i < keys.Count - 1 ? keys[i + 1] : (int?)null;

                    // Проверяем, что linked_col не null
                    if (linked_col != null && robotNodes[row].ContainsKey(linked_col.Value))
                    {
                        robotNodes[row][col].AddLinkedNode(robotNodes[row][linked_col.Value]);
                    }


                    if (robotNodes.ContainsKey(row - 1) & robotNodes.ContainsKey(row) & lineNodes.ContainsKey(row - 1))
                    {
                        if (lineNodes[row - 1].ContainsKey(col) & robotNodes[row].Count == robotNodes[row-1].Count)
                        {
                            robotNodes[row][col].getNodes.Add(lineNodes[row - 1][col][sortConfig.subRowNumber - 1]);
                            Console.WriteLine($"{robotNodes[row][col].Id} {robotNodes[row][col].getNodes[0].Id}");
                            teleportLinesList.Add(new TeleportLine()
                            {
                                startLine = robotNodes[row - 1][col],
                                endLine = robotNodes[row][col],
                            });
                        }
                    }

                    if (lineNodes.ContainsKey(row))
                    {
                        if (lineNodes[row].ContainsKey(col))
                        {
                            robotNodes[row][col].dropNodes.Add(lineNodes[row][col][0]);
                        }
                    }

                }

                if (robotNodes.ContainsKey(row + 1))
                {
                    robotNodes[row].First().Value.AddLinkedNode(robotNodes[row + 1].First().Value);
                }
                if (robotNodes.ContainsKey(row - 1))
                {
                    robotNodes[row].Last().Value.AddLinkedNode(robotNodes[row - 1].Last().Value);
                }
            }
         
            foreach (var point in sortConfig.robotSpawnPoints)
            {
                reserv = new Reservation();
                reservations.Add(reserv);
                var node = new RobotSpawnNode(point.row, point.col, point.join_row, point.join_col, sortConfig.lineNumber);
                reserv.robotNodes.Add(node);
                robotSpawnNodes.Add(node);
                robotSpawnNodes.Last().AddLinkedNode(robotNodes[point.join_row][point.join_col]);
            }

            foreach (var point in sortConfig.depaletizePoints)
            {
                depaletizeNodes.Add(new DepaletizeNode(point.row, point.col,
                    point.row, point.col,
                    $"Depaletize {point.row} {point.col}",
                    point.join_row, point.join_col));
                robotNodes[point.join_row][point.join_col].getNodes.Add(depaletizeNodes.Last());
            }

            foreach (var point in sortConfig.paletizePoint)
            {
                palettizeNodes.Add(new PalettizeNode(point.row, point.col,
                    point.row, point.col,
                    $"Palettize {point.row} {point.col}",
                    point.join_row, point.join_col));
                robotNodes[point.join_row][point.join_col].dropNodes.Add(palettizeNodes.Last());
            }

            foreach(var res in reservations)
            {
                foreach(var node in res.robotNodes)
                {
                    reverceReservation[node] = res;
                }
            }

        }

        internal IEnumerable<TransportRobot> GetWaitingRobot()
        {
            IEnumerable<TransportRobot> result = objects.Values
                .OfType<TransportRobot>()
                .Where(robot => robot.CurrentState == TransportRobotState.Waiting);
            return result;
        }

        internal IEnumerable<TransportRobot> GetRobotByTask(TransportRobotTask task)
        {
            IEnumerable<TransportRobot> result = objects.Values
                .OfType<TransportRobot>()
                .Where(robot => robot.CurrentTask == task);
            return result;
        }

        internal void SetReservation(RobotNode nextNode, string uid, bool v)
        {
            foreach (var node in reverceReservation[nextNode].robotNodes)
            {
                node.Reserved = (v, uid);
            }
        }


        private int robotUid = 0;
        internal string GetNewUid()
        {
            robotUid++;
            return  "bot" + robotUid.ToString("D2");
        }

        public SortCenterWrapper(SortingCenterConfig sortConfig, List<int> uniqueNumbers)
        {
            this.sortConfig = sortConfig;
            TimeSpan startPoint = TimeSpan.Zero;


            geterateDoubleLineConfiguration();


            allRobotNodes = robotNodes.Values.SelectMany(row => row.Values).ToList();
            allRobotNodes.AddRange(robotSpawnNodes);
            shortestPaths = RobotNodeDistanceCalculator.CalculateAllPairsShortestPaths(allRobotNodes);

            foreach (int i in uniqueNumbers)
            {
                skuCount[i] = 0;
            }

        }


    }
}
