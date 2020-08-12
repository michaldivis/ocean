using Logic;
using System.Windows;

namespace DesktopApp
{
    public partial class MainWindow : Window
    {
        private MainViewModel _model;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _model = new MainViewModel();
        }

        private void BtnAddFish_Click(object sender, RoutedEventArgs e)
        {
            _model.AddRandomFish();
        }
    }
}
