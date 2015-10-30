/*
 * Пользователь: igor.evdokimov
 * Дата: 14.10.2015
 * Время: 16:56
 */
using System;
using DwarfDB.DataStructures;
using NUnit.Framework;

namespace DwarfDB.UnitTests
{
	[TestFixture]
	public class DataContainerTest
	{
	/*	[TestCase(@"nunit_db", "nunit_container1", 0, "87A60DBDF68CDCC4E5E831FE554D7874")]		
		public static void GetRecord( string db_name, string container_name, int i, string expected_hash ) {
			DataBaseTest.Create();
			var db1 = DataBase.LoadFrom( db_name, null );
			var dc1 = db1.GetDataContainer( container_name );
			Assert.Less( i, dc1.AllRecordsCount, "Do we have enough records in test DC?" );
			Assert.IsNotNull( dc1.GetRecord( i ) );
			Assert.AreEqual( expected_hash, dc1.GetRecord(i).GetIndex().HashCode );
		}
		*/
	}
}
