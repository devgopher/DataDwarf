/*
 * Пользователь: Igor.Evdokimov
 * Дата: 23.12.2015
 * Время: 15:55
 * Based on: https://msdn.microsoft.com/ru-ru/library/dd335942.aspx#Section4
 */
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;
using System;

namespace DwarfServer.Server
{
	public class ConnectionInfo
	{
		public Socket socket;
		public Thread thread;

		public void StartProcessing()
		{
			if (socket == null || thread == null)
				return;
			thread.Start(this);
		}

		public void StopProcessing()
		{
			if (socket == null || thread == null)
				return;
			if (thread.ThreadState == ThreadState.WaitSleepJoin)
				thread.Interrupt();
			socket.Close();
		}
	}

	/// <summary>
	/// A general logic of DwarfServer.
	/// The main idea is the next: this component is responsible for:
	/// - listening and collecting requests fro DwarfClients;
	/// - receiving specially formed responses from another layers of a DwarfServer ( as an exmaple, we can transmit a DC record )
	/// - receiving and updating of a clients list ( ON/OFF/ERROR/BUSY client states )
	/// </summary>
	public class Server
	{
		Socket server_socket;
		const int server_port = 45000;
		const int max_connections = 256;
		const int buffer_capacity = 32768;
		Logger.Logger logger = Logger.Logger.GetInstance();

		readonly ConcurrentBag<ConnectionInfo> connections =
			new ConcurrentBag<ConnectionInfo>();

		/// <summary>
		/// Server statuses
		/// </summary>
		public enum ServerStatus
		{
			ON,
			OFF,
			BUSY,
			ERROR
		}

		/// <summary>
		/// A server status
		/// </summary>
		public ServerStatus Status
		{
			get; private set;
		}

		public Server()
		{
			Status = ServerStatus.OFF;
			ThreadPool.SetMaxThreads( 32 * Environment.ProcessorCount, Environment.ProcessorCount * 2 );
			ThreadPool.SetMinThreads( 2 * Environment.ProcessorCount,  Environment.ProcessorCount * 2 );
		}

		public void SetupSocket()
		{
			try
			{
				logger.WriteEntry("Setting up a new socket...");
				IPHostEntry server_info = Dns.GetHostEntry(Dns.GetHostName());
				IPAddress[] al = server_info.AddressList;
				IPEndPoint server_ep = new IPEndPoint(al[1], server_port);
				logger.WriteEntry("Socket address: "+ String.Format("{0}", al[1])+":"+server_port.ToString());
				server_socket = new Socket(server_ep.AddressFamily,
				                           SocketType.Stream,
				                           ProtocolType.IP);
				server_socket.Bind( server_ep );
				logger.WriteEntry("Starting a listener...");
				server_socket.Listen( max_connections );
			}
			catch ( SocketException se )
			{
				logger.WriteError("A socket exception occured during socket setup : " + se.Message);
			}
		}

		private void CleanConnectionsBuffer()
		{
			int conn_cnt = connections.Count;
			var connections_cp = connections.ToArray();
			foreach ( var usr_conn in connections_cp )
			{
				var usr_conn_cp = usr_conn;
				if (!usr_conn.socket.Connected)
					connections.TryTake( out usr_conn_cp );
			}
		}

		public void AcceptConnections()
		{
			try
			{
				logger.WriteEntry("Accepting connections...");
				for (;;)
				{
					if (connections.Count >= max_connections)
					{
						logger.WriteEntry("Connections buffer is full!");
						CleanConnectionsBuffer();
						continue;
					}

					Socket conn_socket = server_socket.Accept();

					logger.WriteEntry("We've a new connection... ");

					ConnectionInfo conn_info = new ConnectionInfo();

					conn_info.socket = conn_socket;

					conn_info.thread = new Thread( ProcessConnection );
					conn_info.thread.IsBackground = true;
					conn_info.socket.DontFragment = true;
					conn_info.StartProcessing();
					connections.Add(conn_info);
				}
			}
			catch ( ThreadStartException ex )
			{
				logger.WriteError("Error starting a new thread: " + ex.Message);
			}
		}
		
		private ServerMessage ProcessRequest( string request_contents ) {
			logger.WriteDebug("Processing a request: " + request_contents);

			if ( request_contents.Contains("<root>")
			    && request_contents.Contains("</root>"))
			{
				ClientMessage msg = new ClientMessage();
				var start_xml = request_contents.IndexOf("<root>", StringComparison.InvariantCulture);
				var length = request_contents.IndexOf("</root>", StringComparison.InvariantCulture)+7-
					request_contents.IndexOf("<root>", StringComparison.InvariantCulture);
				var xml = request_contents.Substring( start_xml, length );
				
				msg.FromXML( xml );
				return MessageProcessing.ProcessMessage( msg );
			}
			return ServerMessage.EmptyMessage;
		}

		private void ProcessConnection(object state)
		{
			ConnectionInfo conn_info = (ConnectionInfo)state;
			byte[] buffer = new byte[buffer_capacity];

			try
			{
				logger.WriteEntry("Trying to process a connection...");
				for (;;)
				{
					int bytes_read = 0;
					bytes_read = conn_info.socket.Receive(buffer);
					
					if ( bytes_read > 0 )
					{
						string received = Encoding.UTF8.GetString(buffer);
						var server_msg = this.ProcessRequest( received );
						
						foreach ( var ci in connections )
						{
							if ( ci == conn_info )
							{
								if ( server_msg != null )
									Responder.Respond( ci, server_msg.ToXml() );
								else
									Responder.Respond( ci, String.Empty );
							}
						}
					}
					else
						return;
				}
			}
			catch ( SocketException ex )
			{
				logger.WriteError("Error in connection processing: " + ex.Message);
			}
		}

		/// <summary>
		/// Starts data processing
		/// </summary>
		public void Start()
		{
			try
			{
				logger.WriteEntry("Starting a server...");
				if (Status == ServerStatus.OFF)
				{
					SetupSocket();
					AcceptConnections();
				}
			}
			catch (HttpListenerException ex)
			{
				logger.WriteError(ex.Message);
				Status = ServerStatus.ERROR;
			}
			catch (ThreadStartException ex)
			{
				logger.WriteError(ex.Message);
				Status = ServerStatus.ERROR;
			}

			Status = ServerStatus.ON;
		}

		/// <summary>
		/// Stops data processing
		/// </summary>
		public void Stop()
		{
			try
			{
				logger.WriteEntry("Stopping a server..");
				if (Status != ServerStatus.OFF)
				{
					var connections_copy = connections.ToArray();

					// Connections array cleaning...
					ConnectionInfo tmp;
					while (connections.TryPeek(out tmp)) { };

					server_socket.Shutdown(SocketShutdown.Both);
				}
			}
			catch ( SocketException ex )
			{
				logger.WriteError(ex.Message);
				Status = ServerStatus.ERROR;
			}

			Status = ServerStatus.OFF;
		}
	}
}
