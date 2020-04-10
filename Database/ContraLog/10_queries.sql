SELECT s1.action, s1.hour, s2.c
FROM (
         SELECT action, toStartOfHour(now() - toIntervalDay(7)) + (number * 60 * 60) AS hour
         FROM (
                  SELECT DISTINCT action
                  FROM log
                  ) AS actions, numbers(7 * 24)
         ) AS s1
         LEFT OUTER JOIN (
    SELECT action,
           toStartOfHour(time) AS hour,
           count(*)            AS c
    FROM log
    GROUP BY toStartOfHour(time), action
    ) AS s2
                         ON s1.hour = s2.hour AND s1.action = s2.action;
