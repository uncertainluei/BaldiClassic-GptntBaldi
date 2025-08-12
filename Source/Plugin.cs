using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UncertainLuei.BaldiClassic.GptntBaldi.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncertainLuei.BaldiClassic.GptntBaldi;

[BepInAutoPlugin(ModGuid, "GPTn't Baldi")]
public partial class GptntBaldiPlugin : BaseUnityPlugin
{
    private const string ModGuid = "io.github.uncertainluei.baldiclassic.gptntbaldi";

    internal static ManualLogSource Log { get; private set; }
        
    private void Awake()
    {
        Log = Logger;
        GptntBaldiConfig.BindConfig(Config);

        new Harmony(ModGuid).PatchAll();
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "School")
            return;

        TryGetBullyCells(scene.GetRootGameObjects());
    }

    private void TryGetBullyCells(GameObject[] rootObjects)
    {
        if (BullyPatches.hallCells != null && BullyPatches.hallCells.LongLength > 0L)
            return;

        GameObject env = rootObjects.First(x => x.name == "Environment");
        if (env == null) return;
        Transform transform = env.transform.Find("Halls");
        if (transform == null) return;

		int childCount = transform.childCount;
        Vector3 up = new(0,5);
        
        List<Vector3> cells = [];
		for (int i = 1; i < childCount; i++)
            if (transform.GetChild(i).position.y == 0)
			    cells.Add(transform.GetChild(i).position+up);
        BullyPatches.hallCells = cells.ToArray();
    }

    private static MethodInfo activateMethod = AccessTools.Method(typeof(BullyScript), "Activate");

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaytimeScript[] pts = Resources.FindObjectsOfTypeAll<PlaytimeScript>();
            if (pts.Length == 0) return;
            foreach (PlaytimeScript pt in pts)
            {
                if (pt.gameObject.scene == null) continue;
                pt.gameObject.SetActive(false);
                pt.transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;
                pt.gameObject.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            BullyScript[] bullies = Resources.FindObjectsOfTypeAll<BullyScript>();
            if (bullies.Length == 0) return;
            foreach (BullyScript bully in bullies)
            {
                if (bully.gameObject.scene == null) continue;
                bully.gameObject.SetActive(true);
                activateMethod.Invoke(bully, []);
            }
        }
    }
}
