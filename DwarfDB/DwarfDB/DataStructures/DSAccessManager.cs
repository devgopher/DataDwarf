﻿/*
 * Пользователь: igor.evdokimov
 * Дата: 03.11.2015
 * Время: 11:28
 */
using System;
using System.Collections.Generic;
using DwarfDB.Access;
using DwarfDB.User;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// Description of DSAccessManager.
	/// </summary>
	public class DSAccessManager : IStructureAccess
	{
		object acc_object;
		public DSAccessManager( object _object ) {
			acc_object = _object;
		}
		
		/// <summary>
		/// Adding a new access record for our DB
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		public void AddAccess ( User.User _user,
		                       DwarfDB.Access.Access.AccessLevel _level ) {
			var t = Access.Access.Instance( _user, _level, acc_object );
			accesses.Add(t);
		}
		
		/// <summary>
		/// Changing an access record for our DB
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_new_level"></param>
		public void ChangeAccess ( User.User _user,
		                          Access.Access.AccessLevel _new_level ) {
			foreach ( var ac in accesses ) {
				if ( ac.User.Credentials.Login == _user.Credentials.Login ) {
					ac.SetLevel(_new_level);
				} else
					this.AddAccess( _user, _new_level );
			}
		}
		
		internal List<Access.Access> GetAccesses() {
			return accesses;
		}
		
		/// <summary>
		/// Getting an access level for a given user
		/// </summary>
		/// <param name="_user"></param>
		/// <returns></returns>
		public Access.Access.AccessLevel GetLevel( User.User _user ) {
			foreach ( var ac in accesses ) {
				if ( ac.User.Credentials.Login == _user.Credentials.Login )
					return ac.Level;
			}
			
			return Access.Access.AccessLevel.DENIED;
		}
		
		private enum Permissions {
			CREATE_SUBS, // Subobjects creation permission
			DELETE, // Deleting permission
			WRITE, // Writing permission
			READ, // Reading permission
			DROP // Droping an given object permission
		}
		
		private bool CheckPermission( Permissions prm,
		                             User.User _user,
		                             ref string answer_msg ) {
			if ( !(acc_object is DataBase || acc_object is DataContainer) ) {
				answer_msg = " This object must be a Database or a DataContainer! ";
				return false;
			}
			
			var user_level = ( acc_object as IStructureAccess ).GetLevel( _user );
			
			foreach ( var acc in accesses ) {
				if ( acc.User.Credentials.Login == _user.Credentials.Login ) {
					if ( prm == Permissions.CREATE_SUBS ) {
						answer_msg = prm.ToString() + " - OK!";
						return true;
					} else if ( prm == Permissions.WRITE ) {
						if ( user_level == Access.Access.AccessLevel.ADMIN ||
						    user_level == Access.Access.AccessLevel.READ_WRITE ||
						    user_level == Access.Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.READ ) {
						if ( user_level == Access.Access.AccessLevel.ADMIN ||
						    user_level == Access.Access.AccessLevel.READ_WRITE ||
						    user_level == Access.Access.AccessLevel.READ_WRITE_DROP ||
						    user_level == Access.Access.AccessLevel.READ_ONLY  )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.DROP ) {
						if ( user_level == Access.Access.AccessLevel.ADMIN ||
						    user_level == Access.Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.DELETE ) {
						if ( user_level == Access.Access.AccessLevel.ADMIN ||
						    user_level == Access.Access.AccessLevel.READ_WRITE ||
						    user_level == Access.Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					}

				}
			}
			
			answer_msg = " Access denied for user: "+_user.Credentials.Login +
				". You don't have an "+prm.ToString()+" access! ";
			return false;
		}
		
		public bool CheckWritePermission( User.User _user ) {
			return CheckPermission( Permissions.WRITE, _user );
		}
		
		public bool CheckReadPermission( User.User _user ) {
			return CheckPermission( Permissions.READ, _user );
		}
		
		public bool CheckDeletePermission( User.User _user ) {
			return CheckPermission( Permissions.DELETE, _user );
		}
		
		public bool CheckCreateSubsPermission( User.User _user ) {
			return CheckPermission( Permissions.CREATE_SUBS, _user );
		}
		
		public bool CheckDropPermission( User.User _user ) {
			return CheckPermission( Permissions.DROP, _user );
		}
		
		readonly List<Access.Access> accesses = new List<Access.Access>();
	}
}