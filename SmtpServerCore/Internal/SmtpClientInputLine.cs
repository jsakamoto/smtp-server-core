using System;
using System.Linq;

namespace Toolbelt.Net.Smtp.Internal
{
    public class SmtpClientInputLine
    {
        public string Command { get; protected set; }

        public string[] Params { get; protected set; }

        public string RawInput { get; protected set; }

        public SmtpClientInputLine(string input)
        {
            this.RawInput = input;

            var parts = input.SplitAndTrim(' ')
                .SelectMany(_ =>
                {
                    if (_.Contains(':') == false) return new[] { _ };
                    var m = _.Split(':').Select(a => a + ":").ToArray();
                    m[m.Length - 1] = m[m.Length - 1].TrimEnd(':');
                    return m.Where(StringExtension.IsNotNullOrEmpty);
                })
                .ToArray();
            this.Command = parts.FirstOrDefault() ?? "";
            this.Params = parts.Skip(1).ToArray();
        }
    }
}
