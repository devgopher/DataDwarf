/*
 * Пользователь: igor.evdokimov
 * Дата: 07.10.2015
 * Время: 12:16
 */
using System;
using System.Text;
using NUnit.Framework;
using DwarfDB.ChunkManager;
using DwarfDB.DataStructures;

namespace DwarfDB.UnitTests
{
	[TestFixture]
	public class GenKeyTest
	{
		[Test]
		public void Generate() {
			Assert.IsNotNull( GenKey.phy_address );
			var key = Encoding.UTF8.GetBytes (GenKey.inner_key) | GenKey.phy_address.GetAddressBytes();
			Assert.AreEqual( key, GenKey.Generate() );
		}
	}
}
