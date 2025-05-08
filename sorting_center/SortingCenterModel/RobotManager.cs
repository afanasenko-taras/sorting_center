using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class RobotManager
    {
        private readonly SortCenterWrapper _wrapper;

        public RobotManager(SortCenterWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public void ManageRobots()
        {
            var waitingRobots = _wrapper.GetWaitingRobot();
            if (waitingRobots.Count() > 0)
            {
                foreach (var robot in waitingRobots)
                {

                }
            }
        }

        private void AssignTaskToRobot(TransportRobot robot)
        {
            if (robot.currentBox == null) {
                //robot.currentBox.GetNearDepalitaze();
            } 
            else { 
            }

        }
    }
}
