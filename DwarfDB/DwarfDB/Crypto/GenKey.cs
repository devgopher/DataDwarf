/*
 * Created by SharpDevelop.
 * User: Igor.Evdokimov
 * Date: 13.10.2015
 * Time: 13:18
 *
 */

using System;
using System.Text;
using System.Net.NetworkInformation;

namespace DwarfDB
{
	/// <summary>
	/// A class for key generation
	/// </summary>
	public static class GenKey
	{
		public const string inner_key = "1020200102903d31DFWWEF#E$$Ev12FH";
		public static PhysicalAddress phy_address = null;

		static GenKey() {
			foreach (var ni_item in NetworkInterface.GetAllNetworkInterfaces()) {
				if (ni_item.OperationalStatus == OperationalStatus.Up) {
					phy_address = ni_item.GetPhysicalAddress ();
					break;
				}
			}
		}

		public static byte[] Generate() {
			// TODO: use AES
			var ret = new byte[32];
			if ( phy_address == null )
				throw new GenKeyException("I can't get your MAC-address!");

			byte k1 = 1;
			
			foreach ( byte bk in phy_address.GetAddressBytes()) {
				k1  &= bk ;
			}
						
			int i = 0;
			foreach ( byte bt1 in Encoding.UTF8.GetBytes (inner_key)) {
				ret[i] = bt1;
				ret[i] += k1;
				++i;
			}
			return ret;
		}
	}
}