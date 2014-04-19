using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Toolbelt.Net.Smtp.Internal
{
    public class SmtpServerSession : IDisposable
    {
        public IDictionary<string, object> Items { get; protected set; }

        protected SmtpAuthIdentity _User;

        public IIdentity User { get { return _User; } }

        public SmtpMessage Message { get; protected set; }

        public SmtpServerCore Server { get; protected set; }

        public TcpClient TcpClient { get; protected set; }

        protected StreamReader Reader { get; set; }

        protected Stream BaseStream { get; set; }

        public SmtpServerSession(SmtpServerCore server, TcpClient tcpClient)
        {
            this.Reset();
            this.Server = server;
            this.TcpClient = tcpClient;
            this.BaseStream = tcpClient.GetStream();
            this.Reader = new StreamReader(tcpClient.GetStream());
        }

        public void Reset()
        {
            this.Items = new Dictionary<string, object>();
            this._User = new SmtpAuthIdentity();
            this.Message = new SmtpMessage();
        }

        public void WriteLine(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text + "\r\n");
            try
            {
                this.BaseStream.Write(bytes, 0, bytes.Length);
                this.BaseStream.Flush();
            }
            catch (IOException)
            {
                Thread.CurrentThread.Abort();
                throw;
            }
        }

        public void WriteLine(int status, string text, params string[] args)
        {
            this.WriteLine(string.Format("{0} {1}", status, string.Format(text, args)));
        }

        public SmtpClientInputLine ReadLine()
        {
            try
            {
                return new SmtpClientInputLine(this.Reader.ReadLine());
            }
            catch (IOException)
            {
                Thread.CurrentThread.Abort();
                throw;
            }
        }

        public void ClearIdentity()
        {
            this._User.Name = "";
            this._User.IsAuthenticated = false;
        }

        protected void FireReceieMessage()
        {
            this.Server.FireReceiveMessage(this.Message);
        }

        protected void EndSession()
        {
            this.SessionEnded = true;
        }

        public void Dispose()
        {
            this.BaseStream.Dispose();
            this.Reader.Dispose();
            this.TcpClient.Close();
        }


        protected Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _CurrentHandlers;

        public bool SessionEnded { get; set; }

        public void ExecuteSession()
        {
            _CurrentHandlers = _InitialHandlers;
            this.WriteLine(220, "SmtpServerCore");

            do
            {
                var input = this.ReadLine();
                DispatchHandler(input);
            } while (SessionEnded == false);

            this.Dispose();
        }

        private void DispatchHandler(SmtpClientInputLine input)
        {
            var handler = default(Action<SmtpServerSession, SmtpClientInputLine>);
            if (_CurrentHandlers.TryGetValue(input.Command.ToUpper(), out handler) == false)
            {
                handler = _CurrentHandlers["*"];
            }
            handler(this, input);
        }

        protected static readonly Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _InitialHandlers = new Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> { 
                {"HELO", HandleOK},
                {"EHLO", OnEHLO},
                {"AUTH", OnAuth},
                {"MAIL", OnMailFrom},
                {"RCPT", OnRcptTo},
                {"DATA", OnData},
                {"QUIT", OnQuit},
                {"*", UnknownCommand},
            };

        protected static readonly Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _AuthPlainHandlers = new Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> { 
                {"*", AuthPlain} };

        protected static readonly Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _AuthCRAMMD5Handlers = new Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> { 
                {"*", AuthCRAMMD5} };

        protected static readonly Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _HeaderHandlers = new Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> { 
                {"*", HandleHeader} };

        protected static readonly Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> _BodyHandlers = new Dictionary<string, Action<SmtpServerSession, SmtpClientInputLine>> { 
                {"*", HandleBody} };

        protected static void HandleOK(SmtpServerSession context, SmtpClientInputLine input)
        {
            context.WriteLine(250, "Ok");
        }

        protected static void OnEHLO(SmtpServerSession context, SmtpClientInputLine input)
        {
            if (context.Server.Credentials.Any())
            {
                context.WriteLine(
                    "250-localhost\r\n" +
                    "250-AUTH CRAM-MD5\r\n" +
                    "250-AUTH LOGIN CRAM-MD5\r\n" +
                    "250-AUTH=CRAM-MD5\r\n" +
                    "250 AUTH=LOGIN CRAM-MD5");
            }
            else context.WriteLine(250, "Ok");
        }

        protected static void OnAuth(SmtpServerSession context, SmtpClientInputLine input)
        {
            var loginMethod = input.Params.First();
            switch (loginMethod.ToUpper())
            {
                case "LOGIN":
                    AuthPlain(context, input);
                    return;
                case "CRAM-MD5":
                    AuthCRAMMD5(context, input);
                    return;
                default:
                    FailAuthentication(context);
                    break;
            }
        }

        protected static void AuthPlain(SmtpServerSession context, SmtpClientInputLine input)
        {
            try
            {
                context._CurrentHandlers = _AuthPlainHandlers;
                if (input.Command.ToUpper() == "AUTH")
                {
                    context.ClearIdentity();
                    var user = input.Params.Skip(1).FirstOrDefault() ?? "";
                    if (user.IsNullOrEmpty())
                    {
                        context.WriteLine(334, "Username:".GetBytes().ToBase64());
                    }
                    else
                    {
                        context._User.Name = user.Base64ToBytes().ToString(Encoding.UTF8);
                        context.WriteLine(334, "Password:".GetBytes().ToBase64());
                    }
                }
                else if (context.User.Name.IsNullOrEmpty())
                {
                    context._User.Name = input.RawInput.Base64ToBytes().ToString(Encoding.UTF8);
                    context.WriteLine(334, "Password:".GetBytes().ToBase64());
                }
                else
                {
                    var password = input.RawInput.Base64ToBytes().ToString(Encoding.UTF8);
                    var found = context.Server.Credentials.Any(c => c.UserName == context.User.Name && c.Password == password);
                    if (found == false)
                    {
                        FailAuthentication(context);
                    }
                    else
                    {
                        Authentication(context);
                    }
                }
            }
            catch (FormatException)
            {
                context._CurrentHandlers = _InitialHandlers;
                UnknownCommand(context, input);
            }
        }

        protected static void AuthCRAMMD5(SmtpServerSession context, SmtpClientInputLine input)
        {
            try
            {
                const string AuthCRAMMD5Key = "AuthCRAMMD5Key";
                context._CurrentHandlers = _AuthCRAMMD5Handlers;
                if (input.Command.ToUpper() == "AUTH")
                {
                    context.ClearIdentity();
                    context.Items[AuthCRAMMD5Key] = Guid.NewGuid().ToByteArray().ToBase64();
                    context.WriteLine(334, context.Items[AuthCRAMMD5Key] as string);
                }
                else
                {
                    var pair = input.RawInput.Base64ToBytes().ToString(Encoding.UTF8).Split(' ');
                    context._User.Name = pair.First();
                    var credential = context.Server
                        .Credentials
                        .FirstOrDefault(c => c.UserName == context._User.Name);
                    if (credential == null)
                    {
                        FailAuthentication(context);
                        return;
                    }

                    var hmacmd5str = new HMACMD5(credential.Password.GetBytes())
                        .ComputeHash(context.Items[AuthCRAMMD5Key].ToString().Base64ToBytes())
                        .ToHexString();
                    context.Items.Remove(AuthCRAMMD5Key);
                    if (hmacmd5str != pair.Last())
                    {
                        FailAuthentication(context);
                    }
                    else
                    {
                        Authentication(context);
                    }
                }
            }
            catch (FormatException)
            {
                context._CurrentHandlers = _InitialHandlers;
                UnknownCommand(context, input);
            }
        }

        protected static void Authentication(SmtpServerSession context)
        {
            context._User.IsAuthenticated = true;
            context._CurrentHandlers = _InitialHandlers;
            context.WriteLine(235, "Authentication successful");
        }

        protected static void FailAuthentication(SmtpServerSession context)
        {
            context.ClearIdentity();
            context._CurrentHandlers = _InitialHandlers;
            context.WriteLine(535, " Error: authentication failed");
        }

        protected static void OnMailFrom(SmtpServerSession context, SmtpClientInputLine input)
        {
            if (input.Params.Length != 2 || input.Params.First().ToUpper() != "FROM:") UnknownCommand(context, input);
            else
            {
                context.Message.MailFrom = input.Params.Last();
            }
            context.WriteLine(250, "Ok");
        }

        protected static void OnRcptTo(SmtpServerSession context, SmtpClientInputLine input)
        {
            if (input.Params.Length != 2 || input.Params.First().ToUpper() != "TO:")
            {
                UnknownCommand(context, input);
                return;
            }
            var rcptto = input.Params.Last();
            if (context.Server.Credentials.Any() && context.User.IsAuthenticated == false)
            {
                context.WriteLine(554, "{0}: Recipient address rejected: Access denied", rcptto);
                return;
            }
            context.Message.RcptTo.Add(rcptto);
            context.WriteLine(250, "Ok");
        }

        protected static void OnQuit(SmtpServerSession context, SmtpClientInputLine input)
        {
            context.WriteLine(221, "Bye");
            context.EndSession();
        }

        protected static void UnknownCommand(SmtpServerSession context, SmtpClientInputLine input)
        {
            context.WriteLine(502, "Error: command not implemented");
        }

        protected static void OnData(SmtpServerSession context, SmtpClientInputLine input)
        {
            context._CurrentHandlers = _HeaderHandlers;
            context.WriteLine(354, "Ok");
        }

        protected static void HandleHeader(SmtpServerSession context, SmtpClientInputLine input)
        {
            if (input.RawInput == "") context._CurrentHandlers = _BodyHandlers;
            else context.Message.Headers.Add(input.RawInput);
        }

        protected static void HandleBody(SmtpServerSession context, SmtpClientInputLine input)
        {
            if (input.RawInput == ".")
            {
                context.FireReceieMessage();
                context._CurrentHandlers = _InitialHandlers;
                context.Reset();
                context.WriteLine(250, "Ok");
            }
            else context.Message.Data.Add(input.RawInput);
        }
    }
}
