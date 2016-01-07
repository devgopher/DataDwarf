/*
 * User: Igor
 * Date: 29.11.2015
 * Time: 0:19
 */
using System;
using DwarfDB.DataStructures;

namespace DwarfDB.Links
{
	/// <summary>
	/// A class for links to records
	/// </summary>
	public class RecordLink : ILink
	{
		
		//DwarfServer server = null;
		public RecordLink(object _client)
		{

		}
		
		public ILink CreateNew( string address,
		                       string db_name,
		                       string dc_name,
		                       string rec_hash )
		{
			return Create( address, db_name, dc_name, rec_hash );
		}

		public static ILink Create( string address,
		                           string db_name,
		                           string dc_name,
		                           string rec_hash )
		{
			/*
			 	1. Reference parsing
				2. Receiving content
			 */
			throw new NotImplementedException();
		}

		public ILink CreateNew(string db_name,
		                       string dc_name,
		                       string rec_hash)
		{
			/*
			 	1. Reference parsing
				2. Receiving content
			 */
			throw new NotImplementedException();
		}

		/// <summary>
		/// This function asks DwarfClient to receive copy of a real
		/// record
		/// </summary>
		/// <returns></returns>
		public IStructure Get()
		{
			return inner_record;
		}

		public void Drop()
		{
			inner_record = null;
		}

		// inner copy of a remote record
		private Record inner_record = null;

		private string db_name = String.Empty;
		private string dc_name = String.Empty;
		private string rec_hash = String.Empty;
	}
}
