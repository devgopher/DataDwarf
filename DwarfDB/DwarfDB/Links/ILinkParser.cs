/*
 * User: Igor
 * Date: 01/08/2016
 * Time: 00:42
 */
using System;

namespace DwarfDB.Links
{
	/// <summary>
	/// An interface for link parser
	/// </summary>
	public interface ILinkParser
	{
		void Parse( String input, ILink link );
	}
}
