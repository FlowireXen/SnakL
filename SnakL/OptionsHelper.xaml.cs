using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SnakL
{
    public partial class OptionsHelper : UserControl
    {
        public OptionsHelper(string Title, string PositionInfo, string CurrentValue, string Information)
        {
            InitializeComponent();
            this.Title.Content = Title;
            this.Position.Content = PositionInfo;
            this.Value.Text = CurrentValue;
            if (string.IsNullOrEmpty(Information))
                this.Info.Visibility = Visibility.Collapsed;
            else this.Info.Text = Information;
        }

        public int GetInt()
        {
            if (int.TryParse(this.Value.Text, out int result))
                return result;
            else return 0;
        }

        public double GetDouble()
        {
            if (double.TryParse(this.Value.Text, out double result))
                return result;
            else return 0.0;
        }

        private void NumberValidationTextbox(object sender, TextCompositionEventArgs e) => e.Handled = !IsDigitsOnly(e.Text);
        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
                if (c == '-' || c == '.' || c == ',')
                    return true;
                else if (c < '0' || c > '9')
                    return false;
            return true;
        }
    }
}
