/*
 * User: Igor
 * Date: 03.09.2015
 * Time: 23:36
 */
using System;
using System.Runtime.Serialization;

namespace DwarfDB.ChunkManager
{
	/// <summary>
	/// Description of ChunkException.
	/// </summary>
	public class ChunkException : Exception, ISerializable
	{
		public ChunkException()
		{
		}

	 	public ChunkException(string message) : base(message)
		{
		}

		public ChunkException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected ChunkException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}