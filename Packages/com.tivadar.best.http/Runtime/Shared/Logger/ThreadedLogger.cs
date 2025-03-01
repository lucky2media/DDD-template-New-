using System;
using System.Collections.Concurrent;
using System.Text;
using Best.HTTP.Shared.PlatformSupport.Text;
using Best.HTTP.Shared.PlatformSupport.Threading;

namespace Best.HTTP.Shared.Logger
{
    /// <summary>
    /// <see cref="IFilter"/> implementation to include only one division in the log output.
    /// </summary>
    public sealed class SingleDivisionFilter : IFilter
    {
        private string _division;

        public SingleDivisionFilter(string division) => this._division = division;

        public bool Include(string division) => this._division.Equals(division, StringComparison.Ordinal);
    }

    /// <summary>
    /// <see cref="IFilter"/> implementation to allow filtering for multiple divisions.
    /// </summary>
    public sealed class MultiDivisionFilter : IFilter
    {
        private string[] _divisions;

        public MultiDivisionFilter(string[] divisions)
        {
            this._divisions = divisions;
            for (int i = 0; i < this._divisions.Length; ++i)
                this._divisions[i] = this._divisions[i].Trim();
        }

        public bool Include(string division)
        {
            for (int i = 0; i < this._divisions.Length; ++i)
            {
                ref var div = ref this._divisions[i];

                if (div.Equals(division, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }

    public sealed class ThreadedLogger : Best.HTTP.Shared.Logger.ILogger, IDisposable
    {
        public Loglevels Level { get; set; }

        public bool IsDiagnostic { get => this.Level == Loglevels.All; }

        public ILogOutput Output { get { return this._output; }
            set
            {
                if (this._output != value)
                {
                    if (this._output != null)
                        this._output.Dispose();
                    this._output = value;
                }
            }
        }
        private ILogOutput _output;

        public IFilter Filter { get; set; }

        public bool IsEmpty
#if !UNITY_WEBGL || UNITY_EDITOR
            => this.jobs.IsEmpty;
#else
            => true;
#endif

        public int InitialStringBufferCapacity = 256;

#if !UNITY_WEBGL || UNITY_EDITOR
        public TimeSpan ExitThreadAfterInactivity = TimeSpan.FromMinutes(1);
        public int QueuedJobs { get => this.jobs.Count; }

        private ConcurrentQueue<LogJob> jobs = new ConcurrentQueue<LogJob>();
        private System.Threading.AutoResetEvent newJobEvent = new System.Threading.AutoResetEvent(false);

        private volatile int threadCreated;

        private volatile bool isDisposed;
#endif

        private StringBuilder sb = new StringBuilder(0);

        public ThreadedLogger()
        {
            this.Level = UnityEngine.Debug.isDebugBuild ? Loglevels.Warning : Loglevels.Error;
            this.Output = new UnityOutput();
        }

        public void Verbose(string division, string msg, LoggingContext context) {
            AddJob(Loglevels.All, division, msg, null, context);
        }

        public void Information(string division, string msg, LoggingContext context) {
            AddJob(Loglevels.Information, division, msg, null, context);
        }

        public void Warning(string division, string msg, LoggingContext context) {
            AddJob(Loglevels.Warning, division, msg, null, context);
        }

        public void Error(string division, string msg, LoggingContext context) {
            AddJob(Loglevels.Error, division, msg, null, context);
        }

        public void Exception(string division, string msg, Exception ex, LoggingContext context) {
            AddJob(Loglevels.Exception, division, msg, ex, context);
        }

        private void AddJob(Loglevels level, string div, string msg, Exception ex, LoggingContext context)
        {
            if (this.Level > level)
                return;

            var filter = this.Filter;
            if (filter != null && !filter.Include(div))
                return;

            sb.EnsureCapacity(InitialStringBufferCapacity);

#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.isDisposed)
                return;
#endif

            string json = null;
            if (context != null)
            {
                var jsonBuilder = StringBuilderPool.Get(1);
                context.ToJson(jsonBuilder);
                json = StringBuilderPool.ReleaseAndGrab(jsonBuilder);
            }

            var job = new LogJob
            {
                level = level,
                division = div,
                msg = msg,
                ex = ex,
                time = DateTime.UtcNow,
                threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                stackTrace = System.Environment.StackTrace,
                context = json
            };

#if !UNITY_WEBGL || UNITY_EDITOR
            // Start the consumer thread before enqueuing to get up and running sooner
            if (System.Threading.Interlocked.CompareExchange(ref this.threadCreated, 1, 0) == 0)
                Best.HTTP.Shared.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);

            this.jobs.Enqueue(job);
            try
            {
                this.newJobEvent.Set();
            }
            catch
            {
                try
                {
                    this.Output.Write(job.level, job.ToJson(sb, this.Output.AcceptColor));
                }
                catch
                { }
                return;
            }

            // newJobEvent might timed out between the previous threadCreated check and newJobEvent.Set() calls closing the thread.
            // So, here we check threadCreated again and create a new thread if needed.
            if (System.Threading.Interlocked.CompareExchange(ref this.threadCreated, 1, 0) == 0)
                Best.HTTP.Shared.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);
#else
            this.Output.Write(job.level, job.ToJson(sb, this.Output.AcceptColor));
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void ThreadFunc()
        {
            ThreadedRunner.SetThreadName("Best.HTTP.Logger");
            try
            {
                LogJob job;
                /*
                LogJob job = new LogJob
                {
                    level = Loglevels.Information,
                    division = "ThreadFunc",
                    msg = "Log thread starting!",
                    ex = null,
                    time = DateTime.UtcNow,
                    threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                    stackTrace = null,
                    context1 = null,
                    context2 = null,
                    context3 = null
                };

                WriteJob(ref job);
                */

                do
                {
                    // Waiting for a new log-job timed out
                    if (!this.newJobEvent.WaitOne(this.ExitThreadAfterInactivity))
                    {
                        /*
                        job = new LogJob
                        {
                            level = Loglevels.Information,
                            division = "ThreadFunc",
                            msg = "Log thread quitting!",
                            ex = null,
                            time = DateTime.UtcNow,
                            threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                            stackTrace = null,
                            context1 = null,
                            context2 = null,
                            context3 = null
                        };

                        WriteJob(ref job);
                        */

                        // clear StringBuilder's inner cache and exit the thread
                        sb.Length = 0;
                        sb.Capacity = 0;
                        System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);
                        return;
                    }

                    try
                    {
                        int count = 0;
                        while (this.jobs.TryDequeue(out job))
                        {
                            WriteJob(ref job);

                            if (++count >= 1000)
                                this.Output.Flush();
                        }
                    }
                    finally
                    {
                        this.Output.Flush();
                    }

                } while (!HTTPManager.IsQuitting);
                System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);

                // When HTTPManager.IsQuitting is true, there is still logging that will create a new thread after the last one quit
                //  and always writing a new entry about the exiting thread would be too much overhead.
                // It would also hard to know what's the last log entry because some are generated on another thread non-deterministically.

                //var lastLog = new LogJob
                //{
                //    level = Loglevels.All,
                //    division = "ThreadedLogger",
                //    msg = "Log Processing Thread Quitting!",
                //    time = DateTime.UtcNow,
                //    threadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                //};
                //
                //this.Output.WriteVerbose(lastLog.ToJson(sb));
            }
            catch
            {
                System.Threading.Interlocked.Exchange(ref this.threadCreated, 0);
            }
        }

        void WriteJob(ref LogJob job)
        {
            try
            {
                this.Output.Write(job.level, job.ToJson(sb, this.Output.AcceptColor));
            }
            catch
            { }
        }

#endif

        public void Dispose()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            this.isDisposed = true;

            if (this.newJobEvent != null)
            {
                this.newJobEvent.Close();
                this.newJobEvent = null;
            }
#endif

            if (this.Output != null)
            {
                this.Output.Dispose();
                this.Output = new UnityOutput();
            }

            GC.SuppressFinalize(this);
        }
    }

