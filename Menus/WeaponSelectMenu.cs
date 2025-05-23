using System;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Menu;

namespace TeamGun.Menus;

public static class WeaponSelectMenu
{
    public static void Show(CCSPlayerController player, BasePlugin plugin, CsTeam selectedTeam)
    {
        var menu = new CenterHtmlMenu("Choose Your Weapon", plugin);

        void GiveGun(string weapon, bool hsOnly, CsTeam selectedTeam, bool noscopeOnly = false)
        {
            foreach (
                var teammate in Utilities
                    .GetPlayers()
                    .Where(p => p.Team == selectedTeam && p.IsValid && !p.IsBot)
            )
            {
                var pawn = teammate.PlayerPawn?.Value;
                if (pawn == null)
                    continue;

                foreach (var weap in pawn.WeaponServices!.MyWeapons)
                {
                    var entity = weap.Value;
                    if (entity != null && entity.IsValid)
                        entity.Remove();
                }

                teammate.GiveNamedItem(weapon);
                teammate.PrintToChat(
                    $"You got {weapon}"
                        + (hsOnly ? " (HS only!)" : "")
                        + (noscopeOnly ? " (NoScope!)" : "")
                );

                // Set flags
                if (hsOnly)
                    TeamGunPlugin.HsOnlyPlayers[teammate.SteamID] = true;
                else
                    TeamGunPlugin.HsOnlyPlayers.Remove(teammate.SteamID);

                if (noscopeOnly)
                    TeamGunPlugin.NoScopeOnlyPlayers[teammate.SteamID] = true;
                else
                    TeamGunPlugin.NoScopeOnlyPlayers.Remove(teammate.SteamID);
            }
        }

        menu.AddItem("Deagle", (p, o) => GiveGun("weapon_deagle", false, selectedTeam));
        menu.AddItem("Deagle-HS", (p, o) => GiveGun("weapon_deagle", true, selectedTeam));
        menu.AddItem("USP-S", (p, o) => GiveGun("weapon_usp_silencer", false, selectedTeam));
        menu.AddItem("USP-S-HS", (p, o) => GiveGun("weapon_usp_silencer", true, selectedTeam));
        menu.AddItem(
            "SSG08 (NoScope)",
            (p, o) => GiveGun("weapon_ssg08", false, selectedTeam, noscopeOnly: true)
        );
        menu.AddItem(
            "AWP (NoScope)",
            (p, o) => GiveGun("weapon_awp", false, selectedTeam, noscopeOnly: true)
        );

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
