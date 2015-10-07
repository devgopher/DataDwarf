﻿/*
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
		public void Create() {
			var cm = new ChunkManager.ChunkManager();
			var db  = DataBase.Create( "nunit_db", cm );
			Assert.AreEqual( true, DataBase.Exists( db.Name ));
		}
		
		[Test]
		public void CreateContainer() {
			string container_name = "nunit_container1";
			
			Create();
			var cm = new ChunkManager.ChunkManager();
			var db  = DataBase.LoadFrom( "nunit_db", cm );
			var new_dc = new DataContainer( db, container_name );
			new_dc.AddColumn( new Column( DataType.STRING, "col1" ) );
			new_dc.AddColumn( new Column( DataType.FLOAT, "col2" ) );
			cm.SaveIndexes();
			new_dc.Save();		
		}
	}
}