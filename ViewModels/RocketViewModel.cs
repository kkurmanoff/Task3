using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RocketApp.Models;

namespace RocketApp.ViewModels;

public partial class RocketViewModel : ViewModelBase
{
    [ObservableProperty]
    private Rocket _rocket;

    [ObservableProperty]
    private string _status = "Готов к запуску";

    [ObservableProperty]
    private int _left;

    [ObservableProperty]
    private int _top = 400; // Стартовая позиция (у земли)

    public RocketViewModel(Rocket rocket, int left, int top)
    {
        _rocket = rocket;
        _left = left;
        _top = top;
        
        rocket.RocketEvent += (sender, e) =>
        {
            Status = e switch
            {
                SimulationEvent.LaunchSuccess => "В полёте",
                SimulationEvent.LaunchFailure => "Авария!",
                SimulationEvent.Spacewalk => "В космосе",
                _ => Status
            };

            if (e == SimulationEvent.LaunchSuccess)
            {
                Task.Run(async () =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Top -= 3; // Двигаем вверх
                        await Task.Delay(100);
                    }
                });
            }
        };
    }
}