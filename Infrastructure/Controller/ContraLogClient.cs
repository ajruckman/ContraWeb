using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Net;
using ClickHouse.Ado;

namespace Infrastructure.Controller
{
    public class ContraLogClient : IDisposable
    {
        private readonly ClickHouseConnection _conn;

        public ContraLogClient(string url)
        {
            _conn = new ClickHouseConnection(url);
            _conn.Open();
        }

        public void Dispose()
        {
            _conn.Dispose();
        }

        public (Dictionary<long, Dictionary<string, dynamic>>, List<string>) LogActionsPerHour()
        {
            using ClickHouseCommand cmd    = _conn.CreateCommand("SELECT * FROM log_actions_per_hour;");
            using IDataReader       reader = cmd.ExecuteReader();

            var                                           actions = new List<string>();
            Dictionary<long, Dictionary<string, dynamic>> result  = new Dictionary<long, Dictionary<string, dynamic>>();

            do
            {
                while (reader.Read())
                {
                    DateTime hour   = DateTime.ParseExact(reader.GetString(0), "yyyy-MM-dd H:mm", CultureInfo.InvariantCulture);
                    var      action = reader.GetString(1);
                    long     c      = reader.GetInt64(2);

                    if (!result.ContainsKey(hour.Ticks))
                    {
                        result[hour.Ticks] = new Dictionary<string, dynamic>
                        {
                            {"time", hour}
                        };
                    }

                    if (!actions.Contains(action))
                        actions.Add(action);

                    result[hour.Ticks][action] = c;
                }
            } while (reader.NextResult());

            return (result, actions);
        }

        public Dictionary<string, int> LogActionCounts()
        {
            using ClickHouseCommand cmd    = _conn.CreateCommand("SELECT * FROM log_action_counts;");
            using IDataReader       reader = cmd.ExecuteReader();

            var result = new Dictionary<string, int>();

            do
            {
                while (reader.Read())
                {
                    result[reader.GetString(0)] = reader.GetInt32(1);
                }
            } while (reader.NextResult());

            return result;
        }

        public List<(string, int)> TopBlocked(DateTime since, int limit)
        {
            using ClickHouseCommand cmd = _conn.CreateCommand(@"
                SELECT question,
                       count(question) AS q,
                       min(time)       AS first
                FROM contralog.log
                WHERE action LIKE 'block.%'
                  AND time > @since
                GROUP BY question
                ORDER BY q DESC
                LIMIT @limit;"
            );

            SinceLimit(cmd, since, limit);

            return Scan(cmd, reader => (reader.GetString(0), reader.GetInt32(1)));
        }

        public List<(string, int)> TopPassed(DateTime since, int limit)
        {
            using ClickHouseCommand cmd = _conn.CreateCommand(@"
                SELECT question,
                       count(question) AS q,
                       min(time)       AS first
                FROM contralog.log
                WHERE action LIKE 'pass.%'
                  AND time > @since
                GROUP BY question
                ORDER BY q DESC
                LIMIT @limit;"
            );

            SinceLimit(cmd, since, limit);

            return Scan(cmd, reader => (reader.GetString(0), reader.GetInt32(1)));
        }

        public List<(string, string, string, int)> TopClients(DateTime since, int limit)
        {
            using ClickHouseCommand cmd = _conn.CreateCommand(@"
                SELECT client, client_hostname, client_vendor, count(*) AS c
                FROM contralog.log
                WHERE time > @since
                GROUP BY client, client_hostname, client_vendor
                ORDER BY c DESC
                LIMIT @limit;"
            );

            SinceLimit(cmd, since, limit);

            return Scan(cmd, reader => (
                reader.GetString(0),
                reader.IsDBNull(1) ? "" : reader.GetString(1),
                reader.IsDBNull(2) ? "" : reader.GetString(2),
                reader.GetInt32(3)
            ));
        }

        private static List<T> Scan<T>(IDbCommand cmd, Func<ClickHouseDataReader, T> read)
        {
            List<T> result = new List<T>();

            using ClickHouseDataReader reader = (ClickHouseDataReader) cmd.ExecuteReader();

            do
            {
                while (reader.Read())
                {
                    result.Add(read.Invoke(reader));
                }
            } while (reader.NextResult());

            return result;
        }

        private static void SinceLimit(ClickHouseCommand cmd, DateTime since, int limit)
        {
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "since",
                Value         = since,
                DbType        = DbType.DateTime
            });
            cmd.Parameters.Add(new ClickHouseParameter
            {
                ParameterName = "limit",
                Value         = limit,
                DbType        = DbType.Int32
            });
        }
    }
}