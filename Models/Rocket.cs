using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RocketApp.Models;

public class Rocket
{
    public string Name { get; }
    public int Altitude { get; private set; }
    public bool IsLaunched { get; private set; }
    public bool IsExploded { get; private set; }
    public List<Astronaut> Astronauts { get; } = new();
    public double FailureProbability { get; set; } = 0.2; // 20% chance of failure
    
    public event EventHandler<SimulationEvent>? RocketEvent;

    public Rocket(string name)
    {
        Name = name;
    }

    public async Task LaunchAsync(CancellationToken cancellationToken)
{
    if (IsLaunched || IsExploded) 
    {
        Console.WriteLine($"[{Name}] Ракета уже летала или взорвалась и не может быть запущена снова");
        return;
    }
    
    IsLaunched = true;
    Console.WriteLine($"[{Name}] Запуск начат");
    
    try
    {
        // Симуляция обратного отсчета
        for (int i = 5; i > 0; i--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"[{Name}] Обратный отсчет: {i}");
            await Task.Delay(1000, cancellationToken);
        }

        // Определяем успех/провал
        var random = new Random();
        if (random.NextDouble() < FailureProbability)
        {
            IsExploded = true;
            Console.WriteLine($"[{Name}] АВАРИЯ! Ракета взорвалась");
            RocketEvent?.Invoke(this, SimulationEvent.LaunchFailure);
            return;
        }

        Console.WriteLine($"[{Name}] Успешный запуск!");
        RocketEvent?.Invoke(this, SimulationEvent.LaunchSuccess);
        
        // Симуляция подъема
        for (Altitude = 0; Altitude < 100; Altitude += 5)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"[{Name}] Высота: {Altitude} км");
            await Task.Delay(200, cancellationToken);
        }

        // Космонавты выходят в открытый космос
        Console.WriteLine($"[{Name}] Космонавты выходят в открытый космос");
        foreach (var astronaut in Astronauts)
        {
            astronaut.PerformSpacewalk();
            Console.WriteLine($"[{Name}] {astronaut.Name} в открытом космосе");
            RocketEvent?.Invoke(this, SimulationEvent.Spacewalk);
            await Task.Delay(1000, cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"[{Name}] Полёт прерван");
        throw; // Пробрасываем исключение дальше
    }
}
}