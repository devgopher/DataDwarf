/*
 * User: igor.evdokimov
 * Date: 24.02.2016
 * Time: 12:40
 */
using System;
using System.Runtime.Serialization;

namespace Logger
{
	public class MailLogElementException : Exception, ISerializable
	{
		public MailLogElementException()
		{
		}

	 	public MailLogElementException(string message) : base(message)
		{
		}

		public MailLogElementException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// This constructor is needed for serialization.
		protected MailLogElementException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}