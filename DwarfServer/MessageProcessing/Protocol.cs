/*
 * Пользователь: Igor
 * Дата: 27.02.2016
 * Время: 23:32
 */
using System;

namespace DwarfServer
{
	/// <summary>
	/// SimpleShop protocol messages
	/// </summary>
	public static class Protocol
	{
		#region ClientRequestTypes
		public const string get_record = "GETREC";
		public const string set_value = "SETVAL";
		public const string add_record = "ADDREC";		
		public const string del_record = "DELREC";		
		public const string get_dc_link = "GETDCLNK";	
		public const string get_rec_link = "GETRECLNK";		
		#endregion
		
		#region ServerResponseTypes
		public const string common_response = "COMMRESP";
		public const string success_response = "SUCCRESP";
		public const string error_response = "ERRRESP";
		public const string send_rec = "SENDREC";
		public const string set_dc_link = "SETDCLNK";
		public const string set_rec_link = "SETRECLNK";
		#endregion
	}
}
