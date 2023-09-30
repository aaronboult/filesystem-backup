using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace FilesystemBackup.View.Converter;

public enum RelationalOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

[ValueConversion(typeof(int), typeof(bool), ParameterType = typeof(int))]
public class RelationalOperatorConverter : IValueConverter
{
    public RelationalOperator Type { get; set; }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int dValue && parameter is string sParameter)
        {
            double dParameter = double.Parse(sParameter, CultureInfo.InvariantCulture);

            bool x = Type switch
            {
                RelationalOperator.Equal => dValue == dParameter,
                RelationalOperator.NotEqual => dValue != dParameter,
                RelationalOperator.GreaterThan => dValue > dParameter,
                RelationalOperator.GreaterThanOrEqual => dValue >= dParameter,
                RelationalOperator.LessThan => dValue < dParameter,
                RelationalOperator.LessThanOrEqual => dValue <= dParameter,

                _ => throw new NotImplementedException(),
            };

            Debug.WriteLine($"Type: {Type}, Value: {dValue}, Parameter: {dParameter}, Result: {x}");

            return x;
        }

        throw new ArgumentException($"Value and parameter must be of type int, not Value: {value.GetType()}, Parameter: {parameter.GetType()}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
