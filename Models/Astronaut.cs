namespace RocketApp.Models;

public class Astronaut
{
    public string Name { get; }
    public bool IsInSpace { get; set; }

    public Astronaut(string name)
    {
        Name = name;
    }

    public void PerformSpacewalk()
    {
        IsInSpace = true;
    }

    public void ReturnToRocket()
    {
        IsInSpace = false;
    }
}