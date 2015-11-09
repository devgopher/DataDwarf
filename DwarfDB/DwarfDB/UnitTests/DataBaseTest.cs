/*
 * Пользователь: igor.evdokimov
 * Дата: 07.10.2015
 * Время: 12:16
 */
using System;
using NUnit.Framework;
using DwarfDB.ChunkManager;
using DwarfDB.DataStructures;

namespace DwarfDB.UnitTests
{
	[TestFixture]
	public class DataBaseTest
	{
		[Test]
		public static void Create() {
			var cm = new ChunkManager.ChunkManager();
			var db  = DataBase.Create( "nunit_db", cm );
			Assert.AreEqual( true, DataBase.Exists( db.Name ));
		}
		
		[Test]
		public void Drop() {
			var cm = new ChunkManager.ChunkManager();
			var db  = DataBase.Create( "nunit_db_drop", cm );
			User.User user = User.User.New("root", "12345678");
			db.Drop(user);
			Assert.AreEqual( 0, db.Stack.Count);
			Assert.IsFalse( System.IO.Directory.Exists(db.DbPath));
		}
		
		[Test]
		public void CreateContainer() {
			string container_name = "nunit_container1";
			var db  = DataBase.LoadFrom( "nunit_db", null );
			var new_dc = new DataContainer( db, container_name );
			new_dc.AddColumn( new Column( DataType.STRING, "col1" ) );
			new_dc.AddColumn( new Column( DataType.FLOAT, "col2" ) );
			new_dc.Save();
		}
	}
}
