/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 14:55
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DwarfDB.DataStructures;
using System.Linq;
using System.Text.RegularExpressions;

namespace DwarfDB.ChunkManager
{
	struct IndexPair {
		public String hash_min;
		public String hash_max;
		
		static public int Sort( string idx_pattern, string foo ) {
			return String.Compare( foo, idx_pattern );
		}
	}
	
	/// <summary>
	/// Description of ChunkManager.
	/// </summary>
	public class ChunkManager
	{
		/// <summary>
		/// Chunks list is made of :
		/// IndexPair - structure, contains maximum and minimum hash index values,
		/// string - DataContainer name
		/// </summary>
		private Dictionary<IndexPair, string> chunks_lst = new Dictionary<IndexPair, string>();
		private HashSet<string> all_hashes = new HashSet<string>();
		public static ChunkManager inner_object = null;
		private DataBase conn_db = null;
		private ConcurrentDictionary<Index, KeyValuePair<IStructure, String>> all_indexes =
			new ConcurrentDictionary<Index, KeyValuePair<IStructure, String>>();
		Dictionary<string, DataContainer> inner_dc_dict = null;
		
		public string CurrentDbPath {
			get; private set;
		}
		
		public ChunkManager()
		{
		}
		
		/// <summary>
		/// Loading chunks list
		/// </summary>
		public void Load( string db_name, bool is_new_db ) {
			CurrentDbPath = Config.Config.Instance.DataDirectory+@"\"+db_name;
			
			if ( !Directory.Exists(CurrentDbPath) ) {
				throw new ChunkException("Cannot find a directory for database: "+CurrentDbPath);
			}
			var rec_expr = new Regex(@"\b(.*)rec_(.*)_(.*).dwarf\b");
			
			string[] files = Directory.GetFiles(CurrentDbPath);
			foreach ( var filepath in files ) {
				if ( rec_expr.IsMatch( filepath ) ) {
					var mtc = rec_expr.Match( filepath );
					var min_hash = mtc.Groups[2].Value;
					var max_hash = mtc.Groups[3].Value;
					var ipk = new IndexPair() { hash_min = min_hash, hash_max = max_hash };
					//chunks_lst[ ipk ] = db_name;
					chunks_lst[ ipk ] = filepath;
				}
			}
			
			if ( !is_new_db )
				LoadRecordIndexes();
		}
		
		/// <summary>
		/// Creating new chunk manager for given DB
		/// </summary>
		/// <param name="db_to_connect">DB name</param>
		/// <returns></returns>
		public static ChunkManager Create( string db_to_connect ) {
			if ( inner_object == null ) {
				inner_object = new ChunkManager();
				inner_object.conn_db = null;
				if ( inner_object.conn_db == null ) {
					Errors.Messages.DisplayError(
						"Can't find DB with name \""+db_to_connect+"\"",
						"",
						"Please, check DB name or create such DB",
						DateTime.Now
					);
					return null;
				}
			}
			return inner_object;
		}
		
		/// <summary>
		/// Getting a chunk file path
		/// </summary>
		/// <param name="idx_pair">Index min-max pair</param>
		/// <returns></returns>
		private string GetChunkFilePath( IndexPair idx_pair ) {
			return CurrentDbPath+@"/rec_"+idx_pair.hash_min+"_"+idx_pair.hash_max+".dwarf";
		}
		
		/// <summary>
		/// Finds a chunk for Datacontainer
		/// </summary>
		/// <param name="dc_name">Data Container name</param>
		/// <returns>filepath to chunk</returns>
		private string FindChunkFileForDataContainer( string dc_name ) {
			var files = Directory.GetFiles(CurrentDbPath,@"dc_"+dc_name+".dwarf");
			if ( files.Any() )
				return files[0];
			else
				return null;
		}
		
		/// <summary>
		/// Finds a chunk with index space,
		/// that can contain an Index "idx"
		/// </summary>
		/// <param name="rec">Record</param>
		/// <returns>filepath to chunk</returns>
		private List<string> FindChunkFilesForRecord( Record rec ) {
			return FindChunkFilesForRecord(rec.GetIndex());
		}

		/// <summary>
		/// Finds a chunk with index space,
		/// that can contain an Index "idx"
		/// </summary>
		/// <param name="idx">Index</param>
		/// <returns>filepath to chunk</returns>
		private List<string> FindChunkFilesForRecord( Index idx ) {
			return FindChunkFilesForRecord( idx.DwarfHashCode );
		}
		
