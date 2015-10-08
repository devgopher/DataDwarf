﻿/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 13:32
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// a class for database object
	/// </summary>
	[Serializable][JsonObject]
	public class DataBase : ISerializable
	{
		#region ISerializable
		public DataBase( SerializationInfo info, StreamingContext ctxt )
		{
			Name = info.GetString( "Name" );
			inner_dc_dict = (Dictionary<string,DataContainer>)
				(info.GetValue( "inner_dc_dict", typeof(Dictionary<string,DataContainer>)));
			Stack = new DwarfDB.Stack.DwarfStack( this );
		}
		
		public void GetObjectData(SerializationInfo info, StreamingContext ctxt) {
			info.AddValue("Name", Name);
			info.AddValue("inner_dc_dict", inner_dc_dict );
		}
		#endregion
		
		public ChunkManager.ChunkManager chunk_manager;
		
		protected DataBase( string db_name, ChunkManager.ChunkManager _cm, bool is_new_db)
		{
			Stack = new DwarfDB.Stack.DwarfStack( this );
			
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
			var cpath = Config.Config.Instance.DataDirectory+db_name+@"\";
			if ( Directory.Exists( cpath ) ) {
				if ( File.Exists(cpath+"db_"+db_name+".dwarf"))
					return true;
			}
			
			return false;
		}
		
		private static void CreateIndexesDw( string db_name ) {
			var stream = File.Create( Config.Config.Instance.DataDirectory+db_name+@"\indexes.dw" );
			stream.Close();
		}
		
		public static DataBase Create( string db_name, ChunkManager.ChunkManager _cm ) {
			var cpath = Config.Config.Instance.DataDirectory+db_name;
			
			if ( !Directory.Exists(cpath) )
				Directory.CreateDirectory(cpath);
			
			var new_db = new DataBase( db_name, _cm, true );
			
			CreateIndexesDw( db_name );			
			_cm.CreateChunk( new_db );
			
			return new_db;
		}
		
		public static DataBase LoadFrom( string db_name, ChunkManager.ChunkManager _cm ) {
			var cpath = Config.Config.Instance.DataDirectory+db_name;
			if ( Directory.Exists(cpath) ) {
				var db =  new DataBase( db_name, _cm, false );
				db.DbPath = cpath;
				db.UpdateDataContainers();
				return db;
			} else {
				Errors.ErrorProcessing.Display( "Can't find a directory: "+cpath, "DB loading", "Check DB name or path existance", DateTime.Now );
				return null;
			}
		}
		
		public Dictionary<Index, KeyValuePair<IStructure,string>> Indexes {
			get {
				if ( chunk_manager != null )
					return chunk_manager.AllIndexes;
				return null;
			}
		}
		
		public void Drop() {
			// TODO!!
		}
		
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
		
		#region Connection
		public void Connect( User.User user ) {
			
		}
		#endregion
		
		/// <summary>
		/// Loading and updating data containers list for DB
		/// </summary>
		private void UpdateDataContainers() {
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
		
		public bool AddNewDataContainer( DataContainer new_dc ) {
			if ( CheckDCNameUnique( new_dc.Name )) {
				// TODO: loading data new container into stack and file chunks
				inner_dc_dict.Add( new_dc.Name, new_dc );
				Stack.Push( new_dc );
				new_dc.AssignOwnerDB(this);
				
				new_dc.Save();
				return true;
			} else {
				Errors.ErrorProcessing.Display(
					String.Format("DataContainer named \"{0}\" already exists in DB!", new_dc.Name),
					"Adding new DC",
					"Check the DB structure or select another name",
					DateTime.Now);
			}
			return false;
		}
		
		
		/// <summary>
		/// Getting DataContainer by name
		/// </summary>
		/// <param name="dc_name">DC name</param>
		/// <returns></returns>
		public DataContainer GetDataContainer( string dc_name ) {
			if ( dc_name != String.Empty ) {
				/*foreach ( var k in inner_dc_dict ) {
					if ( k.Key == dc_name ) {
						if ( k.Value != null ) {
							k.Value.AssignOwnerDB(this);
							k.Value.BuildIndex();
						//	k.Value.LoadRecords();
							return k.Value;
						}
					}
				}*/
				
				var chk_dc = chunk_manager.GetDataContainer( dc_name );
				if ( chk_dc != null ) {
					chk_dc.AssignOwnerDB(this);
					chk_dc.LoadRecords();
					return chk_dc;
				}
				// If we hadn't found DC with such name, we should write
				// an error
				
				Errors.ErrorProcessing.Display( "Can't find a DataContainer with name \""+dc_name+"\"",
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
			// TODO: in this function we should check, that
			// a name of new DC, that we introduced as an argument to this func,
			// is not in inner_dc_list.
			return !inner_dc_dict.ContainsKey( dc_name ) ;
		}
		
		public string Name {
			get; private set;
		}
		
		public Stack.DwarfStack Stack {
			get {
				return dbstack;
			} private set {
				dbstack = value;
			}
		}
		
		DwarfDB.Stack.DwarfStack dbstack;
		readonly List<User.User> user_list = new List<User.User>();
		readonly Dictionary< String, DataContainer > inner_dc_dict = new Dictionary<String, DataContainer>();
	}
}
