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
			db = DataBase.LoadFrom("employees", cm);
			dc_employee_load = db.GetDataContainer("employee");
			dc_employee_load.AssignOwnerDB(db);
			GridLoad();
		}
		
		private void GridLoad() {
			var items_query = from x in dc_employee_load.GetRecords()
				orderby x.Id ascending
				select new  {
				id = x.Id,
				surname=x["Surname"].Value,
				name=x["Name"].Value,
				pos_id = x["PosId"].Value
			};
			
			EmployeeGrid.ItemsSource = items_query.ToList();
		}
		
		void GoOn_Click(object sender, RoutedEventArgs e)
		{
			String _name = name.Text.Trim();
			String _surname = surname.Text.Trim();
			var new_rec = new Record( dc_employee_load );
			
			new_rec["Name"].Value = _name;
			new_rec["Surname"].Value = _surname;
			new_rec.Id = dc_employee_load.NextId();
			dc_employee_load.AddRecord(new_rec);
			dc_employee_load.Save();
			
			GridLoad();
		}
		
		void Delete_Click(object sender, RoutedEventArgs e)
		{
			try {
				var cellInfo=EmployeeGrid.SelectedCells[0];
				var cls = cellInfo.Column.GetCellContent(cellInfo.Item);
				
				var query = from x in dc_employee_load.GetRecords()
					where x.Id == Int64.Parse((cls as System.Windows.Controls.TextBlock).Text)
					select x;
				
				var tt = query.ToArray();
				if ( tt.Any() ) {
					var rec = tt[0];
					dc_employee_load.RemoveRecord(rec);
					dc_employee_load.Save();
				}
			} catch ( Exception ex ) {
				MessageBox.Show(ex.Message+":"+ex.StackTrace);
			}
			
			GridLoad();
		}
	}
}