		/// <summary>
		/// Finds a chunk with index space,
		/// that can contain an Index "idx"
		/// </summary>
		/// <param name="hash">Hashcode</param>
		/// <returns>filepath to chunk</returns>
		private List<string> FindChunkFilesForRecord( string hash ) {
			var ret = new List<string>();
			foreach ( var chk in chunks_lst ) {
				if (chk.Key.hash_min.CompareTo( hash ) < 0 ||
				    chk.Key.hash_max.CompareTo( hash ) >= 0) {
					var chk_path = GetChunkFilePath(chk.Key);
					if (File.Exists( chk_path ))
						ret.Add(chk_path);
				}
			}
			
			return ret;
		}
		
		public List<Record> LoadChunk( int chunk_number, string dc_hash ) {
			var ret = new List<Record>();
			var files = Directory.GetFiles(CurrentDbPath,"rec_*.dwarf");
			
			if ( chunk_number  < files.Count() ) {
				ret.AddRange(ChunkFormat.GetRecordsInFile( files[chunk_number], dc_hash ));
			}
			return ret;
		}
		
		public List<Record> LoadAllChunks( string dc_hash ) {
			var ret = new List<Record>();
			var files = Directory.GetFiles(CurrentDbPath,"rec_*.dwarf");

			for ( int cn  = 0; cn < files.Count(); ++cn ) {
				ret.AddRange(ChunkFormat.GetRecordsInFile( files[cn], dc_hash ));
			}
			return ret;
		}
		
		/// <summary>
		/// Creates a new chunk for databases
		/// </summary>
		/// <param name="db">Database</param>
		public void CreateChunk( DataBase db ) {
			try {
				var db_filename = CurrentDbPath+
					@"\db_"+ db.Name + ".dwarf";
				
				Directory.CreateDirectory(CurrentDbPath);
				
				if ( !File.Exists(db_filename) ) {
					var filepath = CurrentDbPath + @"/db_"+ db.Name + ".dwarf";
					
					ChunkFormat.AddItem( filepath, db);
					chunks_lst[ new IndexPair() { hash_min = null, hash_max = null } ] = db.Name;
				} else {
					Errors.Messages.DisplayError("Database \""+db.Name+"\" already exists!",
					                             "creating DB", "Choose another name", DateTime.Now);
					return;
				}
			} catch ( IOException ex ) {
				throw new ChunkException( "Error writing a new chunk!", ex );
			}
		}

		/// <summary>
		/// Creates a new chunk for data containers
		/// </summary>
		/// <param name="dc">DataContainer</param>
		public void CreateChunk( DataContainer dc ) {
			try {
				if ( !all_indexes.Keys.Contains(dc.GetIndex()) ) {
					var filepath = CurrentDbPath +@"/dc_"+ dc.Name + ".dwarf";
					if ( !File.Exists( filepath ) ) {
						ChunkFormat.AddItem( filepath, dc);
						var index = dc.GetIndex();
						all_indexes[index] = new KeyValuePair<IStructure, string>(dc, dc.GetIndex().DwarfHashCode);
						chunks_lst.Add( new IndexPair() {
						               	hash_min =  index.DwarfHashCode, hash_max =  index.DwarfHashCode
						               }, dc.Name );
					}
				}
			} catch ( IOException ex ) {
				throw new ChunkException( "Error writing a new chunk!", ex );
			}
		}
		
		private int IndexComparer( Record rec1, Record rec2 ) {
			return rec1.GetIndex().DwarfHashCode.
				CompareTo(rec2.GetIndex().DwarfHashCode);
		}
		
		private int IndexComparer( string hash1, string hash2 ) {
			return hash1.CompareTo(hash2);
		}
		
