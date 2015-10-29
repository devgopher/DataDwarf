/*
 * Пользователь: igor.evdokimov
 * Дата: 29.10.2015
 * Время: 17:11
 */
using System;
using System.Runtime.Serialization;

namespace DwarfDB.User
{
	/// <summary>
	/// Description of UserException.
	/// </summary>
	public class UserException : Exception, ISerializable
	{
		public UserException()
		{
		}

	 	public UserException(string message) : base(message)
		{
		}

		public UserException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected UserException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}