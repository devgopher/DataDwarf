/*
 * Пользователь: igor.evdokimov
 * Дата: 11.01.2016
 * Время: 10:08
 */
using System;

namespace Logger
{
	/// <summary>
	/// An interface for log elements
	/// </summary>
	public interface ILogElement
	{
		void Output( string input, string msg_type );
		void Output( string input, string msg_type, params object[] pars );
	}
}