		/// <summary>
		/// Creates a new chunk for records
		/// </summary>
		/// <param name="records">A list of records</param>
		/// <param name="max_elem_count">Maximum amount of elements in a chunk</param>
		public void CreateChunk( List<Record> records, int max_elem_count = 15 ) {
			try {
				
				var no_null_records = records.Where((rec) =>{ return rec.GetIndex() != null;}).ToList();
				
				no_null_records.Sort( (e1, e2) => {
				                     	var index1 = e1.GetIndex();
				                     	var index2 = e2.GetIndex();
				                     	return index1.DwarfHashCode.CompareTo(index2.DwarfHashCode);
				                     } );
				
				if ( no_null_records.Count > max_elem_count ) {
					var sub_range = new List<Record>();
					for ( int i = 0; i < no_null_records.Count; i += max_elem_count ) {

						if ( i + max_elem_count < no_null_records.Count )
							sub_range = no_null_records.GetRange( i, max_elem_count );
						else
							sub_range = no_null_records.GetRange( i, no_null_records.Count - i );
						
						CreateChunk( sub_range, max_elem_count );
					}
					
					return;
				}

				if ( no_null_records.Count > 0  ) {
					// 1. Let's sort our hashes
					no_null_records.Sort(IndexComparer);

					// 2. for each hash we looking for it's existance in
					//    already created chunks and an ability to place in existing chunks
					var filepath = CurrentDbPath + "/rec_"+no_null_records.First().GetIndex().DwarfHashCode +
						"_" + no_null_records.Last().GetIndex().DwarfHashCode + ".dwarf";
					no_null_records.ForEach( (rec) => {
					                     //   	if ( !all_hashes.Contains(rec.GetIndex().DwarfHashCode) ) {
					                        		ChunkFormat.AddItem( filepath, rec);
					                        		AllIndexes.TryAdd(rec.GetIndex(),
					                        		                  new KeyValuePair<IStructure, string>(rec, rec.OwnerDC.GetIndex().DwarfHashCode));
					                   //     	}
					                        } );
					
					// adding to chunk list
					try {
						chunks_lst.Add( new IndexPair() {
						               	hash_min = no_null_records.First().GetIndex().DwarfHashCode,
						               	hash_max =  no_null_records.Last().GetIndex().DwarfHashCode
						               }, "none" );
					} catch ( Exception ex ) {
						Errors.Messages.DisplayError("Chunk processing error: "+ex.Message, "", "", DateTime.Now);
					}
				}
			} catch ( Exception ex ) {
				throw new ChunkException( "Error writing a new chunk!", ex );
			}
		}
		
		/// <summary>
		/// Searching a Data Container by name
		/// </summary>
		/// <param name="dc_name">DataContainer name</param>
		/// <returns></returns>
		public DataContainer GetDataContainer( string dc_name ) {
			var chunk = FindChunkFileForDataContainer( dc_name );
			
			if ( chunk != null ) {
				var new_dc = ChunkFormat.GetDCInFile( chunk, dc_name ); //new DataContainer( null, dc_name );
				new_dc.BuildIndex();
				return new_dc;
			}

			return null;
		}
		
		/// <summary>
		/// Searching a Data Container by index
		/// </summary>
		/// <param name="dc_index">DC index</param>
		/// <returns></returns>
		public DataContainer GetDataContainer( Index dc_index ) {
			// TODO: search for data container in file chunks
			return null;
		}
		
		/// <summary>
		/// Searching a Record by index
		/// </summary>
		/// <param name="dc_index">Record index</param>
		/// <returns></returns>
		public Record GetRecord( Index rec_index ) {
			// Let's find our record in chunk files...
			foreach (var convenient_chunk in FindChunkFilesForRecord( rec_index )) {
				var item = ChunkFormat.GetItem( convenient_chunk,  rec_index  );
				if ( item != null )
					return (Record)item;
			}
			
			return null;
		}
		
		/// <summary>
		/// Searching a Record by index
		/// </summary>
		/// <param name="hash">Record hashcode</param>
		/// <returns></returns>
		public Record GetRecord( string hash ) {
			// Let's find our record in chunk files...
			foreach (var convenient_chunk in FindChunkFilesForRecord( hash  )) {
				var item = ChunkFormat.GetItem( convenient_chunk,  hash  );
				if ( item != null ) {
					
					return (Record)item;
				}
			}
			
			return null;
		}
		
		/// <summary>
		/// Save changes in an existant record
		/// </summary>
		/// <param name="rec">Record</param>
		/// <returns>bool</returns>
		public bool SaveRecord( Record rec ) {
			// Let's find our record in chunk files...
			/*foreach (var convenient_chunk in FindChunkFilesForRecord( rec.GetIndex().HashCode  )) {
				var item = ChunkFormat.GetItem( convenient_chunk,  rec.GetIndex().HashCode );
				if ( item != null ) {
					// saving changes...
					ChunkFormat.SaveItemContents( convenient_chunk, rec );
					return true;
				}
			}*/

			return false;
		}
		
		public ConcurrentDictionary<Index, KeyValuePair<IStructure,string>> AllIndexes {
			get {
				if ( all_indexes.Count != all_hashes.Count ) {
					foreach ( var idx in all_indexes ) {
						if ( !all_hashes.Contains(idx.Key.DwarfHashCode) )
							all_hashes.Add(idx.Key.DwarfHashCode);
					}
				}
				return all_indexes;
			}
		}
		
