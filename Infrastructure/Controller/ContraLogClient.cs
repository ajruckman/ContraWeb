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

        public struct HourStat
        {
            public readonly string Action;
            public readonly long   Count;

            public HourStat(string action, long count)
            {
                Action = action;
                Count  = count;
            }
        }

        public ContraLogClient(string url)
        {
            _conn = new ClickHouseConnection(url);
            _conn.Open();
        }

        public (Dictionary<long, Dictionary<string, dynamic>>, List<string>) LogActionsPerHour()
        {
            using ClickHouseCommand cmd = _conn.CreateCommand("SELECT * FROM log_actions_per_hour;");

            // cmd.Parameters.Add(new ClickHouseParameter
            // {
            //     ParameterName = "minTime",
            //     Value         = DateTime.Now.Subtract(TimeSpan.FromDays(7))
            // });

            // Dictionary<long, Dictionary<string, dynamic>> result = new Dictionary<long, Dictionary<string, dynamic>>();
            var actions = new List<string>();

            Dictionary<long, Dictionary<string, dynamic>> result = new Dictionary<long, Dictionary<string, dynamic>>();

            // Dictionary<string, List<HourStat>> result = new Dictionary<string, List<HourStat>>();

            using IDataReader reader = cmd.ExecuteReader();

            do
            {
                while (reader.Read())
                {
                    DateTime hour   = DateTime.ParseExact(reader.GetString(0), "yyyy-MM-dd H", CultureInfo.InvariantCulture);
                    // string hour   = reader.GetString(0);
                    var    action = reader.GetString(1);
                    long   c      = reader.GetInt64(2);
                    // long c = rd.Next(0, 5000);

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
                    // result[action].Add(new HourStat(hour, c));
                }
            } while (reader.NextResult());

            return (result, actions);
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}