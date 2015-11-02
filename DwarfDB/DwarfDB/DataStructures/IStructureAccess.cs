/*
 * User: Igor
 * Date: 02.11.2015
 * Time: 22:41
 */
using System;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// Description of IStructureAccess.
	/// </summary>
	public interface IStructureAccess
	{
		/// <summary>
		/// Adding a new access record
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		void AddAccess ( User.User _user, DwarfDB.Access.Access.AccessLevel _level );		
		
		/// <summary>
		/// Changing an access record
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_new_level"></param>
		void ChangeAccess ( User.User _user, Access.Access.AccessLevel _new_level );
		
		/// <summary>
		/// Getting an access level for a given user
		/// </summary>
		/// <param name="_user"></param>
		/// <returns></returns>
		Access.Access.AccessLevel GetLevel( User.User _user );
	}
}
