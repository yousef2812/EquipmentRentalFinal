using EquipmentProject.Services;

namespace EquipmentProject
{
    public partial class App : Application
    {
        private readonly ApiHostService _apiHostService;
        private readonly MainPage _mainPage;

        public App(MainPage mainPage, ApiHostService apiHostService)
        {
            InitializeComponent();

            _mainPage = mainPage;
            _apiHostService = apiHostService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(_mainPage)
            {
                Title = "EquipmentProject"
            };

            window.Created += async (_, _) => await _apiHostService.EnsureApiReadyAsync();
            window.Destroying += (_, _) => _apiHostService.StopOwnedProcess();

            return window;
        }
    }
}
