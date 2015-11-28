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
		DwarfServer server = null;
		public RecordLink( DwarfServer _server )
		{
			server = _server;
		}
		
		public ILink Create( string reference ) {
			/*
			 	1. Reference parsing
				2. Receiving content 
			 */
			throw new NotImplementedException();
		}
		
		
		public IStructure Get() {
			return inner_record;
		}
		
		public void Drop() {
			inner_record = null;
		}
		
		// inner copy of a remote record
		private Record inner_record = null;
	}
}
