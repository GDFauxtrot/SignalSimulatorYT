using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SignalSimulatorYT.Shared;
using HarmonyLib;
using UnityEngine.UI;

namespace SignalSimulatorYT
{
    class VHSModUIBehavior : MonoBehaviour
    {
        public List<GameObject> listItems;
        public int currentSelectedIndex = -1;

        public BrowserBehavior browserBehavior;

        void Awake()
        {
            listItems = new List<GameObject>();

            // Hide example entry
            VHSModUI.Instance.ExamplePresetListItem.SetActive(false);

            // Set up button callbacks
            VHSModUI.Instance.PresetSaveButton.onClick.AddListener(OnSavePressed);
            VHSModUI.Instance.OpenLinkButton.onClick.AddListener(OnOpenPressed);

            // TODO remove the load button! Hide it for now
            VHSModUI.Instance.PresetLoadButton.gameObject.SetActive(false);

            // Set up embedded browser
            GameObject tvBrowserObject = new GameObject("Browser");
            browserBehavior = tvBrowserObject.AddComponent<BrowserBehavior>();
        }

        public void AddNewListItem(string link, bool saveJson)
        {
            GameObject newItem = Instantiate(VHSModUI.Instance.ExamplePresetListItem);
            newItem.transform.SetParent(VHSModUI.Instance.PresetListContent);
            newItem.SetActive(true);
            newItem.transform.localScale = Vector3.one; // idk what the fuck happened here?

            VHSPresetListItem newItemData = newItem.GetComponent<VHSPresetListItem>();

            int i = listItems.Count;

            newItemData.backgroundButton.onClick.AddListener(delegate { OnListItemBackgroundPressed(i); });
            newItemData.deleteButton.onClick.AddListener(delegate { OnListItemDeletePressed(i, true); });

            newItemData.text.text = link;

            listItems.Add(newItem);

            if (saveJson)
            {
                WriteListToJson();
            }
        }

        public void RemoveAllListItems()
        {
            for (int i = listItems.Count-1; i >= 0; --i)
            {
                // Clear the list top-to-bottom to avoid list resorting nonsense
                OnListItemDeletePressed(i, false);
            }
        }

        void OnSavePressed()
        {
            if (!string.IsNullOrEmpty(VHSModUI.Instance.LinkInputField.text))
            {
                AddNewListItem(VHSModUI.Instance.LinkInputField.text, true);
                SetIndexSelected(listItems.Count-1);
            }
        }

        void OnOpenPressed()
        {
            string link = VHSModUI.Instance.LinkInputField.text;
            if (!string.IsNullOrEmpty(link))
            {
                ParseAndProcessLink(link);
            }
        }

        void OnListItemBackgroundPressed(int index)
        {
            SetIndexSelected(index);

            VHSModUI.Instance.LinkInputField.text = listItems[index].GetComponent<VHSPresetListItem>().text.text;
        }

        void OnListItemDeletePressed(int index, bool saveJson)
        {
            SetIndexSelected(-1);

            // Delete old entry
            GameObject.Destroy(listItems[index]);
            listItems.RemoveAt(index);

            VHSModUI.Instance.LinkInputField.text = "";

            // Rebuild button delegates
            for (int i = 0; i < listItems.Count; ++i)
            {
                int varI = i;
                listItems[i].GetComponent<VHSPresetListItem>().backgroundButton.onClick.RemoveAllListeners();
                listItems[i].GetComponent<VHSPresetListItem>().deleteButton.onClick.RemoveAllListeners();
                listItems[i].GetComponent<VHSPresetListItem>().backgroundButton.onClick.AddListener(
                        delegate { OnListItemBackgroundPressed(varI); });
                listItems[i].GetComponent<VHSPresetListItem>().deleteButton.onClick.AddListener(
                        delegate { OnListItemDeletePressed(varI, true); });
            }

            if (saveJson)
            {
                WriteListToJson();
            }
        }

        void SetIndexSelected(int index)
        {
            if (currentSelectedIndex != -1)
                listItems[currentSelectedIndex].GetComponent<VHSPresetListItem>().backgroundButton.OnDeselect(null);

            currentSelectedIndex = index;

            if (currentSelectedIndex != -1)
                listItems[currentSelectedIndex].GetComponent<VHSPresetListItem>().backgroundButton.OnSelect(null);
        }

