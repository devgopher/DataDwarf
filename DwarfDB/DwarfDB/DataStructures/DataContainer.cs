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
using DwarfDB.AccessFunctions;
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
		
		public Column( DataType dt, String _name )
		{
			this.Name = _name;
			this.Type = dt;
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
				if ( value.Length < 256 ) {
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
		
		private string name = String.Empty;
		private DataType type = DataType.UNDEF;
	}
	
	/// <summary>
	/// DataContainer is the base element of
	/// DwarfDB data structure
	/// </summary>
	[Serializable][JsonObject]
	public class DataContainer :IStructure, IEnumerable<Record>, IStructureAccess
	{
		#region LinkProcessing
		private bool is_link = false;
		private string link_where = null;
		private string link_constant_id = null;
		#endregion

		public Int64 Id { get; set; }
		int all_rec_count = -1;
		private List<Record> inner_records = new List<Record>();
		protected Record enum_rec = null;
		protected int position = 0;
		protected DataBase owner_db;
		protected Index current_index;
		
		[JsonConstructor]
		public DataContainer( DataBase _owner_db, string _dc_name )
		{
			Columns = new List<Column>();
			Records = new List<Record>();
			Name = _dc_name;
			owner_db = _owner_db;
			BuildIndex();
			local_am = new DSAccessManager( this );
		}
		
		#region ISerializable
		public DataContainer( SerializationInfo info, StreamingContext ctxt )
		{
			Name = info.GetString( "Name" );
			current_index = new Index( this );
			current_index.DwarfHashCode = info.GetString( "ElementHash" );
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("Name", Name);
			info.AddValue("ElementHash", current_index.DwarfHashCode);
			info.AddValue("OwnerDB", owner_db, typeof(DataBase));
			info.AddValue("Columns", Columns, typeof(List<Column>));
		}
		#endregion
		
		#region Access
		private DSAccessManager local_am;
		
		/// <summary>
		/// Adding a new access record for our DC
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		public void AddAccess ( User.User _user,
		                       Access.AccessLevel _level ) {
			local_am.AddAccess( _user, _level );
		}
		
		/// <summary>
		/// Changing an access record for our DC
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_new_level"></param>
		public void ChangeAccess ( User.User _user,
		                          Access.AccessLevel _new_level ) {
			local_am.ChangeAccess(_user, _new_level );
		}
		
		internal List<Access> GetAccesses() {
			return local_am.GetAccesses();
		}
		
		/// <summary>
		/// Getting an access level for a given user
		/// </summary>
		/// <param name="_user"></param>
		/// <returns></returns>
		public Access.AccessLevel GetLevel( User.User _user ) {
			return local_am.GetLevel( _user );
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
			return tmp.GetHashCode() == this.GetHashCode();
		}
		
		public static bool operator ==(DataContainer a, DataContainer b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
				return true;

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
				return false;
			
			// Return true if the fields matches:
			return ( a.Name == b.Name /*& a.GetOwnerDB().Name == b.GetOwnerDB().Name*/  );
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
		
		private string inner_name = null;
		
		public String Name {
			get {
				return inner_name;
			}
			set {
				if ( inner_name == null ) {
					inner_name = value;
				} else {
					Errors.Messages.DisplayError("DC renaming is not allowed!", "", "", DateTime.Now);
				}
			}
		}
		
		/// <summary>
		/// Create a new DataContainer
		/// </summary>
		/// <param name="_owner_db">Owner DB object</param>
		/// <param name="_name">DataContainer name</param>
		/// <returns>true or false</returns>
		public static bool Create( DataBase _owner_db, String _name, User.User user ) {
			if ( !Global.CheckAccess.CheckWriteAccess(_owner_db, user) ) {
				Errors.Messages.DisplayError( Global.StaticResourceManager.StringManager.GetString("ACCESS_REASON_DENIED_FOR_THIS_USER"));
				return false;
			}
			
			if ( _owner_db == null )
				return false;
			
			var new_dc = new DataContainer( _owner_db, _name );
			
			return ( _owner_db.AddNewDataContainer( new_dc, user ) );
		}

		/// <summary>
		/// Loading all records from chunks
		/// </summary>
		public void LoadRecords( User.User user ) {
			if ( !Global.CheckAccess.CheckReadAccess(this, user) ) {
				Errors.Messages.DisplayError( Global.StaticResourceManager.StringManager.GetString("ACCESS_REASON_DENIED_FOR_THIS_USER"));
				return;
			}
			var own_hash = this.GetIndex().DwarfHashCode;

			var chunk_recs = owner_db.chunk_manager.LoadAllChunks( this.GetIndex().DwarfHashCode );
			foreach ( var rec in chunk_recs ) {
				AddRecordToDataStorage( rec );
			}
		}
		
		/// <summary>
		/// Adding a next couple of records to stack
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		internal bool AddNextCoupleOfRecordsToStack( int number ) {
			var idx_dict = owner_db.Indexes;
			int cntr = 0;
			bool has_new_recs = false;
			foreach ( var idx_entry in idx_dict ) {
				// First of all, we nedd to check if an index of parent element equals to index of our DC
				if ( idx_entry.Value.Value == GetIndex().DwarfHashCode) {
					if ( cntr >= number )
						break;
					var rec = owner_db.chunk_manager.GetRecord( idx_entry.Key );
					if ( rec != null && !(rec is DummyRecord)) {
						has_new_recs = true;
						AddRecordToDataStorage( rec );
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
			
			foreach ( var clmn in _columns ) {
				if ( clmn.Type != DataType.UNDEF ) {
					Columns.Add(clmn);
				} else {
					Errors.Messages.DisplayError( " The column type cannot be UNDEF ",
					                             "DataContainer: "+Name,
					                             "Select any available data type",
					                             DateTime.Now);
					return false;
				}
			}
			
			// Save to a file chunk
			this.Save();
			return true;
		}
		
		public bool Drop( String name ) {
			// TODO
			return false;
		}
		
		/// <summary>
		/// Adds a new column
		/// </summary>
		/// <param name="new_clmn">Column definition</param>
		/// <param name="user">User</param>
		/// <returns></returns>
		public bool AddColumn( Column new_clmn, User.User user ) {
			if ( !Global.CheckAccess.CheckWriteAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't add a column: "+err);
				return false;
			}
			
			foreach ( var col in Columns ) {
				if ( new_clmn.Name == col.Name )
					return false;
			}
			Columns.Add( new_clmn );
			return true;
		}

		/// <summary>
		/// Removes all records
		/// </summary>
		/// <param name="user">User</param>
		public void RemoveAllRecords( User.User user ) {
			if ( !Global.CheckAccess.CheckWriteAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't remove records: "+err);
				return;
			}
			
			var records = GetRecordsInternal();
			foreach ( var rec in records ) {
				this.RemoveRecord( rec, user );
			}
		}
		
		/// <summary>
		/// Removes a record
		/// </summary>
		/// <param name="rem_rec">Record</param>
		/// <param name="user">User</param>
		/// <returns></returns>
		public bool RemoveRecord( Record rem_rec, User.User user ) {
			if ( !Global.CheckAccess.CheckWriteAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't remove records: "+err);
				return false;
			}
			throw new NotImplementedException( "RebuildIndexes and record deletion process in whole should be rethinked!" );
		}
		
		/// <summary>
		/// Adds records to a DataStorage
		/// </summary>
		/// <param name="new_rec"></param>
		/// <returns></returns>
		public void AddRecordToDataStorage( Record new_rec ) {
			if ( new_rec == null )
				return;		
			var tmp_recs = GetRecordsInternal();
					
			new_rec.BuildIndex();
			new_rec.AssignOwnerDC(this);
			owner_db.MemStorage.Add( new_rec );
		}
		
		/// <summary>
		/// Removes column from a datacontainer
		/// </summary>
		/// <param name="rem_clmn">Column to remove</param>
		/// <param name="user">User</param>
		/// <returns></returns>
		public bool RemoveColumn( Column rem_clmn, User.User user ) {
			if ( !Global.CheckAccess.CheckWriteAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't remove a column: "+err);
				return false;
			}
			Columns.Remove( rem_clmn );
			
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
		public void Save() {
			BuildIndex();
			
			// Let's create a chunk if we need it
			GetOwnerDB().chunk_manager.CreateChunk( this );
			
			// Save records from DC
			var recs = GetRecordsInternal();
			
			GetOwnerDB().chunk_manager.CreateChunk(recs, 300);
			GetOwnerDB().chunk_manager.SaveIndexes();
		}
		
		/// <summary>
		/// Load Element from file chunk
		/// </summary>
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
		/// <param name="dirpath"></param>
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
		internal bool AssignOwnerDB( DataBase new_owner ) {
			if ( new_owner != null ) {
				owner_db = new_owner;
				return true;
			}
			return false;
		}
		
		
		/// <summary>
		/// Incapsulating this.Records[i].get
		/// for making some additional operations safely
		/// </summary>
		/// <param name="i">index</param>
		/// <returns></returns>
		internal Record GetRecordInternal( int i ) {
			if ( i >= AllRecordsCount ) {
				Errors.Messages.DisplayError("Argument "+i.ToString()+" is out of range! ", "Getting  record", "", DateTime.Now);
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
		/// Incapsulating this.Records[i].get
		/// for making some additional operations safely
		/// </summary>
		/// <param name="i">index</param>
		/// <param name="user">user</param>
		/// <returns></returns>
		public Record GetRecord( int i, User.User user ) {
			if ( !Global.CheckAccess.CheckReadAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't get records: "+err);
				return null;
			}
			
			if ( i >= AllRecordsCount ) {
				Errors.Messages.DisplayError("Argument "+i.ToString()+" is out of range! ", "Getting  record", "", DateTime.Now);
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
		public List<Record> GetRecords( User.User user ) {
			if ( !Global.CheckAccess.CheckReadAccess( this, user ) ) {
				var err = Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER");
				Errors.Messages.DisplayError("Can't get records: "+err);
				return null;
			}
			return Records.ToList();
		}
		
		/// <summary>
		/// Getting Records  ( no authentification! )
		/// </summary>
		/// <returns></returns>
		internal List<Record> GetRecordsInternal() {
			return Records.ToList();
		}
		
		/// <summary>
		/// Outputs full amount of records,
		/// that has this DC as a parent element
		/// </summary>
		public int AllRecordsCount {
			get {
				if ( all_rec_count == -1 ) {
					var db = this.GetOwnerDB();
					var indexes = db.Indexes;

					all_rec_count = (indexes.Where( ( idxs ) => {
					                               	return idxs.Value.Value == GetIndex().DwarfHashCode;
					                               })).Count();
				}
				return all_rec_count;
			}
		}
		
		internal bool  GetRecordsFromChunk( int chunk_number = 0 ) {
			var couple = owner_db.chunk_manager.LoadChunk( chunk_number, GetIndex().DwarfHashCode );
			foreach ( var rec in couple )
				AddRecordToDataStorage( rec );
			
			return couple.Any();
		}
		
		/// <summary>
		/// Method for loading all DC content into the stack
		/// </summary>
		internal void PreLoad( User.User user ) {
			int pos = 0;
			int all_rec_cnt = this.AllRecordsCount;
			if ( all_rec_count > 0 )
				GetRecordInternal(0);
			while ( pos < all_rec_count ) {
				GetRecordsFromChunk( pos );
				++pos;
			}
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
				ret_dc.AddRecordToDataStorage( tmp_rec );
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
		
		public List<Column> Columns {
			get; private set;
		}

		[JsonIgnore]
		protected List<Record> Records {
			get {
				DwarfDB.Stack.DataStorage stack = GetOwnerDB().MemStorage;
				
				if ( stack != null ) {
					inner_records = stack.GetRecords( this ).ToList();
				}
				
				return inner_records;
			}
			private set {
			}
		}
		
		/// <summary>
		/// Next ID
		/// </summary>
		/// <returns></returns>
		public Int64 NextId() {
			Int64 next_id = 1;
			var tmp_recs = new HashSet<Record>(Records);
			
			checked {
				foreach ( var rec in tmp_recs ) {
					if ( rec.Id > next_id )
						next_id = rec.Id;
				}
				
				++next_id;
			}
			
			return next_id;
		}
	}
}
