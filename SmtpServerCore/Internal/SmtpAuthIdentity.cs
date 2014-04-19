using System;
using System.Linq;
using System.Security.Principal;

namespace Toolbelt.Net.Smtp.Internal
{
    public class SmtpAuthIdentity : IIdentity
    {
        public string AuthenticationType { get { return "SMTP Authentication"; } }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }

        public SmtpAuthIdentity()
        {
            this.Name = "";
        }

        public override string ToString()
        {
            return string.Format("{{Name:\"{0}\",IsAuthenticated={1}}}", this.Name, this.IsAuthenticated);
        }
    }
}
