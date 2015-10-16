/*
 * Пользователь: igor.evdokimov
 * Дата: 07.10.2015
 * Время: 12:16
 */
using System;
using NUnit.Framework;

namespace DwarfDB.UnitTests
{
	[TestFixture]
	public class GenKeyTest
	{
		[Test]
		public void Generate() {
			Assert.IsNotNull( GenKey.phy_address );
		}
	}
}