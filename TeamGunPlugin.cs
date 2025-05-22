using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;
using TeamGun.Menus;

namespace TeamGun;

[MinimumApiVersion(80)]
public class TeamGunPlugin : BasePlugin
{
    public override string ModuleName => "TeamGun";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Necmi";
    public override string ModuleDescription => "Pick a team and weapon, fight with colored teams";

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("[TeamGun] Plugin loaded!");

        Server.ExecuteCommand("mp_friendlyfire 0");

        AddCommand(
            "tg",
            "Opens team selection menu",
            (player, _) =>
            {
                TeamSelectMenu.Show(player, this);
            }
        );

        AddCommand(
            "based",
            "Shortcut for team selection",
            (player, _) =>
            {
                TeamSelectMenu.Show(player, this);
            }
        );
    }
}
