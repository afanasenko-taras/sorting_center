using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkladModel
{
    public class Position
    {
        public double x; // X coordinate in meters
        public double y; // Y coordinate in meters
        public double angle; // Orientation angle in degrees
        public Position(double x, double y, double angle)
        {
            this.x = x;
            this.y = y;
            this.angle = angle;
        }
        public Position()
        {
            this.x = 0; // Default X coordinate at origin
            this.y = 0; // Default Y coordinate at origin
            this.angle = 0; // Default angle is 0 degrees
        }
    }


    public enum TaskType
    {
        MoveToPosition, // Move to a specific position
        PickUpItem, // Pick up an item from a position
        PlaceItem, // Place an item at a position
        Rotate, // Rotate the robot to a specific angle
        Wait // Wait for a specified duration
    }

    public class RobotTask
    {
        public Position startPosition; // Starting position of the robot
        public Position endPosition; // Ending position of the robot
        public string tableUid; // Unique identifier for the table (if applicable)
        public TaskType taskType; // Type of task to be performed
        public double duration; // Duration of the task in seconds (if applicable)
        public RobotTask(TaskType taskType, Position start, Position end, string tableUid = "", double duration = 0)
        {
            this.taskType = taskType; // Set the type of task
            this.startPosition = start;
            this.endPosition = end;
            this.tableUid = tableUid; // Can be null if not applicable
            this.duration = duration;
        }
        public RobotTask()
        {
            this.startPosition = new Position(0, 0, 0); // Default start position at origin
            this.endPosition = new Position(0, 0, 0); // Default end position at origin
            this.tableUid = ""; // Default table UID is empty
            this.taskType = TaskType.Wait; // Default task type is Wait
            this.duration = 0; // Default duration is 0 seconds
        }
    }

    public class Spawn
    {
        public string uid; // Unique identifier for the robot
        public Position position; // Initial position of the robot
        public Spawn(string uid, Position position)
        {
            this.uid = uid;
            this.position = position;
        }
        public Spawn()
        {
            this.uid = Guid.NewGuid().ToString(); // Generate a unique identifier for the robot
            this.position = new Position(0, 0, 0); // Default position at origin
        }
    }

    public class RobotSpawn : Spawn
    {
        public string robotType = "RobotType1"; // Type of the robot, default is "RobotType1"
        public RobotSpawn(string uid, Position position, string robotType) : base(uid, position) 
        {
        }
        public RobotSpawn (): base()
        {
        }
    }

    public class RobotTasks
    {
        public string robotUid = ""; // Unique identifier for the robot
        public List<RobotTask> tasks = new List<RobotTask>(); // List of tasks for the robot
        public RobotTasks(List<RobotTask> tasks)
        {
            this.tasks = tasks; // Initialize with provided tasks
        }
        public RobotTasks()
        {
            this.tasks = new List<RobotTask>(); // Default to an empty task list
        }
    }



    public class WarehouseGeometry
    {
        public double xSize = 10.0; // Size of the warehouse in meters along the X-axis
        public double ySize = 10.0; // Size of the warehouse in meters along the Y-axis
        public double zSize = 4.0; // Size of the warehouse in meters along the Z-axis (height)
        public WarehouseGeometry(double xSize, double ySize, double zSize)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
        }
        public WarehouseGeometry()
        {
            this.xSize = 10.0; // Default size of the warehouse along the X-axis
            this.ySize = 10.0; // Default size of the warehouse along the Y-axis
            this.zSize = 4.0; // Default size of the warehouse along the Z-axis (height)
        }
    }

    public class RobotConfig
    {
        public string robotType = "RobotType1";
        public double acceleration = 5; // Acceleration of the robot in m/s^2
        public double deceleration = 5; // Deceleration of the robot in m/s^2
        public double maxSpeed = 1.0; // Maximum speed of the robot in m/s
        public double maxSpeedAngle = 180; // Maximum angular speed of the robot in grad/s
        public double accelerationAngle = 10 * 180; // Angular acceleration of the robot in grad/s^2
        public double decelerationAngle = 10 * 180; // Angular deceleration of the robot in grad/s^2
        public double pickTime = 1.0; // Time taken to pick up an item in seconds
        public double placeTime = 1.0; // Time taken to place an item in seconds
        public RobotConfig()
        {
        }
    }

    public class WarehouseConfig
    {

        public List<RobotSpawn> robotSpawnsPoints = new List<RobotSpawn>();
        public List<Spawn> tableSpawnPoints = new List<Spawn>();
        public WarehouseConfig()
        {

        }
    }
}
