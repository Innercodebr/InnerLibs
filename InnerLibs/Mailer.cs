﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace InnerLibs.Mail
{
    /// <summary>
    /// Wrapper de <see cref="System.Net.Mail"/> em FluentAPI com configurações de Template métodos
    /// auxiliares. Utiliza objetos do tipo <see cref="Dictionary{string, object}"/> para objetos de template
    /// </summary>
    public class FluentMailMessage : FluentMailMessage<Dictionary<string, object>>
    {
        #region Public Methods

        /// <summary>
        /// Cria um <see cref="FluentMailMessage{T}"/> com destinatários a partir de uma <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="Recipients">objeto de template de destinatários</param>
        /// <param name="EmailSelector">seletor de email</param>
        /// <param name="NameSelector">seletor de nome</param>
        /// <returns></returns>
        public static FluentMailMessage<T> CreateWithRecipients<T>(IEnumerable<T> Recipients, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null) where T : class => new FluentMailMessage<T>().AddRecipient(Recipients, EmailSelector, NameSelector);

        /// <summary>
        /// Cria um <see cref="FluentMailMessage{T}"/> com destinatários a partir de um objeto do
        /// tipo <typeparamref name="T"/>
        /// </summary>
        /// <param name="Recipient">objeto de template d edestinatário</param>
        /// <param name="EmailSelector"></param>
        /// <param name="NameSelector"></param>
        /// <returns></returns>
        public static FluentMailMessage<T> CreateWithRecipients<T>(T Recipient, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null) where T : class => CreateWithRecipients(new[] { Recipient }, EmailSelector, NameSelector);

        /// <summary>
        /// SMTP do Gmail
        /// </summary>
        /// <returns></returns>
        public static SmtpClient GmailSmtp() => new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true };

        /// <summary>
        /// SMTP da Locaweb
        /// </summary>
        /// <returns></returns>
        public static SmtpClient LocawebSmtp() => new SmtpClient("email-ssl.com.br", 465) { EnableSsl = true };

        /// <summary>
        /// SMTP do Office365
        /// </summary>
        /// <returns></returns>
        public static SmtpClient Office365Smtp() => new SmtpClient("smtp.office365.com", 587) { EnableSsl = true };

        /// <summary>
        /// SMTP do Outlook
        /// </summary>
        /// <returns></returns>
        public static SmtpClient OutlookSmtp() => new SmtpClient("smtp-mail.outlook.com", 587) { EnableSsl = true };

        /// <summary>
        /// Cria e dispara rapidamente uma <see cref="FluentMailMessage"/>
        /// </summary>
        /// <param name="Email">Email do remetende e utilizado nas credenciais de SMTP</param>
        /// <param name="Password">Senha utilizada nas credenciais de SMTP</param>
        /// <param name="Recipient">Destinatários</param>
        /// <param name="Subject">Assunto do Email</param>
        /// <param name="Message">Corpo da Mensagem</param>
        /// <returns></returns>
        public static SentStatus QuickSend(string Email, string Password, string Recipient, string Subject, string Message) => new FluentMailMessage().WithQuickConfig(Email, Password).AddRecipient(Recipient).WithSubject(Subject).WithMessage(Message).OnError((m, a, ex) => Debug.WriteLine(ex.ToFullExceptionString())).SendAndDispose();

        /// <summary>
        /// Cria e dispara rapidamente uma <see cref="FluentMailMessage"/>
        /// </summary>
        /// <param name="Email">Email do remetende e utilizado nas credenciais de SMTP</param>
        /// <param name="Password">Senha utilizada nas credenciais de SMTP</param>
        /// <param name="SmtpHost">Host SMTP</param>
        /// <param name="SmtpPort">Porta SMTP</param>
        /// <param name="UseSSL">SSL do SMTP</param>
        /// <param name="Recipient">Destinatários</param>
        /// <param name="Subject">Assunto do Email</param>
        /// <param name="Message">Corpo da Mensagem</param>
        /// <returns></returns>
        public static SentStatus QuickSend(string Email, string Password, string SmtpHost, int SmtpPort, bool UseSSL, string Recipient, string Subject, string Message) => new FluentMailMessage().WithSmtp(SmtpHost, SmtpPort, UseSSL).WithCredentials(Email, Password).AddRecipient(Recipient).WithSubject(Subject).WithMessage(Message).OnError((m, a, ex) => Debug.WriteLine(ex.ToFullExceptionString())).SendAndDispose();

        #endregion Public Methods
    }

    /// <summary>
    /// Wrapper de <see cref="System.Net.Mail"/> em FluentAPI com configurações de Template métodos
    /// auxiliares. Utiliza objetos do tipo <typeparamref name="T"/> para objetos de template
    /// </summary>
    public class FluentMailMessage<T> : MailMessage where T : class
    {
        #region Private Fields

        private List<(TemplateMailAddress<T>, SentStatus, Exception)> _status = new List<(TemplateMailAddress<T>, SentStatus, Exception)>();

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Envia os emails
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private FluentMailMessage<T> Send(string TestEmail)
        {
            if (Smtp != null)
            {
                if (TestEmail.IsNotBlank() && !TestEmail.IsEmail())
                {
                    throw new ArgumentException("TestEmail is not a valid email", nameof(TestEmail));
                }

                ResetStatus();

                foreach (FluentMailMessage<T> mailMessage in GenerateEmails())
                {
                    try
                    {
                        if (TestEmail.IsEmail())
                        {
                            mailMessage.To.Clear();
                            mailMessage.Bcc.Clear();
                            mailMessage.CC.Clear();
                            mailMessage.To.Add(TestEmail);
                        }

                        Smtp.Send(mailMessage);

                        _status.Add((mailMessage.To.First() as TemplateMailAddress<T>, SentStatus.Success, null));

                        SuccessAction?.Invoke(mailMessage.To.First() as TemplateMailAddress<T>, this);
                    }
                    catch (Exception ex)
                    {
                        _status.Add((mailMessage.To.First() as TemplateMailAddress<T>, SentStatus.Error, ex));

                        ErrorAction?.Invoke(mailMessage.To.First() as TemplateMailAddress<T>, this, ex);
                    }
                    finally
                    {
                        mailMessage.Dispose();
                    }
                }
            }
            else
            {
                throw new ArgumentException("SmtpHost is null", nameof(Smtp));
            }

            return this;
        }

        #endregion Private Methods

        #region Public Constructors

        /// <summary>
        /// Instancia uma nova <see cref="FluentMailMessage{T}"/> vazia
        /// </summary>
        public FluentMailMessage() : base()
        {
            this.IsBodyHtml = true;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Ação executada quando ocorrer erro no disparo
        /// </summary>
        public Action<TemplateMailAddress<T>, FluentMailMessage<T>, Exception> ErrorAction { get; set; }

        /// <summary>
        /// Lista contendo os endereços de email que encontraram algum erro ao serem enviados
        /// </summary>
        public IEnumerable<(TemplateMailAddress<T>, Exception)> ErrorList => SentStatusList.Where(x => x.Status.HasFlag(SentStatus.Error)).Select(x => (x.Destination, x.Error));

        /// <summary>
        /// Status geral do disparo
        /// </summary>
        public SentStatus SentStatus => SentStatusList.Select(x => x.Status).Aggregate((x, y) => x | y);

        /// <summary>
        /// Lista contendo os destinatários e o status do disparo
        /// </summary>
        public IEnumerable<(TemplateMailAddress<T> Destination, SentStatus Status, Exception Error)> SentStatusList => _status.AsEnumerable();

        /// <summary>
        /// Informações de SMTP
        /// </summary>
        public SmtpClient Smtp { get; set; } = new SmtpClient();

        /// <summary>
        /// Ação executada quando o disparo for concluido com êxito
        /// </summary>
        public Action<TemplateMailAddress<T>, FluentMailMessage<T>> SuccessAction { get; set; }

        /// <summary>
        /// Lista contendo os endereços de email que foram enviados com sucesso
        /// </summary>
        public IEnumerable<TemplateMailAddress<T>> SuccessList => SentStatusList.Where(x => x.Status.HasFlag(SentStatus.Success)).Select(x => x.Destination);

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adciona um anexo ao email
        /// </summary>
        /// <param name="Attachment"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddAttachment(params Attachment[] Attachment) => AddAttachment((Attachment ?? Array.Empty<Attachment>()).AsEnumerable());

        /// <summary>
        /// Adciona um anexo ao email
        /// </summary>
        public FluentMailMessage<T> AddAttachment(IEnumerable<Attachment> Attachment)
        {
            if (Attachment != null && Attachment.Any())
                foreach (var item in Attachment)
                    if (item != null && item.ContentStream.Length > 0)
                        this.Attachments.Add(item);
            return this;
        }

        /// <summary>
        /// Adciona um anexo ao email
        /// </summary>
        public FluentMailMessage<T> AddAttachment(Stream Attachment, string Name)
        {
            var att = Attachment.ToAttachment(Name);
            if (att != null) this.Attachments.Add(att);
            return this;
        }

        /// <summary>
        /// Adciona um anexo ao email
        /// </summary>
        public FluentMailMessage<T> AddAttachment(byte[] Attachment, string Name)
        {
            var att = Attachment.ToAttachment(Name);
            if (att != null) this.Attachments.Add(att);
            return this;
        }

        /// <summary>
        /// Adciona um anexo ao email
        /// </summary>
        public FluentMailMessage<T> AddAttachment(FileInfo Attachment)
        {
            var att = Attachment.ToAttachment();
            if (att != null) this.Attachments.Add(att);
            return this;
        }

        /// <summary>
        /// Adiciona anexos individuais a cada destinatário a partir de uma propriedade do template
        /// </summary>
        /// <param name="AttachmentSelector"></param>
        /// <returns></returns>
        /// <remarks>
        /// É necessario que o template ja tenha sido carregado previamente nesta <see cref="FluentMailMessage{T}"/>
        /// </remarks>
        public FluentMailMessage<T> AddAttachmentFromData(Expression<Func<T, IEnumerable<Attachment>>> AttachmentSelector)
        {
            Misc.AddAttachmentFromData(To.Where(x => x is TemplateMailAddress<T>).Cast<TemplateMailAddress<T>>(), AttachmentSelector);
            return this;
        }

        /// <summary>
        /// Adiciona anexos individuais a cada destinatário a partir de uma propriedade do template
        /// </summary>
        /// <param name="AttachmentSelector"></param>
        /// <returns></returns>
        /// <remarks>
        /// É necessario que o template ja tenha sido carregado previamente nesta <see cref="FluentMailMessage{T}"/>
        /// </remarks>
        public FluentMailMessage<T> AddAttachmentFromData(Expression<Func<T, Attachment>> AttachmentSelector)
        {
            Misc.AddAttachmentFromData(To.Where(x => x is TemplateMailAddress<T>).Cast<TemplateMailAddress<T>>(), AttachmentSelector);
            return this;
        }

        /// <summary>
        /// Adiciona endereços ao BCC do email
        /// </summary>
        /// <param name="Emails"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddBcc(params string[] Emails)
        {
            foreach (var email in (Emails ?? Array.Empty<string>()).SelectMany(x => x.ExtractEmails()).ToArray())
                Bcc.Add(new TemplateMailAddress(email));
            return this;
        }

        /// <summary>
        /// Adiciona endereços ao BCC do email
        /// </summary>
        /// <param name="Emails"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddBcc(params MailAddress[] Emails)
        {
            foreach (var email in Emails ?? Array.Empty<MailAddress>())
                Bcc.Add(email);
            return this;
        }

        /// <summary>
        /// Adiciona endereços ao CC do email
        /// </summary>
        /// <param name="Emails"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddCc(params string[] Emails)
        {
            foreach (var email in (Emails ?? Array.Empty<string>()).SelectMany(x => x.ExtractEmails()).ToArray())
                CC.Add(new TemplateMailAddress(email));
            return this;
        }

        /// <summary>
        /// Adiciona endereços ao CC do email
        /// </summary>
        /// <param name="Emails"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddCc(params MailAddress[] Emails)
        {
            foreach (var email in Emails ?? Array.Empty<MailAddress>())
                CC.Add(email);
            return this;
        }

        /// <summary>
        /// Adiciona um destinatário a lista de destinatários deta mensagem
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> AddRecipient(string Email, T TemplateData)
        {
            To.Add(new TemplateMailAddress<T>(Email, TemplateData));
            return this;
        }


        /// <summary>
        /// Adiciona um destinatário a lista de destinatários deta mensagem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Email"></param>
        /// <param name="DisplayName"></param>
        /// <param name="TemplateData"></param>
        /// <returns></returns>
        public FluentMailMessage<T> AddRecipient(string Email, string DisplayName, T TemplateData)
        {
            To.Add(new TemplateMailAddress<T>(Email, DisplayName, TemplateData));
            return this;
        }

        /// <summary>
        /// Adiciona um destinatário a lista de destinatários deta mensagem
        /// </summary>
        public FluentMailMessage<T> AddRecipient(T Data, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null)
        {
            To.Add(TemplateMailAddress<T>.FromObject(Data, EmailSelector, NameSelector));
            return this;
        }

        /// <summary>
        /// Adiciona um destinatário a lista de destinatários deta mensagem
        /// </summary>
        public FluentMailMessage<T> AddRecipient(IEnumerable<T> Data, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null)
        {
            foreach (var m in TemplateMailAddress<T>.FromList(Data, EmailSelector, NameSelector)) To.Add(m);
            return this;
        }

        /// <summary>
        /// Adiciona um destinatário a lista de destinatários desta mensagem
        /// </summary>
        public FluentMailMessage<T> AddRecipient(params string[] Emails)
        {
            foreach (var email in (Emails ?? Array.Empty<string>()).SelectMany(x => x.ExtractEmails()).ToArray()) To.Add(new TemplateMailAddress<T>(email));
            return this;
        }

        public FluentMailMessage<T> AddRecipient(params TemplateMailAddress<T>[] Emails)
        {
            foreach (var email in Emails ?? Array.Empty<TemplateMailAddress<T>>()) To.Add(email);
            return this;
        }

        /// <summary>
        /// Configura o remetente da mensagem
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> FromEmail(string Email)
        {
            From = new MailAddress(Email);
            return this;
        }

        /// <summary>
        /// Configura o remetente da mensagem
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> FromEmail(string Email, string DisplayName)
        {
            From = new MailAddress(Email, DisplayName);
            return this;
        }

        /// <summary>
        /// Configura o remetente da mensagem
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> FromEmail(MailAddress Email)
        {
            From = Email;
            return this;
        }

        /// <summary>
        /// Retorna uma lista de <see cref="FluentMailMessage{T}"/> com todas as mensagens de emails
        /// geradas por essa <see cref="FluentMailMessage{T}"/>
        /// </summary>
        public IEnumerable<FluentMailMessage<T>> GenerateEmails()
        {
            if (To != null)
                foreach (var item in To)
                {
                    if (item != null)
                    {
                        var msgIndiv = new FluentMailMessage<T>();

                        string msg = Body.IfBlank(Text.Empty);
                        string subj = Subject.IfBlank(Text.Empty);

                        if (item is TemplateMailAddress<T> templateMail)
                        {
                            var data = templateMail.TemplateData;
                            if (data != null)
                            {
                                msg = msg.Inject(data);
                                subj = subj.Inject(data);

                                foreach (var att in templateMail.Attachments ?? new List<Attachment>())
                                    msgIndiv.Attachments.Add(att);
                            }
                        }

                        msgIndiv.To.Add(item);
                        msgIndiv.Body = msg;
                        msgIndiv.Subject = subj;
                        msgIndiv.From = this.From;
                        msgIndiv.Sender = this.Sender;
                        msgIndiv.IsBodyHtml = this.IsBodyHtml;
                        msgIndiv.BodyEncoding = this.BodyEncoding;
                        msgIndiv.BodyTransferEncoding = this.BodyTransferEncoding;
                        msgIndiv.DeliveryNotificationOptions = this.DeliveryNotificationOptions;
                        msgIndiv.SubjectEncoding = this.SubjectEncoding;
                        msgIndiv.HeadersEncoding = this.HeadersEncoding;
                        msgIndiv.Priority = this.Priority;
                        msgIndiv.Headers.Add(this.Headers);

                        foreach (var email in this.CC)
                        {
                            msgIndiv.CC.Add(email);
                        }

                        foreach (var email in this.ReplyToList)
                        {
                            msgIndiv.ReplyToList.Add(email);
                        }

                        foreach (var email in this.CC)
                        {
                            msgIndiv.CC.Add(email);
                        }

                        foreach (var email in this.Bcc)
                        {
                            msgIndiv.Bcc.Add(email);
                        }

                        foreach (var att in this.Attachments)
                        {
                            msgIndiv.Attachments.Add(att);
                        }

                        foreach (var alt in this.AlternateViews)
                        {
                            msgIndiv.AlternateViews.Add(alt);
                        }

                        yield return msgIndiv;
                    }
                }
        }

        /// <summary>
        /// Função executada quando houver um disparo com erro
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public FluentMailMessage<T> OnError(Action<TemplateMailAddress<T>, FluentMailMessage<T>, Exception> Action)
        {
            ErrorAction = Action;
            return this;
        }

        /// <summary>
        /// Função executada quando houver um disparo com sucesso
        /// </summary>
        /// <param name="Action"></param>
        /// <returns></returns>
        public FluentMailMessage<T> OnSuccess(Action<TemplateMailAddress<T>, FluentMailMessage<T>> Action)
        {
            SuccessAction = Action;
            return this;
        }

        /// <summary>
        /// Reinicia o status de disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> ResetStatus()
        {
            _status = _status ?? new List<(TemplateMailAddress<T>, SentStatus, Exception)>();
            _status.Clear();
            return this;
        }

        public FluentMailMessage<T> Send() => Send(null);

        /// <summary>
        /// Envia os emails
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SentStatus SendAndDispose()
        {
            Send();
            var _ss = SentStatus;
            this.Dispose();
            return _ss;
        }

        /// <summary>
        /// Envia os emails para um destinatário de teste.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public FluentMailMessage<T> SendTest(string Email) => Email.IsEmail() ? Send(Email) : throw new ArgumentException($"Invalid email: {Email}", nameof(Email));

        /// <summary>
        /// Retorna o corpo da mensagem
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Body;

        /// <summary>
        /// Configura o SMTP do Gmail para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> UseGmailSmtp()
        {
            Smtp = FluentMailMessage.GmailSmtp();
            return this;
        }

        /// <summary>
        /// Configura o SMTP da Locaweb para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> UseLocawebSmtp()
        {
            Smtp = FluentMailMessage.LocawebSmtp();
            return this;
        }

        /// <summary>
        /// Configura o SMTP do Office365 para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> UseOffice365Smtp()
        {
            Smtp = FluentMailMessage.Office365Smtp();
            return this;
        }

        /// <summary>
        /// Configura o SMTP do Outlook para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> UseOutlookSmtp()
        {
            Smtp = FluentMailMessage.OutlookSmtp();
            return this;
        }

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        /// <param name="TemplateOrFilePathOrUrl"></param>
        /// <returns></returns>
        public FluentMailMessage<T> UseTemplate(string TemplateOrFilePathOrUrl) => UseTemplate(TemplateOrFilePathOrUrl, Text.Empty);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        /// <param name="TemplateOrFilePathOrUrl"></param>
        /// <returns></returns>
        public FluentMailMessage<T> UseTemplate(string TemplateOrFilePathOrUrl, string MessageTemplate) => UseTemplate(TemplateOrFilePathOrUrl, new { BodyText = MessageTemplate });

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(HtmlTag Template, string MessageTemplate) => UseTemplate(Template?.OuterHtml ?? Text.Empty, new { BodyText = MessageTemplate }).With(x => x.IsBodyHtml = true);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(HtmlTag Template) => UseTemplate(Template?.OuterHtml ?? Text.Empty, Text.Empty).With(x => x.IsBodyHtml = true);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate<TMessage>(HtmlTag Template, TMessage MessageTemplate) => UseTemplate(Template?.OuterHtml ?? Text.Empty, MessageTemplate).With(x => x.IsBodyHtml = true);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(FileInfo Template) => UseTemplate(Template?.FullName ?? Text.Empty, Text.Empty);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(FileInfo Template, string MessageTemplate) => UseTemplate(Template?.FullName, MessageTemplate);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate<TMessage>(FileInfo Template, TMessage MessageTemplate) => UseTemplate(Template?.FullName ?? Text.Empty, MessageTemplate);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(DirectoryInfo TemplateDirectory, string TemplateFileName) => UseTemplate(Path.Combine(TemplateDirectory?.FullName ?? Text.Empty, TemplateFileName ?? Text.Empty), Text.Empty);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate(DirectoryInfo TemplateDirectory, string TemplateFileName, string MessageTemplate) => UseTemplate(Path.Combine(TemplateDirectory?.FullName ?? Text.Empty, TemplateFileName ?? Text.Empty), MessageTemplate);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        public FluentMailMessage<T> UseTemplate<TMessage>(DirectoryInfo TemplateDirectory, string TemplateFileName, TMessage MessageTemplate) => UseTemplate(Path.Combine(TemplateDirectory?.FullName ?? Text.Empty, TemplateFileName ?? Text.Empty), MessageTemplate);

        /// <summary>
        /// Utiliza um template para o corpo da mensagem
        /// </summary>
        /// <param name="TemplateOrFilePathOrUrl"></param>
        /// <returns></returns>
        public FluentMailMessage<T> UseTemplate<TMessage>(string TemplateOrFilePathOrUrl, TMessage MessageTemplate)
        {
            if (TemplateOrFilePathOrUrl.IsFilePath())
            {
                if (File.Exists(TemplateOrFilePathOrUrl)) TemplateOrFilePathOrUrl = File.ReadAllText(TemplateOrFilePathOrUrl);
            }

            if (TemplateOrFilePathOrUrl.IsURL())
            {
                TemplateOrFilePathOrUrl = WebExtensions.DownloadString(TemplateOrFilePathOrUrl);
            }

            if (MessageTemplate != null)
            {
                TemplateOrFilePathOrUrl = TemplateOrFilePathOrUrl.Inject(MessageTemplate);
            }

            return WithMessage(TemplateOrFilePathOrUrl);
        }

        /// <summary>
        /// Configura as credenciais do SMTP
        /// </summary>
        /// <param name="Credentials"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Ocorre quando <paramref name="Credentials"/> é nulo
        /// </exception>
        /// <exception cref="Exception"></exception>
        public FluentMailMessage<T> WithCredentials(NetworkCredential Credentials)
        {
            if (Smtp == null) throw new ArgumentException("SMTP is null, Define SMTP before defining credentials", nameof(Smtp));
            Smtp.Credentials = Credentials ?? throw new ArgumentNullException("Credentials is null", nameof(Credentials));
            if (From == null || From.Address.IsBlank())
            {
                From = new MailAddress(Credentials.UserName);
            }

            return this;
        }

        /// <summary>
        /// Configura as credenciais do SMTP
        /// </summary>
        public FluentMailMessage<T> WithCredentials(string Login, string Password)
        {
            return Smtp == null || Smtp.Host.IsBlank()
                ? WithQuickConfig(Login, Password)
                : WithCredentials(new NetworkCredential(Login, Password));
        }

        /// <summary>
        /// Configura o email com prioridade alta
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithHighPriority() => WithPriority(MailPriority.High);

        /// <summary>
        /// Configura o email com prioridade baixa
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithLowPriority() => WithPriority(MailPriority.Low);

        /// <summary>
        /// Configura a mensagem a ser enviada
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithMessage(string Text)
        {
            Body = Text;
            return this;
        }

        /// <summary>
        /// Configura a mensagem a ser enviada
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithMessage(HtmlTag Text)
        {
            IsBodyHtml = true;
            Body = Text?.ToString() ?? "";
            return this;
        }

        /// <summary>
        /// Configura o email com prioridade normal
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithNormalPriority() => WithPriority(MailPriority.Normal);

        /// <summary>
        /// Configura a prioridade do email
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public FluentMailMessage<T> WithPriority(MailPriority priority)
        {
            Priority = priority;
            return this;
        }

        /// <summary>
        /// Configura o SMTP para este disparo a partir de um email e senha
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithQuickConfig(string Email, string Password)
        {
            var domain = Email.GetDomain();

            switch (domain)
            {
                case "outlook.com":
                case "outlook.com.br":
                case "hotmail.com":
                    UseOutlookSmtp();
                    break;

                case "office365.com":
                    UseOffice365Smtp();
                    break;

                case "gmail.com":
                    UseGmailSmtp();
                    break;

                default:
                    WithSmtp($"smtp.{domain}", 587, false);
                    break;
            }

            Debug.WriteLine($"Using {Smtp.Host}");

            WithCredentials(Email, Password);

            return this;
        }

        /// <summary>
        /// Configura o SMTP para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithSmtp(SmtpClient Client)
        {
            Smtp = Client;
            return this;
        }

        /// <summary>
        /// Configura o SMTP para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithSmtp(string Host) => WithSmtp(new SmtpClient(Host));

        /// <summary>
        /// Configura o SMTP para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithSmtp(string Host, int Port) => WithSmtp(new SmtpClient(Host, Port));

        /// <summary>
        /// Configura o SMTP para este disparo
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithSmtp(string Host, int Port, bool UseSSL) => WithSmtp(new SmtpClient(Host, Port) { EnableSsl = UseSSL });

        /// <summary>
        /// Configura o assunto do email
        /// </summary>
        /// <returns></returns>
        public FluentMailMessage<T> WithSubject(string Text)
        {
            Subject = Text;
            return this;
        }

        #endregion Public Methods
    }

    public class TemplateMailAddress : TemplateMailAddress<Dictionary<string, object>>
    {
        #region Public Constructors

        public TemplateMailAddress(string address, Dictionary<string, object> TemplateData = null) : base(address, TemplateData)
        {
        }

        public TemplateMailAddress(string address, string displayName, Dictionary<string, object> TemplateData = null) : base(address, displayName, TemplateData)
        {
        }

        public TemplateMailAddress(string address, string displayName, Encoding displayNameEncoding, Dictionary<string, object> TemplateData = null) : base(address, displayName, displayNameEncoding, TemplateData)
        {
        }

        #endregion Public Constructors
    }

    /// <summary>
    /// Um destinatário de email contendo informações que serão aplicadas em um template
    /// </summary>
    public class TemplateMailAddress<T> : MailAddress where T : class
    {
        #region Public Constructors

        public TemplateMailAddress(string address, T TemplateData = null) : base(address)
        {
            this.TemplateData = TemplateData;
        }

        public TemplateMailAddress(string address, string displayName, T TemplateData = null) : base(address, displayName)
        {
            this.TemplateData = TemplateData;
        }

        public TemplateMailAddress(string address, string displayName, Encoding displayNameEncoding, T TemplateData = null) : base(address, displayName, displayNameEncoding)
        {
            this.TemplateData = TemplateData;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Lista de anexos exclusivos deste destinatário
        /// </summary>
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();

        /// <summary>
        /// Objeto com as informações deste destinatário que serão aplicados ao template
        /// </summary>
        public virtual T TemplateData { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Cria uma <see cref="IEnumerable{TemplateMailAddress{T}}"/> a partir de uma lista de
        /// objetos de template
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="EmailSelector"></param>
        /// <param name="NameSelector"></param>
        /// <returns></returns>
        public static IEnumerable<TemplateMailAddress<T>> FromList(IEnumerable<T> Data, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null) => (Data ?? Array.Empty<T>()).AsEnumerable().Select(x => FromObject(x, EmailSelector, NameSelector)).Where(x => x != null);

        /// <summary>
        /// Cria um <see cref="TemplateMailAddress{T}"/> a partir de um objeto de template
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="EmailSelector"></param>
        /// <param name="NameSelector"></param>
        /// <returns></returns>
        public static TemplateMailAddress<T> FromObject(T Data, Expression<Func<T, string>> EmailSelector, Expression<Func<T, string>> NameSelector = null)
        {
            if (Data != null)
            {
                var name = Text.Empty;
                var email = Text.Empty;

                if (EmailSelector != null)
                {
                    email = EmailSelector.Compile().Invoke(Data);
                }

                if (email.IsEmail() == false) return null;

                if (NameSelector != null)
                {
                    name = NameSelector.Compile().Invoke(Data);
                }

                if (name.IsBlank())
                {
                    return new TemplateMailAddress<T>(email, Data);
                }
                else
                {
                    return new TemplateMailAddress<T>(email, name, Data);
                }
            }
            return null;
        }

        /// <inheritdoc cref="AddAttachment(Attachment)"/>
        public TemplateMailAddress<T> AddAttachment(string fileName) => AddAttachment(new Attachment(fileName));

        /// <inheritdoc cref="AddAttachment(Attachment)"/>
        public TemplateMailAddress<T> AddAttachment(FileInfo file) => AddAttachment(file?.FullName);

        /// <summary>
        /// Adiciona um anexo a este email
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public TemplateMailAddress<T> AddAttachment(Attachment attachment)
        {
            Attachments = Attachments ?? new List<Attachment>();
            Attachments.Add(attachment);
            return this;
        }

        #endregion Public Methods
    }

    [Flags]
    public enum SentStatus
    {
        None = 0,
        Success = 1,
        Error = 2,
        PartialSuccess = Error + Success,
    }
}