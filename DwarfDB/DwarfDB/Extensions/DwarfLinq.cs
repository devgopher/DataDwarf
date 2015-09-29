/*
 * Пользователь: Igor.Evdokimov
 * Дата: 24.09.2015
 * Время: 15:40
 */
using System;
using System.Collections.Generic;
using System.Linq;
using DwarfDB.DataStructures;

namespace System.Linq.Dwarf
{
	/// <summary>
	/// Description of DwarfLinq.
	/// </summary>
	public static class DwarfEnumerable
	{
		static DwarfEnumerable()
		{
		}

		public static IEnumerable<Record>
			Select( this IEnumerable<Record> v) {
			foreach ( var s in v )
				yield return s;
		}		
	}
}
