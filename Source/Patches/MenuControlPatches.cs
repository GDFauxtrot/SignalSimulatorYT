using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SignalSimulatorYT
{
    [HarmonyPatch(typeof(MenuControl), "InitMainMenuObjects")]
    class Patch_MenuControl_InitMainMenuObjects
    {
        static AccessTools.FieldRef<MenuControl, Text> gameVersionRef = AccessTools.FieldRefAccess<MenuControl, Text>("gameVersion");
        static void Postfix(MenuControl __instance)
        {
            gameVersionRef(__instance).text += $"\n{Constants.PLUGIN_NAME} (v.{Constants.PLUGIN_VERSION})";
        }
    }
}