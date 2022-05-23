using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HarmonyLib;
using HarmonyLib.Tools;

namespace SignalSimulatorYT
{
    [BepInPlugin(Constants.PLUGIN_GUID, Constants.PLUGIN_NAME, Constants.PLUGIN_VERSION)]
    [BepInProcess("SignalSimulator.exe")]
    public class Main : BaseUnityPlugin
    {
        public static BaseUnityPlugin Instance;

        internal static ManualLogSource Log;

        // List of additional libraries used (loaded in order)
        internal static string[] additionalDlls = { "SignalSimulatorYT.Shared", "ZFBrowser" };

        void Awake()
        {
            Instance = this;

            // In order for ModLogger to work properly
            Log = Logger;

            // Debugging Harmony
            //HarmonyFileLog.Enabled = true;

            // Apply Harmony patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.PLUGIN_GUID);

            if (Harmony.HasAnyPatches(Constants.PLUGIN_GUID))
            {
                ModLogger.Log("Harmony patches successfully applied!");
            }

            // Load additional assemblies (if there are any)
            LoadAdditionalAssemblies();

            // Load custom assetbundle content
            StartCoroutine(ModAssets.InitAssets());

            // Success!
            ModLogger.Log($"Plugin {Constants.PLUGIN_GUID} is loaded!");

            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        void OnSceneChanged(Scene current, Scene next)
        {
            if (next.name == "Game")
            {
                GameObject newVHSUI = GameObject.Instantiate(ModAssets.GetAssetFromBundle<GameObject>("VHSModUI"));
                newVHSUI.SetActive(false);
            }
        }

        /// <summary>
        /// Load additional assemblies in the order defined
        /// </summary>
        private void LoadAdditionalAssemblies()
        {
            ModLogger.Log("Loading additional assemblies...");
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (string dllName in additionalDlls)
            {
                ModLogger.Log($"  {dllName}");
                Assembly.LoadFile(Path.Combine(currentDir, $"{dllName}.dll"));
            }
            ModLogger.Log("Assemblies loaded!");
        }
    }
}
