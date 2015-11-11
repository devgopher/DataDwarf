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

namespace DwarfDB
{
	partial class Program
	{
		public static void DataTableChunkCreation() {
			Console.WriteLine("Create or use an existing db (0/1)?: ");
			string  choise = Console.ReadLine();
			Console.WriteLine("Enter db name: ");
			string db_name = Console.ReadLine();

			User.User user = User.User.New("root", "12345678");
			
			var indexes = new List<Index>();
			var chunk_manager = new ChunkManager.ChunkManager();
			
			if ( choise.Trim() == "0" ) {
				Console.WriteLine("Enter rows count: ");
				int rows_count = Int32.Parse(Console.ReadLine());
				Console.WriteLine("Enter cols count: ");
				int cols_count = Int32.Parse(Console.ReadLine());

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
					dc.AddColumn( column, user );
				}
				
				for ( int i = 0; i < cols_count; ++i ) {
					var column = new Column();
					column.Name = "col"+i.ToString();
					if ( i % 2 == 0 )
						column.Type = DataType.STRING;
					else
						column.Type = DataType.INT;
					dc2.AddColumn( column, user );
				}
				
				for ( int k =0; k< rows_count; ++k ) {
					var rec1 = new Record( dc );
					var rec2 = new Record( dc2 );
					rec1.Id = dc.NextId();
					rec2.Id = dc2.NextId();
					
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
							rec2[col.Name].Value = k*555;
							rec2[col.Name].Type = DataType.INT;
						}
						
						rec1.BuildIndex();
						rec2.BuildIndex();
					}
					dc.AddRecordToStack( rec1 );
					indexes.Add(rec1.GetIndex());
					dc2.AddRecordToStack( rec2 );
					indexes.Add(rec2.GetIndex());
				}
				dc.Save();
				dc2.Save();
			} else {
				Console.WriteLine("Trying to get data from db \""+db_name+"\"");
				
				var db2 = DataBase.LoadFrom( db_name , chunk_manager );
				
				var dc2 = chunk_manager.GetDataContainer( "DataC1" );
				var dc = chunk_manager.GetDataContainer( "DataC2" );
				dc2.AssignOwnerDB(db2);
				dc.AssignOwnerDB(db2);
				
				//Console.WriteLine("Preloading DC1...");
				dc.PreLoad(user);
				//Console.WriteLine("Preloading DC2...");
				dc2.PreLoad(user);

				// Getting a record
				Record rc = null;
				Record rc1 = null;
				Record rc2 = null;
				Record rc3 = null;
				Record rc4 = null;
				
				var get_time = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                 	rc = dc2.GetRecord(2030, user);
				                                                 });
				var get_time2 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc1 = dc2.GetRecord(10, user);
				                                                  });
				var get_time3 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc = dc2.GetRecord(30, user);
				                                                  });
				var get_time4 = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                  	rc3 = dc2.GetRecord(20, user);
				                                                  });
				var get_time2s = Checks.ExecutionTimeCheck.DoCheck(() => {
				                                                   	rc4 = dc.GetRecord(7900, user);
				                                                   });
				Console.WriteLine("Getting value time1, ms: "+get_time.ToString());

				Console.WriteLine("Getting value time2, ms: "+get_time2.ToString());
				Console.WriteLine("Getting value time3, ms: "+get_time3.ToString());
				Console.WriteLine("Getting value time4, ms: "+get_time4.ToString());
				Console.WriteLine("Getting value time2s, ms: "+get_time2s.ToString());

				if ( rc1 != null &&  !(rc is DummyRecord))
					Console.WriteLine("Val: "+rc1.Fields[0].Value.ToString());
				if ( rc2 != null && !(rc2 is DummyRecord) )
					Console.WriteLine("Val: "+rc2.Fields[0].Value.ToString());
				if ( rc3 != null &&  !(rc3 is DummyRecord))
					Console.WriteLine("Val: "+rc3.Fields[0].Value.ToString());
				if ( rc4 != null && !(rc4 is DummyRecord) )
					Console.WriteLine("Val: "+rc4.Fields[0].Value.ToString());
				
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
				                                    		(int.Parse(x.Fields[1].Value.ToString()) % 3 == 0);
				                                    }).ToList();

				foreach ( var rec in aa1) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa1.Count+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa1.Count);
				}


				Console.WriteLine("Trying LINQ #3...");
				var aa2 = from rec in dc
					join recb in dc2 on
					rec.Fields[1].Value.ToString() equals recb.Fields[1].Value.ToString()
					where rec.Fields[1].Type == DataType.INT && recb.Fields[1].Type == DataType.INT
					select rec;

				foreach ( var rec in aa2 ) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa2.Count()+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa2.Count());
				}
				
				Console.WriteLine("DO YOU WANT TO DROP THIS DB (Y/N)?");
				if ( Console.ReadLine() == "Y" ) {
					db2.Drop( user );
				}
			}
		}
	}
}