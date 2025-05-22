using System;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Menu;

namespace TeamGun.Menus;

public static class WeaponSelectMenu
{
    public static void Show(CCSPlayerController player, BasePlugin plugin, CsTeam selectedTeam)
    {
        var menu = new CenterHtmlMenu("Choose Your Weapon", plugin);

        void GiveGun(string weapon, bool hsOnly, CsTeam selectedTeam)
        {
            // Remove existing weapons
            foreach (var weap in player.PlayerPawn.Value.WeaponServices!.MyWeapons)
            {
                var entity = weap.Value;
                if (entity != null && entity.IsValid)
                    entity.Remove();
            }

            // Give new weapon
            foreach (
                var teammate in Utilities
                    .GetPlayers()
                    .Where(p => p.Team == selectedTeam && p.IsValid && !p.IsBot)
            )
            {
                var pawn = teammate.PlayerPawn?.Value;
                if (pawn == null)
                    continue;

                // Remove all weapons
                foreach (var weap in pawn.WeaponServices!.MyWeapons)
                {
                    var entity = weap.Value;
                    if (entity != null && entity.IsValid)
                        entity.Remove();
                }

                // Give new weapon
                teammate.GiveNamedItem(weapon);
                teammate.PrintToChat($"You got {weapon}" + (hsOnly ? " (HS only!)" : ""));
            }
            player.PrintToChat($"You got {weapon}" + (hsOnly ? " (HS only!)" : ""));
        }

        menu.AddItem("Deagle", (p, o) => GiveGun("weapon_deagle", false, selectedTeam));
        menu.AddItem("Deagle-HS", (p, o) => GiveGun("weapon_deagle", true, selectedTeam));
        menu.AddItem("USP-S", (p, o) => GiveGun("weapon_usp_silencer", false, selectedTeam));
        menu.AddItem("USP-S-HS", (p, o) => GiveGun("weapon_usp_silencer", true, selectedTeam));

        menu.Display(player, 0);

        plugin.AddTimer(
            0.2f,
            () =>
            {
                ColorizeTeam(selectedTeam);
            }
        );
    }

    private static void ColorizeTeam(CsTeam team)
    {
        var teamPlayers = Utilities
            .GetPlayers()
            .Where(p => p.Team == team && p.IsValid && !p.IsBot)
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        int half = teamPlayers.Count / 2;
        int extra = teamPlayers.Count % 2;

        for (int i = 0; i < teamPlayers.Count; i++)
        {
            var color =
                (i < half + extra)
                    ? System.Drawing.Color.FromArgb(255, 0, 0) // Red
                    : System.Drawing.Color.FromArgb(0, 0, 255); // Blue

            var pawn = teamPlayers[i].PlayerPawn?.Value;
            if (pawn != null)
            {
                pawn.Render = color;
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            }
        }
    }
}
