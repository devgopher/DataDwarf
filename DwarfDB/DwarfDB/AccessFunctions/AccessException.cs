/*
 * User: Igor
 * Date: 28.10.2015
 * Time: 23:06
 */
using System;
using System.Runtime.Serialization;

namespace DwarfDB.AccessFunctions
{
	/// <summary>
	/// Description of AccessException.
	/// </summary>
	public class AccessException : Exception, ISerializable
	{
		public AccessException()
		{
		}

	 	public AccessException(string message) : base(message)
		{
		}

		public AccessException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected AccessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}