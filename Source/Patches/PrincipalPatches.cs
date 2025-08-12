using HarmonyLib;

namespace UncertainLuei.BaldiClassic.GptntBaldi.Patches
{
    [HarmonyPatch(typeof(PrincipalScript))]
    public static class PrincipalPatches
    {
        [HarmonyPatch("Start"), HarmonyPostfix]
        private static void OnStart(ref int ___detentions, int[] ___lockTime)
        {
            if (GptntBaldiConfig.togglePrincipalTweaks.Value)
                ___detentions = ___lockTime.Length-1;
        }
    }
}