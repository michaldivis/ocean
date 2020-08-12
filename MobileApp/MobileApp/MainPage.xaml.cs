using Logic;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _model;
        public MainPage()
        {
            InitializeComponent();
            BindingContext = _model = new MainViewModel();
        }

        private void BtnAddRandomFish_Clicked(object sender, System.EventArgs e)
        {
            _model.AddRandomFish();
        }
    }
}
