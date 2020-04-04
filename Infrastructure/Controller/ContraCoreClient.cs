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
        private readonly FixedSizedQueue<Log> _data                      = new FixedSizedQueue<Log>(1000);
        private readonly Channel<List<Log>>   _cacheChannel              = Channel.CreateUnbounded<List<Log>>();
        private readonly SemaphoreSlim        _pongEvent                 = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim        _reloadConfigCompleteEvent = new SemaphoreSlim(1, 1);

        public event Action         OnConnected;
        public event Action         OnDisconnected;
        public event Action         OnStatusChange; // OnConnected + OnDisconnected
        public event Action         OnNewLog;
        public event Action<string> OnGenRulesCallback;

        public bool         GeneratingRules { get; private set; }
        public event Action OnGenRulesChange;

        public readonly ManualResetEventSlim ConnectionComplete = new ManualResetEventSlim();

        // public TaskCompletionSource<bool> ConnectionComplete = new TaskCompletionSource<bool>();

        public bool Connected { get; private set; }

        public ContraCoreClient(string hostname)
        {
            Task.Run(() => Read(hostname));

            OnConnected += Setup;

            OnConnected += () =>
            {
                Connected = true;
                Common.Logger.Info("Connected to ContraCore");
                ConnectionComplete.Set();
                OnStatusChange?.Invoke();
            };
            OnDisconnected += () =>
            {
                Connected = false;
                Common.Logger.Info("Disconnected from ContraCore");
                OnStatusChange?.Invoke();
            };
        }

        private void Setup()
        {
            Task.Run(async () =>
            {
                await Ping("Setup.1");
                Common.Logger.Info("Begin loading initial log data");
                Stopwatch       stopwatch = Stopwatch.StartNew();
                Task<List<Log>> cacheTask = GetCache();
                List<Log>       cache     = cacheTask.GetAwaiter().GetResult();
                _data.Enqueue(cache);
                stopwatch.Stop();
                Common.Logger.Info("Initial log data loaded",
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
                    // ConnectionComplete.SetResult(true);
                }
                catch (Exception e)
                {
                    Common.Logger.Warning("Failed to connect to server; retrying...", e: e, meta: new Fields {{"Hostname", hostname}});
                    Thread.Sleep(1000);

                    OnDisconnected?.Invoke();

                    continue;
                }

                try
                {
                    string line;
                    while ((line = await _reader.ReadLineAsync()) != null)
                    {
                        // string[] parts = line.Split();
                        // string   cmd   = parts[0];

                        string cmd = line.Contains(" ") ? line.Substring(0, line.IndexOf(" ")) : line;
                        string val = line.Substring(line.IndexOf(" ") + 1);
                        
                        if (cmd != "query")
                            Common.Logger.Debug($"NetMgr <- {cmd}");

                        switch (cmd)
                        {
                            case "ping":
                                await Send("ping.pong");
                                break;

                            case "ping.pong":
                                _pongEvent.Release();
                                break;

                            case "get_cache.cache":
                                List<Log> logs = JsonConvert.DeserializeObject<List<Log>>(val);
                                await _cacheChannel.Writer.WriteAsync(logs);
                                break;

                            case "query":
                                Log log = JsonConvert.DeserializeObject<Log>(val);
                                Common.Logger.Debug($"Incoming: {log}");
                                _data.Enqueue(log);

                                OnNewLog?.Invoke();
                                break;

                            case "gen_rules.sources":
                            case "gen_rules.gen_progress":
                            case "gen_rules.save_progress":
                            case "gen_rules.saved_in":
                            case "gen_rules.recache_progress":
                            case "gen_rules.recached_in":
                                OnGenRulesCallback?.Invoke(val);
                                break;

                            case "gen_rules.complete":
                                GeneratingRules = false;
                                OnGenRulesChange?.Invoke();
                                break;
                            
                            case "reload_config.complete":
                                _reloadConfigCompleteEvent.Release();
                                break;

                            default:
                                Common.Logger.Error(
                                    $"Unmatched command received from NetManagerClient: {cmd}",
                                    new InvalidOperationException()
                                );
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Common.Logger.Warning("Failed to read from server; reconnecting...", e, new Fields {{"Hostname", hostname}});
                    Thread.Sleep(1000);

                    OnDisconnected?.Invoke();
                }
            }
        }

        // public IEnumerable<Log> Data() => _data.Queue.Any() ? _data.Queue.Reverse() : new List<Log>();

        public IEnumerable<Log> Data()
        {
            if (_data.Queue.Count == 0)
            {
                yield break;
            }

            for (int i = _data.Queue.Count - 1; i >= 0; i--)
            {
                yield return _data.Queue.ElementAt(i);
            }
        }

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

        public async Task GenRules()
        {
            GeneratingRules = true;
            OnGenRulesChange?.Invoke();
            await Send("gen_rules");
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
                Common.Logger.Info("Pong received " + caller, new Fields {{"Time (ms)", stopwatch.ElapsedMilliseconds}});
                return true;
            }

            Common.Logger.Warning("Pong timeout occured " + caller);
            _pongEvent.Release();

            return false;
        }

        public async Task<bool> ReloadConfig()
        {
            if (_client == null)
                return false;

            CancellationTokenSource source = new CancellationTokenSource();
            OnDisconnected += source.Cancel;

            await Send("reload_config");
            Stopwatch stopwatch = Stopwatch.StartNew();

            Task task = _reloadConfigCompleteEvent.WaitAsync(source.Token);
            if (await Task.WhenAny(task, Task.Delay(1000, source.Token)) == task)
            {
                Common.Logger.Info("Reload config complete message received", new Fields {{"Time (ms)", stopwatch.ElapsedMilliseconds}});
                return true;
            }

            Common.Logger.Warning("Reload config timeout occured");
            _reloadConfigCompleteEvent.Release();
            
            return false;
        }

        private async Task Send(string msg)
        {
            Common.Logger.Debug($"NetMgr -> {msg}");
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