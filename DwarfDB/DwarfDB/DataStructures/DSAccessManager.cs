/*
 * Пользователь: igor.evdokimov
 * Дата: 03.11.2015
 * Время: 11:28
 */
using System;
using System.Collections.Generic;
using DwarfDB.AccessFunctions;
using DwarfDB.User;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// DataStructure access manager
	/// </summary>
	public class DSAccessManager
	{
		object acc_object;
		public DSAccessManager( object _object ) {
			acc_object = _object;
		}
		
		/// <summary>
		/// Adding a new access record for our DataStructure
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		public void AddAccess ( User.User _user,
		                       Access.AccessLevel _level ) {
			var t = Access.Instance( _user, _level, acc_object );
			accesses.Add(t);
		}
		
		/// <summary>
		/// Changing an access record for our DB
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_new_level"></param>
		public void ChangeAccess ( User.User _user,
		                          Access.AccessLevel _new_level ) {
			foreach ( var ac in accesses ) {
				if ( ac.User.Credentials.Login == _user.Credentials.Login ) {
					ac.SetLevel(_new_level);
					ac.Save();
				} else
					this.AddAccess( _user, _new_level );
			}
		}
		
		internal List<Access> GetAccesses() {
			return accesses;
		}
		
		/// <summary>
		/// Getting an access level for a given user
		/// </summary>
		/// <param name="_user"></param>
		/// <returns></returns>
		public Access.AccessLevel GetLevel( User.User _user ) {
			var new_acc_array = AccessFile.ReadAccessFile( acc_object, _user );
			accesses.AddRange(new_acc_array);
			foreach ( var ac in accesses ) {
				if ( ac.User.Credentials.Login == _user.Credentials.Login )
					return ac.Level;
			}
			
			return Access.AccessLevel.DENIED;
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
			AccessFile.ReadAccessFile( acc_object, _user );
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
					} 
					if ( prm == Permissions.WRITE ) {
						if ( user_level == Access.AccessLevel.ADMIN ||
						    user_level == Access.AccessLevel.READ_WRITE ||
						    user_level == Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.READ ) {
						if ( user_level == Access.AccessLevel.ADMIN ||
						    user_level == Access.AccessLevel.READ_WRITE ||
						    user_level == Access.AccessLevel.READ_WRITE_DROP ||
						    user_level == Access.AccessLevel.READ_ONLY  )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.DROP ) {
						if ( user_level == Access.AccessLevel.ADMIN ||
						    user_level == Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					} else if ( prm == Permissions.DELETE ) {
						if ( user_level == Access.AccessLevel.ADMIN ||
						    user_level == Access.AccessLevel.READ_WRITE ||
						    user_level == Access.AccessLevel.READ_WRITE_DROP )
						{
							answer_msg = prm.ToString() + " - OK!";
							return true;
						}
					}

				}
			}
			
			answer_msg = " Access denied for user: "+_user.Credentials.Login +
				". You don't have a \""+prm.ToString()+"\" access! ";
			return false;
		}
		
		public bool CheckWritePermission( User.User _user ) {
			string answer = String.Empty;
			var ret = CheckPermission( Permissions.WRITE, _user, ref answer );
			
			if ( ret == false )
				Errors.ErrorProcessing.Display( answer, String.Empty, String.Empty, DateTime.Now );
			
			return ret;
		}
		
		public bool CheckReadPermission( User.User _user ) {
			string answer = String.Empty;
			var ret = CheckPermission( Permissions.READ, _user, ref answer );
			
			if ( ret == false )
				Errors.ErrorProcessing.Display( answer, String.Empty, String.Empty, DateTime.Now );
			
			return ret;
		}
		
		public bool CheckDeletePermission( User.User _user ) {
			string answer = String.Empty;
			var ret = CheckPermission( Permissions.DELETE, _user, ref answer );
			
			if ( ret == false )
				Errors.ErrorProcessing.Display( answer, String.Empty, String.Empty, DateTime.Now );
			
			return ret;
		}
		
		public bool CheckCreateSubsPermission( User.User _user ) {
			string answer = String.Empty;
			var ret = CheckPermission( Permissions.CREATE_SUBS, _user, ref answer );
			
			if ( ret == false )
				Errors.ErrorProcessing.Display( answer, String.Empty, String.Empty, DateTime.Now );
			
			return ret;
		}
		
		public bool CheckDropPermission( User.User _user ) {
			string answer = String.Empty;
			var ret = CheckPermission( Permissions.DROP, _user, ref answer );
			
			if ( ret == false )
				Errors.ErrorProcessing.Display( answer, String.Empty, String.Empty, DateTime.Now );
			
			return ret;
		}
		
		readonly List<Access> accesses = new List<Access>();
	}
}