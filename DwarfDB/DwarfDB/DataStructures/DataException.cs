/*
 * Пользователь: igor.evdokimov
 * Дата: 25.08.2015
 * Время: 15:25
 */
using System;
using System.Runtime.Serialization;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// Exception for datastructures
	/// </summary>
	[Serializable]
	public class DataException<T> : Exception
	{
		public DataException()
		{
		}

		public T Object
		{
			get;
			private set;
		}
		
		public DateTime When
		{
			get;
			private set;
		}
		
		public DataException(T _object, string reason) : base(_object.GetType() +":"+ reason)
		{
			Init( _object );
		}
		
		public DataException(string reason) : base(reason)
		{
			Init();
		}

		public DataException(T _object,
		                     string reason,
		                     Exception innerException) : base(_object.GetType() +":"+ reason, innerException)
		{
			Init( _object );
		}
		
		private void Init() {
			When = DateTime.Now.ToLocalTime();
		}

		private void Init(T _object) {
			if ( _object is IStructure || _object is Index )
				Object = _object;
			When = DateTime.Now.ToLocalTime();
		}
		
		// This constructor is needed for serialization.
		protected DataException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	public class IndexException : DataException<Index> {
		public IndexException() : base() {
		}
		
		public IndexException(string reason) : base(reason) {
		}
		
		public IndexException(Index _object, string reason) : base(_object, reason ) {			
		}
	}

	public class DataBaseException : DataException<DataBase> {
		public DataBaseException() : base() {
		}
		
		public DataBaseException(string reason) : base(reason) {
		}
		
		public DataBaseException(DataBase _object, string reason) : base(_object, reason ) {			
		}
	}
}