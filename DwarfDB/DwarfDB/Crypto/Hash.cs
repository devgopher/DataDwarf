using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using DwarfDB.DataStructures;

namespace DwarfDB.Crypto
{
	/// <summary>
	/// Класс для формирования и выдачи шифровального ключа
	/// </summary>
	static class ComputeHash
	{
		static public Int32 IntHash( object obj ) {
			return obj.GetHashCode();
		}
		
		/// <summary>
		/// Getting a MD hash sequence for datastructure
		/// </summary>
		/// <param name="dta_struct">Data Structure</param>
		/// <returns></returns>
		static public string MD5Hash( IStructure dta_struct )
		{
			var str_hash = new StringBuilder( 32 );
			var str_elements = new StringBuilder( 32 );
			byte[] data_bytes = null;
			if (dta_struct != null) {
				if ( dta_struct is DataContainer ) {
					foreach ( var clmn in  ((DataContainer)dta_struct).Columns ){
						str_elements.Append( clmn.Name );
						str_elements.Append( clmn.Type.ToString());
					}
					data_bytes = Encoding.UTF8.GetBytes(((DataContainer)dta_struct).Name +
					                                    str_elements.ToString());
				} else {
					if ( dta_struct is Record ) {
						// Let's assume, that Record Id is unique
						str_elements.Append(((Record)dta_struct).Id.ToString());
						data_bytes = Encoding.UTF8.GetBytes(((Record)dta_struct).Id.ToString() +
						                                    str_elements.ToString());
					}
				}
				
				foreach (byte b in new MD5CryptoServiceProvider().ComputeHash(data_bytes))
				{
					str_hash.Append(b.ToString("X2"));
				}
			} else
				throw new IOException( "Error in generating hash code: data structure is null" );
			return str_hash.ToString();
		}
		
		/// <summary>
		/// Getting a MD hash sequence
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		static public string MD5Hash( string input )
		{
			var str_hash = new StringBuilder( 32 );
			byte[] data_bytes = null;
			if (input != null) {
				data_bytes = Encoding.UTF8.GetBytes(input);
				foreach (byte b in new MD5CryptoServiceProvider().ComputeHash(data_bytes))
				{
					str_hash.Append(b.ToString("X2"));
				}
			} else
				throw new IOException( "Error in generating a hash code: input string is null" );
			return str_hash.ToString();
		}

		static public bool MD5Compare( String cmp, String orig )
		{
			return MD5Hash(cmp).Trim() == MD5Hash(orig).Trim();
		}
		
		static public bool MD5Compare( IStructure cmp_struct, IStructure orig_struct )
		{
			return MD5Hash(cmp_struct).Trim() == MD5Hash(orig_struct).Trim();
		}
	}
}