/*
 * Пользователь: Igor.Evdokimov
 * Дата: 23.12.2015
 * Время: 15:55
 */
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace DwarfServer.Server
{
	/// <summary>
	/// A general logic of DwarfServer.
	/// The main idea is the next: this component is responsible for:
	/// - listening and collecting requests fro DwarfClients;
	/// - receiving specially formed responses from another layers of a DwarfServer ( as an exmaple, we can transmit a DC record )
	/// - receiving and updating of a clients list ( ON/OFF/ERROR/BUSY client states )
	/// </summary>
	public class Server {
		int port = 45000;
		WebClient web_client = new WebClient();
		HttpListener listener = new HttpListener();
		Logger.Logger logger = new Logger.Logger( "./", "DwarfServer", Encoding.Default );
		Thread listener_thread = null;
		Thread response_proc_thread = null;

		readonly ConcurrentBag<String> active_hosts = new ConcurrentBag<string>();
		readonly ConcurrentBag<HttpListenerRequest> requests = new ConcurrentBag<HttpListenerRequest>();
		readonly ConcurrentDictionary<HttpListenerResponse, Byte[]> responses =
			new ConcurrentDictionary<HttpListenerResponse, Byte[]>();
		
		/// <summary>
		/// Server statuses
		/// </summary>
		public enum ServerStatus {
			ON,
			OFF,
			BUSY,
			ERROR
		}
		
		/// <summary>
		/// A server status
		/// </summary>
		public ServerStatus Status {
			get; private set;
		}
		
		public Server()
		{
			// Setting a prefix to listen for a needed port
			listener.Prefixes.Add("http://*:"+port.ToString()+"/");
			Status = ServerStatus.OFF;
			ThreadPool.SetMaxThreads(32, 2);
			ThreadPool.SetMinThreads(2,2);
		}
		
		/// <summary>
		/// Adds  response to our Concurrent bag if
		/// a request was registered
		/// </summary>
		/// <param name="hlr"></param>
		/// <param name="response"></param>
		public void AddResponse( HttpListenerResponse hlr, Byte[] response ) {
			if (responses.ContainsKey(hlr) )
				responses[hlr] = response;
		}
		
		/// <summary>
		/// Starting our listening procedure
		/// </summary>
		private void Listening() {
			try {
				for (;;) {
					var current_context = listener.GetContext();
					if ( current_context != null ) {
						if ( current_context.Request != null  && current_context.Response != null) {
							requests.Add( current_context.Request );
							responses[current_context.Response] = null;
						}
					}
				}
			} catch ( HttpListenerException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			} catch ( InvalidOperationException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			}
		}
		
		/// <summary>
		/// Sends a response
		/// </summary>
		/// <param name="state_info"></param>
		private void SendResponse(Object state_info) {
			try {
				var req_resp = (HttpListenerResponse)state_info;
				if ( req_resp != null )
					req_resp.OutputStream.Write(responses[req_resp], 0, responses[req_resp].Length );
				req_resp.OutputStream.Close();
			} catch ( EndOfStreamException ex ) {
				logger.WriteError( ex.Message );
				//Status = ServerStatus.ERROR;
			}
		}
		
		/// <summary>
		/// Processing our responses
		/// </summary>
		private void ResponseProc() {
			try {
				HttpListenerRequest request = null;
				KeyValuePair<HttpListenerResponse, Byte[]> kvp;
				if ( responses.Keys.Count == 0 )
					return;
				while ( responses.Count > 0 ) {
					foreach ( var key in responses.Keys )
						ThreadPool.QueueUserWorkItem(new WaitCallback(SendResponse), key );
				}
			} catch ( WebException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			} catch ( InvalidOperationException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			}
		}
		
		/// <summary>
		/// Starts data processing
		/// </summary>
		public void Start() {
			try {
				if ( Status == ServerStatus.OFF ) {
					listener.Start();
					listener_thread = new Thread( Listening );
					listener_thread.Start();
					
					
					response_proc_thread = new Thread( ResponseProc );
					response_proc_thread.Start();
				}
			} catch ( HttpListenerException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			} catch ( ThreadStartException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			}
			
			Status = ServerStatus.ON;
		}
		
		/// <summary>
		/// Stops data processing
		/// </summary>
		public void Stop() {
			try {
				if ( Status != ServerStatus.OFF ) {
					response_proc_thread.Abort();
					listener_thread.Abort();
					listener.Stop();
				}
			} catch ( HttpListenerException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			} catch ( ThreadAbortException ex ) {
				logger.WriteError( ex.Message );
				Status = ServerStatus.ERROR;
			}
			
			Status = ServerStatus.OFF;
		}
		
		/// <summary>
		/// Checks availability of a host
		/// </summary>
		/// <param name="host">Host</param>
		/// <param name="delay">Delay, ms</param>
		/// <returns></returns>
		private bool IsAvailable( string host, int delay ) {
			// TODO: send request
			Thread.Sleep( delay );
			// TODO: do we have any response?
			
			return false;
		}
		
		/// <summary>
		/// Checks availability of all hosts by sending special requests
		/// </summary>
		private void CheckAvailability() {
			var active_hosts_copy = active_hosts.ToArray();
			foreach ( var host in active_hosts_copy ) {
				// TODO: send a special request and wait
				// for a response if timeout expires - delete in
				// from active hosts
				if ( !IsAvailable( host, 2000 ) ) {
					var thr = new Thread (
						() => active_hosts.TryTake( out host )
					);
					
					thr.Start();
				}
			}
		}
	}
}
