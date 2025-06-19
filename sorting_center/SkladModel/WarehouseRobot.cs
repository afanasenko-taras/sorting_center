using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkladModel
{
    public class WarehouseRobot : FastAbstractObject
    {
        private RobotSpawn spawn;
        private RobotTasks robotTasks = new RobotTasks();
        private TimeSpan lastUpdated;
        private WarehouseWrapper wrapper;

        public Position position { get; private set; } = new Position();
        public double speed { get; private set; } = 0;
        public double angleSpeed { get; private set; } = 0;
        public double acceleration { get; private set; } = 0;
        public double rotationAcceleration { get; private set; } = 0;

        private Queue<(TimeSpan, FastAbstractEvent)> localEvent = new Queue<(TimeSpan, FastAbstractEvent)>();
        private RobotTask currentTask;
        private bool isMoving = false;
        private bool isRotating = false;
        private double distanceToTarget;
        private double angleToTarget;
        private double maxAngularSpeedRad;
        private int movementDirection = 1; // 1 - вперед, -1 - назад
        private RobotConfig robotConfig;

        public WarehouseRobot(RobotSpawn spawn, TimeSpan timeSpan, WarehouseWrapper wrapper, RobotConfig robotConfig, RobotTasks robotTasks)
        {
            this.spawn = spawn;
            this.lastUpdated = timeSpan;
            this.wrapper = wrapper;
            this.uid = spawn.uid;
            this.position = new Position(spawn.position.x, spawn.position.y, spawn.position.angle);
            this.robotConfig = robotConfig;
            this.robotTasks = robotTasks;
            this.maxAngularSpeedRad = robotConfig.maxSpeedAngle * Math.PI / 180;
            PrintState();
        }

        public void AssignTasks(RobotTasks robotTasks)
        {
            this.robotTasks = robotTasks;
        }

        public void SetAcceleration(double acceleration)
        {
            this.acceleration = acceleration;
        }

        public void FinishLocalEvent()
        {
            localEvent.Dequeue();
        }

        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            if (localEvent.Count == 0)
            {
                if (robotTasks.tasks.Count == 0)
                    return (TimeSpan.MaxValue, null);

                currentTask = robotTasks.tasks[0];
                robotTasks.tasks.RemoveAt(0);

                ValidatePosition(currentTask.startPosition, position);

                if (currentTask.taskType == TaskType.MoveToPosition)
                {
                    CalculateMovementEvents();
                }
                else if (currentTask.taskType == TaskType.Rotate)
                {
                    CalculateRotationEvents();
                }
                else if (currentTask.taskType == TaskType.Wait)
                {
                    localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(currentTask.duration), new WaitEvent(this)));
                }
                else if (currentTask.taskType == TaskType.PickUpItem)
                {
                    double operationTime = robotConfig.pickTime;
                    localEvent.Enqueue((lastUpdated, new StartPickUp(this, currentTask.tableUid)));
                    localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(operationTime), new EndPickUp(this, currentTask.tableUid)));
                }
                else if (currentTask.taskType == TaskType.PlaceItem)
                {
                    double operationTime = robotConfig.placeTime;
                    localEvent.Enqueue((lastUpdated, new StartPlace(this, currentTask.tableUid)));
                    localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(operationTime), new EndPlace(this, currentTask.tableUid)));
                }
            }
            return localEvent.Any() ? localEvent.Peek() : (TimeSpan.MaxValue, null);
        }

        private void CalculateMovementEvents()
        {
            double dx = currentTask.endPosition.x - position.x;
            double dy = currentTask.endPosition.y - position.y;
            distanceToTarget = Math.Sqrt(dx * dx + dy * dy);

            // Конвертируем текущий угол из градусов в радианы
            double currentAngleRad = position.angle * Math.PI / 180;

            // Направление к цели (в радианах)
            double targetDirectionRad = Math.Atan2(dy, dx);

            // Разница между текущим углом (в радианах) и направлением к цели
            double angleDiffRad = NormalizeAngle(targetDirectionRad - currentAngleRad);

            // Определяем направление движения (меньший поворот)
            movementDirection = Math.Abs(angleDiffRad) < Math.PI / 2 ? 1 : -1;

            // Рассчитываем параметры движения
            double maxSpeed = robotConfig.maxSpeed * movementDirection;
            double acceleration = movementDirection > 0 ?
                robotConfig.acceleration : -robotConfig.acceleration;
            double deceleration = movementDirection > 0 ?
                -robotConfig.deceleration : robotConfig.deceleration;

            // Время разгона до максимальной скорости
            double accelerationTime = Math.Abs(maxSpeed - speed) / robotConfig.acceleration;
            double accelerationDistance = speed * accelerationTime +
                0.5 * acceleration * accelerationTime * accelerationTime;

            // Время торможения до полной остановки
            double decelerationTime = Math.Abs(maxSpeed) / robotConfig.deceleration;
            double decelerationDistance = maxSpeed * decelerationTime +
                0.5 * deceleration * decelerationTime * decelerationTime;

            if (Math.Abs(accelerationDistance + decelerationDistance) > distanceToTarget)
            {
                // Треугольный профиль скорости
                double a = 0.5 * (1 / robotConfig.acceleration + 1 / robotConfig.deceleration);
                double b = speed / robotConfig.acceleration;
                double c = -distanceToTarget - 0.5 * speed * speed / robotConfig.acceleration;

                double maxReachableSpeed = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                maxReachableSpeed = movementDirection > 0 ? maxReachableSpeed : -maxReachableSpeed;

                accelerationTime = Math.Abs(maxReachableSpeed - speed) / robotConfig.acceleration;
                decelerationTime = Math.Abs(maxReachableSpeed) / robotConfig.deceleration;

                localEvent.Enqueue((lastUpdated, new ChangeAcceleration(this, acceleration)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime), new ChangeAcceleration(this, 0)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime), new ChangeAcceleration(this, deceleration)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + decelerationTime), new ChangeAcceleration(this, 0)));
            }
            else
            {
                // Трапециевидный профиль скорости
                double constantSpeedDistance = distanceToTarget - Math.Abs(accelerationDistance) - Math.Abs(decelerationDistance);
                double constantSpeedTime = constantSpeedDistance / Math.Abs(maxSpeed);

                localEvent.Enqueue((lastUpdated, new ChangeAcceleration(this, acceleration)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime), new ChangeAcceleration(this, 0)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + constantSpeedTime), new ChangeAcceleration(this, deceleration)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + constantSpeedTime + decelerationTime), new ChangeAcceleration(this, 0)));
            }

            isMoving = true;
        }

        // Вспомогательный метод для нормализации угла в диапазон [-π, π]
        private double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        private void CalculateRotationEvents()
        {
            // Нормализуем углы в градусах [0, 360)
            double currentAngle = position.angle % 360;
            if (currentAngle < 0) currentAngle += 360;

            double targetAngle = currentTask.endPosition.angle % 360;
            if (targetAngle < 0) targetAngle += 360;

            // Вычисляем минимальный угол поворота [-180, 180]
            angleToTarget = targetAngle - currentAngle;
            if (angleToTarget > 180) angleToTarget -= 360;
            else if (angleToTarget < -180) angleToTarget += 360;

            int rotationDirection = Math.Sign(angleToTarget);
            double absAngle = Math.Abs(angleToTarget);

            // Максимальное угловое ускорение/замедление (градусы/с²)
            double maxAccelerationDeg = robotConfig.accelerationAngle;
            double maxDecelerationDeg = robotConfig.decelerationAngle;
            double maxSpeedDeg = robotConfig.maxSpeedAngle;

            // Рассчитываем время разгона и торможения
            double accelerationTime = maxSpeedDeg / maxAccelerationDeg;
            double decelerationTime = maxSpeedDeg / maxDecelerationDeg;

            // Рассчитываем углы разгона и торможения
            double accelerationAngle = 0.5 * maxAccelerationDeg * accelerationTime * accelerationTime;
            double decelerationAngle = 0.5 * maxDecelerationDeg * decelerationTime * decelerationTime;

            if (accelerationAngle + decelerationAngle > absAngle)
            {
                // Треугольный профиль скорости
                double maxReachableSpeed = Math.Sqrt(
                    (2 * maxAccelerationDeg * maxDecelerationDeg * absAngle) /
                    (maxAccelerationDeg + maxDecelerationDeg));

                accelerationTime = maxReachableSpeed / maxAccelerationDeg;
                decelerationTime = maxReachableSpeed / maxDecelerationDeg;

                localEvent.Enqueue((lastUpdated, new ChangeAccelerationRotation(this,
                    rotationDirection * maxAccelerationDeg)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime),
                    new ChangeAccelerationRotation(this, 0)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime),
                    new ChangeAccelerationRotation(this,
                    -rotationDirection * maxDecelerationDeg)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + decelerationTime),
                    new ChangeAccelerationRotation(this, 0)));
            }
            else
            {
                // Трапециевидный профиль скорости
                double constantSpeedAngle = absAngle - accelerationAngle - decelerationAngle;
                double constantSpeedTime = constantSpeedAngle / maxSpeedDeg;

                localEvent.Enqueue((lastUpdated, new ChangeAccelerationRotation(this,
                    rotationDirection * maxAccelerationDeg)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime),
                    new ChangeAccelerationRotation(this, 0)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + constantSpeedTime),
                    new ChangeAccelerationRotation(this,
                    -rotationDirection * maxDecelerationDeg)));
                localEvent.Enqueue((lastUpdated + TimeSpan.FromSeconds(accelerationTime + constantSpeedTime + decelerationTime),
                    new ChangeAccelerationRotation(this, 0)));
            }

            isRotating = true;
        }

        private void ValidatePosition(Position expected, Position actual)
        {
            if (Math.Abs(expected.x - actual.x) > 0.001 ||
                Math.Abs(expected.y - actual.y) > 0.001 ||
                Math.Abs(expected.angle - actual.angle) > 0.001)
            {
                throw new InvalidOperationException(
                    $"Robot position mismatch. Expected: {expected}, Actual: {actual}");
            }
        }

        public void PrintState()
        {
            Console.WriteLine($"=== Robot State [{lastUpdated}] ===");
            Console.WriteLine($"Position: X = {position.x:F3} m, Y = {position.y:F3} m, Angle = {position.angle:F1}°");

            // Угловая скорость в градусах/с
            double angularSpeedDeg = angleSpeed * 180 / Math.PI;
            // Угловое ускорение в градусах/с²
            double angularAccelDeg = rotationAcceleration * 180 / Math.PI;

            Console.WriteLine($"Linear: Speed = {speed:F3} m/s, Acceleration = {acceleration:F3} m/s²");
            Console.WriteLine($"Angular: Speed = {angularSpeedDeg:F1} °/s, Acceleration = {angularAccelDeg:F1} °/s²");
            Console.WriteLine($"Movement Direction: {(movementDirection > 0 ? "Forward" : "Backward")}");
            Console.WriteLine($"Current Task: {(currentTask?.taskType.ToString() ?? "None")}");
            Console.WriteLine($"Next Events: {localEvent.Count} in queue");
            Console.WriteLine("==============================");
        }


        public override void Update(TimeSpan timeSpan)
        {
            UpdatePosition(timeSpan - lastUpdated);
            lastUpdated = timeSpan;
            PrintState();
        }

        public void UpdatePosition(TimeSpan elapsedTime)
        {
            double elapsedSeconds = elapsedTime.TotalSeconds;

            // Обновляем скорость с учетом ускорения
            double newSpeed = speed + acceleration * elapsedSeconds;

            // Ограничиваем скорость максимальным значением
            if (Math.Abs(newSpeed) > robotConfig.maxSpeed)
            {
                newSpeed = Math.Sign(newSpeed) * robotConfig.maxSpeed;
            }

            // Обновляем угловую скорость
            double newAngleSpeed = angleSpeed + rotationAcceleration * elapsedSeconds;

            // Ограничиваем угловую скорость (уже в радианах)
            if (Math.Abs(newAngleSpeed) > maxAngularSpeedRad)
            {
                newAngleSpeed = Math.Sign(newAngleSpeed) * maxAngularSpeedRad;
            }

            // Обновляем позицию на основе средней скорости
            double averageSpeed = (speed + newSpeed) / 2;

            // Конвертируем угол из градусов в радианы для вычисления направления
            double currentAngleRad = position.angle * Math.PI / 180;
            position.x += averageSpeed * Math.Cos(currentAngleRad) * elapsedSeconds;
            position.y += averageSpeed * Math.Sin(currentAngleRad) * elapsedSeconds;

            // Обновляем угол (работаем в градусах)
            double averageAngleSpeed = (angleSpeed + newAngleSpeed) / 2;
            position.angle += averageAngleSpeed * elapsedSeconds * 180 / Math.PI;

            // Нормализуем угол в градусах к диапазону [0, 360)
            position.angle %= 360;
            if (position.angle < 0) position.angle += 360;

            // Сохраняем новые значения скоростей
            speed = newSpeed;
            angleSpeed = newAngleSpeed;
        }

        internal void SetAccelerationRotation(double accelerationDeg)
        {
            // Конвертируем градусы/с² в радианы/с² для внутренних расчетов
            this.rotationAcceleration = accelerationDeg * Math.PI / 180;
        }
    }
}