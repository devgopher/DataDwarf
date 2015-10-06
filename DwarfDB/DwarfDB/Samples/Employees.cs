/*
 * Пользователь: igor.evdokimov
 * Дата: 02.10.2015
 * Время: 16:33
 */
using System;
using System.Linq;
using DwarfDB.DataStructures;
using DwarfDB.ChunkManager;
using System.Collections.Generic;

namespace DwarfDB
{
	/// <summary>
	/// Description of Employees.
	/// </summary>
	partial class Program
	{
		public static void Employees() {
			string db_name= "employees";
			DataBase db = null;
			DataContainer dc_employee = null;
			DataContainer dc_positions = null;
			DataContainer dc_divisions = null;
			var db_dir = Config.Config.Instance.DataDirectory+db_name;
			var cm = new ChunkManager.ChunkManager();
			
			
			// Creating DB structure
			if (DataBase.Exists(db_name)) {
				db = DataBase.LoadFrom( db_name, cm );
			} else {
				db = DataBase.Create( db_name, cm );
				DataContainer.Create( db, "employee" );
				DataContainer.Create( db, "positions" );
				DataContainer.Create( db, "divisions" );
				
				dc_employee = db.GetDataContainer("employee");
				dc_positions = db.GetDataContainer("positions");
				dc_divisions = db.GetDataContainer("divisions");
				
				dc_employee.AddColumn( new Column( DataType.INT, "EmplId" ) );
				dc_employee.AddColumn( new Column( DataType.STRING, "Surname" ) );
				dc_employee.AddColumn( new Column( DataType.STRING, "Name" ) );
				dc_employee.AddColumn( new Column( DataType.INT, "PosId" ) );
				dc_employee.AddColumn( new Column( DataType.INT, "DivId" ) );
				
				dc_positions.AddColumn( new Column( DataType.INT, "PosId" ) );
				dc_positions.AddColumn( new Column( DataType.STRING, "Name" ) );

				dc_divisions.AddColumn( new Column( DataType.INT, "DivId" ) );
				dc_divisions.AddColumn( new Column( DataType.STRING, "Name" ) );
				
				cm.CreateChunk( dc_employee );
				cm.CreateChunk( dc_positions );
				cm.CreateChunk( dc_divisions );
				cm.SaveIndexes();
			}

			// Filling DB structure
			Console.WriteLine("Filling our db...");
			Console.WriteLine("Loading db \""+db_name+"\"");
			Console.WriteLine("Loading container \"employee\"");
			DataContainer dc_employee_load = db.GetDataContainer( "employee" );
			Console.WriteLine("Adding records into \"employee\"");
			
			dc_employee_load.PreLoad();
			
			// viewing existing records
			Console.WriteLine("Now, let's see our employees list: ");
			foreach ( var rec in dc_employee_load.GetRecords() ) {
				Console.WriteLine( String.Format("Name: {0}, Surname: {1}",
				                                 rec["Name"].Value.ToString(),
				                                 rec["Surname"].Value.ToString())  );
			}
			
			
			
			UInt64 rec_id = 110+(UInt64)DateTime.Now.Ticks % 2000;
			
			do  {
				var tmp = new Record( dc_employee_load );
				Console.WriteLine("Enter a new employee name: ");
				var name = Console.ReadLine();
				Console.WriteLine("surname: ");
				var surname = Console.ReadLine();
				
				tmp["Name"].Value = name;
				tmp["Surname"].Value = surname;				
				tmp.Id = ++rec_id;
				
				if (!dc_employee_load.AddRecord(tmp)) {
					Console.WriteLine("Failed! Press \"Y\" - to enter an another employee");
					if (  Console.ReadKey().Key != ConsoleKey.N )
						continue;
					else
						break;
				}
				
				Console.WriteLine("Added successfully! Press \"Y\" - to enter an another employee");
			} while ( Console.ReadKey().Key != ConsoleKey.N );
			dc_employee_load.Save();
			cm.SaveIndexes();
		}
	}
}