		/// <summary>
		/// Getting records for a DataContainer
		/// </summary>
		/// <param name="rec_idxs">Indexes list</param>
		/// <returns></returns>
		public List<Record> GetRecords( List<Index> rec_idxs ) {
			var ret = new List<Record>();
			
			foreach ( var idx in rec_idxs ) {
				ret.Add(GetRecord(idx));
			}
			
			return ret;
		}
		
		public void ClearAllIndexes() {
			all_indexes.Clear();
		}
		
		public void LoadDCIndexes( DataBase db ) {
			var filepath = CurrentDbPath+@"/indexes.dw";
			var rgx = new Regex(@"(.*):DataContainer:(.*):(.*)");
			
			if ( File.Exists(filepath) ) {
				using ( var fs = File.OpenRead( filepath )) {
					var sr = new StreamReader( fs );
					string line = sr.ReadLine();
					while ( line != null ) {
						if ( line.IndexOf(":DataContainer:", 0, StringComparison.CurrentCulture) > 0 ) {
							if ( rgx.IsMatch( line ) ) {
								var mtc = rgx.Matches( line );
								if ( mtc[0].Groups.Count > 0 ) {
									var dc_name = mtc[0].Groups[1].Value;
									var dc_hash = mtc[0].Groups[3].Value;
									var idx = Index.CreateFromDwarfHashCode( dc_hash );
									var new_dc = GetDataContainer(dc_name);
									new_dc.AssignOwnerDB(db);
									all_indexes.TryAdd( idx, new KeyValuePair<IStructure, string>(new_dc, dc_name));
								}
							}
						}
						line = sr.ReadLine();
					}
				}
			} else
				throw new ChunkException( "Indexes.dw is absent! " );
		}
		
		/// <summary>
		/// Loading an index for each record
		/// </summary>
		public void LoadRecordIndexes() {
			var filepath = CurrentDbPath+@"/indexes.dw";
			var seek_str = "Record:Record:";
			if ( File.Exists(filepath) ) {
				using ( var fs = File.OpenRead( filepath )) {
					var sr = new StreamReader( fs );
					string line = sr.ReadLine();
					while ( line != null ) {
						if ( line.IndexOf( seek_str ) == 0 ) {
							var rec_hash = line.Substring(
								line.IndexOf(seek_str)+seek_str.Length, 32 );
							var own_dc_hash = line.Substring(line.IndexOf(rec_hash+":")+33, 32);
							var idx = Index.CreateFromDwarfHashCode( rec_hash );
							
							all_indexes.TryAdd( idx,
							                   new KeyValuePair<IStructure, string>(DummyRecord.Create(own_dc_hash, conn_db), own_dc_hash));
						}
						line = sr.ReadLine();
					}
				}
			} else
				throw new ChunkException( "Indexes.dw is absent! " );
		}
		
		/// <summary>
		/// Removing an index from index.dw and AllIndexes
		/// </summary>
		/// <param name="idx">Index</param>
		public void RemoveIndex( Index idx ) {
			if ( idx != null )
				RemoveIndex( idx.DwarfHashCode );
		}
		
		/// <summary>
		/// Removing an index from index.dw and AllIndexes
		/// </summary>
		/// <param name="hashcode">Hash code</param>
		public void RemoveIndex( string hashcode ) {
			Index rem_idx = null;
			
			foreach ( var chk_idx in AllIndexes.Keys ) {
				if ( chk_idx.DwarfHashCode == hashcode ) {
					rem_idx = chk_idx;
					break;
				}
			}
			
			if ( rem_idx != null ) {
				var tmp_val = new KeyValuePair<IStructure, string>();
				AllIndexes.TryRemove( rem_idx, out tmp_val );
			}
		}
		
		/// <summary>
		/// Removes a record from chunk
		/// </summary>
		/// <param name="rec">Record to delete</param>
		public void RemoveRecord( Record rec ) {
			var idx =  rec.GetIndex();
			if ( idx != null ) {
				var ind_hash = idx.DwarfHashCode;
				foreach ( var strg in chunks_lst.Values ) {
					if ( strg!= null )
						if ( strg != "none" && strg != String.Empty )
							ChunkFormat.RemoveItem( strg, idx );
				}
				
				//ClearIndexesDw();
				SaveIndexes();
			}
		}
		
		/// <summary>
		/// Clearing an index file
		/// </summary>
		/// <returns></returns>
		private void ClearIndexesDw() {
			
			// TODO : making a backup of indexes.dw!
			
			FileStream fs = null;
			var filepath = CurrentDbPath+@"/indexes.dw";
			fs = File.Create( filepath );
			fs.Close();
		}
		
