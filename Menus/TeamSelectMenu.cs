using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Menu;

namespace TeamGun.Menus;

public static class TeamSelectMenu
{
    public static void Show(CCSPlayerController player, BasePlugin plugin)
    {
        var menu = new CenterHtmlMenu("Choose Your Team", plugin);

        menu.AddItem(
            "Team T",
            (p, o) =>
            {
                WeaponSelectMenu.Show(p, plugin, CsTeam.Terrorist);
            }
        );

        menu.AddItem(
            "Team CT",
            (p, o) =>
            {
                WeaponSelectMenu.Show(p, plugin, CsTeam.CounterTerrorist);
            }
        );

        menu.Display(player, 0);
    }
}
