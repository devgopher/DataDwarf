/*
 * User: Igor
 * Date: 18.09.2015
 * Time: 22:20
 */

using System;
using System.Collections.Generic;
using DwarfDB.DataStructures;
using DwarfDB.AccessFunctions;

namespace DwarfDB
{
	partial class Program
	{
		public static void DataBaseChunkCreation() {
			var time = Checks.ExecutionTimeCheck.DoCheck ( () => {
			                                              	Console.WriteLine("Creating DB...");
			                                              	var db = DataBase.Create( "testbase3" );			                                              	
			                                              	User.User new_usr = User.User.New( "root", "12345678" );
			                                              	db.AddAccess( new_usr, Access.AccessLevel.ADMIN);		                                              	
			                                              	var dc = new DataContainer( db, "DataC2" );
			                                              	db.AddNewDataContainer( dc, new_usr );
			                                              	db.chunk_manager.CreateChunk( db );
			                                              } );
			
			Console.WriteLine("Execution time, ms: "+time);
		}
	}
}
