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
			
			var cm = new ChunkManager.ChunkManager();
			
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
			}
			
			
			
		}
	}
}
