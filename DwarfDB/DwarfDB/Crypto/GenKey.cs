/*
 * Created by SharpDevelop.
 * User: Igor.Evdokimov
 * Date: 13.10.2015
 * Time: 13:18
 *
 */

using System;
using System.Text;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace DwarfDB
{
	/// <summary>
	/// A class for key generation
	/// </summary>
	public static class GenKey
	{
		public const string inner_key = "1020200102903d31DFWWEF#E$$Ev12FH";
		static NetworkInterface ni = null;
		public static PhysicalAddress phy_address = null;

		static GenKey() {
			foreach (var item in NetworkInterface.GetAllNetworkInterfaces()) {
				if (ni.OperationalStatus == OperationalStatus.Up) {
					phy_address = ni.GetPhysicalAddress ();
					break;
				}
			}
		}

		public static byte[] Generate() {
			// TODO: use AES
			byte[] ret = new byte[32];
			if ( phy_address == null )
				throw new GenKeyException("I can't get your MAC-address!");

			byte k1 = 1;
			
			foreach ( byte bk in  phy_address.GetAddressBytes()) {
				k1  |= bk ;
			}
						
			int i = 0;
			foreach ( var bt1 in  Encoding.UTF8.GetBytes (inner_key)) {
				bt1 |= k1;
				ret[i] = bt1;
				++i;
			}
			return ret;
		}
	}
}

