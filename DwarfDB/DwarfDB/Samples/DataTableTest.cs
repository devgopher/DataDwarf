﻿/*
 * Пользователь: Igor.Evdokimov
 * Дата: 07.09.2015
 * Время: 13:21
 */
using System;
using System.Linq;
using DwarfDB.DataStructures;
using System.Collections.Generic;
using DwarfDB.ChunkManager;

namespace DwarfDB
{
	partial class Program
	{
		public static void DataTableChunkCreation() {
			string systemVersionVal = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory().ToString();
			Console.WriteLine("Framework: "+systemVersionVal);

			Console.WriteLine("Create or use existing db (0/1)?: ");
			string  choise = Console.ReadLine();
			Console.WriteLine("Enter db name: ");
			string db_name = Console.ReadLine();

			var indexes = new List<Index>();
			var chunk_manager = new ChunkManager.ChunkManager();
			
			if ( choise.Trim() == "0" ) {
				Console.WriteLine("Enter rows count: ");
				int rows_count = Int32.Parse(Console.ReadLine());
				Console.WriteLine("Enter cols count: ");
				int cols_count = Int32.Parse(Console.ReadLine());
				Console.WriteLine("Enter chunk size: ");
				int chunk_size = Int32.Parse(Console.ReadLine());
				
				Console.WriteLine("Creating DB...");
				var db = DataBase.Create( db_name,
				                         chunk_manager );
				
				Console.WriteLine("Creating DC...");
				
				var dc = new DataContainer( db, "DataC1" );
				var dc2 = new DataContainer( db, "DataC2" );
				
				for ( int i = 0; i < cols_count; ++i ) {
					var column = new Column();
					column.Name = "col"+i.ToString();
					if ( i % 2 == 0 )
						column.Type = DataType.STRING;
					else
						column.Type = DataType.INT;
					dc.AddColumn( column );
				}
				
				for ( int i = 0; i < cols_count; ++i ) {
					var column = new Column();
					column.Name = "col"+i.ToString();
					if ( i % 2 == 0 )
						column.Type = DataType.STRING;
					else
						column.Type = DataType.INT;
					dc2.AddColumn( column );
				}
				
				chunk_manager.CreateChunk( dc );
				chunk_manager.CreateChunk( dc2 );
				
				for ( int k =0; k< rows_count; ++k ) {
					var rec1 = new Record( dc );
					var rec2 = new Record( dc2 );
					rec1.Id = (UInt64)k;
					rec2.Id = (UInt64)k+3;
					
					foreach ( var col in dc.Columns) {
						if ( k % 2 == 0 ) {
							rec1[col.Name].Value = "съешь ещё этих мягких французских булок да выпей чаю "+col.Name+" "+k.ToString();
							rec1[col.Name].Type = DataType.STRING;
						} else {
							rec1[col.Name].Value = k*100;
							rec1[col.Name].Type = DataType.INT;
						}
						
						if ( k % 2 == 0 ) {
							rec2[col.Name].Value = "Цитрус "+col.Name+" "+k.ToString()+":::"+db.Name;
							rec2[col.Name].Type = DataType.STRING;
						} else {
							rec2[col.Name].Value = k*5555;
							rec2[col.Name].Type = DataType.INT;
						}
					}
					dc.AddRecord( rec1 );
					indexes.Add(rec1.GetIndex());
					dc2.AddRecord( rec2 );
					indexes.Add(rec2.GetIndex());
				}

				chunk_manager.CreateChunk( dc2.GetRecords(), chunk_size );
				chunk_manager.CreateChunk( dc.GetRecords(), chunk_size );
				chunk_manager.SaveIndexes();
			} else {
				Console.WriteLine("Trying to get data from db \""+db_name+"\"");
				
				var db2 = DataBase.LoadFrom( db_name , chunk_manager );
				
				var dc2 = chunk_manager.GetDataContainer( "DataC1" );
				var dc = chunk_manager.GetDataContainer( "DataC2" );
				dc2.AssignOwnerDB(db2);
				dc.AssignOwnerDB(db2);
				
				// Getting a record
				Record rc = null;
				Record rc1 = null;
				Record rc2 = null;

				var get_time = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                 	rc = dc2.GetRecord(2030);
				                                                 });
				var get_time2 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc1 = dc2.GetRecord(10);
				                                                  });
				var get_time3 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc = dc2.GetRecord(30);
				                                                  });
				var get_time4 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc2 = dc2.GetRecord(20);
				                                                  });
				var get_time2s = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                   	rc2 = dc.GetRecord(2230);
				                                                   });
				Console.WriteLine("Getting value time1, ms: "+get_time.ToString());

				Console.WriteLine("Getting value time2 (in stack), ms: "+get_time2.ToString());
				Console.WriteLine("Getting value time3 (not in stack, probably), ms: "+get_time3.ToString());
				Console.WriteLine("Getting value time4 (in stack after time3), ms: "+get_time4.ToString());
				Console.WriteLine("Getting value time2s (in stack), ms: "+get_time2s.ToString());
				if ( rc1 != null &&  !(rc is DummyRecord))
					Console.WriteLine("Val: "+rc1.Fields[0].Value.ToString());
				if ( rc2 != null && !(rc2 is DummyRecord) )
					Console.WriteLine("Val: "+rc2.Fields[0].Value.ToString());
				
				Console.WriteLine("Trying LINQ #1...");
				var query = from x in dc
					where x.Fields[0].Value.ToString().Contains("итру")
					select x;
				
				var aa = query.ToList();
				foreach ( var rec in aa) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa.Count);
				}

				Console.WriteLine("Trying LINQ #2...");
				var aa1 = dc.Select((x,y)=>x).Where( (x) => {
				                                    	return x.Fields[1].Type == DataType.INT &&
				                                    		(int.Parse(x.Fields[1].Value.ToString()) % 5 == 0);
				                                    }).ToList();
				
				foreach ( var rec in aa1) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa1.Count+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa1.Count);
				}
			}
		}
	}
}
