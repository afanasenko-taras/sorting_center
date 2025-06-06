WITH data AS (
  SELECT
    id,
    robot,
    "botId",
    "startTime",
    "endTime",
    "TrID",
    command,
    LAG("endTime") OVER (PARTITION BY robot, "botId" ORDER BY id) AS raw_prev_end,
    LAG("startTime") OVER (PARTITION BY robot, "botId" ORDER BY id) AS raw_prev_start,
    LEAD("startTime") OVER (PARTITION BY robot, "botId" ORDER BY id) AS raw_next_start,
    LEAD("endTime") OVER (PARTITION BY robot, "botId" ORDER BY id) AS raw_next_end,
    LEAD(command) OVER (PARTITION BY robot, "botId" ORDER BY id) AS next_command,
    ROW_NUMBER() OVER (PARTITION BY robot, "botId" ORDER BY "startTime", id) AS row_num,
    COUNT(*) OVER (PARTITION BY robot, "botId") AS total_rows
  FROM public.palletize_result
),
converted_data AS (
  SELECT
    *,
    -- Конвертация времени в секунды
    EXTRACT(EPOCH FROM "startTime") AS start_seconds,
    EXTRACT(EPOCH FROM "endTime") AS end_seconds,
    EXTRACT(EPOCH FROM raw_prev_end) AS prev_end_seconds,
    EXTRACT(EPOCH FROM raw_prev_start) AS prev_start_seconds,
    EXTRACT(EPOCH FROM raw_next_start) AS next_start_seconds,
    EXTRACT(EPOCH FROM raw_next_end) AS next_end_seconds
  FROM data
),
filtered_data AS (
  SELECT
    robot,
    -- Длительность предыдущей операции (с проверкой NULL и отрицательных значений)
    COALESCE(GREATEST(prev_end_seconds - prev_start_seconds, 0), 0) AS prev_operation_duration_seconds,

    -- Время до текущей операции (0 для первой записи)
    CASE
      WHEN row_num = 1 THEN 0
      ELSE GREATEST(start_seconds - prev_end_seconds, 0)
    END AS time_before_seconds,

    -- Длительность текущей операции
    EXTRACT(EPOCH FROM ("endTime" - "startTime")) AS operation_duration_seconds,

    -- Время после текущей операции (0 для последней записи)
    CASE
      WHEN row_num = total_rows THEN 0
      ELSE GREATEST(next_start_seconds - end_seconds, 0)
    END AS time_after_seconds,

    -- Время следующей операции с каналом (если команда корректна)
    CASE
      WHEN next_command = 'start_movebox2channel'
        THEN next_end_seconds - next_start_seconds
      ELSE 0
    END AS unload2channel_seconds
  FROM converted_data
  WHERE "TrID" = '1.58.44_1.1_0' -- Проверьте значение TrID
)
SELECT
  robot,
  -- Агрегированные результаты с конвертацией в интервал
  SUM(prev_operation_duration_seconds) * INTERVAL '1 second' AS total_load2bot,
  SUM(time_before_seconds) * INTERVAL '1 second' AS total_time_before,
  SUM(operation_duration_seconds) * INTERVAL '1 second' AS total_time_load2tr
  --SUM(time_after_seconds) * INTERVAL '1 second' AS total_time_after,
  --SUM(unload2channel_seconds) * INTERVAL '1 second' AS total_unload2channel
FROM filtered_data
GROUP BY robot
ORDER BY robot;