		private string ReadIndexesDw() {
			FileStream fs = null;
			string contents = "";
			var filepath = CurrentDbPath+@"/indexes.dw";
			
			if ( !File.Exists( filepath ) )
				fs = File.Create( filepath );
			else {
				// First of all, let's read existing contents
				var fs_read = File.OpenRead( filepath );
				
				using ( var sr = new StreamReader( fs_read ) ) {
					contents = sr.ReadToEnd();
				}

			}
			
			return contents;
		}
		
		internal void ReceiveDCDict( Dictionary<string, DataContainer> dc_dict ) {
			inner_dc_dict = dc_dict;
		}
		
		private HashSet<string> tmp_hashes_copy = new HashSet<string>();
		private ConcurrentDictionary<Index, KeyValuePair<IStructure, String>> tmp_indexes_copy =
			new ConcurrentDictionary<Index, KeyValuePair<IStructure, string>>();
		
		/// <summary>
		/// Collects indexes into temporary arrays
		/// </summary>
		/// <param name="db"></param>
		private void CollectIndexes( DataBase db ) {
			tmp_hashes_copy.Clear();
			tmp_indexes_copy.Clear();
			
			db.FillChunkManagerDCDict();
			
			foreach ( var elem in inner_dc_dict )
			{
				var dc_index_obj = elem.Value.GetIndex();
				tmp_hashes_copy.Add( dc_index_obj.DwarfHashCode );
				tmp_indexes_copy[dc_index_obj] =
					new KeyValuePair<IStructure, String>( elem.Value,  null );
				var tmp_records = elem.Value.GetRecordsInternal();
				foreach( var rec in tmp_records ) {
					var rec_index_obj = rec.GetIndex();
					tmp_hashes_copy.Add( rec_index_obj.DwarfHashCode );
					tmp_indexes_copy[rec_index_obj] =
						new KeyValuePair<IStructure, String>( rec, dc_index_obj.DwarfHashCode );
				}
			}
		}
		
		/// <summary>
		/// Saves all indexes in format Name:Type:IndexHash
		/// </summary>
		public void RebuildIndexes( DataBase db ) {
			// collecting indexes
			CollectIndexes( db );
			
			// cleaning indexes and hashes
			all_indexes.Clear();
			all_hashes.Clear();
			
			all_hashes = tmp_hashes_copy;
			all_indexes = tmp_indexes_copy;
			
			
			ClearIndexesDw();
			var filepath = CurrentDbPath+@"/indexes.dw";
			string contents = ReadIndexesDw();
			
			var fs = File.Open( filepath, FileMode.Append );
			
			// let's add new records to index.dw
			using ( var sw = new StreamWriter( fs ) ) {
				foreach ( var idx in AllIndexes ) {
					if ( idx.Value.Key is Record ) {
						var rec =  idx.Value.Key as Record;
						var owner_dc = rec.OwnerDC;
						string hash_code = idx.Key.DwarfHashCode;
						sw.WriteLine( "Record:Record:"+hash_code+":"+owner_dc.GetIndex().DwarfHashCode);
					} else if ( idx.Value.Key is DataContainer ) {
						var dc = (DataContainer)idx.Value.Key;
						sw.WriteLine( dc.Name+
						             ":DataContainer:"+
						             dc.GetOwnerDB().Name+":"+
						             idx.Key.DwarfHashCode );
					}
				}
			}
		}		
		
		/// <summary>
		/// Saves all indexes in format Name:Type:IndexHash
		/// </summary>
		public void SaveIndexes() {
			var filepath = CurrentDbPath+@"/indexes.dw";
			string contents = ReadIndexesDw();
			
			var fs = File.Open( filepath, FileMode.Append );
			// let's add new records
			using ( var sw = new StreamWriter( fs ) ) {
				foreach ( var idx in AllIndexes ) {
					if ( !contents.Contains(idx.Key.DwarfHashCode) ) {
						if ( idx.Value.Key is Record && !( idx.Value.Key is DummyRecord ) ) {
							var rec =  idx.Value.Key as Record;
							var owner_dc = rec.OwnerDC;
							string hash_code = idx.Key.DwarfHashCode;
							sw.WriteLine( "Record:Record:"+hash_code+":"+owner_dc.GetIndex().DwarfHashCode);
						} else if ( idx.Value.Key is DataContainer  ) {
							if ( !contents.Contains(idx.Key.DwarfHashCode) ) {
								var dc = (DataContainer)idx.Value.Key;
								sw.WriteLine( dc.Name+
								             ":DataContainer:"+
								             dc.GetOwnerDB().Name+":"+
								             idx.Key.DwarfHashCode );
							}
						}
					}
				}
			}
		}
	}
}
