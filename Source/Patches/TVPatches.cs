using HarmonyLib;
using UnityEngine;
using SignalSimulatorYT.Shared;

namespace SignalSimulatorYT
{
    [HarmonyPatch(typeof(TV), nameof(TV.VhsUIOpen))]
    class Patch_TV_VhsUIOpen
    {
        static GameObject vhsUI = AccessTools.StaticFieldRefAccess<TV, GameObject>("vhsUI");
        static VHSModUI vhsModUI = AccessTools.StaticFieldRefAccess<VHSModUI, VHSModUI>("Instance");

        static void Postfix(TV __instance)
        {
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
}
