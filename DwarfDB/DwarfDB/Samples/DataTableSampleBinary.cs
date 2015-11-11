/*
 * Пользователь: Igor.Evdokimov
 * Дата: 07.09.2015
 * Время: 13:21
 */
using System;
using System.Linq;
using DwarfDB.DataStructures;
using DwarfDB.ChunkManager;
using System.Collections.Generic;
using System.IO;

namespace DwarfDB
{
	partial class Program
	{
		public static void DataTableChunkCreationBin ()
		{

			string test_binary = String.Empty;
			var user = User.User.New( "root", "12345678");
			
			if (!File.Exists ("./testdata/binary.jpg")) {
				Errors.ErrorProcessing.Display ("No file : ./testdata/binary.jpg", String.Empty, String.Empty, DateTime.Now);
			}


			var bts = File.ReadAllBytes( "./testdata/binary.jpg");

			test_binary = System.Text.Encoding.Default.GetString( bts );

			Console.WriteLine ("Create or use an existing db (0/1)?: ");
			string choise = Console.ReadLine ();
			Console.WriteLine ("Enter db name: ");
			string db_name = Console.ReadLine ();

			var indexes = new List<Index> ();
			var chunk_manager = new ChunkManager.ChunkManager ();
			
			if (choise.Trim () == "0") {
				Console.WriteLine ("Enter rows count: ");
				int rows_count = Int32.Parse (Console.ReadLine ());
				Console.WriteLine ("Enter cols count: ");
				int cols_count = Int32.Parse (Console.ReadLine ());
				Console.WriteLine ("Enter chunk size: ");
				int chunk_size = Int32.Parse (Console.ReadLine ());
				
				Console.WriteLine ("Creating DB...");
				var db = DataBase.Create (db_name,
				                          chunk_manager);
				
				Console.WriteLine ("Creating DC...");
				
				var dc = new DataContainer (db, "DCBin");

				for (int i = 0; i < cols_count; ++i) {
					var column = new Column ();
					column.Name = "col" + i.ToString ();
					column.Type = DataType.BINARY;
					dc.AddColumn (column, user);
				}
				
				
				for (int k = 0; k < rows_count; ++k) {
					var rec1 = new Record (dc);
					rec1.Id = dc.NextId ();

					foreach (var col in dc.Columns) {
						rec1 [col.Name].Value = test_binary;
						rec1 [col.Name].Type = DataType.BINARY;

						rec1.BuildIndex ();
					}
					dc.AddRecordToStack (rec1);
					indexes.Add (rec1.GetIndex ());
				}
				dc.Save ();
			} else {
				Console.WriteLine ("Trying to get data from db \"" + db_name + "\"");
				var db2 = DataBase.LoadFrom (db_name, chunk_manager);
				
				var dc = chunk_manager.GetDataContainer ("DCBin");
				dc.AssignOwnerDB (db2);
				//Console.WriteLine("Preloading DC1...");
				dc.PreLoad ( user );
				
				// Getting a record
				Record rc = null;
				Record rc1 = null;
				Record rc2 = null;
				Record rc3 = null;
				Record rc4 = null;
				
				var get_time = Checks.ExecutionTimeCheck.DoCheck (() => {
				                                                  	rc = dc.GetRecord (2030, user);
				                                                  });
				var get_time2 = Checks.ExecutionTimeCheck.DoCheck (() => {
				                                                   	rc1 = dc.GetRecord (10, user);
				                                                   });
				var get_time3 = Checks.ExecutionTimeCheck.DoCheck (() => {
				                                                   	rc = dc.GetRecord (30, user);
				                                                   });
				var get_time4 = Checks.ExecutionTimeCheck.DoCheck (() => {
				                                                   	rc3 = dc.GetRecord (20, user);
				                                                   });
				var get_time2s = Checks.ExecutionTimeCheck.DoCheck (() => {
				                                                    	rc4 = dc.GetRecord (7900, user);
				                                                    });
				
				Console.WriteLine ("Getting value time1, ms: " + get_time.ToString ());
				Console.WriteLine ("Getting value time2, ms: " + get_time2.ToString ());
				Console.WriteLine ("Getting value time3, ms: " + get_time3.ToString ());
				Console.WriteLine ("Getting value time4, ms: " + get_time4.ToString ());
				Console.WriteLine ("Getting value time2s, ms: " + get_time2s.ToString ());

				if (rc1 != null && !(rc is DummyRecord)) {
					var binval = System.Text.Encoding.Default.GetBytes(rc1.Fields [0].Value.ToString ());
					Console.WriteLine ("Val: " + binval.ToString());
					
					using ( var fl = File.Create( "./testdata/binary_1.jpg" ) ) {
						var sw = new  BinaryWriter( fl );
						foreach ( var chr in binval )
							sw.Write( chr );
						sw.Close();
					}
					
					
				}
				if (rc2 != null && !(rc2 is DummyRecord))
					Console.WriteLine ("Val: " + rc2.Fields [0].Value.ToString ());
				if (rc3 != null && !(rc3 is DummyRecord))
					Console.WriteLine ("Val: " + rc3.Fields [0].Value.ToString ());
				if (rc4 != null && !(rc4 is DummyRecord))
					Console.WriteLine ("Val: " + rc4.Fields [0].Value.ToString ());
				
				
				
				Console.WriteLine ("DO YOU WANT TO DROP THIS DB (Y/N)?");
				if (Console.ReadLine ().ToUpper() == "Y") {
					db2.Drop (user);
				}
			}
		}
	}
}