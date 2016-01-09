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
	public class DataContainerLink : ILink
	{
		public DataContainerLink(object _client)
		{

		}
		
		public static ILink Create( string address,
		                           string db_name,
		                           string dc_name )
		{
			/*
			 	1. Reference parsing
				2. Receiving content
			 */
			throw new NotImplementedException();
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
		public IStructure Get()
		{
			return inner_dc;
		}

		public void Drop()
		{
			inner_dc = null;
		}

		// inner copy of a remote DC
		private DataContainer inner_dc = null;
		private DataContainerLinkParser parser = new DataContainerLinkParser();
		private string db_name = String.Empty;
		private string dc_name = String.Empty;
	}
}