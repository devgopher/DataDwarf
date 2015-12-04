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
		ILink Create( string reference );
		IStructure Get();
		void Drop();		
	}
}
