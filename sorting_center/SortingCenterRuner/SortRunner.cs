using AbstractModel;
using SortingCenterModel;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SortingCenterModel
{
    class SortRunner
    {


        static void Run(int run_id)
        {
            //string folderPath = "simulated_light"; // Путь к папке с файлами
            string folderPath = "simulated";
            var fileQueue = new FileQueue();

            // Загружаем файлы в очередь
            fileQueue.LoadFiles(folderPath);

            // Получаем список уникальных номеров
            var uniqueNumbers = fileQueue.GetUniqueNumbers();
            Console.WriteLine($"Количество уникальных номеров: {fileQueue.GetUniqueCount()}");

            //SortingCenterConfig sortConfig = new SortingCenterConfig();
            //Helper.SerializeXMLToFile(sortConfig, "sorting_center_config.xml");

            SortingCenterConfig sortConfig = Helper.DeserializeXMLFromFile<SortingCenterConfig>("sorting_center_config.xml");
            Helper.SerializeXMLToFile(sortConfig, "sorting_center_config.xml");
            sortConfig.shutleNumber = run_id;


            SortCenterWrapper wrapper = new SortCenterWrapper(sortConfig, uniqueNumbers);
            TimeSpan lastEvent = TimeSpan.Zero;

            wrapper.palettizeNodesCreate(fileQueue);
            wrapper.depalettizeNodesCreate(uniqueNumbers);
            wrapper.TransportRobotSpawnPointCreate();


            //RobotManager robotManager = new RobotManager(wrapper);
            RandomRobotManager randomRobotManager = new RandomRobotManager(wrapper);
            DepaletizeManager depaletizeManager = new DepaletizeManager(wrapper);

            StreamWriter outputFile = new StreamWriter("sklad_debug.log");
            wrapper.SetupDebug(outputFile.WriteLine);
            wrapper.isDebug = false;


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (wrapper.Next() & //wrapper.updatedTime < TimeSpan.FromDays(5) & 
                    !((ConsumerPoint)wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0]).FifoQueue.IsEmpty)
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
            stopwatch.Stop(); // Останавливаем таймер
            Console.WriteLine($"Время выполнения цикла: {stopwatch.Elapsed}");

            outputFile.Close();

            StreamWriter eventLogFile;
            /*
            eventLogFile = new StreamWriter("store-log.csv");
            eventLogFile.WriteLine("ID,startTime,endTime,botId,command,Start,End,boxType,channel,TrID");
            for (int i=0; i< wrapper.logs.Count(); i++)
            {
                EventLog log = wrapper.logs[i];
                eventLogFile.WriteLine($"{i},{log.startTime},{log.endTime},{log.botId},{log.command},{log.Start},{log.End},{log.boxType},{log.channel},{log.TrID}");
            }
            eventLogFile.Close();
            */

            eventLogFile = new StreamWriter($"run/store-log-{sortConfig.shutleNumber}-{sortConfig.skuSize}-{sortConfig.palleteSize}-{folderPath}.csv");
            eventLogFile.WriteLine("ID,startTime,endTime,botId,command,Start,End,boxType,channel,TrID");
            for (int i = 0; i < wrapper.logs_2.Count(); i++)
            {
                EventLog log = wrapper.logs_2[i];
                eventLogFile.WriteLine($"{i},{log.startTime.ToString(@"d\.hh\:mm\:ss")},{log.endTime.ToString(@"d\.hh\:mm\:ss")},{log.botId},{log.command},{log.Start},{log.End},{log.boxType},{log.channel},{log.TrID}");

                //eventLogFile.WriteLine($"{i},{log.startTime},{log.endTime},{log.botId},{log.command},{log.Start},{log.End},{log.boxType},{log.channel},{log.TrID}");
            }
            eventLogFile.Close();

        }


        static void Main(string[] args)
        {

            for (int i = 1; i <= 15; i++)
            {
                Run(i);
            }




        }
    }
}

