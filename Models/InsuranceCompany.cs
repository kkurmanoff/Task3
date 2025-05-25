using System;
namespace RocketApp.Models;

public class InsuranceCompany
{
    public string Name { get; }
    public decimal Balance { get; private set; }
    public event EventHandler<decimal>? PayoutMade;

    public InsuranceCompany(string name, decimal initialBalance)
    {
        Name = name;
        Balance = initialBalance;
    }

    public void MakePayout(decimal amount)
    {
        if (Balance >= amount)
        {
            Balance -= amount;
            Console.WriteLine($"[{Name}] Выплата страховки: {amount:C}");
            PayoutMade?.Invoke(this, amount);
        }
        else
        {
            Console.WriteLine($"[{Name}] Недостаточно средств для выплаты {amount:C}");
        }
    }
}