using Chat.MVVM.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Chat
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            
            this.DataContext = MainViewModel.GetInstance();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

       

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Focus();
            this.Close();
        }

        private void cp_SelectedColorChanged_1(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (cp.SelectedColor.HasValue)
            {
                Color C = cp.SelectedColor.Value;
                long Red = C.R;
                long Green = C.G;
                long Blue = C.B;
                long colorVal = Convert.ToInt64(Blue * (Math.Pow(256, 0)) + Green * (Math.Pow(256, 1)) + Red * (Math.Pow(256, 2)));
                string hexColor = "#" + colorVal.ToString("X");
                MainViewModel.GetInstance().UserNameColor = hexColor;
            }

        }


    }
}
