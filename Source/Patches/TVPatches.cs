using System.Reflection;
using HarmonyLib;
using UnityEngine;
using SignalSimulatorYT.Shared;

namespace SignalSimulatorYT
{
    [HarmonyPatch(typeof(TV), nameof(TV.VhsUIOpen))]
    class Patch_TV_VhsUIOpen
    {
        static GameObject vhsUI = AccessTools.StaticFieldRefAccess<TV, GameObject>("vhsUI");
        static MethodInfo reloadVHSListRef = AccessTools.Method(typeof(TV), "ReloadVHSList");

        static void Postfix()
        {
            // Reloading save in the same play session makes this null
            if (!vhsUI)
            {
                vhsUI = AccessTools.StaticFieldRefAccess<TV, GameObject>("vhsUI");
            }

            // Turn off old UI, turn on new one
            vhsUI.SetActive(false);

            if (TV.MovieUIOpened)
            {
                // Turn on new UI, attach behavior component if not already on it
                VHSModUI.Instance.gameObject.SetActive(true);
                if (VHSModUI.Instance.gameObject.GetComponent<VHSModUIBehavior>() == null)
                {
                    VHSModUIBehavior behavior = VHSModUI.Instance.gameObject.AddComponent<VHSModUIBehavior>();
                }
            }
            else
            {
                // Turn off new UI
                VHSModUI.Instance.gameObject.SetActive(false);
            }

            // Force refresh VHS list (could be bad perf since it's an IO? idk)
            reloadVHSListRef.Invoke(TV.inst, null);
        }
    }

    [HarmonyPatch(typeof(TV), nameof(TV.StartStopTV))]
    class Patch_TV_StartStopTV
    {
        static bool Prefix(TV __instance)
        {
            if (BrowserBehavior.Instance && !BrowserBehavior.Instance.browserPageEmpty)
            {
                if (TV.inst.TVOn)
                {
                    BrowserBehavior.Instance.PlayPauseBrowser();
                }
                else
                {
                    BrowserBehavior.Instance.StopBrowser();
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(TV), nameof(TV.SaveVHSvol))]
    class Patch_TV_SaveVHSvol
    {
        static void Postfix(TV __instance)
        {
            if (BrowserBehavior.Instance)
            {
                BrowserBehavior.Instance.SetVolume(__instance.gameObject.GetComponent<AudioSource>().volume);
            }
        }
    }

    [HarmonyPatch(typeof(TV), nameof(TV.ForwardVideo))]
    class Patch_TV_ForwardVideo
    {
        static bool Prefix(TV __instance)
        {
            if (BrowserBehavior.Instance && !BrowserBehavior.Instance.browserPageEmpty)
            {
                BrowserBehavior.Instance.TrySkipVideo();
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(TV), "LoadMovies")]
    class Patch_TV_LoadMovies
    {
        static AccessTools.FieldRef<TV, VHSListContainer> vhsStructRef = AccessTools.FieldRefAccess<TV, VHSListContainer>("vhsStruct");

        static bool Prefix(TV __instance)
        {
            if (BrowserBehavior.Instance)
            {
                VHSModUI.Instance.gameObject.GetComponent<VHSModUIBehavior>().RemoveAllListItems();

                int vhsCount = vhsStructRef(__instance).dataList.Count;
                
                if (vhsCount <= 0)
                {
                    return true; // It's just going to early out in the OG anyways, exact same condition handled
                }
                
                for (int i = 0; i < vhsCount; ++i)
                {
                    string vhsLink = vhsStructRef(__instance).dataList[i].MovieLink.ToString();

                    VHSModUI.Instance.gameObject.GetComponent<VHSModUIBehavior>().AddNewListItem(vhsLink, false);
                }

                return false; // Don't run the OG JSON parsing, unnecessary work being done
            }
            return true;
        }
    }
}
