/*
 * User: Igor
 * Date: 18.09.2015
 * Time: 22:20
 */

using System;
using System.Collections.Generic;
using DwarfDB.DataStructures;

namespace DwarfDB
{
	partial class Program
	{
		public static void DataBaseChunkCreation() {
			var indexes = new List<Index>();
			var chunk_manager = new ChunkManager.ChunkManager();
			
			var time = Checks.ExecutionTimeCheck.DoCheck ( () => {
			                                              	Console.WriteLine("Creating DB...");
			                                              	var db = DataBase.Create( "testbase3",
			                                              	                         chunk_manager );
	
			                                              	var dc = new DataContainer( db, "DataC2" );
			                                              	db.AddNewDataContainer( dc );
			                                              	chunk_manager.CreateChunk( db );
			                                              } );
			Console.WriteLine("Execution time, ms: "+time);
		}
	}
}
