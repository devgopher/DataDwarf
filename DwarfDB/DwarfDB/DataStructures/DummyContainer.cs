/*
 * Пользователь: Igor.Evdokimov
 * Дата: 25.09.2015
 * Время: 13:07
 */
using System;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// DummyContainer class - it's class for using instead of NULL
	/// </summary>
	public class DummyContainer : DataContainer
	{
		public DummyContainer( DataBase _owner_db ) : base ( _owner_db, "dummy" )
		{
		}
		
		static public DummyContainer Create( DataBase _owner_db ) {
			return new DummyContainer( _owner_db );
		}
	}
}
