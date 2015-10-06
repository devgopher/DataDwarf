/*
 * Пользователь: igor.evdokimov
 * Дата: 06.10.2015
 * Время: 13:57
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using DwarfDB;

namespace AppSample
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
			Load();
		}
		
		private void Load() {
			var cm = new DwarfDB.ChunkManager.ChunkManager();
			var db = DwarfDB.DataStructures.DataBase.LoadFrom( "employees", cm );
			DwarfDB.DataStructures.DataContainer dc_employee_load = db.GetDataContainer( "employee" );
			
			var items_query = from x in dc_employee_load.GetRecords()
				select x["Surname"];
			
			EmployeeGrid.ItemsSource = items_query.ToList();
		}		
	}
}