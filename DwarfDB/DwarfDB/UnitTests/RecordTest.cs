/*
 * Пользователь: igor.evdokimov
 * Дата: 07.10.2015
 * Время: 16:13
 */
using System;
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
			
			// Creating a new record
			var rec = new Record(dc);
			rec["col1"].Value = "AAABBBCCC";
			rec["col2"].Value = 20.390494;
			rec.BuildIndex();
			dc.AddRecord(rec);
			
			dc.Save();
			
			Assert.IsTrue( dc.GetRecords().Count >= 1 );
		}
		
		[TestCase(@"nunit_db", "nunit_container1")]
		public static void BuildIndex( string db_name, string container_name ) {
			
			var db1 = DataBase.LoadFrom( db_name, null );
			var dc1 = new DataContainer( db1, container_name );
			var rec1 = new Record( dc1 );
			rec1.Id = 102;
			rec1.BuildIndex();			
			var hash_expected = "2D8C1282A16592B40656F06F80D75172";
			
			Assert.AreEqual( hash_expected, rec1.GetIndex().HashCode );
		}
	}
}
