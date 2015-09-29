/*
 * Пользователь: Igor.Evdokimov
 * Дата: 25.09.2015
 * Время: 13:07
 */
using System;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// DummyRecord class - it's class for using instead of NULL
	/// </summary>
	public class DummyRecord : Record
	{
		public DummyRecord( DataContainer _owner_dc ) : base( _owner_dc )
		{
		}

		public DummyRecord( ) : base()
		{
		}
		
		static public DummyRecord Create() {
			return new DummyRecord();
		}
	}
}
