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
	/// A log element for sending emails with logs with some period of time
	/// </summary>
	public class MailLogElement : ILogElement, IDisposable
	{
		/// <summary>
		/// A sending time interval, sec
		/// </summary>
		public int Interval {
			get;
			private set;
		}
		
		private List<string> contents = new List<string>();
		private Timer log_send_timer = null;
		private bool is_loaded = false;
		private List<string> recipients = new List<string>();
		private object sync_object = new object();
		
		public string UserName{ get; private set; }
		public string UserPwd{ get; private set; }
		public string Server{ get; private set; }
		public string SenderAddress{ get; private set; }
		public Encoding MailEncoding{ get; private set; }
		
		public MailLogElement()
		{
		}
		
		private void SendLog( object sender, ElapsedEventArgs e ) {
			if ( contents.Count > 0 ) {
				var text = CommonFacilities.Common.ListToString( contents, @"\r\n" );
				
				Sendmail.SendText(
					StaticResourceManager.GetStringResource("MLE_SUBJECT_TITLE") + DateTime.Now.ToString(),
					text,
					recipients,
					UserName,
					UserPwd,
					Server,
					SenderAddress,
					MailEncoding );
				
				contents.Clear();
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
				
				log_send_timer = new Timer((double)(Interval*1000));
				log_send_timer.Elapsed += SendLog;
				log_send_timer.Start();
			} catch ( Exception ex ) {
				// TODO:
			}
			is_loaded = true;
		}
		
		public void Output( string input, string msg_type ) {
			if ( !is_loaded )
				throw new MailLogElementException( " Mail log Element wasn't initialised! " );
			lock ( sync_object ) {
				contents.Add( input );
			}
		}
		
		public void Output( string input, string msg_type, params object[] pars ) {
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
