using System;
using System.Runtime.Serialization;

/// <summary>
/// Exception for datastructures
/// </summary>
[Serializable]
public class GenKeyException : Exception
{
	public GenKeyException()
	{
	}

	public GenKeyException(string message) : base(message)
	{
	}

	public GenKeyException(string message,
		Exception innerException) : base( message, innerException )
	{
	}


	// This constructor is needed for serialization.
	protected GenKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}