using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenFulcrum.EmbeddedBrowser;
using HarmonyLib;

namespace SignalSimulator
{
    [HarmonyPatch(typeof(BrowserCursor), "Load")]
    class Patch_BrowserCursor_Load
    {
        static bool loaded = AccessTools.StaticFieldRefAccess<BrowserCursor, bool>("loaded");
        static int size = AccessTools.StaticFieldRefAccess<BrowserCursor, int>("size");

        static bool Prefix(BrowserCursor __instance)
        {
            if (loaded)
            {
                return false;
            }
            size = 25;

            loaded = true;

            return false;
        }
    }

    [HarmonyPatch(typeof(BrowserCursor), nameof(BrowserCursor.SetActiveCursor))]
    class Patch_BrowserCursor_SetActiveCursor
    {
        static bool Prefix(BrowserCursor __instance)
        {
            return false;
        }
    }
}
