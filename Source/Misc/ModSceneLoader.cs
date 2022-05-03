using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SignalSimulatorYT
{
    static class ModSceneLoader
    {
        static AccessTools.FieldRef<MenuControl, GameObject> loadingCanvasRef =
                AccessTools.FieldRefAccess<MenuControl, GameObject>("loadingCanvas");

        public static void SceneChanged(string sceneName)
        {
            if (sceneName == "Game") //////////////////////////////////////////////////////////////////////////////
            {
                GameObject newVHSUI = GameObject.Instantiate(ModAssets.GetAssetFromBundle<GameObject>("VHSModUI"));
                newVHSUI.SetActive(false);
            }
        }
    }
}
