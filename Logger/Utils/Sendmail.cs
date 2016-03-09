/*
 * Создано в SharpDevelop.
 * Пользователь: Igor.Evdokimov
 * Дата: 03.03.2015
 * Время: 12:46
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace Utils
{
	public class UtilsException : Exception {
		public UtilsException( string message ) : base(message) {
			
		}
		public UtilsException( string message, Exception inner_exception ) : base(message, inner_exception) {
			
		}
	}
	
	/// <summary>
	/// Класс для отсылки почты
	/// </summary>
	public static class Sendmail
	{
		public static void SendText(string subject,
		                            string text,
		                            string recipient,
		                            string user_name,
		                            string user_pwd,
		                            string server,
		                            string sender_address,
		                            System.Text.Encoding encoding ) {
			var rcps = new List<string>();
			rcps.Add(recipient);
			SendText( subject, text, rcps, user_name, user_pwd, server, sender_address, encoding );
		}
		
		public static void SendText(string subject,
		                            string text, List<string> recipients,
		                            string user_name,
		                            string user_pwd,
		                            string server,
		                            string sender_address,
		                            System.Text.Encoding encoding ) {
			try {
				var message = new MailMessage();
				message.Subject = subject;
				message.BodyEncoding = encoding;
				message.HeadersEncoding = encoding;
				message.SubjectEncoding = encoding;
				message.Body = text;
				message.From = new MailAddress(sender_address);
				recipients.ForEach(( s ) => { message.To.Add(s); });
				var client = new SmtpClient(server);
				client.Credentials = new NetworkCredential(user_name, user_pwd);

				client.Send(message);				
			} catch ( Exception ex ) {
				throw new UtilsException(ex.Message, ex);
			}
		}
	}
}
