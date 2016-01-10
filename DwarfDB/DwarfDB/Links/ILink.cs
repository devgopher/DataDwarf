/*
 * User: Igor
 * Date: 27.11.2015
 * Time: 23:42
 */
using System;
using DwarfDB.DataStructures;

namespace DwarfDB.Links
{
	/// <summary>
	/// An interface for DB link
	/// </summary>
	public interface ILink
	{
		void Init( string _address,
		                 string _db_name,
		                 string _dc_name,
		                 string _rec_hash );
		//IStructure Get( string orig_string );
		void Drop();
	}
}
