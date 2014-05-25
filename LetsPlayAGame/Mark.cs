using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace LetsPlayAGame
{
    /// <summary>
    /// Mark represents the state of a single noughts and crosses grid coordinate.
    /// </summary>
    enum Mark : byte {Empty, Nought, Cross};

    public class MarkToString : MarkupExtension, IValueConverter
    {
        public MarkToString() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Mark)value)
            {
                case Mark.Nought:
                    return "O";
                case Mark.Cross:
                    return "X";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "O":
                    return Mark.Nought;
                case "X":
                    return Mark.Cross;
                default:
                    return Mark.Empty;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class MarkToColor : MarkupExtension, IValueConverter
    {
        private readonly string noughtColor;
        private readonly string crossColor;

        public MarkToColor() : this("#FFB1321B", "#FF5FA4C4") { }

        public MarkToColor(string noughtColor, string crossColor)
        {
            this.noughtColor = noughtColor;
            this.crossColor = crossColor;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Mark)value)
            {
                case Mark.Nought:
                    return noughtColor;
                case Mark.Cross:
                    return crossColor;
                default:
                    return "#00000000";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var s = (string)value;

            if (s == noughtColor) return Mark.Nought;
            else if (s == crossColor) return Mark.Cross;
            else return "#00000000";
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
