using AbstractModel;
using SkaldModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkladModel
{
    public class WarehouseWrapper : FastAbstractWrapper
    {
        public WarehouseConfig config { get; private set; }
        public List<WarehouseEvent> logs = new List<WarehouseEvent>();
        public List<RMSInstruction> instructions = new List<RMSInstruction>();
        public List<RobotTasks> robotsTasks;

        public WarehouseWrapper(WarehouseConfig config, List<RobotConfig> robotConfigs, List<RobotTasks> robotsTasks)
        {
            this.config = config;
            this.robotsTasks = robotsTasks;
            TimeSpan timeSpan = TimeSpan.Zero;
            foreach (var spawn in config.robotSpawnsPoints)
            {
                addObject(new WarehouseRobot(spawn, timeSpan, this, robotConfigs.First(x=>x.robotType==spawn.robotType),
                    robotsTasks.Find(x=>x.robotUid ==spawn.uid)));
                //timeSpan = AddEvent(timeSpan, new WarehouseSpawnRobotEvent(spawn));
            }
            foreach (var table in config.tableSpawnPoints)
            {
                addObject(new WarehouseTable(table, timeSpan, this));
                //timeSpan = AddEvent(timeSpan, new WarehouseSpawnTableEvent(table));
            }
        }

    }
}
