/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 23:13
 */
using System;
using System.Collections.Generic;

namespace DwarfDB.TransactionJournals
{
	/// <summary>
	/// Description of TransactionJournals.
	/// </summary>
	public class Journal
	{
		public Journal()
		{
		}
		
		protected List<DwarfDB.DataStructures.IStructure> journal_objects = new List<DwarfDB.DataStructures.IStructure>();
		
	}
	
	
}
