/*
 * Пользователь: Igor.Evdokimov
 * Дата: 07.09.2015
 * Время: 10:53
 */
using System;
using System.IO;
using DwarfDB.DataStructures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
//http://www.newtonsoft.com/json/help/html/Introduction.htm
namespace DwarfDB.ChunkManager
{
	
	/// <summary>
	/// Chunk element structure class
	/// </summary>
	internal class InnerChunkElement {
		public InnerChunkElement() {
			NeedToRemove = 0;
		}
		
		public enum ElemType {
			RECORD = 0,
			DATACONTAINER = 1,
			DB = 2,
			REMOVED = 3
		}
		
		public string ElemDB {
			get; set;
		}
		
		public string ElemParentName {
			get; set;
		}

		public string ElemParentHash {
			get; set;
		}
		
		public ElemType ElementType {
			get; set;
		}
		
		public string ElementName {
			get; set;
		}
		
		public string ElementHash {
			get; set;
		}
		
		public string Contents {
			get; set;
		}
		
		/// <summary>
		/// This is special marker for postponed remove this entry
		/// </summary>
		public ushort NeedToRemove {
			get; set;
		}
	}
	
	/// <summary>
	/// A class for writing and formatting DB chunks
	/// </summary>
	public static class ChunkFormat
	{
		static ChunkFormat(  )
		{
			config = Config.Config.Instance;
			json_serializer = new JsonSerializer();
			MaxChunkSize = 1048576;
		}
		
		public static int MaxChunkSize {
			get;
			private set;
		}
		
