/*
 * Created by SharpDevelop.
 * User: Igor
 * Date: 24.08.2015
 * Time: 22:40
 */
using System;

namespace DwarfDB
{
	partial class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			//DataTableChunkCreation();
			//DataBaseChunkCreation();
			Employees();
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}