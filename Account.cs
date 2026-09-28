namespace SteamSwitcherApp;

public class Account
{
    public string? Displaygame { get; set; }
    public string? Name  { get; set; }  
    public string? Password { get; set; } 
    
    public Account(string nameGame, string name, string password)
    {
        Displaygame = nameGame;
        Name = name;
        Password = password;
    }

    public override string ToString()
    {
        return Displaygame ?? "";
    }
}