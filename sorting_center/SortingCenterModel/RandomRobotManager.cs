using SortingCenterModel;
using System.ComponentModel.DataAnnotations;

namespace SortingCenterModel
{
    public class RandomRobotManager
    {
        private readonly SortCenterWrapper _wrapper;
        private Random rnd = new Random();

        public RandomRobotManager(SortCenterWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public void ManageRobots(TimeSpan timeSpan)
        {
            var waitingRobots = _wrapper.GetWaitingRobot();
            if (waitingRobots.Count() > 0)
            {
                foreach (var waitingRobot in waitingRobots)
                {
                    if (waitingRobot.commandList.commands.Count() == 0)
                    {
                        if (waitingRobot.currentBox == null) { //we try depaletize in this route

                            RobotNode nextNode = null;
                            foreach(var source in _wrapper.allSourcePoint)
                            {

                                var nodeGet = _wrapper.robotNodes[source.dNode.join_row][source.dNode.join_col];
                                if (_wrapper.shortestPaths[waitingRobot.currentNode][nodeGet].path.Count > 1)
                                {
                                    nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][nodeGet].path[1];
                                    if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                    {
                                        waitingRobot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                                        break;
                                    }
                                }
                                else
                                {
                                    waitingRobot.AddCommandGetFromSource(source); //we in Depaletize Node 
                                    break;
                                }
                            } 
                        } 
                        else //robot have box
                        {
                            if (waitingRobot.CurrentTask == TransportRobotTask.MoveBoxToLine) //and we need put this box in any line
                            {
                                while (waitingRobot.teleportLine == null)
                                {
                                    var teleportLine = _wrapper.teleportLinesList[rnd.Next(_wrapper.teleportLinesList.Count())];
                                    if (teleportLine.boxes.Count < _wrapper.sortConfig.subRowNumber)
                                    {
                                        waitingRobot.teleportLine = teleportLine;
                                    }
                                }
                                if (_wrapper.shortestPaths[waitingRobot.currentNode][waitingRobot.teleportLine.startLine].path.Count > 1)
                                {
                                    var nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][waitingRobot.teleportLine.startLine].path[1];
                                    if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                    {
                                        waitingRobot.AddCommandMove(nextNode);
                                    }
                                }
                                else
                                {
                                    waitingRobot.AddCommandDropToLine(waitingRobot.teleportLine);
                                }


                            }
                        }
                    }

                    if (waitingRobot.commandList.commands.Count() == 0)
                    {
                        waitingRobot.AddCommandMove(waitingRobot.currentNode.nextNodes[0]);
                    }
                }
            }
        }
    }
}