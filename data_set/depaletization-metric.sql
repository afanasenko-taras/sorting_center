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
    EXTRACT(EPOCH FROM "startTime") AS start_seconds,
    EXTRACT(EPOCH FROM "endTime") AS end_seconds,
    EXTRACT(EPOCH FROM raw_prev_end) AS prev_end_seconds,
    EXTRACT(EPOCH FROM raw_next_start) AS next_start_seconds,
    EXTRACT(EPOCH FROM raw_next_end) AS next_end_seconds
  FROM data
),
filtered_data AS (
  SELECT
    robot,
    CASE
      WHEN row_num = 1 THEN 0
      ELSE GREATEST(start_seconds - prev_end_seconds, 0)
    END AS time_before_seconds,
    EXTRACT(EPOCH FROM ("endTime" - "startTime")) AS operation_duration_seconds,
    CASE
      WHEN row_num = total_rows THEN 0
      ELSE GREATEST(next_start_seconds - end_seconds, 0)
    END AS time_after_seconds,
    CASE
      WHEN next_command = 'start_movebox2channel' THEN next_end_seconds - next_start_seconds
      ELSE 0
    END AS unload2channel_seconds
  FROM converted_data
  WHERE "TrID" = '1.-1.11_1.1_0'
)
SELECT
  robot,
  SUM(time_before_seconds) * INTERVAL '1 second' AS total_time_before,
  SUM(operation_duration_seconds) * INTERVAL '1 second' AS total_time_load2bot,
  SUM(time_after_seconds) * INTERVAL '1 second' AS total_time_after,
  SUM(unload2channel_seconds) * INTERVAL '1 second' AS total_unload2channel
FROM filtered_data
GROUP BY robot
ORDER BY robot;