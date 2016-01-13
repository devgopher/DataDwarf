/*
 * User: Igor
 * Date: 09.01.2016
 * Time: 0:19
 */
using System;
using DwarfDB.DataStructures;

namespace DwarfDB.Links
{
	/// <summary>
	/// A class for links to datacontainers
	/// </summary>
	public class DataContainerLink : ILink
	{
		public DataContainerLink(object _client)
		{
		}

		public DataContainerLink()
		{
		}
		
		public void Init( string _address,
		                 string _db_name,
		                 string _dc_name,
		                 string _rec_hash = null )
		{
			this.db_name = _db_name;
			this.dc_name = _dc_name;
		}


		public ILink CreateNew( string address,
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
			
			return inner_dc;
		}

		public void Drop()
		{
			inner_dc = null;
		}

		// inner copy of a remote DC
		private DataContainer inner_dc = null;
		static private DataContainerLinkParser parser = new DataContainerLinkParser();
		private string db_name = String.Empty;
		private string dc_name = String.Empty;
		//private DwarfServer.Receiver.Receiver receiver;
	}
}