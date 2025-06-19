using AbstractModel;
using SkladModel;

namespace SkladRunner
{
    class SkladRunner
    {
        static void Main(string[] args)
        {



            WarehouseGeometry geometry =  Helper.DeserializeYamlFromFile<WarehouseGeometry>("WAREHOUSE-Geometry.yaml");

            List<RobotConfig> robotConfigs = Helper.DeserializeYamlFromFile<List<RobotConfig>>("WAREHOUSE-Robots.yaml");
            Helper.SerializeYamlToFile(robotConfigs, "WAREHOUSE-Robots.yaml");

            WarehouseConfig config = new WarehouseConfig();
            RobotSpawn spawn = new RobotSpawn("robot1", new Position(5, 5, 270), "RobotType1");
            Spawn table1 = new Spawn("table1", new Position(1, 1, 0));
            RobotTasks tasks = new RobotTasks(new List<RobotTask>());

            tasks.robotUid = spawn.uid; // Assign the robot UID to the tasks
            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
                new Position(5, 5, 270), // Start position
                new Position(5, 1, 270), // End position
                "", // No table UID
                0 // No duration
            ));

            tasks.tasks.Add(new RobotTask(
                TaskType.Rotate, // Task type
                new Position(5, 1, 270), // Start position
                new Position(5, 1, 180), // End position
                "", // No table UID
                0 // No duration
             ));


            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
                new Position(5, 1, 180), // Start position
                new Position(1, 1, 180), // End position
                "", // No table UID
                0 // No duration
             ));

            tasks.tasks.Add(new RobotTask(
                TaskType.PickUpItem, // Task type
                new Position(1, 1, 180), // Start position
                new Position(1, 1, 180), // End position
                "table1", // No table UID
                0 // No duration
             ));

            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
               new Position(1, 1, 180), // Start position
               new Position(5, 1, 180), // End position
               "", // No table UID
               0 // No duration
            ));


            tasks.tasks.Add(new RobotTask(
                TaskType.Rotate, // Task type
                new Position(5, 1, 180), // Start position
                new Position(5, 1, 270), // End position
                "", // No table UID
                0 // No duration
             ));

            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
               new Position(5, 1, 270), // Start position
               new Position(5, 3, 270), // End position
               "", // No table UID
               0 // No duration
            ));


            tasks.tasks.Add(new RobotTask(
                TaskType.Rotate, // Task type
                new Position(5, 3, 270), // Start position
                new Position(5, 3, 180), // End position
                "", // No table UID
                0 // No duration
             ));

            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
               new Position(5, 3, 180), // Start position
               new Position(1, 3, 180), // End position
               "", // No table UID
               0 // No duration          
            ));

            tasks.tasks.Add(new RobotTask(
                TaskType.PlaceItem, // Task type
                new Position(1, 3, 180), // Start position
                new Position(1, 3, 180), // End position
                "table1", // No table UID
                0 // No duration
             ));


            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
               new Position(1, 3, 180), // Start position
               new Position(5, 3, 180), // End position
               "", // No table UID
               0 // No duration
            ));


            tasks.tasks.Add(new RobotTask(
                TaskType.Rotate, // Task type
                new Position(5, 3, 180), // Start position
                new Position(5, 3, 270), // End position
                "", // No table UID
                0 // No duration
             ));


            tasks.tasks.Add(new RobotTask(
                TaskType.MoveToPosition, // Task type
               new Position(5, 3, 270), // Start position
               new Position(5, 5, 270), // End position
               "", // No table UID
               0 // No duration
            ));

            config.robotSpawnsPoints.Add(spawn);
            config.tableSpawnPoints.Add(table1);
            Helper.SerializeYamlToFile(config, "WAREHOUSE-Object-Config.yaml");

            List<RobotTasks> tasksList = new List<RobotTasks>();
            tasksList.Add(tasks);
            Helper.SerializeYamlToFile(tasksList, "WAREHOUSE-Robots-Tasks.yaml");

            
            

            WarehouseWrapper warehouseWrapper = new WarehouseWrapper(config, robotConfigs, tasksList);
            while (warehouseWrapper.Next())
            {
            }
            Helper.SerializeYamlToFile(warehouseWrapper.logs, "WAREHOUSE-Log.yaml");
            Helper.SerializeYamlToFile(warehouseWrapper.instructions, "WAREHOUSE-RMSInstruction.yaml");


        }
    }
}