using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Font
{
    /// <summary>
    /// WPF Simple Font Dialog
    /// </summary>
    public partial class FontDialog : Window
    {
        #region Member

        List<string> listFont = new List<string>();
        List<string> listFontStyle;
        List<FamilyTypeface> listFontTypeface;
        #endregion

        #region Property

        public FontFamily ResultFontFamily { get; private set; }

        public double ResultFontSize { get; private set; }

        public FamilyTypeface ResultTypeFace { get; private set; }
        #endregion

        #region Constructor

        public FontDialog()
        {
            InitializeComponent();

            Control control = this;
            ResultFontFamily = control.FontFamily;
            ResultFontSize = control.FontSize;
            ResultTypeFace = new FamilyTypeface();
            ResultTypeFace.Stretch = control.FontStretch;
            ResultTypeFace.Style = control.FontStyle;
            ResultTypeFace.Weight = control.FontWeight;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                if (item.FamilyNames.ContainsKey(cond))
                    listFont.Add(item.FamilyNames[cond]);
                else
                    listFont.Add(item.ToString());
            }
            listFont.Sort();
            lboxFont.ItemsSource = listFont;

            lboxFont.SelectedItem = control.FontFamily.ToString();
            lboxFont.ScrollIntoView(lboxFont.SelectedItem);
            textFont.Text = control.FontFamily.ToString();

            double[] listSize = { 8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 44, 48, 54, 60, 66, 72, 80, 88, 96 };
            lboxFontSize.ItemsSource = listSize;

            lboxFontSize.SelectedItem = FontSize;
            textFontSize.Text = control.FontSize.ToString();
        }

        public FontDialog(Control control)
        {
            InitializeComponent();

            ResultFontFamily = control.FontFamily;
            ResultFontSize = control.FontSize;
            ResultTypeFace = new FamilyTypeface();
            ResultTypeFace.Stretch = control.FontStretch;
            ResultTypeFace.Style = control.FontStyle;
            ResultTypeFace.Weight = control.FontWeight;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                if (item.FamilyNames.ContainsKey(cond))
                    listFont.Add(item.FamilyNames[cond]);
                else
                    listFont.Add(item.ToString());
            }
            listFont.Sort();
            lboxFont.ItemsSource = listFont;

            lboxFont.SelectedItem = control.FontFamily.ToString();
            lboxFont.ScrollIntoView(lboxFont.SelectedItem);
            textFont.Text = control.FontFamily.ToString();

            double[] listSize = { 8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 44, 48, 54, 60, 66, 72, 80, 88, 96 };
            lboxFontSize.ItemsSource = listSize;

            lboxFontSize.SelectedItem = FontSize;
            textFontSize.Text = control.FontSize.ToString();
        }
        #endregion

        #region Event Handler

        private void textFontSize_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                || e.Key == Key.Back
                || e.Key == Key.OemPeriod
                || e.Key == Key.Delete))
            {
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod)
            {
                if ((sender as TextBox).Text.IndexOf('.') > -1)
                    e.Handled = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ResultFontFamily = new FontFamily(listFont[lboxFont.SelectedIndex]);
            ResultFontSize = double.Parse(textFontSize.Text);
            ResultTypeFace = listFontTypeface[lboxFontStyle.SelectedIndex];

            DialogResult = true;
        }

        private void lboxFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            FontFamily family;
            List<string> tempListFontStyle = new List<string>();
            List<FamilyTypeface> tempListFontTypeface;
            try
            {
                tempListFontStyle = new List<string>();
                family = new FontFamily((sender as ListBox).SelectedItem as string);
                tempListFontTypeface = family.FamilyTypefaces.ToList();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            listFontTypeface = tempListFontTypeface;

            int selectIndex = -1;

            var cond = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentUICulture.Name);

            var list = family.GetTypefaces().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.FaceNames.ContainsKey(cond))
                {
                    tempListFontStyle.Add(item.FaceNames[cond]);
                }
                else
                {
                    tempListFontStyle.Add(item.FaceNames[System.Windows.Markup.XmlLanguage.GetLanguage("en-us")]);
                }

                if (family.ToString() == ResultFontFamily.ToString())
                {
                    if (item.Stretch == ResultTypeFace.Stretch
                        && item.Style == ResultTypeFace.Style
                        && item.Weight == ResultTypeFace.Weight)
                        selectIndex = i;
                }
            }
            listFontStyle = tempListFontStyle;
            lboxFontStyle.ItemsSource = listFontStyle;

            if (selectIndex > -1)
                lboxFontStyle.SelectedIndex = selectIndex;
            else
                lboxFontStyle.SelectedIndex = 0;

            textFont.TextChanged -= textFont_TextChanged;
            textFont.Text = family.ToString();
            textFont.TextChanged += textFont_TextChanged;
        }

        private void textFont_TextChanged(object sender, TextChangedEventArgs e)
        {
            string lower = textFont.Text.ToLower();

            foreach (var item in listFont)
            {
                if (item.ToLower().StartsWith(lower))
                {
                    lboxFont.SelectionChanged -= lboxFont_SelectionChanged;
                    lboxFont.SelectedItem = item;
                    lboxFont.SelectionChanged += lboxFont_SelectionChanged;

                    lboxFont.ScrollIntoView(item);
                    return;
                }
            }
        }

        private void lboxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textFontSize.Text = (sender as ListBox).SelectedItem.ToString();
        }

        private void textFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            string num = textFontSize.Text;

            foreach (var item in lboxFontSize.Items)
            {
                if (item.ToString() == num)
                {
                    lboxFontSize.SelectedItem = item;
                    lboxFontSize.ScrollIntoView(item);
                    return;
                }
            }
        }
        #endregion

        #region Function

        public void ApplyToControl(Control control)
        {
            control.FontFamily = ResultFontFamily;
            control.FontSize = ResultFontSize;
            control.FontStretch = ResultTypeFace.Stretch;
            control.FontStyle = ResultTypeFace.Style;
            control.FontWeight = ResultTypeFace.Weight;
        }
        #endregion
    }
}
