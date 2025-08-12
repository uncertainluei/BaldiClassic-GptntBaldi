using BepInEx.Configuration;

namespace UncertainLuei.BaldiClassic.GptntBaldi
{
    internal static class GptntBaldiConfig
    {
        internal static ConfigEntry<bool> toggleBaldiTweaks;
        internal static ConfigEntry<bool> togglePlaytimeTweaks;
        internal static ConfigEntry<bool> togglePrincipalTweaks;
        internal static ConfigEntry<bool> toggleBullyTweaks;
        internal static ConfigEntry<bool> toggleYctpTweaks;


        internal static void BindConfig(ConfigFile config)
        {
            toggleBaldiTweaks = config.Bind(
                "Toggles.Npcs",
                "Baldi",
                true,
                "Enables all Baldi NPC-related 'enhancements'.");
            togglePlaytimeTweaks = config.Bind(
                "Toggles.Npcs",
                "Playtime",
                true,
                "Enables all Playtime-related 'enhancements'.");
            togglePrincipalTweaks = config.Bind(
                "Toggles.Npcs",
                "Principal",
                true,
                "Currently makes the Principal of the Thing always give you 99 seconds of detention. How fun!");
            toggleBullyTweaks = config.Bind(
                "Toggles.Npcs",
                "Bully",
                true,
                "Makes It's a Bully spawn as soon, and as close as possible to the player, and teleport you to a random location five seconds after taking an item.");
            toggleYctpTweaks = config.Bind(
                "Toggles",
                "YCTP",
                true,
                "Makes YCTP questions harder.");
        }
    }
}
