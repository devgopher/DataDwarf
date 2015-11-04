/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 15:17
 */
using System;
using System.IO;

namespace DwarfDB.User
{
	/// <summary>
	/// Description of User.
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
				var tmp = strg.Split(':');
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
				Errors.ErrorProcessing.Display("User with a such login already exists!", "Adding a new user", "", DateTime.Now);
				return Get( login );
			}
			var user = new User();
			user.Credentials = new UserCredentials();
			user.Credentials.Login = login;
			user.Credentials.Password = passwd;
			AddToUsersList( user );
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
		/// Adding a new user to userlist file
		/// </summary>
		/// <param name="_user"></param>
		private static void AddToUsersList( User _user ) {
			CreateUsersList();
			if ( FindLogin(_user.Credentials.Login) == null ) {
				using ( var fs = new FileStream( users_file_path, FileMode.Append, FileAccess.Write ) ) {
					var sw = new StreamWriter( fs );
					var strg = _user.Credentials.Login + ":" + _user.Credentials.hashed_pwd;
					sw.WriteLine(strg);
					sw.Close();
				}
			}
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
		private static string FindLogin( string login ) {
			using ( var fs = new FileStream( users_file_path, FileMode.Open, FileAccess.Read ) ) {
				var sr = new StreamReader( fs );
				string strg = String.Empty;
				int strg_number = 0;
				while ( (strg = sr.ReadLine()) != null ) {
					var tmp = strg.Split(':');
					++strg_number;
					if ( tmp.Length > 1 )
						if ( tmp[0].Trim() == login.Trim() )
							return strg;
				}
			}
			return null;
		}
		
		
		private static string users_file_path = "";
		private UserCredentials user_credentials = null;
		private UserPermissions user_permissions = null;
	}
}
