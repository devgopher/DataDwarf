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
	public class DataContainerLinkParser : ILinkParser
	{
		private String regexp =
			@"(.*):(.*):(.*)";
		
		public void Parse( String input, ILink link ) {
			try {				
				var match = Regex.Match( input, regexp );
				if ( match.Groups.Count == 4 ) {
					var address = match.Groups[0].Value;
					var db_name = match.Groups[1].Value;
					var dc_name = match.Groups[2].Value;
					link = (DataContainerLink)(DataContainerLink.Create(
						address,
						db_name,
						dc_name
					));
				}
			} catch ( Exception ex ) {
				Errors.Messages.Display( "DC link parsing: "+ex.Message );
			}
		}
	}
}