		/// <summary>
		/// Creating a new chunk file in multithread mode
		/// </summary>
		/// <param name="filepath"></param>
		public static bool CreateNewFile( string filepath ) {
			if ( !File.Exists(filepath) ) {
				var fs = File.Create(filepath);
				fs.Close();
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Adding a new item to a chunk
		/// </summary>
		/// <param name="filepath">Path to a chunk file</param>
		/// <param name="db">Database</param>
		public static void AddItem( string filepath, DataBase db ) {
			if ( db != null ) {
				var elem= new InnerChunkElement();
				var sw = new StreamWriter( File.Open( filepath, FileMode.Append ) );
				using (var json_writer = new JsonTextWriter(sw)) {
					elem.ElementType = InnerChunkElement.ElemType.DB;
					elem.ElemDB = null;
					elem.ElemParentName = null;
					elem.ElemParentHash = null;
					elem.ElementName = (db).Name;
					elem.Contents = JsonConvert.SerializeObject(db, Formatting.Indented);

					sw.Write(JsonConvert.SerializeObject(elem, Formatting.Indented));
				}
			}
		}
		
		/// <summary>
		/// Saving an existant item to a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to a file</param>
		/// <param name="ds">datastructure</param>
		public static void SaveItemContents( string filepath, IStructure ds ) {
			var elem= new InnerChunkElement();
			if ( ds != null ) {
				var sw_read = new StreamReader( File.Open( filepath, FileMode.Open ) );
				InnerChunkElement icm = null;
				
				using ( var json_reader = new JsonTextReader(sw_read) ) {
					icm = FindItem( json_reader, ds.GetIndex().HashCode );
				}
				
				if ( icm == null )
				{
					Errors.Messages.DisplayError("Can't find such item!", "Saving an item", "", DateTime.Now);
					return;
				}
				
				var sw_write = new StreamWriter( File.Open( filepath, FileMode.Append ) );
				using (var json_writer = new JsonTextWriter(sw_write)) {
					if ( ds is Record ) {
						icm.Contents = JsonConvert.SerializeObject(ds, Formatting.Indented );
					} else if ( ds is DataContainer ) {
						icm.Contents = JsonConvert.SerializeObject(ds as DataContainer, Formatting.Indented);
					}
					elem.ElementHash = ds.GetIndex().HashCode;
					sw_write.Write(JsonConvert.SerializeObject(elem, Formatting.Indented));
				}
			} else {
				Errors.Messages.DisplayError( "DataStructure object is null!", "", "", DateTime.Now);
			}
		}
		
		/// <summary>
		/// Adding a new item to a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to a file</param>
		/// <param name="ds">datastructure</param>
		public static void AddItem( string filepath, IStructure ds ) {
			if ( ds != null ) {
				var sw = new StreamWriter( File.Open( filepath, FileMode.Append ) );
				using (var json_writer = new JsonTextWriter(sw)) {
					var elem= new InnerChunkElement();
					if ( ds is Record ) {
						elem.ElementType = InnerChunkElement.ElemType.RECORD;
						elem.ElemParentName = ((Record)ds).OwnerDC.Name;
						elem.ElemParentHash = ((Record)ds).OwnerDC.GetIndex().HashCode;
						elem.Contents = JsonConvert.SerializeObject(ds, Formatting.Indented );
					} else if ( ds is DataContainer ) {
						elem.ElementType = InnerChunkElement.ElemType.DATACONTAINER;
						elem.ElemDB = ((DataContainer)ds).GetOwnerDB().Name;
						elem.ElemParentName = null;
						elem.ElemParentHash = null;
						elem.ElementName = ((DataContainer)ds).Name;
						var ds_sec = (DataContainer)ds;
						elem.Contents = JsonConvert.SerializeObject(ds_sec, Formatting.Indented);
					}
					elem.ElementHash = ds.GetIndex().HashCode;
					sw.Write(JsonConvert.SerializeObject(elem, Formatting.Indented));
				}
			} else {
				Errors.Messages.DisplayError( "DataStructure object is null!", "", "", DateTime.Now);
			}
		}
		
		/// <summary>
		/// Getting an item from a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to a chunk file</param>
		/// <param name="idx">item index</param>
		public static IStructure GetItem( string filepath, Index idx ) {
			return GetItem( filepath, idx.HashCode );
		}
		
		/// <summary>
		/// Getting an item from a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to file</param>
		/// <param name="hash">hash</param>
		public static IStructure GetItem( string filepath, string hash ) {
			var tmp_path = CreateTemporaryFile( filepath );
			IStructure ret_item = null;
			using (var json_reader = new JsonTextReader(new StreamReader(File.Open( tmp_path, FileMode.Open )))) {
				json_reader.SupportMultipleContent = true;
				ret_item = GetItemInfile( json_reader, hash );
			}
			DeleteTemporaryFile( tmp_path );	
			return ret_item;
		}
		
		/// <summary>
		/// Getting an item from a chunk file in a multithread mode
		/// </summary>
		/// <param name="json_reader">JSON text reader/param>
		/// <param name="idx">item index</param>
		private static IStructure GetItemInfile( JsonTextReader json_reader, Index idx ) {
			InnerChunkElement inner = FindItem( json_reader, idx );
			if ( inner != null ) {
				if ( inner.NeedToRemove == 0 ) { // If we neeed not remove this element
					var obj_json = inner.Contents;
					if ( inner.ElementType == InnerChunkElement.ElemType.DATACONTAINER ) {
						var ret_dc = JsonConvert.DeserializeObject<DataContainer>( obj_json );
						return ret_dc;
					} else {
						var ret_rec = JsonConvert.DeserializeObject<Record>( obj_json );
						return ret_rec;
					}
				} else
					return null;
			} else
				return null;
		}
		
		private static Random randg = new Random(DateTime.Now.Millisecond);
        private static object randg_sync_object = new object();

        /// <summary>
        /// A random generation method for
        /// usage in multithread environment
        /// </summary>
        /// <returns></returns>
        private static int GetNextRandom() {
            lock ( randg_sync_object )
            {
                return randg.Next();
            }
        }
		
		private static string CreateTemporaryFile( string orig_filepath ) {
			string tmp_filepath = orig_filepath + ".tmp" + randg.Next().ToString();

			while ( File.Exists(tmp_filepath) ) {
				tmp_filepath += GetNextRandom().ToString();
			}
			File.Copy( orig_filepath, tmp_filepath );
			return tmp_filepath;
		}
		
		private static void DeleteTemporaryFile( string tmp_filepath ) {
			if ( File.Exists(tmp_filepath) ) {
				File.Delete(tmp_filepath);
			}

		}
		
		/// <summary>
		/// Getting all records from a chunk
		/// </summary>
		/// <param name="filepath">path to file</param>
		/// <param name="dc_hash">DataContainer hash</param>
		public static List<Record> GetRecordsInFile( string filepath, string dc_hash ) {
			var ret = new List<Record>();
			
			var tmp_path = CreateTemporaryFile( filepath );
			
			FileStream fstream = File.Open( tmp_path, FileMode.Open );
			
			using (var json_reader = new JsonTextReader(new StreamReader(fstream))) {
				json_reader.SupportMultipleContent = true;
				var item = new InnerChunkElement();
				while ( json_reader.Read() ) {
					item = json_serializer.Deserialize<InnerChunkElement>( json_reader );
					if ( item.ElementType == InnerChunkElement.ElemType.RECORD ) {
						if ( item.ElemParentHash == dc_hash ) {
							var ret_rec = JsonConvert.DeserializeObject<Record>( item.Contents );
							ret.Add(ret_rec);
						}
					}
				}
			}

			DeleteTemporaryFile( tmp_path );
			return ret;
		}
		
		/// <summary>
		/// Getting a DataContainer from the chunk
		/// </summary>
		/// <param name="filepath">path to file</param>
		/// <param name="dc_name">DC name</param>
		public static DataContainer GetDCInFile( string filepath, string dc_name ) {
			
			var tmp_path = CreateTemporaryFile( filepath );
			
			using (var json_reader = new JsonTextReader(new StreamReader(File.Open( tmp_path, FileMode.Open )))) {
				json_reader.SupportMultipleContent = true;
				var item = new InnerChunkElement();
				while ( json_reader.Read() ) {
					item = json_serializer.Deserialize<InnerChunkElement>( json_reader );
					if ( item.ElementType == InnerChunkElement.ElemType.DATACONTAINER ) {
						if ( item.ElementName == dc_name ) {
							var ret_dc = JsonConvert.DeserializeObject<DataContainer>( item.Contents );
							ret_dc.Name = dc_name;
							ret_dc.BuildIndex();
							return ret_dc;
						}
					}
				}
			}
			
			DeleteTemporaryFile( tmp_path );
			return null;
		}
		
		/// <summary>
		/// Getting an item from a chunk file in multithread mode
		/// </summary>
		/// <param name="json_reader">JSON reader</param>
		/// <param name="hash">item index</param>
		private static IStructure GetItemInfile( JsonTextReader json_reader, string hash ) {
			InnerChunkElement inner = FindItem( json_reader, hash );
			if ( inner != null ) {
				if ( inner.NeedToRemove == 0 ) { // If we neeed not remove this element
					var obj_json = inner.Contents;
					if ( inner.ElementType == InnerChunkElement.ElemType.DATACONTAINER ) {
						var ret_dc = JsonConvert.DeserializeObject<DataContainer>( obj_json );
						return ret_dc;
					} else {
						var ret_rec = JsonConvert.DeserializeObject<Record>( obj_json );
						return ret_rec;
					}
				} else
					return null;
			} else
				return null;
		}
		
		/// <summary>
		/// Getting several elements from a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to file</param>
		/// <param name="idx_arr">indexes array</param>
		public static IStructure[] GetItemRange( string filepath, Index[] idx_arr ) {
			var ret_arr = new IStructure[0];

			var tmp_path = CreateTemporaryFile( filepath );
			
			using (var json_reader = new JsonTextReader(new StreamReader(File.Open( tmp_path, FileMode.Open )))) {
				json_reader.SupportMultipleContent = true;
				foreach ( var index in idx_arr ) {
					var elem = GetItemInfile( json_reader, index );
					if ( elem != null ) {
						Array.Resize( ref ret_arr, ret_arr.Length + 1 );
						ret_arr[ret_arr.Length-1] = elem;
					}
				}
			}
			
			DeleteTemporaryFile( tmp_path );
			
			return ret_arr;
		}
		
		/// <summary>
		/// Removing an item to a chunk file in multithread mode
		/// </summary>
		/// <param name="filepath">path to file</param>
		/// <param name="idx">datastructure index</param>
		public static void RemoveItem( string filepath, Index idx ) {
			var buffer = new byte[MaxChunkSize];
			int offset = 0;
			int needed_idx_pos = -1;
			int needed_elem_type_pos = -1;
			
			// let's use a direct method to find our element by hash index
			using ( var fs = File.Open( filepath, FileMode.Open )) {
				if ( fs.Read( buffer, offset, MaxChunkSize  ) > 0 ) {
					string buf = System.Text.Encoding.UTF8.GetString(buffer);
					const string tmp_el_type  ="\"ElementType\":";
					string tmp_el_hash  ="\"ElementHash\": \""+idx.HashCode+"\"";

					int str_elem_type_pos = buf.IndexOf(tmp_el_type, StringComparison.InvariantCultureIgnoreCase);
					int str_hash_pos = buf.IndexOf(tmp_el_hash, StringComparison.InvariantCultureIgnoreCase);
					if ( str_hash_pos > -1 )
						needed_idx_pos = str_hash_pos+15;
					if ( str_elem_type_pos > -1 )
						needed_elem_type_pos = str_elem_type_pos + 15;
				}
			}
			
			// let's delete a hash index for object, that we should remove
			if ( needed_idx_pos > -1 ) {
				using ( var fs = File.Open( filepath, FileMode.Open )) {
					//var hash_bytes = System.Text.Encoding.UTF8.GetBytes(idx.HashCode);
					byte[] replacement = new byte[34];
					//fs.Seek( needed_idx_pos
					fs.Position = needed_elem_type_pos;
					fs.Write( System.Text.Encoding.UTF8.GetBytes( 3.ToString() /* REMOVED */ ), 0, 1);
					
					fs.Position = needed_idx_pos;
					fs.Write( replacement, 0, 34);
					fs.Position -= 34;
					fs.Write( System.Text.Encoding.UTF8.GetBytes( "null" ), 0, 4);
				}
			}
		}
		
		public static IStructure FindItem( JsonReader json_reader, IStructure ds ) {
			var item = new InnerChunkElement();
			while ( json_reader.Read() ) {
				item = json_serializer.Deserialize<InnerChunkElement>( json_reader );
				if ( item.ElementHash == ds.GetIndex().HashCode ) {
					return ds;
				}
			}
			
			return null;
		}
		
		private static InnerChunkElement FindItem( JsonTextReader jr, Index idx ) {
			return FindItem( jr, idx.HashCode );
		}
		
		private static InnerChunkElement FindItem( JsonTextReader jr, string hash ) {
			var item = new InnerChunkElement();
			while ( jr.Read()) {
				item = json_serializer.Deserialize<InnerChunkElement>( jr );
				if ( item.ElementHash == hash ) {
					return item;
				}
			}
			
			return null;
		}
		
		/// <summary>
		/// Removing unused data chunks
		/// </summary>
		public static void RemoveUnusedFiles() {
			// TODO!
		}
		
		static JsonSerializer json_serializer;
		//	static JsonWriter writer;
		static readonly Config.Config config;
	}
}
