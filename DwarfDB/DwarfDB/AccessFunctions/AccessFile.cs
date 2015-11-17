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
					throw new AccessException( " Database is not defined for this datacontainer! " );
				
				filepath = dc.GetOwnerDB().DbPath+"/_dc_"+dc.Name+".access";
			}
		}
		
		public void CreateAccessFile() {
			if ( !File.Exists( filepath ) )
				using ( var fs = File.Create( filepath ) ) {};
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
		
		public static Access[] ReadAccessFile( object dwarf_obj ) {
			/*	Access[] ret = new Access[0];
			using ( var fs = new FileStream( filepath, FileMode.Open, FileAccess.Read ) ) {
				var sr = new StreamReader( fs );
				string strg = String.Empty;
				while ((strg = sr.ReadLine()) != null) {
					User new_user = new User.User();
					var tmp = strg.Split(':');
					if ( tmp.Length == 2 ) {
						new_user.
					}
				}
 			}
			 */
			// TODO: reading an access file for a given object into an array
			throw new NotImplementedException("Not implemented yet! Sorry!");
		}
		
	}
}
