/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 22:56
 */
using System;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// An interface for DwarfDB data structures, such as:
	/// DataContainer and Record
	/// </summary>
	public interface IStructure
	{
		/// <summary>
		/// Save to file chunk
		/// </summary>
		/// <param name="filepath"></param>
		void Save();
		
		/// <summary>
		/// Load Element from file chunk
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		void Load( Index index );
		
		/// <summary>
		/// Load Element from file chunk directory
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="index"></param>
		// void LoadFromChunkDir( string dirpath, Index index );
		
		/// <summary>
		/// Setting up a transaction for this element
		/// </summary>
		/// <param name="InTransaction"></param>
		void SetTransaction(DwarfDB.Transactions.DwarfTransaction InTransaction);
		
		/// <summary>
		/// Getting an index for element
		/// </summary>
		/// <returns></returns>
		Index GetIndex();
		
		/// <summary>
		/// Building an index for element
		/// </summary>
		void BuildIndex();
		
		/// <summary>
		/// Cloning procedure for DataStructure
		/// with reassigning a new owner object( for DC! )
		/// </summary>
		/// <returns></returns>
		IStructure Clone();
		
		/// <summary>
		/// Unique element id
		/// </summary>
		Int64 Id { get; set; }
	}
}
