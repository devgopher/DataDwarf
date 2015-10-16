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
	public class DummyContainer : Record
	{
		public DummyContainer( DataContainer _owner_dc ) : base( _owner_dc )
		{
		}

		public DummyContainer( ) : base()
		{
		}
		
		static public DummyContainer Create() {
			return new DummyContainer();
		}
	}
}
