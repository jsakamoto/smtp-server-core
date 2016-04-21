using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbelt.Net.Smtp.Test
{
    [TestClass]
    public class SmtpServerCoreTest
    {
        private SmtpServerCore _Server;

        private List<SmtpMessage> _Messages;

        [TestInitialize]
        public void OnTestInitialize()
        {
            _Server = new SmtpServerCore();
            _Messages = new List<SmtpMessage>();
            _Server.ReceiveMessage += _Server_ReceiveMessage;
            _Server.Start();
        }

        private void _Server_ReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            _Messages.Add(e.Message);
        }

        [TestCleanup]
        public void OnTestCleanup()
        {
            _Messages.Clear();
            _Server.Dispose();
        }

        [TestMethod]
        public void SendMail_Test()
        {
            var attachment1FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"azure_websites_b32.png");

            using (var smtpClient = new SmtpClient { Timeout = int.MaxValue })
            {

                var iso2022jp = Encoding.GetEncoding("iso-2022-jp");

                var msg = new MailMessage
                {
                    From = new MailAddress("anderson@example.com", "Mr.アンダーソン"),
                    BodyEncoding = iso2022jp,
                    SubjectEncoding = iso2022jp,
                    HeadersEncoding = iso2022jp
                };
                msg.To.Add(new MailAddress("oracle@example.com", "Mrs.オラクル"));
                msg.ReplyToList.Add(new MailAddress("trinity@example.com", "Ms.トリニティ"));
                msg.ReplyToList.Add("モーフィアス <morphias@example.com>");
                msg.Bcc.Add("ネオ <neo@example.com>");
                msg.Subject = "たとえば、一致する表紙、ヘッダー、サイドバーを追加できます。";
                msg.Body = "日本語";

                msg.Attachments.Add(Attachment.CreateAttachmentFromString("こんにちは世界", "日本語.txt"));
                msg.Attachments.Add(new Attachment(File.OpenRead(attachment1FilePath), Path.GetFileName(attachment1FilePath)));

                smtpClient.Send(msg);
            }

            // Assert received messages.

            _Messages.Count.Is(1);
            var msg1 = _Messages.First();

            // Assert mail addresses.

            msg1.From.DisplayName.Is("Mr.アンダーソン");
            msg1.From.Address.Is("anderson@example.com");

            msg1.ReplyTo.Length.Is(2);
            var replyto1 = msg1.ReplyTo.First();
            replyto1.DisplayName.Is("Ms.トリニティ");
            replyto1.Address.Is("trinity@example.com");
            var replyto2 = msg1.ReplyTo.Last();
            //replyto2.DisplayName.Is("モーフィアス");
            replyto2.Address.Is("morphias@example.com");

            msg1.To.Length.Is(1);
            var to1 = msg1.To.First();
            to1.DisplayName.Is("Mrs.オラクル");
            to1.Address.Is("oracle@example.com");

            msg1.CC.Length.Is(0);

            msg1.RcptTo.Count.Is(2);
            msg1.RcptTo.OrderBy(_ => _).Is(
                "<neo@example.com>",
                "<oracle@example.com>");

            // Assert subject and body.

            msg1.Subject.Is("たとえば、一致する表紙、ヘッダー、サイドバーを追加できます。");
            msg1.Body.Is("日本語");

            // Assert attachments.

            msg1.Attachments.Length.Is(2);

            var attachment1 = msg1.Attachments.First();
            attachment1.Name.Is("日本語.txt");
            attachment1.ContentBytes
                .Is(Encoding.UTF8.GetBytes("こんにちは世界"));

            var attachment2 = msg1.Attachments.Last();
            attachment2.Name.Is(Path.GetFileName(attachment1FilePath));
            attachment2.ContentBytes
                .Is(File.ReadAllBytes(attachment1FilePath));
        }
    }
}
