/*
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
	public class DataStackException : Exception
	{
		public DataStackException()
		{
		}
		
		public DataStackException(string reason) : base("DataStack: "+ reason)
		{
			Init();
		}

		public DataStackException( string reason,
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
		protected DataStackException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
	
	/// <summary>
	/// A stack for improving an access to dwarf records
	/// </summary>
	public class DwarfStack : ConcurrentStack<IStructure>
	{
		public DwarfStack( DataBase _db )
		{
			Modified = true;
			if ( _db != null )
				db = _db;
			parallel_opts.MaxDegreeOfParallelism = Environment.ProcessorCount*4;
		}
		
		private bool IsCapable( IStructure dta_struct ) {
			return true;
		}
		
		public new void Push( IStructure dta_struct ) {
			string new_index_hash = dta_struct.GetIndex().HashCode;
			if ( !idx_hashes.Contains( new_index_hash ) )
				idx_hashes.Add( new_index_hash );

			if ( dta_struct is Record ) {
				records_list.Add( dta_struct as Record );
				Modified = true;
			}
			base.Push(dta_struct);
		}
		
		new public bool TryPop( IStructure data ) {
			var ret = base.TryPop( out data );
			if ( ret ) {
				lock (data) {
					lock (idx_hashes) {
						data.Save();
						idx_hashes.Remove(data.GetIndex().HashCode);
					}
				}
				Modified = true;
			}
			return ret;
		}
		
		public bool ContainsHash( string hash ) {
			return idx_hashes.Contains(hash);
		}
		
		/// <summary>
		/// Receiving records for concrete DataContainer
		/// </summary>
		/// <param name="dc">A data container</param>
		/// <returns></returns>
		public List<Record> GetRecords( DataStructures.DataContainer dc ) {
			try {
				if ( this.Modified ) {
					var ret = new ConcurrentBag<Record>();
					var tmp_stack = new ConcurrentStack<IStructure>(); // A temporary stack
					IStructure tmp = null;
					
					int element_count = this.Count;
					
					//	Parallel.For( 0, element_count, parallel_opts, ( int cntr ) => {
					for ( int cntr = 0; cntr <= element_count; ++cntr) {
						if  (this.TryPop( out tmp )) {
							// Looking for needed records
							if ( tmp is Record ) {
								if ( ( tmp as Record ).OwnerDC == dc &&  ( tmp as Record ).Id >= 0 ) {
									ret.Add( tmp as Record );
								}
							}
							
							// If this record is not what we need - let's put it back
							// in the temporary stack
							tmp_stack.Push( tmp );
						}
					};
					
					PushFromStack( tmp_stack );
					// Putting down a flag for next GetRecords operations
					//Modified = false;
					
					return ret.ToList();
				} else {
					var ret = new HashSet<Record>();
					if ( records_list.Count > 0 )
						Parallel.ForEach( records_list, parallel_opts,
						                 ( rec ) => {
						                 	if ( rec.OwnerDC == dc )
						                 		ret.Add(rec);
						                 });
					
					return ret.ToList();
				}
			} catch ( Exception ex ) {
				Errors.ErrorProcessing.Display( "FAILED TO GET STRUCTURE: "+ex.Message+":"+ex.StackTrace,
				                               "", "", DateTime.Now );
			}
			return null;
		}
		
		/// <summary>
		/// Receiving records a record with given index
		/// </summary>
		/// <param name="ind"></param>
		/// <returns></returns>
		public IStructure GetStructure( Index ind ) {
			try {
				var tmp_stack = new ConcurrentStack<IStructure>(); // A temporary stack
				IStructure tmp = null;
				
				Console.WriteLine("TRYING TO GET STRUCTURE #"+ind.HashCode);
				
				int element_count = this.Count;
				for ( int cntr = 0; cntr < element_count; ++cntr ) {
					if ( (this.TryPop( out tmp )) == false)
						continue;
					
					// If we've found what we need -
					// let's push it from our temporary stack
					// and return it!
					if ( tmp.GetIndex() == ind ) {
						PushFromStack( tmp_stack );
						return tmp;
					}
					
					// If this record is not what we need - let's put it back
					// in the temporary stack
					tmp_stack.Push(tmp);
				}
				PushFromStack( tmp_stack );
			} catch ( Exception ex ) {
				Errors.ErrorProcessing.Display( "FAILED TO GET STRUCTURE: "+ex.Message+":"+ex.StackTrace,
				                               "", "", DateTime.Now );
			}
			return null;
		}

		private void PushFromStack( ConcurrentStack<IStructure> input_stack ) {
			Parallel.ForEach( input_stack, parallel_opts, ( st ) => {
			                 	base.Push( st );
			                 });
		}
		
		/// <summary>
		/// Are stack elements array modified?
		/// </summary>
		public bool Modified {
			get; private set;
		}
		
		ParallelOptions parallel_opts = new ParallelOptions();
		DataBase db;
		ConcurrentBag<Record> records_list = new ConcurrentBag<Record>();
		volatile HashSet<string> idx_hashes = new HashSet<string>();
	}
}
