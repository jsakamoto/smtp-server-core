using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbelt.Net.Smtp
{
    public class ReceiveMessageEventArgs : EventArgs
    {
        public SmtpMessage Message { get; protected set; }

        public ReceiveMessageEventArgs(SmtpMessage message)
        {
            this.Message = message;
        }
    }
}
