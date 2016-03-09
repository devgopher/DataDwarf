/*
 * User: igor.evdokimov
 * Date: 24.02.2016
 * Time: 11:45
 */
using System;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using Utils;
using Global;

namespace Logger
{
	/// <summary>
	/// A log element for sending emails with logs regularly.
	/// </summary>
	public class MailLogElement : LogElement, IDisposable
	{
		private List<string> contents_buffer = new List<string>();
		private List<string> recipients = new List<string>();
		private Timer log_send_timer = null;
		private bool is_loaded = false;
		
		public string UserName{ get; private set; }
		public string UserPwd{ get; private set; }
		public string Server{ get; private set; }
		public string SenderAddress{ get; private set; }
		public Encoding MailEncoding{ get; private set; }
		
		/// <summary>
		/// A sending time interval, sec
		/// </summary>
		public int Interval {
			get {
				if ( log_send_timer != null )
					return (int)(log_send_timer.Interval/1000);
				return -1;
			}
		}
		
		private void SendLog( object sender, ElapsedEventArgs e ) {
			try {
				lock ( sync_object ) {
					if ( contents_buffer.Count > 0 ) {
						var text = CommonFacilities.Common.ListToString( contents_buffer, @"" );
						
						Sendmail.SendText(
							StaticResourceManager.GetStringResource("MLE_SUBJECT_TITLE") + " " + DateTime.Now.ToString(),
							text,
							recipients,
							UserName,
							UserPwd,
							Server,
							SenderAddress,
							MailEncoding );
						
						contents_buffer.Clear();
					}
				}
			} catch ( Exception ex ) {
				is_loaded = false;
				throw new MailLogElementException( "Error while sending a email: "+ex.Message, ex );
			}
		}
		
		public void Load ( string recipient,
		                  string user_name,
		                  string user_pwd,
		                  string server,
		                  string sender_address,
		                  int interval,
		                  Encoding encoding ) {
			if ( is_loaded )
				return;
			try {
				recipients.Add(recipient);
				UserName = user_name;
				UserPwd = user_pwd;
				Server = server;
				MailEncoding = encoding;
				SenderAddress = sender_address;
				
				log_send_timer = new Timer((double)(interval*1000));
				log_send_timer.Elapsed += SendLog;
				log_send_timer.Start();
			} catch ( Exception ex ) {
				throw new MailLogElementException( "Mail Log Element - initialization failed: "+ex.Message, ex );
			}
			is_loaded = true;
		}
		
		public override void Output( string input, string msg_type ) {
			if ( !is_loaded )
				throw new MailLogElementException( "Mail Log Element wasn't initialized! " );
			lock ( sync_object ) {
				contents_buffer.Add( String.Format( "{0}: {1}: {2}",
				                            DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss"),
				                            msg_type,
				                            input) );
			}
		}
		
		public override void Output( string input, string msg_type, params object[] pars ) {
			Output( input, msg_type );
		}
		
		#region IDisposable
		public void Dispose() {
			log_send_timer.Stop();
			log_send_timer.Dispose();
		}
		#endregion
	}
}
