using ZenFulcrum.EmbeddedBrowser;
using HarmonyLib;

namespace SignalSimulator
{
    // Prevent BrowserCursor from loading assets. This lets us skip including things we don't need
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

    // Prevent this function from doing anything
    [HarmonyPatch(typeof(BrowserCursor), nameof(BrowserCursor.SetActiveCursor))]
    class Patch_BrowserCursor_SetActiveCursor
    {
        static bool Prefix(BrowserCursor __instance)
        {
            return false;
        }
    }
}
