using System;
using System.Data;
using ClickHouse.Ado;

namespace Database.ContraLog
{
    public class Client : IDisposable
    {
        private readonly ClickHouseConnection _conn;

        public Client(string url)
        {
            _conn = new ClickHouseConnection(url);
            _conn.Open();
        }

        public IDataReader Query(string command)
        {
            ClickHouseCommand cmd    = _conn.CreateCommand(command);
            IDataReader       reader = cmd.ExecuteReader();
            return reader;
        }

        public void Dispose()
        {
            _conn?.Dispose();
        }
    }
}