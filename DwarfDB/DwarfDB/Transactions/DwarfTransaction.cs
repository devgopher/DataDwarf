/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 23:03
 */
using System;
using System.Collections.Generic;
using DwarfDB.DwarfCommand;
using DwarfDB.TransactionJournals;

namespace DwarfDB.Transactions
{
	/// <summary>
	/// DwarfTransaction - transaction without read blocking
	/// </summary>
	public class DwarfTransaction : IDisposable
	{
		public DwarfTransaction()
		{
		}
		
		public DwarfTransaction( Command cmd )
		{
			AddCommand( cmd );
		}
		
		/// <summary>
		/// Transaction object for atomic operations
		/// </summary>
		/// <returns></returns>
		public static DwarfTransaction Atomic( Command cmd ) {
			return new DwarfTransaction( cmd );
		}
		
		/// <summary>
		/// Add new command into a commands queue
		/// </summary>
		/// <param name="cmd"></param>
		public void AddCommand( Command cmd ) {
			if ( !is_atomic && commands_chain.Count == 0 )
				commands_chain.Enqueue(cmd);
		}
		
		
		public void Commit() {
			// TODO
		}
		
		public void Rollback() {
			// TODO
		}
		
		public void Abort() {
			if ( is_ongoing ) {
				// TODO
			}
		}
		
		#region IDisposable
		public void Dispose() {
			Abort();
			commands_chain.Clear();
		}
		#endregion
		
		~DwarfTransaction() {
			this.Dispose();
		}
		
		protected Journal current_journal = new Journal();
		protected Journal rollback_journal = new Journal();
		protected Queue<Command> commands_chain = new Queue<Command>();
		protected bool is_ongoing = false;
		protected bool is_atomic = false;
	}
}
