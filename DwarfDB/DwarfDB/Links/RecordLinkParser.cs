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
	/// A Record link parser
	/// Link: ip_address:db_name:dc_name:rec_hash
	/// </summary>
	public class RecordLinkParser : ILinkParser
	{
		private String regexp =
			@"(.*):(.*):(.*):(.*)";
		
		public void Parse( String input, ILink link ) {
			try {
				var rec_link = link as RecordLink;
				if ( rec_link == null )
					return;
				
				var match = Regex.Match( input, regexp );
				if ( match.Groups.Count == 4 ) {
					var address = match.Groups[0].Value;
					var db_name = match.Groups[1].Value;
					var dc_name = match.Groups[2].Value;
					var rec_hash = match.Groups[3].Value;
					rec_link = (RecordLink)(RecordLink.Create(
						address,
						db_name,
						dc_name,
						rec_hash
					));
				}
			} catch ( Exception ex ) {
				
			}
		}
	}
}
