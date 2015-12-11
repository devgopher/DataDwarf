/*
 * Пользователь: igor.evdokimov
 * Дата: 10.12.2015
 * Время: 11:41
 */
using System;
using System.Globalization;
using System.Windows.Media;
using System.Reflection;

namespace AppSample
{
	/// <summary>
	/// Description of GradientConverter.
	/// </summary>
	public class GradientConverter : System.Windows.Data.IValueConverter
	{
		public GradientConverter()
		{
		}
	
		LinearGradientBrush lgb = null;
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if ( value is String ) {
				var colors = ((String)value).Split('-');
				var col1 = new Color();
				var cconv1 = System.ComponentModel.TypeDescriptor.GetConverter( col1 );
				var col2 = new Color();
				var cconv2 = System.ComponentModel.TypeDescriptor.GetConverter( col2 );				
	
				col1 = (Color)(cconv1.ConvertFromString( colors[0] ));
				col2 = (Color)(cconv1.ConvertFromString( colors[1] ));
				lgb = new LinearGradientBrush( col1, col2, 45.0 );
				return lgb;
			}
			return null;
		}
		
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}
