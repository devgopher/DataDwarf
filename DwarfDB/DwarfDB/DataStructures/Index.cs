/*
 * User: Igor
 * Date: 24.08.2015
 * Time: 22:59
 */
using System;

namespace DwarfDB.DataStructures
{
	/// <summary>
	/// Index of data structure elements
	/// </summary>
	public class Index
	{
		public Index( IStructure _index_object )
		{
			index_object = _index_object;
		}
		
		private Index( )
		{
			index_object = null;
		}
		
		/// <summary>
		/// Creating hash code...
		/// </summary>
		private void MakeHashCode()
		{
			if ( index_object is DataContainer ) {
				var tmp = (index_object as DataContainer);
				hash_code = Crypto.ComputeHash.MD5Hash(tmp.Name + ":" + tmp.GetOwnerDB().Name);
			} else if ( index_object is Record ) {
				var tmp = (index_object as Record);
				hash_code = Crypto.ComputeHash.MD5Hash(tmp.OwnerDC.Name + ":" + tmp.Id.GetHashCode());
			} else
				hash_code = Crypto.ComputeHash.MD5Hash(index_object);
		}

		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			var other = obj as Index;
			if (other != null) {
				return this.HashCode == other.HashCode;
			}
			return false;
		}

		public static bool operator ==(Index lhs, Index rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Index lhs, Index rhs) {
			return !(lhs == rhs);
		}

		/*override public int GetHashCode() {
			return base.GetHashCode();
		}*/
		
		#endregion()
		
		static public Index CreateNew( IStructure _index_object ) {
			if ( _index_object == null )
				throw new IndexException( "object for indexing is NULL" );
			return new Index( _index_object );
		}
		
		static public Index CreateFromHashCode( string hash ) {
			var new_idx = new Index();
			new_idx.HashCode = hash;
			return new_idx;
		}
		
		public string HashCode {
			get {
				if ( hash_code == null )
					MakeHashCode();
				return hash_code;
			}
			set {
				hash_code = value;
			}
		}
		
		private string hash_code = null;
		protected IStructure index_object;
	}
}
