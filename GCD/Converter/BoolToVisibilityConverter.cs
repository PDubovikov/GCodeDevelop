using System;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;


namespace GCD.Converter
{
//	[ValueConversion(typeof(bool), typeof(Visibility))]
	public sealed class BoolToVisibilityConverter : IValueConverter
	{
		
		public Visibility TrueValue {get; set;}
		public Visibility FalseValue {get; set;}
		
		
		public BoolToVisibilityConverter()
		{
			TrueValue = Visibility.Visible ;
			FalseValue = Visibility.Collapsed ;
		}
		
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(!(value is bool))
				return null ;
			
			return (bool)value ? TrueValue : FalseValue ;
		}
		
		
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(Equals(value, TrueValue))
				return true;
			
			if(Equals(value, FalseValue))
				return false ;
			
			return null ;
		}
	}
}
