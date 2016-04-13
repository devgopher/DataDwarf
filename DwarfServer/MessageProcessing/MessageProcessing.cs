/*
 * Пользователь: Igor
 * Дата: 28.02.2016
 * Время: 13:17
 */
using System;
using System.Data;


namespace DwarfServer
{
	/// <summary>
	/// Processing client messages
	/// </summary>
	public static class MessageProcessing
	{
		private static Logger.Logger logger =
			Logger.Logger.GetInstance();
		
		
		public static ServerMessage ProcessMessage( ClientMessage msg ) {
			try {
				DataTable process_dt = null;
				var resp_msg = new ServerMessage();

				if ( process_dt != null ) {
					resp_msg = ResponseMessage( process_dt );
				}
				return resp_msg;
			} catch ( Exception ex ) {
				logger.WriteError("Error in message processing: "+ex.Message);
			}
			return null;
		}
		
		public static ServerMessage ResponseMessage( bool success ) {
			var resp_msg = new ServerMessage();
			resp_msg.Type = Protocol.success_response;
			resp_msg.AddItem()["result"] = success.ToString();			
			return resp_msg;
		}
		
		public static ServerMessage ResponseMessage( DataTable input_dt ) {
			var resp_msg = new ServerMessage();
			resp_msg.Type = Protocol.common_response;
			
			foreach ( DataRow row in input_dt.Rows ) {
				var item_dict = resp_msg.AddItem();
				foreach ( DataColumn col in input_dt.Columns ) {
					item_dict[col.ColumnName] = row[col].ToString();
				}
			}
			
			return resp_msg;
		}
	}
}
