/*
 * User: Igor
 * Date: 01/08/2016
 * Time: 00:54
 */
using System;
using System.Text.RegularExpressions;

namespace DwarfDB.Links
{
	/// <summary>
	/// A DC link parser
	/// Link: ip_address:db_name:dc_name
	/// </summary>
	public class DataContainerLinkParser : LinkParser
	{
		public DataContainerLinkParser() : base( @"(.*):(.*):(.*)" ) {
			
		}
		
		protected override void InitLink( String input, ILink link ) {
			try {
				var match = Regex.Match( input, RegExp );
				var address = match.Groups[0].Value;
				var db_name = match.Groups[1].Value;
				var dc_name = match.Groups[2].Value;
				link.Init( address, db_name, dc_name, null );
			} catch ( Exception ex ) {
				Errors.Messages.DisplayError( "DC link parsing: "+ex.Message );
			}
		}
	}
}
