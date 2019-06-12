using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TempMail.API.Extensions;

namespace TempMail.API
{
    public class Mail : MimeMessage
    {
        public new InternetAddressList From { get; private set; }
        public new InternetAddressList ResentFrom { get; private set; }
        public new InternetAddressList ReplyTo { get; private set; }
        public new InternetAddressList ResentReplyTo { get; private set; }
        public new InternetAddressList To { get; private set; }
        public new InternetAddressList ResentTo { get; private set; }
        public new InternetAddressList Cc { get; private set; }
        public new InternetAddressList ResentCc { get; private set; }
        public new InternetAddressList Bcc { get; private set; }
        public new InternetAddressList ResentBcc { get; private set; }
        public new string TextBody { get; private set; }
        public new string HtmlBody { get; private set; }
        public new HeaderList Headers { get; private set; }
        public new IEnumerable<MimeEntity> BodyParts { get; private set; }
        public new IEnumerable<MimePart> Attachments { get; private set; }
        public string Id { get; set; }
        public string Link { get; set; }
        public string StrSender { get; set; }


        public static Mail FromId(Client session, string id)
        {
            var sourceUrl = $"https://temp-mail.org/en/source/{id}";

            var raw_mail = "";

            try
            {
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.AllowAutoRedirect = true;

                using (HttpClient client = new HttpClient(httpClientHandler))
                {
                      client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

                        raw_mail = client.GetString(sourceUrl);
                }
              
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return GetMailFromRaw(raw_mail, id);
        }

        public static Mail FromLink(Client session, string link)
        {
            var id = ExtractId(link);

            var sourceUrl = $"https://temp-mail.org/en/source/{id}";

            var raw_mail = session.HttpClient.GetString(sourceUrl);

            return GetMailFromRaw(raw_mail, id);
        }

        private static Mail GetMailFromRaw(string raw_mail, string id)
        {
            var mail = Parse(raw_mail);
            mail.Id = id;
            
            return mail;
        }


        public static Mail Parse(string raw_mail)
        {
            var message = Load(GenerateStreamFromString(raw_mail));
            
            return ConvertMessageToMail(message);
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        private static Mail ConvertMessageToMail(MimeMessage message)
        {
            var mail = new Mail
            {
                Attachments = message.Attachments.Cast<MimePart>(),
                Bcc = message.Bcc,
                Body = message.Body,
                BodyParts = message.BodyParts,
                Cc = message.Cc,
                Date = message.Date,
                From = message.From,
                Headers = message.Headers,
                HtmlBody = message.HtmlBody,
                Importance = message.Importance,
                InReplyTo = message.InReplyTo,
                MessageId = message.MessageId,
                MimeVersion = message.MimeVersion,
                Priority = message.Priority,
                //References = message.References,
                ReplyTo = message.ReplyTo,
                ResentBcc = message.ResentBcc,
                ResentCc = message.ResentCc,
                ResentDate = message.ResentDate,
                ResentFrom = message.ResentFrom,
                //ResentMessageId = message.ResentMessageId,
                ResentReplyTo = message.ResentReplyTo,
                ResentSender = message.ResentSender,
                ResentTo = message.ResentTo,
                Sender = message.Sender,
                Subject = message.Subject,
                TextBody = message.TextBody,
                To = message.To,
                XPriority = message.XPriority
            };
            
            if (message.ResentMessageId != null)
                mail.ResentMessageId = message.ResentMessageId;

            mail.References.AddRange(message.References);
            
            return mail;
            //return (Mail)message.CastTo(typeof(Mail));
        }
        
        public static string ExtractId(string link)
        {
            return Regex.Match(link, @"https://temp-mail.org/en/.*?/(?<id>.*)").Groups["id"].Value;
        }

        public void SaveAttachment(MimePart attachment, string directory = "", string altFileName = null)
        {
            var fileName = attachment.FileName ?? altFileName ?? $"file{ new Random().Next(10000) }";

            using (var stream = File.Create(Path.Combine(directory, fileName)))
            {
                attachment.Content.DecodeTo(stream);
            }
        }

    }

}
