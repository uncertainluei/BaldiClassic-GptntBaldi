using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace UncertainLuei.BaldiClassic.GptntBaldi.Patches
{
    [HarmonyPatch(typeof(BullyScript))]
    public static class BullyPatches
    {
        private const float waitTimeMul = 0.25f;
        public static Vector3[] hallCells;

        [HarmonyPatch("Start"), HarmonyPatch("Reset"), HarmonyPostfix]
        private static void SetWaitTime(ref float ___waitTime)
        {
            if (GptntBaldiConfig.toggleBullyTweaks.Value)
                ___waitTime *= waitTimeMul; 
        }


        [HarmonyPatch("Activate"), HarmonyPrefix]
        private static bool Activate(BullyScript __instance)
        {
            if (!GptntBaldiConfig.toggleBullyTweaks.Value)
                return true;

            Vector3 playerPos = __instance.player.transform.position;
            playerPos.y = 5;
            float dist = 0;

            // Get hallway cells 10-40 units away from the player
            Vector3[] cells = hallCells.Where(x =>
            {
                dist = Vector3.Distance(x, playerPos);
                Debug.Log(dist);
                return dist >= 10 && dist < 30;
            }).ToArray();

            // Do regular bully spawning algorithm if player is in a classroom
            if (cells.LongLength == 0L)
                return true;

            __instance.transform.position = cells[Random.Range(0,cells.Length)];
            __instance.active = true;
            return false;
        }

        private static void PerformTimedTeleport(BullyScript bully, Collider other)
        {
            if (GptntBaldiConfig.toggleBullyTweaks.Value)
                bully.gc.StartCoroutine(TimedTeleport(5f, bully.wanderer, other.transform));
        }

        private static IEnumerator TimedTeleport(float time, AILocationSelectorScript wanderer, Transform target)
        {
            yield return new WaitForSeconds(time);
            wanderer.GetNewTarget();
            target.position = new(wanderer.transform.position.x, target.position.y, wanderer.transform.position.z);
            yield break;
        }

        private static readonly MethodInfo reset = AccessTools.Method(typeof(BullyScript), "Reset");
        private static readonly MethodInfo performTimedTeleport = AccessTools.Method(typeof(BullyPatches), "PerformTimedTeleport");

        [HarmonyPatch("OnTriggerEnter"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TriggerEnterTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                // this.Reset();
                if (!patched &&
                    i+1 < length &&
                    array[i].opcode    == OpCodes.Ldarg_0 &&
                    array[i+1].opcode  == OpCodes.Call &&
                    array[i+1].operand == reset)
                {
                    patched = true;
                    // PerformTimedTeleport(this, other);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, performTimedTeleport);
                }

                yield return array[i];
            }

            if (!patched)
                GptntBaldiPlugin.Log.LogError("Transpiler \"BullyPatches.TriggerEnterTranspiler\" wasn't properly applied!");

            yield break;
        }
    }
}