/*
 * Пользователь: Igor.Evdokimov
 * Дата: 25.09.2015
 * Время: 13:07
 */
using System;
using System.Collections.Generic;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// DummyField class - it's class for using instead of NULL
	/// </summary>
	public sealed class DummyField : Field
	{
		private static string dummy_val = "(DUMMY)";
		
		private DummyField( String _name, DataType _type ) : base( _name, _type, dummy_val ){
			
		}
		
		static DummyField() {
			
		}
		
		static public DummyField Create( Field example )
		{
			Instances[ example.Type] = new DummyField( example.Name, example.Type );
			return Instances[ example.Type];
		}
		
		static private Dictionary<DataType, DummyField> Instances =
			new Dictionary<DataType, DummyField>();
		
	}
	
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
		
		static public DummyRecord Create( string own_dc_hash, DataBase own_db ) {
			var rec = new DummyRecord();			
			rec.AssignOwnerDC(DummyContainer.Create( own_db ));
			
			return rec;
		}
	}
}
