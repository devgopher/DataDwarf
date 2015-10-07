/*
 * Пользователь: igor.evdokimov
 * Дата: 07.10.2015
 * Время: 16:13
 */
using System;
using DwarfDB;
using DwarfDB.DataStructures;
using NUnit.Framework;

namespace DwarfDB.UnitTests
{
	[TestFixture]
	public class RecordTest
	{
		[TestCase(@"nunit_db", "nunit_container1")]
		public static void CreateRecord( string db_name, string container_name ) {
			var cm = new ChunkManager.ChunkManager();
			var db = DataBase.LoadFrom( db_name, cm );
			var dc = db.GetDataContainer( container_name );
			
			dc.RemoveAllRecords();

			Assert.AreEqual( 0, dc.GetRecords().Count );
			
			// Creating a new record 
			var rec = new Record(dc);
			rec["col1"].Value = "AAABBBCCC";
			rec["col2"].Value = 20.390494;
			
			dc.AddRecord(rec);
			
			dc.Save();
			
			
			
			
		}
	}
}
