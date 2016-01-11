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
		public static void Drop() {
			var cm = new ChunkManager.ChunkManager();
			var db  = DataBase.Create( "nunit_db_drop", cm );
			User.User user = User.User.New("root", "12345678");
			db.AddAccess( user, AccessFunctions.Access.AccessLevel.ADMIN );
			db.Drop(user);
			Assert.AreEqual( 0, db.MemStorage.Count );
			Assert.IsFalse( System.IO.Directory.Exists(db.DbPath));
		}
		
		[Test]
		public static void LoadFromTest() {
			var cm = new ChunkManager.ChunkManager();	
			DataBase.Create( "nunit_db_loadfrom", cm );
			
			var db = DataBase.LoadFrom( "nunit_db_loadfrom", cm );
			
			Assert.IsNotNull(db, "Can't load DB from a filesystem!");
		}
		
		[Test]
		public static void CreateContainer() {
			var user = User.User.New( "root", "12345678");
			string container_name = "nunit_container1";
			var db  = DataBase.LoadFrom( "nunit_db", null );
			var new_dc = new DataContainer( db, container_name );
			new_dc.AddAccess( user, AccessFunctions.Access.AccessLevel.READ_WRITE );
			new_dc.AddColumn( new Column( DataType.STRING, "col1" ), user );
			new_dc.AddColumn( new Column( DataType.FLOAT, "col2" ), user );
			new_dc.BuildIndex();
			new_dc.Save();
		}
	}
}
