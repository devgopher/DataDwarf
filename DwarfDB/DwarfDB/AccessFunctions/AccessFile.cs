/*
 * Пользователь: igor.evdokimov
 * Дата: 29.10.2015
 * Время: 12:21
 */
using System;
using System.IO;
using DwarfDB.DataStructures;

namespace DwarfDB.AccessFunctions
{
	/// <summary>
	/// A class for basic operations with ".access" files
	/// </summary>
	internal class AccessFile
	{
		string filepath = String.Empty;
		DataBase db = null;
		DataContainer dc = null;
		
		public AccessFile( DataBase _db ) {
			if ( _db != null ) {
				db = _db;
				filepath = db.DbPath+"/_db_"+db.Name+".access";
			} else
				throw new AccessException( " Database is not defined! " );
		}
		
		public AccessFile( DataContainer _dc ) {
			if ( _dc != null ) {
				dc = _dc;
				
				if ( dc.GetOwnerDB() == null )
					throw new AccessException( " Database is not defined for this DataСontainer! " );
				
				filepath = dc.GetOwnerDB().DbPath+"/_dc_"+dc.Name+".access";
			}
		}
		
		public void CreateAccessFile() {
			CreateAccessFile(filepath);
		}
		
		public static void CreateAccessFile( string acc_filepath ) {
			if ( !File.Exists( acc_filepath ) )
				using ( var fs = File.Create( acc_filepath ) ) {};
		}

		public void Save() {
			CreateAccessFile();
			var users_acc = db.GetAccesses();
			if ( users_acc.Count > 0 ) {
				using ( var fs = new FileStream( filepath, FileMode.Append, FileAccess.Write ) ) {
					var sw = new StreamWriter( fs );
					foreach ( var acc in users_acc ) {
						sw.WriteLine( acc.User.Credentials.Login + ":" +acc.Level );
					}
					
					sw.Close();
				}
			}
		}
		
		public void Save( Access _acc ) {
			CreateAccessFile();
			using ( var fs = new FileStream( filepath, FileMode.Append, FileAccess.Write ) ) {
				var sw = new StreamWriter( fs );
				sw.WriteLine( _acc.User.Credentials.Login + ":" +_acc.Level );
				sw.Close();
			}
		}
		
		private static string GetAccessFilePath( object dwarf_obj ) {
			var cfg = Config.Config.Instance;
			if ( dwarf_obj == null )
				return null;
			
			if ( dwarf_obj is DataBase ) {
				var db_name = (dwarf_obj as DataBase).Name;
				return cfg.DataDirectory+"/"+db_name+"/_db_"+db_name+".access";
			}
			
			if ( dwarf_obj is DataContainer ) {
				var dc = (dwarf_obj as DataContainer);
				var dc_name = dc.Name;
				var db_name = dc.GetOwnerDB().Name;
				return cfg.DataDirectory+"/"+db_name+"/_dc_"+dc_name+".access";
			}
			
			return null;
		}
		
		/// <summary>
		/// Reading an access file to an array of "Access" objects
		/// </summary>
		/// <param name="dwarf_obj">An object, for which we need to get a set of accesses</param>
		/// <param name="_user"></param>
		/// <returns></returns>
		public static Access[] ReadAccessFile( object dwarf_obj, User.User _user ) {
			var ret = new Access[0];
			
			var acc_filepath = GetAccessFilePath( dwarf_obj );

			if ( !File.Exists( acc_filepath ) ) {
				CreateAccessFile(acc_filepath);
			}
			
			string contents = String.Empty;
			
			using ( var fs = new FileStream( acc_filepath, FileMode.Open, FileAccess.Read ) ) {
				var sr = new StreamReader( fs );
				
				contents = sr.ReadToEnd();
				sr.Close();
			}
			
			var contents_splitted = contents.Replace("\r",String.Empty).Split('\n');
			
			foreach ( var strg in contents_splitted ) {
				var tmp = strg.Split(':');
				if ( tmp.Length == 2 ) {
					Access.AccessLevel al = Access.AccessLevel.DENIED;
					
					switch ( tmp[1] ) {
						case "ADMIN":
							al = Access.AccessLevel.ADMIN;
							break;
						case "READ_ONLY":
							al = Access.AccessLevel.READ_ONLY;
							break;
						case "READ_WRITE":
							al = Access.AccessLevel.READ_WRITE_DROP;
							break;
						case "READ_WRITE_DROP":
							al = Access.AccessLevel.READ_WRITE_DROP;
							break;
					}
					
					var ret_acc = Access.Instance( _user, al, dwarf_obj );
					Array.Resize(ref ret, ret.Length+1);
					ret[ret.Length-1] = ret_acc;
				}
			}
			return ret;
		}
		
	}
}
