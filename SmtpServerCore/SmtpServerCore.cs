using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Toolbelt.Net.Smtp.Internal;

namespace Toolbelt.Net.Smtp
{
    public class SmtpServerCore : IDisposable
    {
        public bool IsRunning { get; protected set; }

        public NetworkCredential[] Credentials { get; protected set; }

        public event EventHandler<ReceiveMessageEventArgs> ReceiveMessage;

        protected IPEndPoint[] _EndPoints;

        protected TcpListener[] _Listeners;

        protected List<SmtpServerSessionThread> _SessionThreads;

        public SmtpServerCore()
        {
            Initialize(new[] { new IPEndPoint(IPAddress.Loopback, 25) }, null);
        }

        public SmtpServerCore(IPAddress address, int port, IEnumerable<NetworkCredential> credentials = null)
        {
            Initialize(new[] { new IPEndPoint(address, port) }, credentials);
        }

        public SmtpServerCore(IEnumerable<IPEndPoint> endPoints, IEnumerable<NetworkCredential> credentials = null)
        {
            Initialize(endPoints, credentials);
        }

        protected virtual void Initialize(IEnumerable<IPEndPoint> endPoints, IEnumerable<NetworkCredential> credentials)
        {
            this.Credentials = (credentials ?? Enumerable.Empty<NetworkCredential>()).ToArray();
            this._EndPoints = endPoints.ToArray();
            this._SessionThreads = new List<SmtpServerSessionThread>();
            this.ReceiveMessage += (_, __) => { };
        }

        public void Start()
        {
            lock (this)
            {
                if (this.IsRunning) return;
                this._Listeners = this._EndPoints
                    .Select(p => new TcpListener(p))
                    .ToArray();
                Array.ForEach(_Listeners, l =>
                {
                    l.Start();
                    l.BeginAcceptTcpClient(OnAcceptTcpClient, l);
                });
                this.IsRunning = true;
            }
        }

        private void OnAcceptTcpClient(IAsyncResult ar)
        {
            var listener = ar.AsyncState as TcpListener;
            try
            {
                var tcpClient = listener.EndAcceptTcpClient(ar);
                lock (this)
                {
                    CleanUpSessionTreadsList();
                    var session = new SmtpServerSession(this, tcpClient);
                    var sessionThread = new SmtpServerSessionThread(session,
                        new Thread(() =>
                    {
                        session.ExecuteSession();
                    }));
                    _SessionThreads.Add(sessionThread);
                    sessionThread.Thread.Start();
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            listener.BeginAcceptTcpClient(OnAcceptTcpClient, listener);
        }

        private void CleanUpSessionTreadsList()
        {
            lock (this)
            {
                var deadThreads = _SessionThreads.Where(t => t.Thread.IsAlive == false).ToList();
                deadThreads.ForEach(t => _SessionThreads.Remove(t));
            }
        }

        public bool WaiteForEndOfAllSessions(int millisecondsTimeout = 5000)
        {
            var threads = default(List<SmtpServerSessionThread>);
            lock (this)
            {
                threads = _SessionThreads.AsEnumerable().ToList();
            }

            var timedOut = false;
            var sw = new Stopwatch();
            sw.Start();
            threads.ForEach(t =>
            {
                timedOut |= (sw.ElapsedMilliseconds >= millisecondsTimeout);
                if (timedOut) return;
                timedOut |= !t.Thread.Join(millisecondsTimeout);
            });
            return !timedOut;
        }

        public void Stop(int millisecondsTimeout = 5000)
        {
            lock (this)
            {
                if (this.IsRunning == false) return;
                Array.ForEach(_Listeners, l => l.Stop());

                var timedOut = !this.WaiteForEndOfAllSessions(millisecondsTimeout);
                if (timedOut) KillAllSessionTreads();

                CleanUpSessionTreadsList();
                this.IsRunning = false;
            }
        }

        protected void KillAllSessionTreads()
        {
            var aliveThreads = _SessionThreads.Where(t => t.Thread.IsAlive).ToList();
            aliveThreads.ForEach(t =>
            {
                t.Session.Dispose();
                t.Thread.Abort();
            });
        }

        public void Dispose()
        {
            this.Stop(millisecondsTimeout: 0);
        }

        internal void FireReceiveMessage(SmtpMessage message)
        {
            this.ReceiveMessage(this, new ReceiveMessageEventArgs(message));
        }
    }
}
