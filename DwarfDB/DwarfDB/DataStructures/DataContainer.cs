/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 22:51
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DwarfDB.DataStructures
{
	public enum DataType {
		UNDEF,
		INT,
		FLOAT,
		CHAR,
		STRING,
		BINARY,
		DATETIME
	}
	
//	[JsonObject]
	public class Column : ISerializable {
		
		public Column()
		{
		}
		
		#region ISerializable
		public Column( SerializationInfo info, StreamingContext ctxt )
		{
			Name = info.GetString( "Name" );
			Type = (DataType)info.GetValue( "Type", typeof(DataType));
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("Name", Name);
			info.AddValue("Type", Type, typeof(DataType));
		}
		
		#endregion
		
		public String Name {
			get {
				return name;
			}
			set {
				if ( value.Length < 32 ) {
					name = value;
				}
			}
		}
		
		public DataType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
		
		private string name = "";
		private DataType type = DataType.UNDEF;
	}
	
	/// <summary>
	/// DataContainer is the base element of
	/// DwarfDB data structure
	/// </summary>
	[Serializable][JsonObject]
	public class DataContainer :IStructure, IEnumerable<Record>
	{
		[JsonConstructor]
		public DataContainer( DataBase _owner_db, string _dc_name )
		{
			Columns = new List<Column>();
			Records = new List<Record>();
			Name = _dc_name;
			owner_db = _owner_db;
			BuildIndex();
		}
		#region ISerializable
		public DataContainer( SerializationInfo info, StreamingContext ctxt )
		{
			Name = info.GetString( "Name" );
			current_index = new Index( this );
			current_index.HashCode = info.GetString( "ElementHash" );
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("Name", Name);
			info.AddValue("ElementHash", current_index.HashCode);
			info.AddValue("OwnerDB", owner_db, typeof(DataBase));
			info.AddValue("Columns", Columns, typeof(List<Column>));
		}
		#endregion
		
		#region Object
		public override bool Equals( System.Object obj ) {
			var tmp = obj as DataContainer;
			if ( tmp == null )
				return false;
			if ( this.GetOwnerDB() == null || tmp.GetOwnerDB() == null )
				return false;
			if ( tmp.Name == this.Name &&
			    tmp.GetOwnerDB().Name == this.GetOwnerDB().Name )
				return true;
			if ( tmp.GetHashCode() == this.GetHashCode() )
				return true;
			return false;
		}
		
		public static bool operator ==(DataContainer a, DataContainer b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}
			
			if ( a.Name == b.Name /*& a.GetOwnerDB().Name == b.GetOwnerDB().Name*/  )
				return true;

			// Return true if the fields matches:
			return false;
		}
		
		public static bool operator !=(DataContainer a, DataContainer b)
		{
			// If both are null, or both are same instance, return false.
			if (System.Object.ReferenceEquals(a, b))
			{
				return false;
			}

			// If one is null, but not both, return true.
			if (((object)a == null) || ((object)b == null))
			{
				return true;
			}
			
			if ( a.GetOwnerDB() == null || b.GetOwnerDB() == null )
				return true;
			
			if ( a.Name == b.Name && a.GetOwnerDB().Name == b.GetOwnerDB().Name  )
				return false;

			return true;
		}
		
		public override int GetHashCode() {
			return base.GetHashCode(); // WARNING: it may cause some problems!
		}
		
		#endregion
		
		
		public String Name {
			get; private set;
		}
		
		/// <summary>
		/// Create new DataContainer
		/// </summary>
		/// <param name="_owner_db">Owner DB object</param>
		/// <param name="_name">DataContainer name</param>
		/// <returns>true or false</returns>
		public static bool Create( DataBase _owner_db, String _name ) {
			if ( _owner_db == null )
				return false;
			
			var new_dc = new DataContainer( _owner_db, _name );
			return ( _owner_db.AddNewDataContainer( new_dc ) );
		}

		/// <summary>
		/// Loading all records from chunks
		/// </summary>
		public void LoadRecords() {
			var own_hash = this.GetIndex().HashCode;

			var chunk_recs = owner_db.chunk_manager.LoadAllChunks( this.GetIndex().HashCode );
			foreach ( var rec in chunk_recs ) {
				AddRecord( rec );
			}
		}
		
		/// <summary>
		/// Adding a next couple of records to stack 
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public bool AddNextCoupleOfRecords( int number = 20 ) {
			var idx_dict = owner_db.Indexes;
			int cntr = 0;
			bool has_new_recs = false;
			foreach ( var idx_entry in idx_dict ) {
				// First of all, we nedd to check if an index of parent element equals to index of our DC
				if ( idx_entry.Value.Value == GetIndex().HashCode) {
					if ( cntr >= number )
						break;
					var rec = owner_db.chunk_manager.GetRecord( idx_entry.Key );
					if ( rec != null && !(rec is DummyRecord)) {
						has_new_recs = true;
						AddRecord(rec);
						++cntr;
					}
				}
			}
			
			return has_new_recs;
		}
				
		/// <summary>
		/// Create new DataContainer
		/// </summary>
		/// <param name="new_name">DataContainer name</param>
		/// <param name="_columns">columns</param>
		/// <returns>true or false</returns>
		public bool Create( String new_name, Column[] _columns ) {
			Name = new_name;
			
			// TODO : Check for existance!
			
			foreach ( var clmn in _columns ) {
				if ( clmn.Type != DataType.UNDEF ) {
					Columns.Add(clmn);
				} else {
					Errors.ErrorProcessing.Display( " The column type cannot be UNDEF ",
					                               "DataContainer: "+Name,
					                               "Select any available data type",
					                               DateTime.Now);
					return false;
				}
			}
			
			// Save to file chunk
			this.Save();
			return true;
		}
		
		public bool Delete( String name ) {
			// TODO
			return false;
		}
		
		public bool AddColumn( Column new_clmn ) {
			foreach ( var col in Columns ) {
				if ( new_clmn.Name == col.Name )
					return false;
			}
			Columns.Add( new_clmn );
			return true;
		}

		public bool AddRecord( Record new_rec ) {
			if ( new_rec == null )
				return false;
			foreach ( var rec in Records ) {
				if ( rec.Id == new_rec.Id ) {
					new_rec = null; // to avoid another operations with this record
					return false;
				}
			}
			new_rec.BuildIndex();
			new_rec.OwnerDC = this;
			// TODO: Add data to DataStack and file chunks
			owner_db.Stack.Push( new_rec );
			
			return true;
		}
		
		public bool RemoveColumn( Column new_clmn ) {
			Columns.Remove( new_clmn );
			
			return false;
		}

		public int ColumnsCount( ) {
			return Columns.Count;
		}

		public int StackRecordsCount( ) {
			return Records.Count;
		}
		
		/// <summary>
		/// Building an index for element
		/// </summary>
		public void BuildIndex() {
			current_index = new Index( this );
		}
		
		/// <summary>
		/// Save to file chunk
		/// </summary>
		/// <param name="filepath"></param>
		public void Save( ) {
			// TODO!!
		}
		
		/// <summary>
		/// Load Element from file chunk
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		public void Load( Index index ){
			// TODO!!
		}
		
		/// <summary>
		/// Setting up a transaction for this element
		/// </summary>
		/// <param name="InTransaction"></param>
		public void SetTransaction(DwarfDB.Transactions.DwarfTransaction InTransaction) {
			;
		}
		
		/// <summary>
		/// Load DataContainer from file chunk directory
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		public void LoadFromChunkDir( string dirpath, Index index ) {
			// TODO
		}
		
		/// <summary>
		/// Getting an owner database object
		/// </summary>
		/// <returns></returns>
		public DataBase GetOwnerDB() {
			return owner_db;
		}
		
		/// <summary>
		/// Assinging owner database object
		/// </summary>
		/// <returns></returns>
		public bool AssignOwnerDB( DataBase new_owner ) {
			if ( new_owner != null ) {
				owner_db = new_owner;
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Incapsulating this.Records[i]
		/// for making some additional operations safely
		/// </summary>
		/// <param name="i">index</param>
		/// <returns></returns>
		public Record GetRecord( int i ) {
			if ( i >= AllRecordsCount ) {
				Errors.ErrorProcessing.Display("Argument "+i.ToString()+" is out of range! ", "Getting  record", "", DateTime.Now);
				return new DummyRecord( this );
			}
			
			// Looking in stack
			if ( i < Records.Count )
				return Records[i];
			// Still didn't found ? Let's seek in chunks...
			int start_position = 0;
			
			while ( Records.Count <=  i ) {
				GetRecordsFromChunk( start_position );
				start_position += 1;
			}
			return Records[i];
		}
		
		/// <summary>
		/// Getting Records
		/// </summary>
		/// <returns></returns>
		public List<Record> GetRecords() {
			return this.Records;
		}
		
		/// <summary>
		/// Outputs full amount of records,
		/// that has this DC as a parent element
		/// </summary>
		public int AllRecordsCount {
			get {
				var db = this.GetOwnerDB();
				var indexes = db.Indexes;

				int cntr = indexes.Where( ( idxs ) => {
				                         	return idxs.Value.Value == GetIndex().HashCode;
				                         } ).Count();
				
				return cntr;
			}
		}
		
		public bool  GetRecordsFromChunk( int chunk_number = 0 ) {
			var couple = owner_db.chunk_manager.LoadChunk( chunk_number,  this.GetIndex().HashCode );
			foreach ( var rec in couple )
				AddRecord( rec );
			
			return couple.Any();
		}
		
		#region IEnumerable
		public Record this[int pos] {
			get {
				int rec_count = StackRecordsCount();
				if ( pos >= rec_count || pos < 0  ) {
					// try to load from chunks if the stack doesn't contain needed element
					for ( int i = rec_count; i < pos; ++i ) {
						GetRecordsFromChunk( pos );
					}
				}
				
				//throw new DataException<DataContainer>(this, "Index is out of bounds!");
				return this.Records[pos];
			}
			set {
				if ( pos <= StackRecordsCount() && pos >= 0 ) {
					Records[pos] = value;
				} else {
					throw new DataException<DataContainer>(this, "Index is out of bounds!");
				}
			}
		}
		
		IEnumerator<Record> IEnumerable<Record>.GetEnumerator() {
			return (IEnumerator<Record>)GetEnumerator();
		}		
		
		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator)GetEnumerator();
		}
		
		public Record GetEnumerator() {
			if ( enum_rec == null ) {
				enum_rec = new Record( this );
			}
			enum_rec.Reset();
			return enum_rec;
		}
		#endregion

		#region Cloning
		public IStructure Clone() {
			// TODO!!
			var ret_dc = new DataContainer( GetOwnerDB(), this.Name );
			ret_dc.Create( this.Name, this.Columns.ToArray() );
			ret_dc.Name = this.Name;
			
			
			foreach ( var rec in Records ) {
				var tmp_rec = (Record)rec.Clone();
				tmp_rec.AssignOwnerDC( this );
				ret_dc.AddRecord( tmp_rec );
				if ( tmp_rec != null )
					ret_dc.BuildIndex();
			}

			return ret_dc;
		}
		#endregion
		
		/// <summary>
		/// Getting an index for element
		/// </summary>
		/// <returns></returns>
		public Index GetIndex() {
			return current_index;
		}
		
		/// <summary>
		/// Element id
		/// </summary>
		public UInt64 Id { get; set; }
		
		public List<Column> Columns {
			get; private set;
		}

		[JsonIgnore]
		protected List<Record> Records {
			get {
				DwarfDB.Stack.DwarfStack stack = GetOwnerDB().Stack;
				
				if ( stack != null ) {
					inner_records = stack.GetRecords( this );
				}
				
				return inner_records;
			}
			private set {
			}
		}

		private List<Record> inner_records = new List<Record>();
		protected Record enum_rec = null;
		protected int position = 0;
		protected DataBase owner_db;
		protected Index current_index;
	}
}
