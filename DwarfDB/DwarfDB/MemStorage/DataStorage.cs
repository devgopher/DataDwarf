﻿/*
 * Created by SharpDevelop.
 * User: Igor.Evdokimov
 * Date: 16.08.2015
 * Time: 15:59
 *
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DwarfDB.DataStructures;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Linq;

namespace DwarfDB.Stack
{
	/// <summary>
	/// Exception for data stacks
	/// </summary>
	[Serializable]
	public class DataStorageException : Exception
	{
		public DataStorageException()
		{
		}
		
		public DataStorageException(string reason) : base("DataStack: "+ reason)
		{
			Init();
		}

		public DataStorageException( string reason,
		                            Exception innerException) : base( "DataStack: "+ reason, innerException)
		{
			Init();
		}

		public DateTime When
		{
			get;
			private set;
		}

		private void Init() {
			When = DateTime.Now.ToLocalTime();
		}
		
		// This constructor is needed for serialization.
		protected DataStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	/// <summary>
	/// A stack for improving an access to dwarf records
	/// </summary>
	public class DataStorage
	{
		public DataStorage( DataBase _db )
		{
			Modified = true;
			if ( _db != null )
				db = _db;

			parallel_opts.MaxDegreeOfParallelism = Environment.ProcessorCount * 4;
		}

		public new void Add( IStructure dta_struct ) {
			string new_index_hash = dta_struct.GetIndex().HashCode;
			if ( dta_struct is Record ) {
				items_dict.TryAdd( new_index_hash, dta_struct );
				Modified = true;
			}
		}
		
		new public bool TryRemove( IStructure data ) {
			if ( items_dict.ContainsKey(data.GetIndex().HashCode) ) {
				IStructure dummy = null;
				return items_dict.TryRemove( data.GetIndex().HashCode, out dummy);
			}
			
			return false;
		}
		
		public void Clear() {
			items_dict.Clear();
		}
		
		public int Count {
			get {
				return items_dict.Count;
			}
			private set {}
		}
		
		public bool ContainsHash( string hash ) {
			return items_dict.ContainsKey( hash );
		}
		
	/*	public List<Record> GetRecords( DataContainer dc ) {
			return GetRecordsEnum( dc ).ToList();
		}
	*/	
		/// <summary>
		/// Receiving records for concrete DataContainer
		/// </summary>
		/// <param name="dc">A data container</param>
		/// <returns></returns>
		public IEnumerable<Record> GetRecords( DataContainer dc ) {
			/*	Parallel.ForEach( items_dict, parallel_opts, ( kvp ) => {
			                 	if ( kvp.Value is Record ) {
			                 		if ( ( kvp.Value as Record).OwnerDC.Name == dc.Name )
			                 			yield return kvp.Value;
			                 	}
			                 } );
			 */
			foreach ( var kvp in items_dict ) {
				if ( kvp.Value is Record ) {
					if ( ( kvp.Value as Record).OwnerDC.Name == dc.Name )
						yield return (  kvp.Value as Record );
				}
			}
		}
		
		/// <summary>
		/// Receiving records a record with given index
		/// </summary>
		/// <param name="ind"></param>
		/// <returns></returns>
		public IStructure GetStructure( Index ind ) {
			try {
				if ( items_dict.ContainsKey(ind.HashCode) )
					return items_dict[ind.HashCode];
				else
					return null;
			} catch ( Exception ex ) {
				Errors.ErrorProcessing.Display( "FAILED TO GET STRUCTURE: "+ex.Message+":"+ex.StackTrace,
				                               "", "", DateTime.Now );
			}
			return null;
		}


		/// <summary>
		/// Are stack elements array modified?
		/// </summary>
		public bool Modified {
			get; private set;
		}
		
		ParallelOptions parallel_opts = new ParallelOptions();
		DataBase db;
		ConcurrentDictionary<String, IStructure> items_dict = new ConcurrentDictionary<String, IStructure>();
	}
}
