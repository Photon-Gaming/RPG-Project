using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using RPGGame;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for TimeSpanEdit.xaml
    /// </summary>
    public sealed partial class TimeSpanEdit : PropertyEditBox<TimeSpan>
    {
        public override string LabelText
        {
            get => propertyName.Text;
            set => propertyName.Text = value;
        }

        public override string LabelTooltip
        {
            get => (string)propertyName.ToolTip;
            set => propertyName.ToolTip = value;
        }

        public override PropertyInfo? Property { get; init; }

        public override TimeSpan Value
        {
            get => ConvertValueToTimeSpan(double.Parse(propertyValue.Text), (TimeUnit)timeUnitSelect.SelectedIndex);
            set => propertyValue.Text = ConvertTimeSpanToValue(value, (TimeUnit)timeUnitSelect.SelectedIndex)
                .ToString(CultureInfo.InvariantCulture);
        }

        public override object ObjectValue => Value;

        public override Predicate<TimeSpan> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => double.TryParse(propertyValue.Text, out double value)
            && ExtraValidityCheck(ConvertValueToTimeSpan(value, (TimeUnit)timeUnitSelect.SelectedIndex));

        public TimeSpanEdit(string labelText, string labelTooltip, PropertyInfo? property, TimeSpan initialValue,
            Predicate<TimeSpan> extraValidityCheck)
        {
            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            Value = initialValue;
        }

        private TimeSpan ConvertValueToTimeSpan(double value, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => TimeSpan.FromDays(value),
                TimeUnit.Hours => TimeSpan.FromHours(value),
                TimeUnit.Minutes => TimeSpan.FromMinutes(value),
                TimeUnit.Seconds => TimeSpan.FromSeconds(value),
                TimeUnit.Milliseconds => TimeSpan.FromMilliseconds(value),
                TimeUnit.Microseconds => TimeSpan.FromMicroseconds(value),
                TimeUnit.Ticks => TimeSpan.FromTicks((long)value),
                _ => throw new ArgumentException("Invalid value of unit")
            };
        }

        private double ConvertTimeSpanToValue(TimeSpan span, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => span.TotalDays,
                TimeUnit.Hours => span.TotalHours,
                TimeUnit.Minutes => span.TotalMinutes,
                TimeUnit.Seconds => span.TotalSeconds,
                TimeUnit.Milliseconds => span.TotalMilliseconds,
                TimeUnit.Microseconds => span.TotalMicroseconds,
                TimeUnit.Ticks => span.Ticks,
                _ => throw new ArgumentException("Invalid value of unit")
            };
        }

        private void propertyValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            Brush background = !IsValueValid ? Brushes.Salmon : Brushes.White;
            propertyValue.Background = background;
        }

        private void TimeUnitSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!double.TryParse(propertyValue.Text, out double value))
            {
                return;
            }

            // Value setter will reformat the value to the new unit
            Value = ConvertValueToTimeSpan(value, (TimeUnit)((ComboBoxItem)e.RemovedItems[0]!).Tag);
        }
    }
}
