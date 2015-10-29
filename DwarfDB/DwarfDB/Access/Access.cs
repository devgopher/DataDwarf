/*
 * User: Igor
 * Date: 28.10.2015
 * Time: 23:05
 */
using System;
using System.Collections.Generic;
using DwarfDB.User;
using DwarfDB.DataStructures;

namespace DwarfDB.Access
{
	/// <summary>
	/// A class for defining parameters of access
	/// to a given structure
	/// </summary>
	public class Access
	{
		public enum AccessLevel {
			READ_ONLY = 0,
			READ_WRITE = 1,
			READ_WRITE_DROP = 2,
			ADMIN = 4,
			DENIED = 5
		}
		
		/// <summary>
		/// User
		/// </summary>
		public User.User User {
			get; private set;
		}
		
		/// <summary>
		/// Access level
		/// </summary>
		public AccessLevel Level {
			get; private set;
		}
		
		/// <summary>
		/// Object for access
		/// </summary>
		public Object AccessObject {
			get; private set;
		}
		
		internal void SetLevel( AccessLevel new_lvl ) {
			Level = new_lvl;
		}
		
		private Access( User.User _user, AccessLevel _level, Object _object )
		{
			if ( _user == null || _object == null )
				throw new AccessException( "User object and/or it's access parameters are NULL" );
			
			User = _user;
			Level = _level;
			AccessObject = _object;
		}
		
		/// <summary>
		/// Getting an instance for given user, level and object
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		/// <param name="_object"></param>
		/// <returns></returns>
		public static Access Instance( User.User _user, AccessLevel _level, Object _object ) {
			// Let's look in our array. Do we already have a such object?
			foreach ( var al in instances ) {
				if ( al.User.Credentials.Login == _user.Credentials.Login &&
				    al.Level== _level &&
				    al.AccessObject == _object ) {
					return al;
				}
			}
			
			// If we don't have => let's create a new Access instance and add it to our array
			var new_user = new Access( _user, _level, _object );
			instances.Add( new_user );
			return new_user;
		}
		
		/// <summary>
		/// An array of instances
		/// </summary>
		private static List<Access> instances = new List<Access>();
		
	}
}
