/*
 * User: Igor
 * Date: 27.11.2015
 * Time: 23:42
 */
using System;

namespace DwarfDB.Links
{
	/// <summary>
	/// An interface for DB link
	/// </summary>
	public interface ILink
	{
		ILink Create( string reference );
		IStructure Get();
		void Drop();		
	}
}
