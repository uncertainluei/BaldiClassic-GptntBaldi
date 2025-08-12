using HarmonyLib;

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;

namespace UncertainLuei.BaldiClassic.GptntBaldi.Patches
{
    [HarmonyPatch(typeof(JumpRopeScript))]
    public static class JumpRopePatches
    {
        private static bool failedOnce;

        private const int jumpRequirement = 10;
        private static readonly Vector2 ropeDelayMuls = new(0.5f,1.5f);
        private static readonly Vector2 ropeSpeedMuls = new(0.8f,2.1f);

        private static string CountText(string def)
            => GptntBaldiConfig.togglePlaytimeTweaks.Value ? ("/"+jumpRequirement) : def;

        [HarmonyPatch("OnEnable"), HarmonyPostfix]
        private static void ResetFailCount()
        {
            failedOnce = false;
        }

        [HarmonyPatch("Fail"), HarmonyPostfix]
        private static void ResetFailCount(JumpRopeScript __instance)
        {
            if (failedOnce)
            {
                if (__instance.ps.baldi != null && __instance.ps.baldi.isActiveAndEnabled)
                {
                    // Placeholder sound
                    __instance.ps.gc.GetComponent<AudioSource>().PlayOneShot(__instance.ps.baldi.slap);
                    __instance.ps.baldi.Hear(__instance.playtime.transform.position, 2f);
                }
                __instance.ps.DeactivateJumpRope();
                __instance.playtime.Disappoint();
                return;
            }
            failedOnce = true;
        }

        [HarmonyPatch("OnEnable"), HarmonyPatch("RopeHit"), HarmonyPostfix]
        private static void RandomizeDelay(JumpRopeScript __instance)
        {
            if (!GptntBaldiConfig.togglePlaytimeTweaks.Value) return;
            __instance.jumpDelay *= Random.Range(ropeDelayMuls[0], ropeDelayMuls[1]);
        }

        private static float RandomizeRopeSpeed(JumpRopeScript jumpRope, float originalSpeed)
        {
            if (!GptntBaldiConfig.togglePlaytimeTweaks.Value) return originalSpeed;

            float newSpeed = Random.Range(ropeSpeedMuls[0], ropeSpeedMuls[1]);
            jumpRope.rope.speed = newSpeed;
            return originalSpeed/newSpeed;
        }

        private static bool CheckJumpCount(int jumps, int num)
        {
            if (GptntBaldiConfig.togglePlaytimeTweaks.Value)
                num = jumpRequirement;
            return jumps >= num;   
        }

        private static readonly FieldInfo ropePosition = AccessTools.Field(typeof(JumpRopeScript), "ropePosition");
        private static readonly MethodInfo randomizeRopeSpeed = AccessTools.Method(typeof(JumpRopePatches), "RandomizeRopeSpeed");

        [HarmonyPatch("Update"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && !patched; i++)
            {
                yield return array[i];

                // ropePosition = 1f;
                if (i+2 < length &&
                    array[i].opcode    == OpCodes.Ldarg_0 &&
                    array[i+1].opcode  == OpCodes.Ldc_R4 &&
                    array[i+2].opcode  == OpCodes.Stfld &&
                    array[i+2].operand == ropePosition)
                {
                    patched = true;
                    yield return array[i];
                    yield return array[i+1];
                    yield return new CodeInstruction(OpCodes.Call, randomizeRopeSpeed);
                    yield return array[i+2];
                    i += 3;
                    break;
                }
            }
            for (; i < length; i++)
                yield return array[i];

            if (!patched)
                GptntBaldiPlugin.Log.LogError("Transpiler \"PlaytimePatches.UpdateTranspiler\" wasn't properly applied!");

            yield break;
        }

        private static readonly FieldInfo jumps = AccessTools.Field(typeof(JumpRopeScript), "jumps");
        private static readonly MethodInfo checkJumpCount = AccessTools.Method(typeof(JumpRopePatches), "CheckJumpCount");

        [HarmonyPatch("Success"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SuccessTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && !patched; i++)
            {
                yield return array[i];
                // ropePosition = 1f;
                if (i+3 < length &&
                    array[i].opcode    == OpCodes.Ldarg_0 &&
                    array[i+1].opcode  == OpCodes.Ldfld &&
                    array[i+1].operand == jumps &&
                    array[i+3].opcode  == OpCodes.Blt)
                {
                    patched = true;
                    yield return array[i+1];
                    yield return array[i+2];
                    yield return new CodeInstruction(OpCodes.Call, checkJumpCount);
                    yield return new CodeInstruction(OpCodes.Brfalse)
                    {
                        operand = array[i+3].operand
                    };
                    i += 4;
                    break;
                }
            }
            for (; i < length; i++)
            {
                yield return array[i];
            }

            if (!patched)
                GptntBaldiPlugin.Log.LogError("Transpiler \"PlaytimePatches.SuccessTranspiler\" wasn't properly applied!");

            yield break;
        }

        private static readonly FieldInfo jumpCountTxt = AccessTools.Field(typeof(JumpRopeScript), "jumpCount");
        private static readonly MethodInfo txtSet = AccessTools.DeclaredPropertySetter(typeof(TMP_Text), "text");
        private static readonly MethodInfo countText = AccessTools.Method(typeof(JumpRopePatches), "CountText");

        [HarmonyPatch("OnEnable"), HarmonyPatch("Success"), HarmonyPatch("Fail"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CounterTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0, j = 0;

            for (; i < length && !patched; i++)
            {
                yield return array[i];
                // jumpCount reference 
                if (i+7 >= length ||
                    array[i].opcode    != OpCodes.Ldarg_0   ||
                    array[i+1].opcode  != OpCodes.Ldfld     ||
                    array[i+1].operand != jumpCountTxt)
                    continue;

                for (j = i+3; j <= i+5; j++)
                {
                    if (j+3 < length &&
                        array[j+1].opcode  == OpCodes.Ldstr     &&
                        array[j+2].opcode  == OpCodes.Call      &&
                        array[j+3].opcode  == OpCodes.Callvirt  &&
                        array[j+3].operand == txtSet)
                    {
                        patched = true;
                        break;
                    }
                }
                if (!patched)
                    continue;

                for (int k = i+1; k < j+2; k++)
                    yield return array[k];
                i += j-i+2;
                yield return new CodeInstruction(OpCodes.Call, countText);
                break;
            }
            for (; i < length; i++)
                yield return array[i];

            if (!patched)
                GptntBaldiPlugin.Log.LogWarning("Transpiler \"PlaytimePatches.CounterTranspiler\" wasn't properly applied. This might be a result of an Assembly-CSharp mod modifying it.");

            yield break;
        }
    }
}