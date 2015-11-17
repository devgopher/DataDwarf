/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 15:17
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace DwarfDB.User
{
	/// <summary>
	/// A class for users representation
	/// </summary>
	public class User
	{
		private User()
		{
			Credentials = new UserCredentials();
		}
		
		static User() {
			users_file_path = Config.Config.Instance.DataDirectory+"/.users";
			CreateUsersList();
		}
		
		/// <summary>
		/// A user without any credentials and permissions
		/// </summary>
		/// <returns></returns>
		public static User Dummy() {
			return new User();
		}
		
		/// <summary>
		/// Getting user from a 'users' list
		/// </summary>
		/// <param name="login"></param>
		/// <returns></returns>
		public static User Get( string login ) {
			var strg = FindLogin( login );
			if ( strg != null ) {
				var tmp = strg.Item1.Split(':');
				var user = new User();
				user.Credentials.Login = login;
				user.Credentials.hashed_pwd = tmp[1];
				return user;
			}
			return null;
		}
		
		/// <summary>
		/// Creaing a new user
		/// </summary>
		/// <param name="login"></param>
		/// <param name="passwd"></param>
		/// <returns></returns>
		public static User New( string login, string passwd ) {
			if ( FindLogin( login ) != null ) {
				Errors.ErrorProcessing.Display("User with a such login already exists!", "Adding a new user", 
				                               "", DateTime.Now);
				return Get( login );
			}
			var user = new User();
			user.Credentials = new UserCredentials();
			user.Credentials.Login = login;
			user.Credentials.Password = passwd;
			SaveToUsersList( user );
			return user;
		}
		
		/// <summary>
		/// User access credentials
		/// </summary>
		public UserCredentials Credentials {
			get {
				return user_credentials;
			}
			private set {
				user_credentials = value;
			}
		}
		
		/// <summary>
		/// Creating a file with a list of users
		/// </summary>
		private static void CreateUsersList() {
			if ( !File.Exists( users_file_path ) ) {
				using (var fs = File.Create( users_file_path )) {};
			}
		}
		
		/// <summary>
		/// Adding a new user to a userlist file
		/// </summary>
		/// <param name="_user"></param>
		private static void SaveToUsersList( User _user ) {
			CreateUsersList();
			CreateBackup();
			var fl = FindLogin(_user.Credentials.Login);
			var usr_strg = _user.Credentials.Login + ":" + _user.Credentials.hashed_pwd;
			if ( fl == null ) {
				using ( var fs = new FileStream( users_file_path, FileMode.Append, FileAccess.Write ) ) {
					var sw = new StreamWriter( fs );
					sw.WriteLine(usr_strg);
					sw.Close();
				}
			} else {
				using ( var fs = new FileStream( users_file_path, FileMode.Append, FileAccess.Write ) ) {
					var sw = new StreamWriter( fs );
					sw.BaseStream.Position = fl.Item2;
					sw.WriteLine(usr_strg);
					sw.Close();
				}
			}
		}
		
		static private void CreateBackup() {
			int numb = 0;
			if ( File.Exists( users_file_path ) ) {
				File.Copy( users_file_path, users_file_path + "0.bak" );
				while ( File.Exists( users_file_path + numb.ToString() + ".bak" ) ) {
					++numb;
					File.Copy( users_file_path, users_file_path + numb.ToString() + ".bak" );
				}
			}
		}
		
		/// <summary>
		/// Adding a new user to a userlist file
		/// </summary>
		/// <param name="_user"></param>
		private static void ChangePassword( User _user, string new_password ) {
			_user.Credentials.Password = new_password;
		}
		
		/*	private static void RemoveFromUsersList( User _user ) {
			int user_strg_num = FindLogin(_user.Credentials.Login);
			if ( user_strg_num  > -1 ) {
				using ( var fs = new FileStream( users_file_path, FileMode.Append, FileAccess.Write ) ) {
					var sw = new StreamWriter( fs );
					sw.
				}
			}
		}
		 */
		
		/// <summary>
		/// Seeking for a given login in userlist file
		/// </summary>
		/// <param name="login"></param>
		/// <returns></returns>
		private static Tuple<string, long> FindLogin( string login ) {
			using ( var fs = new FileStream( users_file_path, FileMode.Open, FileAccess.Read ) ) {
				var sr = new StreamReader( fs );
				string strg = String.Empty;
				long strg_pos = -1;
				while ( (strg = sr.ReadLine()) != null ) {
					var tmp = strg.Split(':');
					if ( tmp.Length > 1 )
						if ( tmp[0].Trim() == login.Trim() )
							return Tuple.Create<string, long>(strg, strg_pos++);
					strg_pos = sr.BaseStream.Position;
				}
				
			}
			return null;
		}
		
		
		private static string users_file_path = "";
		private UserCredentials user_credentials = null;
		private UserPermissions user_permissions = null;
	}
}
