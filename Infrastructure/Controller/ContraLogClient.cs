using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
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
                    DateTime hour   = DateTime.ParseExact(reader.GetString(0), "yyyy-MM-dd H", CultureInfo.InvariantCulture);
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

        public Dictionary<string, long> LogActionCounts()
        {
            using ClickHouseCommand cmd    = _conn.CreateCommand("SELECT * FROM log_action_counts;");
            using IDataReader       reader = cmd.ExecuteReader();

            var result = new Dictionary<string, long>();

            do
            {
                while (reader.Read())
                {
                    result[reader.GetString(0)] = reader.GetInt64(1);
                }
            } while (reader.NextResult());

            return result;
        }
    }
}