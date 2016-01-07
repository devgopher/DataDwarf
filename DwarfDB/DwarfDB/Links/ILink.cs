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
		ILink CreateNew( string address,
		                string db_name,
		                string dc_name,
		                string rec_hash );
		IStructure Get();
		void Drop();
	}
}
