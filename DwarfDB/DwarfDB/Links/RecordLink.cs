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
		public RecordLink(object _client)
		{
		}

		public RecordLink()
		{
		}
		
		public static ILink CreateNew( string _address,
		                              string _db_name,
		                              string _dc_name,
		                              string _rec_hash )
		{
			var ret = new RecordLink();
			ret.Init( _address, _db_name, _dc_name, _rec_hash );
			return ret;
		}

		public void Init( string _address,
		                 string _db_name,
		                 string _dc_name,
		                 string _rec_hash )
		{
			this.db_name = _db_name;
			this.dc_name = _dc_name;
			this.rec_hash = _rec_hash;
		}

		public ILink CreateNew( string _db_name,
		                       string _dc_name,
		                       string _rec_hash )
		{
			return CreateNew( null, _db_name, _dc_name, _rec_hash );
		}

		/// <summary>
		/// This function asks DwarfClient to receive copy of a real
		/// record
		/// </summary>
		/// <returns></returns>
		public IStructure Get( string orig_string )
		{
			/*
			 	1. Reference parsing
				2. Receiving content
			 */
			parser.Parse( orig_string, this );

			
			return inner_record;
		}

		public void Drop()
		{
			inner_record = null;
		}

		// inner copy of a remote record
		private Record inner_record = null;
		static private RecordLinkParser parser = new RecordLinkParser();
		private string db_name = String.Empty;
		private string dc_name = String.Empty;
		private string rec_hash = String.Empty;
	}
}
