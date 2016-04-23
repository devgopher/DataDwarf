/*
 * Пользователь: Igor.Evdokimov
 * Дата: 07.09.2015
 * Время: 13:21
 */
using System;
using System.Linq;
using System.Threading;
using DwarfDB.DataStructures;
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
				User.User new_usr = User.User.New( "root", "12345678" );
				db.AddAccess( new_usr, DwarfDB.AccessFunctions.Access.AccessLevel.ADMIN);
				Console.WriteLine("Creating DC...");
				
				var dc = new DataContainer( db, "DataC1" );
				dc.AddAccess( new_usr, DwarfDB.AccessFunctions.Access.AccessLevel.ADMIN);
				var dc2 = new DataContainer( db, "DataC2" );
				dc2.AddAccess( new_usr, DwarfDB.AccessFunctions.Access.AccessLevel.ADMIN);
				
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
				
				for ( int k = 0; k < rows_count; ++k ) {
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
					}
					
					dc.AddRecordToDataStorage( rec1 );
					indexes.Add(rec1.GetIndex());
					dc2.AddRecordToDataStorage( rec2 );
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
				
				
				Console.WriteLine("DC2 reccount:  "+dc2.AllRecordsCount);
				Console.WriteLine("DC reccount:  "+dc.AllRecordsCount);

				// Getting a record
				Console.WriteLine("DCs preloading... ");

				dc.PreLoad( user );
				//dc2.PreLoad( user );

				// Additional records
				Console.WriteLine("Adding some new records... ");
				for ( int i =0; i < 400;  ++i ) {
					var rec = new Record( dc );
					rec.Id = dc.NextId();
					foreach ( var col in dc.Columns) {
						rec[col.Name].Value = "Цитрус "+col.Name+" "+i.ToString()+"AAAAAAAAA";
						rec[col.Name].Type = DataType.STRING;
					}
					
					rec.BuildIndex();
					indexes.Add(rec.GetIndex());
					dc.AddRecordToDataStorage(rec);
				}
				
				chunk_manager.RebuildIndexes( dc.GetOwnerDB() );
				dc.Save();
				dc.PreLoad( user );
				Console.WriteLine("Trying LINQ #1...");
				var aa1 = dc.Select((x,y)=>x);
				
				foreach ( var rec in aa1) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa1.Count()+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa1.Count());
				}

				Console.WriteLine("Trying LINQ #2...");
				var aa2 = dc.Select((x,y)=>x).Where( (x) => {
				                                    	return x.Fields[1].Type == DataType.INT &&
				                                    		(int.Parse(x.Fields[1].Value.ToString()) % 5 == 0);
				                                    }).ToArray();

				foreach ( var rec in aa2) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa2.Count()+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa2.Count());
				}
				
				Console.WriteLine("Trying LINQ #3 (dc & dc2)...");

				var aa3 = (from rec in dc
				           join recb in dc2 on
				           rec.Fields[1].Value.ToString() equals recb.Fields[1].Value.ToString()
				           where rec.Fields[1].Type == DataType.INT && recb.Fields[1].Type == DataType.INT
				           select rec).ToArray();

				foreach ( var rec in aa3 ) {
					Console.WriteLine("Rec:"+rec.Fields[0].Type+"  :  "+rec.Fields[0].Value+" $$"+aa3.Count()+
					                  ":"+rec.Fields[1].Type+"  :  "+rec.Fields[1].Value+" $$"+aa3.Count());
				}
				
				Console.WriteLine("DO YOU WANT TO DROP THIS DB (Y/N)?");
				if ( Console.ReadLine() == "Y" ) {
					db2.Drop( user );
				}
			}
		}
	}
}