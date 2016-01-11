/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 13:32
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using DwarfDB.AccessFunctions;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// A class for database object
	/// </summary>
	[Serializable][JsonObject]
	public class DataBase : ISerializable, IDisposable, IStructureAccess
	{
		#region ISerializable
		public DataBase( SerializationInfo info, StreamingContext ctxt )
		{
			Name = info.GetString( "Name" );
			inner_dc_dict = (Dictionary<string,DataContainer>)
				(info.GetValue( "inner_dc_dict", typeof(Dictionary<string,DataContainer>)));
			MemStorage = new DwarfDB.Stack.DataStorage( this );
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("Name", Name);
			info.AddValue("inner_dc_dict", inner_dc_dict );
		}
		#endregion
		
		public ChunkManager.ChunkManager chunk_manager;
		
		protected DataBase( string db_name, ChunkManager.ChunkManager _cm, bool is_new_db)
		{
			local_am = new DSAccessManager( this );
			MemStorage = new DwarfDB.Stack.DataStorage( this );
			
			if ( _cm != null )
				chunk_manager = _cm;
			else {
				chunk_manager = new DwarfDB.ChunkManager.ChunkManager();
			}
			Name = db_name;
			chunk_manager.Load( Name, is_new_db );
		}

		public string DbPath {
			get; private set;
		}
		
		public static bool Exists( string db_name ) {
			var cpath = Config.Config.Instance.DataDirectory+db_name+@"/";
			if ( Directory.Exists( cpath ) ) {
				if ( File.Exists(cpath+"db_"+db_name+".dwarf"))
					return true;
			}
			
			return false;
		}
		
		private static void CreateIndexesDw( string db_name ) {
			var stream = File.Create( Config.Config.Instance.DataDirectory+db_name+@"/indexes.dw" );
			stream.Close();
		}
		
		/// <summary>
		/// Creates a new DB
		/// </summary>
		/// <param name="db_name"></param>
		/// <param name="_cm"></param>
		/// <returns></returns>
		public static DataBase Create( string db_name, 
		                              ChunkManager.ChunkManager _cm = null ) {
			var cpath = Config.Config.Instance.DataDirectory+db_name;
			
			if ( !Directory.Exists(cpath) )
				Directory.CreateDirectory(cpath);
			
			var new_db = new DataBase( db_name, _cm, true );
			
			CreateIndexesDw( db_name );
			new_db.chunk_manager.CreateChunk( new_db );
			new_db.DbPath = Config.Config.Instance.DataDirectory+db_name;

			return new_db;
		}
		
		/// <summary>
		/// Loads database from a filesystem
		/// </summary>
		/// <param name="db_name">DB name</param>
		/// <param name="_cm">Chunk manager</param>
		/// <returns></returns>
		public static DataBase LoadFrom( string db_name, 
		                                ChunkManager.ChunkManager _cm ) {
			var cpath = Config.Config.Instance.DataDirectory+db_name;
			if ( Directory.Exists(cpath) ) {
				var db =  new DataBase( db_name, _cm, false );
				db.DbPath = cpath;
				db.UpdateDataContainers();
				return db;
			} else {
				Errors.Messages.DisplayError( "Can't find a directory: "+cpath, "DB loading", "Check DB name or path existance", DateTime.Now );
				return null;
			}
		}
		
		/// <summary>
		/// Drops the DB
		/// </summary>
		public void Drop( User.User user ) {
			// Access check
			if ( this.GetLevel( user ) != Access.AccessLevel.ADMIN ) {
				Errors.Messages.DisplayError(
					Global.StaticResourceManager.GetStringResource( "ACCESS_REASON_DENIED_FOR_THIS_USER" ),
					"",
					"",
					DateTime.Now );
				return;
			}
			
			// Cleaning memory
			this.MemStorage.Clear();
			
			// Cleaning files
			var cpath = Config.Config.Instance.DataDirectory+this.Name;
			if ( Directory.Exists(cpath) ) {
				foreach ( var file in Directory.GetFiles(cpath, "*.dwarf") )
					File.Delete( file );
				foreach ( var file in Directory.GetFiles(cpath, "*.access") )
					File.Delete( file );
				foreach ( var file in Directory.GetFiles(cpath, "*.dw") )
					File.Delete( file );
				Directory.Delete(cpath);
			}
			
			// Destroying this object
			this.Dispose();
		}
		
		#region Access

		private DSAccessManager local_am;
		
		/// <summary>
		/// Adding a new access record for our DB
		/// </summary>
		/// <param name="_user"></param>
		/// <param name="_level"></param>
		public void AddAccess ( User.User _user,
		                       Access.AccessLevel _level ) {
			local_am.AddAccess( _user, _level );
			
			// If we're setting admin rights => we need to set it on all of
			// DCs under this DB
			UpdateDataContainers();
			if ( _level == Access.AccessLevel.ADMIN ) {
				foreach ( DataContainer dc in inner_dc_dict.Values ) {
					dc.AddAccess( _user, _level );
				}
			}			
		}
		
		/// <summary>
		/// Changing an access record for our DB
		/// </summary>
		/// <param name="_user">User</param>
		/// <param name="_new_level">Required access level</param>
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
		
		#region DC cloning
		/// <summary>
		/// Cloning DC from another DB
		/// </summary>
		/// <param name="from">DB-transmitter</param>
		/// <param name="dc_name">DC name to clone</param>
		/// <returns></returns>
		public bool CloneDataContainer( DataBase from, String dc_name ) {
			// TODO: cloning operation with calling an Transmit() function on the other DB
			
			if ( from != null ) {
				if ( inner_dc_dict.ContainsKey( dc_name ) )
					return false;
				
				var cloned_dc = new DataContainer( this, this.Name );
				if (from.Transmit( ref cloned_dc, dc_name )) {
					cloned_dc.AssignOwnerDB( this ); // assigning new owner DB
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Transmitting a cloned DataContainer object to Recipient
		/// </summary>
		/// <param name="clone_dc">Where can we put a new cloned DC?</param>
		/// <param name="dn_name">DC name to clone</param>
		/// <returns></returns>
		protected bool Transmit( ref DataContainer clone_dc, String dn_name ) {
			// TODO: checking permissions,
			foreach ( var dc in inner_dc_dict ) {
				if ( dc.Key == dn_name ) {
					clone_dc = (DataContainer)dc.Value.Clone();
					return true;
				}
			}
			return false;
		}
		#endregion
		
		/// <summary>
		/// Loading and updating data containers list for DB
		/// </summary>
		private void UpdateDataContainers() {
			chunk_manager.ClearAllIndexes();
			chunk_manager.LoadDCIndexes(this);
			chunk_manager.LoadRecordIndexes();
			var indexes = chunk_manager.AllIndexes;
			if ( inner_dc_dict.Count > 0 )
				inner_dc_dict.Clear();
			
			foreach ( var item in indexes ) {
				if ( item.Value.Key is DataContainer )
					inner_dc_dict.Add( item.Value.Value, (DataContainer)(item.Value.Key) );
			}
			
		}
		
		/// <summary>
		/// Adding a new data container to a database
		/// </summary>
		/// <param name="new_dc">DataContainer</param>
		/// <returns></returns>
		public bool AddNewDataContainer( DataContainer new_dc, User.User user ) {
			var user_acc_lvl = GetLevel( user );
			if ( Global.CheckAccess.CheckWriteAccess( this, user  ) ) {
				if ( CheckDCNameUnique( new_dc.Name )) {
					// TODO: loading data new container into stack and file chunks
					inner_dc_dict.Add( new_dc.Name, new_dc );
					MemStorage.Add( new_dc );
					new_dc.AssignOwnerDB(this);
					// Don't try to save new_dc in this method!
					return true;
				} else {
					Errors.Messages.DisplayError(
						String.Format("DataContainer named \"{0}\" already exists in DB!", new_dc.Name),
						"Adding new DC",
						"Check the DB structure or select another name",
						DateTime.Now);
				}
			} else {
				Errors.Messages.DisplayError(
					String.Format( Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_SOME PROBLEMS"), new_dc.Name),
					"Adding new DC",
					"Check the DB structure or select another name",
					DateTime.Now);
			}
			return false;
		}
		
		private DataContainer RetDCAccess( DataContainer dc, User.User user) {
			if ( !Global.CheckAccess.CheckWriteAccess(dc, user) ) {
				Errors.Messages.DisplayError( Global.StaticResourceManager.GetStringResource("ACCESS_REASON_DENIED_FOR_THIS_USER"));
				return null;
			}
			return dc;			
		}
		
		/// <summary>
		/// Getting DataContainer by name
		/// </summary>
		/// <param name="dc_name">DC name</param>
		/// <returns></returns>
		public DataContainer GetDataContainer( string dc_name, User.User user ) {
			if ( dc_name != String.Empty ) {
				foreach ( var k in inner_dc_dict ) {
					if ( k.Key == dc_name ) {
						if ( k.Value != null ) {
							k.Value.AssignOwnerDB(this);
							k.Value.BuildIndex();
							k.Value.LoadRecords( user );	
							return RetDCAccess( k.Value, user );
						}
					}
				}
				
				var chk_dc = chunk_manager.GetDataContainer( dc_name );
				if ( chk_dc != null ) {
					chk_dc.AssignOwnerDB(this);
					chk_dc.LoadRecords( user );
					return RetDCAccess( chk_dc, user );
				}
				// If we hadn't found DC with such name, we should write
				// an error
				
				Errors.Messages.DisplayError( "Can't find a DataContainer with name \""+dc_name+"\"",
				                               "Getting a DataContainer drom DB",
				                               "Please, check a name or create such DataContainer",
				                               DateTime.Now );
				return null;
			} else
				throw new DataBaseException( this, "DataContainer name is empty!" );
		}
		
		/// <summary>
		/// Let's check if the name of a new DC unique in DB?
		/// </summary>
		/// <param name="dc_name"></param>
		/// <returns></returns>
		private bool CheckDCNameUnique( String dc_name ) {
			return !inner_dc_dict.ContainsKey( dc_name ) ;
		}
		
		public string Name {
			get; private set;
		}
		
		[JsonIgnore]
		public ConcurrentDictionary<Index, KeyValuePair<IStructure,string>> Indexes {
			get {
				if ( chunk_manager != null )
					return chunk_manager.AllIndexes;
				return null;
			}
		}
		
		[JsonIgnore]
		public Stack.DataStorage MemStorage {
			get {
				return dbstack;
			} private set {
				dbstack = value;
			}
		}
		
		/// <summary>
		/// Disposing
		/// </summary>
		public void Dispose() {
			dbstack.Clear();
			user_list.Clear();
			inner_dc_dict.Clear();
		}
		
		#region LinkProcessing
		private bool is_link = false;
		private string link_where = null;
		private string link_constant_id = null;
		#endregion

		DwarfDB.Stack.DataStorage dbstack;
		readonly List<User.User> user_list = new List<User.User>();
		readonly Dictionary< String, DataContainer > inner_dc_dict = new Dictionary<String, DataContainer>();
	}
}
