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
	/// Стек для сообщений, хранимых в памяти сервера
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
		
		// Максимальное кол-во элементов в стеке
		#if DEBUG
		const int ID_DIM = 200000;
		#else
		const int ID_DIM = 200000;
		#endif
		
		private bool IsCapable( IStructure dta_struct ) {
			return true;
		}
		
		public new void Push( IStructure dta_struct ) {
			if ( this.Count < ID_DIM ) {
				// пополняем список использованных id
				string new_index_hash = dta_struct.GetIndex().HashCode;
				if ( !idx_hashes.Contains( new_index_hash ) )
					idx_hashes.Add( new_index_hash );
				
				if ( dta_struct is Record ) {
					//if ( !records_list..Contains(dta_struct as Record) ) {
					records_list.Add( dta_struct as Record );
					Modified = true;
					//}
				}
				
				base.Push(dta_struct);
			}
		}
		
		new public bool TryPop( IStructure data ) {
			var ret = base.TryPop( out data );
			// удаляем освобожденный Index
			if ( ret ) {
				data.Save(); // Сохраняем в файле
				idx_hashes.Remove(data.GetIndex().HashCode);
				Modified = true;
			}
			return ret;
		}
		
		public bool ContainsHash( string hash ) {
			return idx_hashes.Contains(hash);
		}
		
		/// <summary>
		/// Получаем сообщение с заданным index
		/// </summary>
		/// <param name="ind"></param>
		/// <returns></returns>
		public List<Record> GetRecords( DataStructures.DataContainer dc ) {
			var ret = new HashSet<Record>();
			try {
				if ( this.Modified ) {
					var tmp_stack = new ConcurrentStack<IStructure>(); // временный стек для перебора элементов в основном стеке
					IStructure tmp = null;
					
					// Перебираем элементы основного стека
					int element_count = this.Count;
					
					for ( int cntr = 0; cntr <= element_count; ++cntr) {
						if ( (this.TryPop( out tmp )) == true) {
							// Нашли сообщение?
							// Возвращаем перебранные элементы в основной
							// стек и возвращаем найденное
							if ( tmp is Record ) {
								if ( (tmp as Record).OwnerDC == dc ) {
									ret.Add( tmp as Record );
								}
							}
							
							// Не нашли? Кладем во временный стек и идем дальше
							tmp_stack.Push( tmp );
						}
					}
					PushFromStack( tmp_stack );
					// Putting down a flag for next GetRecords operations
					//Modified = false;
				} else {
					if ( records_list.Count > 0 )
						Parallel.ForEach( records_list, parallel_opts, ( rec ) => {
						                 	if ( rec.OwnerDC == dc )
						                 		ret.Add(rec);
						                 });
				}
			} catch ( Exception ex ) {
				Errors.ErrorProcessing.Display( "FAILED TO GET STRUCTURE: "+ex.Message+":"+ex.StackTrace,
				                               "", "", DateTime.Now );
			}

			return ret.ToList ();
		}
		
		/// <summary>
		/// Получаем сообщение с заданным index
		/// </summary>
		/// <param name="ind"></param>
		/// <returns></returns>
		public IStructure GetStructure( Index ind ) {
			try {
				var tmp_stack = new ConcurrentStack<IStructure>(); // временный стек для перебора элементов в основном стеке
				IStructure tmp = null;
				
				Console.WriteLine("TRYING TO GET STRUCTURE #"+ind.HashCode);
				
				// Перебираем элементы основного стека
				int element_count = this.Count;
				for ( int cntr = 0; cntr < element_count; ++cntr ) {
					if ( (this.TryPop( out tmp )) == false)
						continue;
					
					// Нашли сообщение?
					// Возвращаем перебранные элементы в основной
					// стек и возвращаем найденное
					if ( tmp.GetIndex() == ind ) {
						PushFromStack( tmp_stack );
						return tmp;
					}
					
					// Не нашли? Кладем во временный стек и идем дальше
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
			foreach ( var st in input_stack ) {
				base.Push( st );
			}
		}
		
		/// <summary>
		/// Is stack elements array modified?
		/// </summary>
		public bool Modified {
			get; private set;
		}
		
		ParallelOptions parallel_opts = new ParallelOptions();
		DataBase db;
		ConcurrentBag<Record> records_list = new ConcurrentBag<Record>();
		HashSet<string> idx_hashes = new HashSet<string>();
	}
}
