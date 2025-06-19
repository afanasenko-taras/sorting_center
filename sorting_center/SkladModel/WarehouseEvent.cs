using SkladModel;
using System.Runtime.CompilerServices;

namespace SkaldModel
{
    public enum WarehouseEventType
    {
        Create,
        Wait,
        ChangeAcceleration,
        ChangeAccelerationRotation,
        StartPickUp,
        EndPickUp,
        StartPlace,
        EndPlace
    }

    public enum WarehouseObjectType
    {
        Table,
        Robot
    }

    public class RMSInstruction
    {
        public WarehouseEventType warehouseEventType;
        public WarehouseObjectType warehouseObjectType;
        public string objectUid; // Unique identifier for the object related to the event
        public string tableUid; // Unique identifier for the table (if applicable)
        public double acceleration = 0; // Acceleration of the object at the time of the event (if applicable)
        public double rotationAcceleration = 0; // Rotation acceleration of the object at the time of the event (if applicable)
        public TimeSpan timeSpan; // Time at which the event occurs

        public RMSInstruction(TimeSpan timeSpan,
                     WarehouseEventType EventType,
                     WarehouseObjectType ObjectType,
                     WarehouseRobot robot,
                     string tableUid)
        { 
            this.warehouseEventType = EventType;
            this.warehouseObjectType = ObjectType;
            this.objectUid = robot.uid; // Set object UID  
            this.tableUid = tableUid; // Set table UID if applicable  
            this.acceleration = robot.acceleration; // Set acceleration   
            this.rotationAcceleration = robot.rotationAcceleration; // Set rotation acceleration  
            this.timeSpan = timeSpan; // Set the time at which the event occurs  
        }

        public RMSInstruction()
        {
            this.warehouseEventType = WarehouseEventType.Create; // Default event type is Create
            this.warehouseObjectType = WarehouseObjectType.Robot; // Default object type is Robot
            this.timeSpan = TimeSpan.Zero; // Default time is zero
            this.objectUid = ""; // Default UID is empty
            this.tableUid = ""; // Default table UID is empty
        }

    }

    public class WarehouseEvent
    {
        public WarehouseEventType warehouseEventType;
        public WarehouseObjectType warehouseObjectType;
        public double x = 0; // X coordinate in meters at the time of the event 
        public double y = 0; // Y coordinate in meters at the time of the event
        public double angle = 0; // Orientation angle in degrees at the time of the event
        public double speed = 0; // Speed of the object at the time of the event (if applicable)
        public double angleSpeed = 0; // Rotation speed of the object at the time of the event (if applicable)
        public double acceleration = 0; // Acceleration of the object at the time of the event (if applicable)
        public double rotationAcceleration = 0; // Rotation acceleration of the object at the time of the event (if applicable)
        public TimeSpan timeSpan; // Time at which the event occurs
        public string objectUid; // Unique identifier for the object related to the event
        public string tableUid; // Unique identifier for the table (if applicable)


        public WarehouseEvent(TimeSpan timeSpan, WarehouseEventType EventType, WarehouseObjectType ObjectType, Position position, string objectUid)
        {
            this.warehouseEventType = EventType;
            this.warehouseObjectType = ObjectType;
            this.x = position.x; // Set X coordinate
            this.y = position.y; // Set Y coordinate
            this.angle = position.angle; // Set orientation angle
            this.timeSpan = timeSpan;
            this.objectUid = objectUid;
        }
        public WarehouseEvent()
        {
            this.warehouseEventType = WarehouseEventType.Create; // Default event type is Create
            this.warehouseObjectType = WarehouseObjectType.Robot; // Default object type is Robot
            this.timeSpan = TimeSpan.Zero; // Default time is zero
            this.objectUid = ""; // Default UID is empty
        }

        public WarehouseEvent(TimeSpan timeSpan, WarehouseEventType EventType, WarehouseObjectType ObjectType, WarehouseRobot robot, string tableUid)
        {
            this.timeSpan = timeSpan;
            this.warehouseEventType = EventType;
            this.warehouseObjectType = ObjectType;
            this.x = robot.position.x; // Set X coordinate
            this.y = robot.position.y; // Set Y coordinate
            this.angle = robot.position.angle; // Set orientation angle
            this.objectUid = robot.uid;
            this.speed = robot.speed;
            this.angleSpeed = robot.angleSpeed;
            this.acceleration = robot.acceleration;
            this.rotationAcceleration = robot.rotationAcceleration;
            this.tableUid = tableUid; // Set table UID if applicable
        }

    }
}