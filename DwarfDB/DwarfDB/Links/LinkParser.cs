/*
 * User: Igor
 * Date: 01/08/2016
 * Time: 00:42
 */
using System;
using System.Text.RegularExpressions;

namespace DwarfDB.Links
{
	/// <summary>
	/// An interface for link parser
	/// </summary>
	public abstract class LinkParser
	{
		protected LinkParser() {}
		
		protected LinkParser( String regex ) {
			RegExp = regex;
		}
		
		protected String RegExp { get; private set; }
		
		protected virtual bool CheckMatch( String input ) {
			return Regex.IsMatch( input, RegExp );
		}
		
		protected abstract void InitLink( String input, ILink link );
		
		/// <summary>
		/// A template method, that defines main logic of a parsing process
		/// </summary>
		/// <param name="input"></param>
		/// <param name="link"></param>
		public void Parse( String input, ILink link ) {
			if ( CheckMatch( input ) )
				InitLink( input, link );
		}
	}
}
