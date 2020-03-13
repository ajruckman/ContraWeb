using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Superset.Logging;
using Log = Infrastructure.Schema.Log;

namespace Infrastructure.Controller
{
    public class ContraCoreClient
    {
        private          TcpClient            _client;
        private          NetworkStream        _stream;
        private          StreamReader         _reader;
        private readonly FixedSizedQueue<Log> _data         = new FixedSizedQueue<Log>(1000);
        private readonly Channel<List<Log>>   _cacheChannel = Channel.CreateUnbounded<List<Log>>();
        private readonly SemaphoreSlim        _pongEvent    = new SemaphoreSlim(1, 1);

        private readonly Logger _logger = new Logger();

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnStatusChange; // OnConnected + OnDisconnected
        public event Action OnNewLog;
        
        public TaskCompletionSource<bool> ConnectionComplete = new TaskCompletionSource<bool>();

        public bool Connected { get; private set; }

        public ContraCoreClient(string hostname)
        {
            Task.Run(() => Read(hostname));

            OnConnected += Setup;

            OnConnected += () =>
            {
                Connected = true;
                _logger.Info("Connected to ContraCore");
                OnStatusChange?.Invoke();
            };
            OnDisconnected += () =>
            {
                Connected = false;
                _logger.Info("Disconnected from ContraCore");
                OnStatusChange?.Invoke();
            };
        }

        private void Setup()
        {
            Task.Run(async () =>
            {
                await Ping("Setup.1");
                _logger.Info("Begin loading initial log data");
                Stopwatch       stopwatch = Stopwatch.StartNew();
                Task<List<Log>> cacheTask = GetCache();
                List<Log>       cache     = cacheTask.GetAwaiter().GetResult();
                _data.Enqueue(cache);
                stopwatch.Stop();
                _logger.Info("Initial log data loaded",
                    new Fields {{"Count", cache.Count}, {"Time (ms)", stopwatch.ElapsedMilliseconds}});
                OnNewLog?.Invoke();
            });

            OnConnected -= Setup;
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private async void Read(string hostname)
        {
            while (true)
            {
                try
                {
                    _client = new TcpClient(hostname, 64417);
                    _stream = _client.GetStream();
                    _reader = new StreamReader(_stream);

                    OnConnected?.Invoke();
                    ConnectionComplete.SetResult(true);
                }
                catch (Exception)
                {
                    _logger.Warning("Failed to connect to server; retrying...", meta: new Fields {{"Hostname", hostname}},
                        printStacktrace: true);
                    Thread.Sleep(1000);

                    OnDisconnected?.Invoke();

                    continue;
                }

                try
                {
                    string line;
                    while ((line = await _reader.ReadLineAsync()) != null)
                    {
                        string[] parts = line.Split();
                        string   cmd   = parts[0];

                        _logger.Debug($"NetMgr <- {cmd}");

                        switch (cmd)
                        {
                            case "ping":
                                await Send("pong");
                                break;

                            case "pong":
                                _pongEvent.Release();
                                break;

                            case "cache":
                                List<Log> logs = JsonConvert.DeserializeObject<List<Log>>(parts[1]);
                                await _cacheChannel.Writer.WriteAsync(logs);
                                break;

                            case "query":
                                Log log = JsonConvert.DeserializeObject<Log>(parts[1]);
                                _logger.Debug($"Incoming: {log}");
                                _data.Enqueue(log);

                                OnNewLog?.Invoke();
                                break;

                            default:
                                _logger.Error(
                                    $"Unmatched command received from NetManagerClient: {cmd}",
                                    new InvalidOperationException()
                                );
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Warning("Failed to read from server; reconnecting...", e, new Fields {{"Hostname", hostname}});
                    Thread.Sleep(1000);

                    OnDisconnected?.Invoke();
                }
            }
        }

        public IEnumerable<Log> Data() => _data.Queue.Any() ? _data.Queue.Reverse() : new List<Log>();

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }

        public async Task<List<Log>> GetCache()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            OnDisconnected += source.Cancel;
            await Send("get_cache");
            return await _cacheChannel.Reader.ReadAsync(source.Token);
        }

        public async Task<bool> Ping(string caller)
        {
            if (_client == null)
                return false;

            CancellationTokenSource source = new CancellationTokenSource();
            OnDisconnected += source.Cancel;

            await Send("ping");
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task task = _pongEvent.WaitAsync(source.Token);
            if (await Task.WhenAny(task, Task.Delay(1000, source.Token)) == task)
            {
                _logger.Info("Pong received " + caller, new Fields {{"Time (ms)", stopwatch.ElapsedMilliseconds}});
                return true;
            }

            _logger.Warning("Pong timeout occured " + caller);
            _pongEvent.Release();

            return false;
        }

        private async Task Send(string msg)
        {
            _logger.Debug($"NetMgr -> {msg}");
            byte[] b = Encoding.ASCII.GetBytes(msg + "\n");
            await _stream.WriteAsync(b, 0, b.Length);
        }

        // https://stackoverflow.com/a/9371346
        private class FixedSizedQueue<T>
        {
            public readonly ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();

            public FixedSizedQueue(int size)
            {
                Size = size;
            }

            private int Size { get; }

            public void Enqueue(T obj)
            {
                Queue.Enqueue(obj);

                while (Queue.Count > Size)
                    Queue.TryDequeue(out T _);
            }

            public void Enqueue(IEnumerable<T> objs)
            {
                foreach (T obj in objs)
                    Queue.Enqueue(obj);

                while (Queue.Count > Size)
                    Queue.TryDequeue(out T _);
            }
        }
    }
}