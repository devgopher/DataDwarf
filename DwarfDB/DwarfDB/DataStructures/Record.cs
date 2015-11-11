/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 22:54
 */
using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// Class for field of Record
	/// </summary>
	[JsonObject][Serializable]
	public class Field {
		[JsonConstructor]
		public Field( String _name, DataType _type, Object _value ) {
			Value = _value;
			Type = _type;
			Name = _name;
		}
		
		public DataType Type {
			get;set;
		}

		public String Name {
			get; set;
		}
		
		public Object Value {
			get {
				return _inner_value;
			}
			set {
				_inner_value = value;
			}
		}

		private Object _inner_value;
		
		public Field Clone() {
			var new_field = new Field( Name, Type , Value );
			return new_field;
		}
		
		private readonly Column own_column = null;
	}
	
	/// <summary>
	/// Record is the element of DataContainer
	/// </summary>
	[JsonObject][Serializable]
	public class Record : IStructure, IEnumerator<Record>
	{
		public Record() {
			
		}
		[JsonConstructor]
		public Record( DataContainer _owner_dc )
		{
			Fields = new List<Field>();
			OwnerDC = _owner_dc;
			FillFields();
			BuildIndex();
		}
		
		private void FillFields() {
			if ( OwnerDC != null ) {
				foreach ( var col in OwnerDC.Columns ) {
					Fields.Add( new Field( col.Name, col.Type, null ));
				}
			}
		}
		
		#region ISerializable
		public Record( SerializationInfo info, StreamingContext ctxt )
		{
			//Name = info.GetString( "Name" );
			//current_index = new Index( this );
			//current_index.HashCode = info.GetString( "ElementHash" );
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("ElementHash", current_index.HashCode);
		}
		
		#endregion
		
		/// <summary>
		/// Save changes to file chunk
		/// </summary>
		public void Save() {
			var cm = OwnerDC.GetOwnerDB().chunk_manager;
			this.BuildIndex();
			if ( cm != null ) {
				cm.SaveRecord(this);
			}
		}
		
		/// <summary>
		/// Load Element from file chunk
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		public void Load( Index index ){
			// TODO
		}
		
		/// <summary>
		/// Setting up a transaction for this element
		/// </summary>
		/// <param name="InTransaction"></param>
		public void SetTransaction(DwarfDB.Transactions.DwarfTransaction InTransaction) {
			// TODO
		}
		
		/// <summary>
		/// Getting an index for element
		/// </summary>
		/// <returns></returns>
		public Index GetIndex() {
			return current_index;
		}
		
		/// <summary>
		/// Building an index for element
		/// </summary>
		public void BuildIndex() {
			current_index = new Index( this );
		}
		
		/// <summary>
		/// Destroying an index for an element ( for deletion )
		/// </summary>
		public void DestroyIndex() {
			if ( current_index != null ) {
				current_index = null;
			}
		}
		
		/// <summary>
		/// Load Record from file chunk directory
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		public void LoadFromChunkDir( string dirpath, Index index ) {
			// TODO
		}
		
		/// <summary>
		/// Element id
		/// </summary>
		public Int64 Id { get; set; }
		
		/// <summary>
		/// List of fields
		/// </summary>
		public List< Field > Fields {
			get; private set;
		}
		
		private Field FindField(string field_name) {
			if (field_name != String.Empty) {
				foreach ( var fld in Fields ) {
					if ( fld.Name == field_name ) {
						return fld;
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Getting fields and their values
		/// </summary>
		public Field this[string field_name] {
			get {
				var f_field = FindField( field_name );
				if ( f_field == null ) {
					Errors.ErrorProcessing.Display(
						"There're no field \""+field_name+"\" in your record! ",
						"Fetching the record",
						"Please, specify the field name",
						DateTime.Now
					);
				}
				return f_field;
			}
			
			set {
				var ff = FindField( field_name );
				if ( ff != null ) {
					ff.Value = value;
				}
			}
		}
		
		#region IEnumerator
		object IEnumerator.Current {
			get {
				return Current;
			}
		}
		
		Record IEnumerator<Record>.Current {
			get {
				return Current;
			}
		}
		
		Record Current {
			get {
				try {
					if ( position >= 0 )
						return OwnerDC.GetRecordInternal( position );
					else
						return new DummyRecord(null);
				} catch ( DataException<Record> de ) {
					throw new InvalidOperationException( de.Message, de );
				}
			}
		}
		
		public void Reset() {
			position = -1;
		}
		
		public bool MoveNext() {
			position++;
			if ( (OwnerDC.StackRecordsCount() - position < 2 ) &&
			    OwnerDC.StackRecordsCount()< OwnerDC.AllRecordsCount) {
				var diff = OwnerDC.AllRecordsCount - OwnerDC.StackRecordsCount();
				if ( diff < 30 )
					OwnerDC.AddNextCoupleOfRecordsToStack(diff);
				else
					OwnerDC.AddNextCoupleOfRecordsToStack(30);
			}
			return ( position < OwnerDC.AllRecordsCount );
		}
		#endregion
		
		#region IDisposable
		public void Dispose() {
			Save();
		}
		#endregion
		
		#region Cloning
		public IStructure Clone() {
			var ret_rec = new Record( OwnerDC );
			ret_rec.position = -1;

			foreach ( var field in Fields ) {
				ret_rec.Fields.Add( field );
			}
			
			return ret_rec;
		}
		#endregion
		public DataContainer OwnerDC {get; set;}
		public void AssignOwnerDC( DataContainer _owner_dc ) {
			OwnerDC = _owner_dc;
		}
		
		protected Index current_index;
		protected int position = -1;
	}
}
