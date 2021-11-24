using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrainF____2_Interpreter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height <= 200)
                ((MainWindow)sender).Height = 200;
            
            if(e.NewSize.Width <= 250)
                ((MainWindow)sender).Width = 250;
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;
            string text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
            FormattedText ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(rtb.FontFamily, rtb.FontStyle, rtb.FontWeight, rtb.FontStretch), rtb.FontSize, Brushes.Black);
            rtb.Document.PageWidth = ft.Width + 14;
            rtb.HorizontalScrollBarVisibility = (rtb.Width < ft.Width + 14) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }
    }
}
