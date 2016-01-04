/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 15:18
 */
using System;
using DwarfDB.Crypto;

namespace DwarfDB.User
{
	/// <summary>
	/// Class for user logins and password keeping
	/// </summary>
	public class UserCredentials
	{
		public UserCredentials()
		{
		}
		
		public String Login {
			get; set;
		}

		public String Password {
			get {
				return hashed_pwd;
			}
			set {
				if ( value != null )
					hashed_pwd = ComputeHash.MD5Hash( value );
			}
		}
		
		// A password in MD5 hash
		public string hashed_pwd = null;
	}
}
