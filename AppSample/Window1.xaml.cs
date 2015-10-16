/*
 * Пользователь: igor.evdokimov
 * Дата: 06.10.2015
 * Время: 13:57
 */
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using DwarfDB.DataStructures;

namespace AppSample
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class AppSampleWindow : Window
	{
		
		private int id_cntr = 140;
		
		private DwarfDB.ChunkManager.ChunkManager cm = null;
		private DataBase db = null;
		private DataContainer dc_employee_load = null;
		
		public AppSampleWindow()
		{
			InitializeComponent();
			Load();
		}
		
		private void Load() {
			cm = new DwarfDB.ChunkManager.ChunkManager();
			db = DwarfDB.DataStructures.DataBase.LoadFrom( "employees", cm );
			dc_employee_load = db.GetDataContainer( "employee" );
			GridLoad();
		}
		
		private void GridLoad() {
			var items_query = from x in dc_employee_load.GetRecords()
				orderby x.Id ascending
				select new  {id = x.Id, surname=x["Surname"].Value, name=x["Name"].Value};
			
			EmployeeGrid.ItemsSource = items_query.ToList();
		}
		
		void GoOn_Click(object sender, RoutedEventArgs e)
		{
			String _name = name.Text.Trim();
			String _surname = surname.Text.Trim();			
			Record new_rec = new Record( dc_employee_load );
			
			//new_rec.Id = ;
			new_rec["Name"].Value = _name;
			new_rec["Surname"].Value = _surname;
			new_rec.Id = dc_employee_load.NextId();
			dc_employee_load.AddRecord(new_rec);
			dc_employee_load.Save();
			
			GridLoad();
			
			++id_cntr;
		}
	}
}