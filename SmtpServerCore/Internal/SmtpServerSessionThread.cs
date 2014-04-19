using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Toolbelt.Net.Smtp.Internal
{
    public class SmtpServerSessionThread
    {
        public SmtpServerSession Session { get; protected set; }

        public Thread Thread { get; protected set; }

        public SmtpServerSessionThread(SmtpServerSession session, Thread thread)
        {
            this.Session = session;
            this.Thread = thread;
        }
    }
}
