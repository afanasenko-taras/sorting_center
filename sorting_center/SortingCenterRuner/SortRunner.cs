using AbstractModel;
using SortingCenterModel;
using System.Security.Cryptography;

namespace SortingCenterModel
{
    class SortRunner
    {


        static void Main(string[] args)
        {

            //SortingCenterConfig sortConfig = new SortingCenterConfig();
            //Helper.SerializeXMLToFile(sortConfig, "sorting_center_config.xml");

            SortingCenterConfig sortConfig = Helper.DeserializeXMLFromFile<SortingCenterConfig>("sorting_center_config.xml");
            Helper.SerializeXMLToFile(sortConfig, "sorting_center_config.xml");

            SortCenterWrapper wrapper = new SortCenterWrapper(sortConfig);
            TimeSpan lastEvent = TimeSpan.Zero;

            wrapper.palettizeNodesCreate();
            wrapper.depalettizeNodesCreate();
            wrapper.TransportRobotSpawnPointCreate();


            //RobotManager robotManager = new RobotManager(wrapper);
            RandomRobotManager randomRobotManager = new RandomRobotManager(wrapper);
            DepaletizeManager depaletizeManager = new DepaletizeManager(wrapper);

            StreamWriter outputFile = new StreamWriter("sklad_debug.log");
            wrapper.SetupDebug(outputFile.WriteLine);
            wrapper.isDebug = true;

            while (wrapper.Next() & //wrapper.updatedTime < TimeSpan.FromDays(5) & 
                    ((ConsumerPoint)wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0]).fifoQueue.Count > 0)
            {
                foreach (var robot in wrapper.robots)
                { 
                    wrapper.WriteDebug($"{robot.uid} {robot.commandList.commands.Count} {robot.currentNode.Id} {robot.movedTo.Id} {robot.CurrentState}");
                }
                depaletizeManager.ManageDepaletize();
                //robotManager.ManageRobots();
                randomRobotManager.ManageRobots(wrapper.updatedTime);
                wrapper.WriteDebug("");

            }

            outputFile.Close();

            StreamWriter eventLogFile = new StreamWriter("store-log.csv");
            eventLogFile.WriteLine("ID,startTime,endTime,botId,command,Start,End,boxType,channel,TrID");
            for (int i=0; i< wrapper.logs.Count(); i++)
            {
                EventLog log = wrapper.logs[i];
                eventLogFile.WriteLine($"{i},{log.startTime},{log.endTime},{log.botId},{log.command},{log.Start},{log.End},{log.boxType},{log.channel},{log.TrID}");
            }
            eventLogFile.Close();

            eventLogFile = new StreamWriter($"store-log-{sortConfig.shutleNumber}-{sortConfig.skuSize}-{sortConfig.palleteSize}.csv");
            eventLogFile.WriteLine("ID,startTime,endTime,botId,command,Start,End,boxType,channel,TrID");
            for (int i = 0; i < wrapper.logs_2.Count(); i++)
            {
                EventLog log = wrapper.logs_2[i];
                eventLogFile.WriteLine($"{i},{log.startTime},{log.endTime},{log.botId},{log.command},{log.Start},{log.End},{log.boxType},{log.channel},{log.TrID}");
            }
            eventLogFile.Close();


        }
    }
}

