using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
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

    public static Dictionary<ulong, bool> HsOnlyPlayers = new();
    public static Dictionary<ulong, bool> NoScopeOnlyPlayers = new();

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("[TeamGun] Plugin loaded!");
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        Server.ExecuteCommand("mp_friendlyfire 1");

        // Start scope monitoring loop
        AddTimer(0.2f, MonitorNoScopePlayers, TimerFlags.REPEAT);

        AddCommand(
            "tg",
            "Opens team selection menu",
            (player, _) =>
            {
                TeamSelectMenu.Show(player, this);
            }
        );
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    private HookResult OnTakeDamage(DynamicHook hook)
    {
        var victim = hook.GetParam<CEntityInstance>(0);
        var info = hook.GetParam<CTakeDamageInfo>(1);

        if (victim.DesignerName != "cs_player_controller")
            return HookResult.Continue;

        var weapon = info.Ability.Value?.As<CBasePlayerWeapon>();
        if (weapon == null)
            return HookResult.Continue;

        var attacker = info.Attacker.Value?.As<CCSPlayerPawn>()?.Controller.Value;
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        // Only enforce HS-only if attacker is using HS-only gun
        if (
            TeamGunPlugin.HsOnlyPlayers.TryGetValue(attacker.SteamID, out bool isHsOnly) && isHsOnly
        )
        {
            if (info.GetHitGroup() != HitGroup_t.HITGROUP_HEAD)
                return HookResult.Handled; // block non-headshots
        }

        return HookResult.Continue;
    }

    private void MonitorNoScopePlayers()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            if (
                !TeamGunPlugin.NoScopeOnlyPlayers.TryGetValue(player.SteamID, out bool isNoScope)
                || !isNoScope
            )
                continue;

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                continue;

            if (pawn.IsScoped)
            {
                // Force unscope by switching weapon slots
                player.ExecuteClientCommand("slot3"); // Knife
                AddTimer(0.1f, () => player.ExecuteClientCommand("slot1")); // Switch back to primary
            }
        }
    }
}
