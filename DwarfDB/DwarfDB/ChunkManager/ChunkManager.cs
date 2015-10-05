/*
 * Пользователь: Igor.Evdokimov
 * Дата: 27.08.2015
 * Время: 14:55
 */
using System;
using System.IO;
using System.Collections.Generic;
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
		
		// < Our index, < parent structure, parent index hash > >
		private readonly Dictionary<Index, KeyValuePair<IStructure, String>> all_indexes = new Dictionary<Index, KeyValuePair<IStructure, String>>();
		
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
					chunks_lst[ ipk ] = db_name;
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
				inner_object.conn_db = inner_object.FindDB( db_to_connect );
				if ( inner_object.conn_db == null ) {
					Errors.ErrorProcessing.Display(
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
		/// Searching for DB in chunks
		/// </summary>
		/// <param name="db_name">DB name</param>
		/// <returns></returns>
		private DataStructures.DataBase FindDB( string db_name ) {
			
			return null;
		}
		
		/// <summary>
		/// Getting a chunk file path
		/// </summary>
		/// <param name="idx_pair">Index min-max pair</param>
		/// <returns></returns>
		private string GetChunkFilePath( IndexPair idx_pair ) {
			return CurrentDbPath+@"\"+"rec_"+idx_pair.hash_min+"_"+idx_pair.hash_max+".dwarf";
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
		private List<string> FindChunkFilesForRecord(Index idx ) {
			return FindChunkFilesForRecord( idx.HashCode );
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
				var add_records = new List<Record>();
				var db_filename = CurrentDbPath+
					@"\db_"+ db.Name + ".dwarf";
				
				Directory.CreateDirectory(CurrentDbPath);
				
				if ( !File.Exists(db_filename) ) {
					var filepath = CurrentDbPath + @"\"+
						"db_"+ db.Name + ".dwarf";
					
					var new_chunk = ChunkFormat.CreateNewFile( filepath );
					ChunkFormat.AddItem( filepath, db);
					chunks_lst[ new IndexPair() { hash_min = null, hash_max = null } ] = db.Name;
				} else {
					Errors.ErrorProcessing.Display("Database \""+db.Name+"\" already exists!",
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
				var add_records = new List<Record>();

				if ( !all_indexes.Keys.Contains(dc.GetIndex()) ) {
					var filepath = CurrentDbPath +@"\dc_"+ dc.Name + ".dwarf";
					if ( !File.Exists( filepath ) ) {
						var new_chunk = ChunkFormat.CreateNewFile( filepath );
						ChunkFormat.AddItem( filepath, dc);
						var index = dc.GetIndex();
						all_indexes.Add(index, new KeyValuePair<IStructure, string>(dc, dc.GetIndex().HashCode));
						chunks_lst.Add( new IndexPair() {
						               	hash_min =  index.HashCode, hash_max =  index.HashCode
						               }, dc.Name );
					}
				}
			} catch ( IOException ex ) {
				throw new ChunkException( "Error writing a new chunk!", ex );
			}
		}
		
		private int IndexComparer( Record rec1, Record rec2 ) {
			return rec1.GetIndex().HashCode.
				CompareTo(rec2.GetIndex().HashCode);
		}
		
		private int IndexComparer( string hash1, string hash2 ) {
			return hash1.CompareTo(hash2);
		}
		
		/// <summary>
		/// Creates a new chunk for records
		/// </summary>
		/// <param name="last_new_elems_count">A count of new elements to put in new chunk( if needed! )</param>
		/// <param name="max_idx_count"></param>
		public void CreateChunk( List<Record> records, int max_elem_count = 100 ) {
			try {
				records.Sort( (e1, e2) => {
				             	var index1 = e1.GetIndex();
				             	var index2 = e2.GetIndex();
				             	return index1.HashCode.CompareTo(index2.HashCode);
				             } );
				if ( records.Count > max_elem_count ) {
					var sub_range = new List<Record>();
					for ( int i = 0; i < records.Count; i += max_elem_count ) {

						if ( i + max_elem_count < records.Count )
							sub_range = records.GetRange( i, max_elem_count );
						else
							sub_range = records.GetRange( i, records.Count - i );
						
						CreateChunk( sub_range, max_elem_count );
					}
					
					return;
				}
				
				var add_records = new List<Record>();
				
				// 1. Let's sort our hashes
				records.Sort(IndexComparer);

				// 2. for each hash we looking for it's existance in
				//    already created chunks and an ability to place in existing chunks
				var filepath = CurrentDbPath + "/"+
					"rec_"+records.First().GetIndex().HashCode +
					"_" + records.First().GetIndex().HashCode + ".dwarf";
				var new_chunk = ChunkFormat.CreateNewFile( filepath );
				records.ForEach( (rec) => {
				                	if ( !all_indexes.Keys.Contains(rec.GetIndex()) ) {
				                		ChunkFormat.AddItem( filepath, rec);
				                		all_indexes.Add(rec.GetIndex(), new KeyValuePair<IStructure, string>(rec, rec.OwnerDC.GetIndex().HashCode));
				                	}
				                } );
				
				// adding to chunk list
				try {
					chunks_lst.Add( new IndexPair() {
					               	hash_min = records.First().GetIndex().HashCode,
					               	hash_max =  records.First().GetIndex().HashCode
					               }, "none" );
				} catch ( Exception ex ) {
					// XXX: doing nothing...
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
				var new_dc =  ChunkFormat.GetDCInFile( chunk, dc_name ); //new DataContainer( null, dc_name );
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
		
		public Dictionary<Index, KeyValuePair<IStructure,string>> AllIndexes {
			get {
				return all_indexes;
			}
		}
		
		/// <summary>
		/// Getting records for a DataContainer
		/// </summary>
		/// <param name="dc_index">Record index</param>
		/// <returns></returns>
		public List<Record> GetRecords( List<Index> rec_idxs ) {
			var ret = new List<Record>();
			
			foreach ( var idx in rec_idxs ) {
				ret.Add(GetRecord(idx));
			}
			
			return ret;
		}
		
		public void LoadDCIndexes() {
			var filepath = CurrentDbPath+@"\indexes.dw";
			var rgx = new Regex(@"(.*):DataContainer:(.*):(.*)");
			
			if ( File.Exists(filepath) ) {
				using ( var fs = File.OpenRead( filepath )) {
					var sr = new StreamReader( fs );
					string line = sr.ReadLine();
					while ( line != null ) {
						if ( line.IndexOf(":DataContainer:", 0) > 0 ) {
							if ( rgx.IsMatch( line ) ) {
								var mtc = rgx.Matches( line );
								if ( mtc[0].Groups.Count > 0 ) {
									var dc_name = mtc[0].Groups[1].Value;
									var dc_hash = mtc[0].Groups[3].Value;
									var idx = Index.CreateFromHashCode( dc_hash );
									var new_dc = GetDataContainer(dc_name);
									
									if ( !all_indexes.ContainsKey( idx ) ) {
										all_indexes.Add( idx, new KeyValuePair<IStructure, string>(new_dc, dc_name));
									}
								}
							}
						}
						line = sr.ReadLine();
					}
				}
			} else
				throw new ChunkException( "Indexes.dw is absent! " );
		}
		
		public void LoadRecordIndexes() {
			var filepath = CurrentDbPath+@"\indexes.dw";
			var rgx = new Regex(@"Record:Record:(.*):(.*)");
			
			if ( File.Exists(filepath) ) {
				using ( var fs = File.OpenRead( filepath )) {
					var sr = new StreamReader( fs );
					string line = sr.ReadLine();
					while ( line != null ) {
						if ( line.IndexOf("Record:Record:", 0) == 0 ) {
							if ( rgx.IsMatch(line ) ) {
								var mtc = rgx.Matches( line );
								if ( mtc[0].Groups.Count > 0 ) {
									var own_dc_hash = mtc[0].Groups[2].Value;
									var rec_hash = mtc[0].Groups[1].Value;
									var idx = Index.CreateFromHashCode( rec_hash );
									
									if ( !all_indexes.ContainsKey( idx ) ) {
										all_indexes.Add( idx, new KeyValuePair<IStructure, string>(DummyRecord.Create(), own_dc_hash));
									}
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
		/// Removing an index from index.dw and AllIndexes
		/// </summary>
		/// <param name="idx">Index</param>
		public void RemoveIndex( Index idx ) {
			RemoveIndex( idx.HashCode );
		}
		
		/// <summary>
		/// Removing an index from index.dw and AllIndexes
		/// </summary>
		/// <param name="hashcode">Hash code</param>
		public void RemoveIndex( string hashcode ) {
			Index rem_idx = null;
			
			foreach ( var chk_idx in AllIndexes.Keys ) {
				if ( chk_idx.HashCode == hashcode ) {
					rem_idx = chk_idx;
					break;
				}
			}
			
			if ( rem_idx != null ) {
				AllIndexes.Remove( rem_idx );
				SaveIndexes();
			}
		}
		
		/// <summary>
		/// Removes a record from chunk
		/// </summary>
		/// <param name="rec"></param>
		public void RemoveRecord( Record rec ) {
			foreach ( var strg in chunks_lst.Values ) {
				ChunkFormat.RemoveItem( strg, rec.GetIndex() );
			}
		}
		
		/// <summary>
		/// Saves all indexes in format Name:Type:IndexHash
		/// </summary>
		public void SaveIndexes() {
			var filepath = CurrentDbPath+@"\indexes.dw";
			FileStream fs = null;
			string contents = "";
			
			if ( !File.Exists( filepath ) )
				fs = File.Create( filepath );
			else {
				// First of all, let's read existing contents
				var fs_read = File.OpenRead( filepath );
				
				using ( var sr = new StreamReader( fs_read ) ) {
					contents = sr.ReadToEnd();
				}
				
				fs = File.Open( filepath, FileMode.Append );
			}
			
			// let's add new records
			using ( var sw = new StreamWriter( fs ) ) {
				foreach ( var idx in AllIndexes ) {
					if ( !contents.Contains(idx.Key.HashCode) ) {
						if ( idx.Value.Key is Record ) {
							sw.WriteLine( "Record:Record:"+idx.Key.HashCode+":"+(idx.Value.Key as Record).OwnerDC.GetIndex().HashCode);
						} else if ( idx.Value.Key is DataContainer  ) {
							if ( !contents.Contains(idx.Key.HashCode) )
								sw.WriteLine( ((DataContainer)idx.Value.Key).Name+
								             ":DataContainer:"+
								             ((DataContainer)idx.Value.Key).GetOwnerDB().Name+":"+
								             idx.Key.HashCode );
						}
					}
				}
			}
		}
		
		public static ChunkManager inner_object = null;
		
		private DataBase conn_db = null;
	}
}
