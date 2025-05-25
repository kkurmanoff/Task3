using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RocketApp.Models;

namespace RocketApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<RocketViewModel> _rockets = new();

    [ObservableProperty]
    private string _insuranceStatus = "Страховка: Готова";

    [ObservableProperty]
    private decimal _insuranceBalance = 1_000_000m;

    [ObservableProperty]
    private string _consoleOutput = "Система готова к работе\n";

    private InsuranceCompany _insuranceCompany;
    private CancellationTokenSource? _launchCts;
    private readonly object _consoleLock = new();

    public MainWindowViewModel()
    {
        _insuranceCompany = new InsuranceCompany("SpaceInsure", InsuranceBalance);
        _insuranceCompany.PayoutMade += (sender, amount) =>
        {
            InsuranceBalance = _insuranceCompany.Balance;
            InsuranceStatus = $"Страховка: Выплачено {amount:C}";
            AddToConsole($"Страховая выплата: {amount:C}");
        };
    }

    private void AddToConsole(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            ConsoleOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        });
    }

    [RelayCommand]
    private void AddRocket()
    {
        var rocket = new Rocket($"Ракета-{Rockets.Count + 1}");
        rocket.RocketEvent += (sender, e) =>
        {
            if (e == SimulationEvent.LaunchFailure)
            {
                _insuranceCompany.MakePayout(500_000m);
            }
        };

        rocket.Astronauts.Add(new Astronaut("Иван"));
        rocket.Astronauts.Add(new Astronaut("Анна"));

        var random = new Random();
        var rocketVm = new RocketViewModel(rocket, random.Next(100, 700), 400);
        
        Rockets.Add(rocketVm);
        AddToConsole($"Добавлена {rocket.Name} с космонавтами Иваном и Анной");
    }

    [RelayCommand]
private async Task LaunchAll()
{
    if (Rockets.Count == 0)
    {
        AddToConsole("Нет ракет для запуска!");
        return;
    }

    // Фильтруем только ракеты, которые еще не запускались
    var rocketsToLaunch = Rockets
        .Where(r => !r.Rocket.IsLaunched)
        .ToList();

    if (rocketsToLaunch.Count == 0)
    {
        AddToConsole("Нет новых ракет для запуска!");
        return;
    }

    AddToConsole($"Начинаем запуск {rocketsToLaunch.Count} ракет...");
    _launchCts?.Cancel();
    _launchCts = new CancellationTokenSource();
    
    try
    {
        var tasks = rocketsToLaunch
            .Select(r => Task.Run(() => LaunchRocket(r, _launchCts.Token)))
            .ToList();

        await Task.WhenAll(tasks);
        AddToConsole("Все ракеты завершили полёт");
    }
    catch (Exception ex)
    {
        AddToConsole($"Ошибка при запуске: {ex.Message}");
    }
}

    private async Task LaunchRocket(RocketViewModel rocketVm, CancellationToken token)
{
    // Перехватываем вывод консоли
    var originalOut = Console.Out;
    var writer = new ConsoleWriter(this);
    Console.SetOut(writer);

    try
    {
        AddToConsole($"{rocketVm.Rocket.Name}: Обратный отсчёт начат");
        await rocketVm.Rocket.LaunchAsync(token);
    }
    catch (TaskCanceledException)
    {
        AddToConsole($"{rocketVm.Rocket.Name}: Запуск отменён");
    }
    catch (Exception ex)
    {
        AddToConsole($"{rocketVm.Rocket.Name}: Ошибка: {ex.Message}");
    }
    finally
    {
        Console.SetOut(originalOut);
    }
}

    [RelayCommand]
    private void CancelLaunch()
    {
        AddToConsole("Отмена всех запусков...");
        _launchCts?.Cancel();
    }

    private class ConsoleWriter : System.IO.TextWriter
    {
        private readonly MainWindowViewModel _viewModel;

        public ConsoleWriter(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void WriteLine(string? value)
        {
            if (value != null)
                _viewModel.AddToConsole(value);
        }

        public override void Write(string? value)
        {
            if (value != null)
                _viewModel.AddToConsole(value);
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
}