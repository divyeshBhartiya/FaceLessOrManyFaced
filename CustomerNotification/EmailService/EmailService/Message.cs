using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public List<byte[]> Attachments { get; set; }
        public Message(IEnumerable<string> to, string subject, string content, List<byte[]> attachments)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(email => new MailboxAddress(email[email.IndexOf('@')..], email)));

            Subject = subject;
            Content = content;
            Attachments = attachments;
        }
    }
}
