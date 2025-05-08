using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class CommandList
    {
        public List<(TimeSpan Key, FastAbstractEvent Ev)> commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
        private TransportRobot robot;

        public CommandList(TransportRobot robot)
        {
            this.robot = robot; 
        }

        public bool AddCommand(FastAbstractEvent abstractEvent)
        {
            TimeSpan lastTime = robot.lastUpdated;
            commands.Add((lastTime, abstractEvent));
            return true;
        }
    }
}