    [Best.HTTP.Shared.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    struct LogJob
    {
        private static string[] LevelStrings = new string[] { "Verbose", "Information", "Warning", "Error", "Exception" };
        public Loglevels level;
        public string division;
        public string msg;
        public Exception ex;

        public DateTime time;
        public int threadId;
        public string stackTrace;

        public string context;

        private static string WrapInColor(string str, string color, bool acceptColor)
        {
#if UNITY_EDITOR
            return acceptColor ? string.Format("<b><color={1}>{0}</color></b>", str, color) : str;
#else
            return str;
#endif
        }

        public string ToJson(StringBuilder sb, bool acceptColor)
        {
            sb.Length = 0;

            sb.AppendFormat("{{\"tid\":{0},\"div\":\"{1}\",\"msg\":\"{2}\"",
                WrapInColor(this.threadId.ToString(), "yellow", acceptColor),
                WrapInColor(this.division, "yellow", acceptColor),
                WrapInColor(LoggingContext.Escape(this.msg), "yellow", acceptColor));

            if (ex != null)
            {
                sb.Append(",\"ex\": [");

                Exception exception = this.ex;

                while (exception != null)
                {
                    sb.Append("{\"msg\": \"");
                    sb.Append(LoggingContext.Escape(exception.Message));
                    sb.Append("\", \"stack\": \"");
                    sb.Append(LoggingContext.Escape(exception.StackTrace));
                    sb.Append("\"}");

                    exception = exception.InnerException;

                    if (exception != null)
                        sb.Append(",");
                }

                sb.Append("]");
            }

            if (this.stackTrace != null)
            {
                sb.Append(",\"stack\":\"");
                ProcessStackTrace(sb);
                sb.Append("\"");
            }
            else
                sb.Append(",\"stack\":\"\"");

            if (this.context != null)
            {
                sb.Append(",\"ctx\": [");
                sb.Append(this.context);
                sb.Append("]");
            }
            else
                sb.Append(",\"ctxs\":[]");

            sb.AppendFormat(",\"t\":{0},\"ll\":\"{1}\",",
                this.time.Ticks.ToString(),
                LevelStrings[(int)this.level]);

            sb.Append("\"bh\":1}");

            return sb.ToString();
        }

        private void ProcessStackTrace(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(this.stackTrace))
                return;

            var lines = this.stackTrace.Split('\n');

            // skip top 4 lines that would show the logger.
            for (int i = 3; i < lines.Length; ++i)
                sb.Append(LoggingContext.Escape(lines[i].Replace("Best.HTTP.", "")));
        }
    }
}
