/*
 * Пользователь: igor.evdokimov
 * Дата: 10.11.2015
 * Время: 14:52
 */
using System;
using DwarfDB.DataStructures;
using DwarfDB.AccessFunctions;

namespace DwarfDB.Global
{
	/// <summary>
	/// A static class for a simplified access check
	/// </summary>
	public static class CheckAccess
	{
		static CheckAccess()
		{
		}
		
		public static bool CheckAdminAccess( object dwarf_obj, User.User user )  {
			return CheckLevel( dwarf_obj,  Access.AccessLevel.ADMIN, user );
		}
		
		public static bool CheckReadAccess( object dwarf_obj, User.User user )  {
			return CheckLevel( dwarf_obj,  Access.AccessLevel.ADMIN, user ) ||
				CheckLevel( dwarf_obj,  Access.AccessLevel.READ_WRITE, user ) ||
				CheckLevel( dwarf_obj,  Access.AccessLevel.READ_WRITE_DROP, user ) ||
				CheckLevel( dwarf_obj,  Access.AccessLevel.READ_ONLY, user );
		}
		
		public static bool CheckWriteAccess( object dwarf_obj, User.User user )  {
			return CheckLevel( dwarf_obj,  Access.AccessLevel.ADMIN, user ) ||
				CheckLevel( dwarf_obj,  Access.AccessLevel.READ_WRITE, user ) ||
				CheckLevel( dwarf_obj,  Access.AccessLevel.READ_WRITE_DROP, user );
		}
		
		public static bool CheckDropAccess( object dwarf_obj, User.User user )  {
			return CheckLevel( dwarf_obj,  Access.AccessLevel.READ_WRITE_DROP, user );
		}
		
		private static bool CheckLevel( object dwarf_obj, Access.AccessLevel level, User.User user ) {
			if ( dwarf_obj != null ) {
				var chk_lvl = Access.AccessLevel.DENIED;
				if ( dwarf_obj is DataContainer ) {
					var obj = ( dwarf_obj as DataContainer );
					chk_lvl = obj.GetLevel( user );
				} else if ( dwarf_obj is DataBase ) {
					var obj = ( dwarf_obj as DataContainer );
					chk_lvl = obj.GetLevel( user );
				} else
					return false;
				
				return chk_lvl == level;
			} else
				throw new AccessException( " dwarf_obj is NULL in CheckAccess.CheckLevel!!! " );
		}
	}
}