        static InputField vhsInputField = AccessTools.StaticFieldRefAccess<TV, InputField>("vhsInputField");

        private void ParseAndProcessLink(string link)
        {
            string playlistFormat = "https://www.youtube-nocookie.com/embed/videoseries?list={0}&{1}";
            string videoFormat = "https://www.youtube-nocookie.com/embed/{0}?{1}";
            string extraArgs = "iv_load_policy=3&modestbranding=1&autoplay=1&loop=1";

            // User could've either entered an HTTP/HTTPS link, the www, or just the site name. These are all valid
            if (link.StartsWith("http") || link.StartsWith("www") || link.StartsWith("youtu"))
            {
                // Look for a playlist ID. If there's a valid one, we run with that instead of video ID
                string playlistID = "";
                string videoID = "";

                // Begin by looking for playlist
                if (link.Contains("list="))
                {
                    string[] linkSplit = link.Split('?', '&');

                    foreach (string spl in linkSplit)
                    {
                        if (spl.StartsWith("list="))
                        {
                            playlistID = spl.Substring(5, spl.Length-5);

                            // Don't allow playlists that don't start with "PL" (YouTube rule - mix playlists don't work here)
                            if (!playlistID.StartsWith("PL"))
                                playlistID = "";
                            break;
                        }
                    }
                }

                // If no playlist found, then look for video ID
                if (playlistID == "")
                {
                    int videoIDLen = 11;

                    // Longhand link - ID is a "v=" arg
                    if (link.Contains("/watch?"))
                    {
                        int idIndex = link.IndexOf("v=");

                        if (idIndex != -1)
                        {
                            videoID = link.Substring(idIndex+2, videoIDLen);
                        }
                    }
                    else // Shorthand link - ID proceeds the first slash
                    {
                        int urlIndex = link.IndexOf("youtu.be/");

                        if (urlIndex != -1)
                        {
                            videoID = link.Substring(urlIndex+9, videoIDLen);
                        }
                    }
                }

                if (playlistID != "")
                {
                    TV.inst.TVOn = false;
                    TV.inst.StartStopTV();
                    vhsInputField.text = "";

                    browserBehavior.OpenURL(string.Format(playlistFormat, playlistID, extraArgs));

                    // Need to reset TV "open" state
                    TV.VhsUIOpen();

                }
                else if (videoID != "")
                {
                    TV.inst.TVOn = false;
                    TV.inst.StartStopTV();
                    vhsInputField.text = "";

                    browserBehavior.OpenURL(string.Format(videoFormat, videoID, extraArgs)); 

                    // Need to reset TV "open" state
                    TV.VhsUIOpen();
                }
            }
            else
            {
                // Reset browser behavior
                browserBehavior.OpenURL("");

                // Force alignment with OG vhsStruct and our own list
                WriteListToVHSStruct();

                // Treat it like a file link and pass it off to the TV for handling
                vhsInputField.text = link;
                TV.inst.ApplyButtonTV();
            }
        }

        void WriteListToVHSStruct()
        {
            AccessTools.FieldRef<TV, VHSListContainer> vhsStructRef = AccessTools.FieldRefAccess<TV, VHSListContainer>("vhsStruct");

            vhsStructRef(TV.inst).dataList.Clear();
            for (int i = 0; i < listItems.Count; ++i)
            {
                VHSPresetListItem listItem = listItems[i].GetComponent<VHSPresetListItem>();
                vhsStructRef(TV.inst).dataList.Add(new VHSstruct(listItem.text.text));
            }
        }

        void WriteListToJson()
        {
            // Format list to Signal Sim's structs/data and write the exact same way it normally would
            VHSListContainer vhsStruct = default;
            vhsStruct.dataList = new List<VHSstruct>();

            for (int i = 0; i < listItems.Count; ++i)
            {
                VHSPresetListItem listItem = listItems[i].GetComponent<VHSPresetListItem>();
                vhsStruct.dataList.Add(new VHSstruct(listItem.text.text));
            }

            string contents = JsonUtility.ToJson(vhsStruct, true);
            File.WriteAllText(Application.persistentDataPath + "/VHS.json", contents);
        }
    }
}
