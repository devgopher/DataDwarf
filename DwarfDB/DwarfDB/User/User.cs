/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 15:17
 */
using System;

namespace DwarfDB.User
{
	/// <summary>
	/// Description of User.
	/// </summary>
	public class User
	{
		public User()
		{
		}
		
		/// <summary>
		/// A user without any credentials and permissions
		/// </summary>
		/// <returns></returns>
		public static User Dummy() {
			return new User();
		}
		
		public UserCredentials Credentials {
			get {
				return user_credentials;
			}
			private set {
				user_credentials = value;
			}
		}
		
		private UserCredentials user_credentials = null;
		private UserPermissions user_permissions = null;
	}
}
