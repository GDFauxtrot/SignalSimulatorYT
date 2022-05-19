using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using ZenFulcrum.EmbeddedBrowser;

namespace SignalSimulatorYT
{
    class BrowserBehavior : MonoBehaviour
    {
        public const string BROWSER_EMPTY_PAGE = "data:text/html,<!DOCTYPE html>";
        public const int BROWSER_VIDEO_WIDTH = 1280;
        public const int BROWSER_VIDEO_HEIGHT = 720;
        public static float VOLUME_MAX_DIST = 16f;
        public static float OUTDOOR_VOLUME_SCALE = 0.6f;
        public static float OUTDOOR_FILTER_FREQ = 350f;
        public static float BROWSER_PAN_MAX = 0.7f;

        public static BrowserBehavior Instance { get; private set; }

        public Browser tvBrowser;
        public PointerUIMesh browserPointer;

        public float volume = 1f;

        public bool browserPageEmpty;
        public bool elementsHidden;
        public bool filtersApplied;
        public bool isPressingPlayPause;
        public bool playerIsOutdoors;

        #region Unity functions

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;

            // Set self up - sibling of TV with a specific Transform placement, mat, and a Browser component added on
            gameObject.transform.SetParent(TV.inst.transform.parent);
            gameObject.transform.localPosition = new Vector3(0.313f, 0f, 0.413f);
            gameObject.transform.localEulerAngles = new Vector3(270f, 0f, 90f);
            gameObject.transform.localScale = new Vector3(0.825f, 0.625f, 1f);

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

            // Make a quad mesh and apply it
            {
                Mesh mesh = new Mesh();

                Vector3[] vertices = new Vector3[4]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0)
                };
                mesh.vertices = vertices;

                int[] tris = new int[6]
                {
                    // lower left triangle
                    0, 2, 1,
                    // upper right triangle
                    2, 3, 1
                };
                mesh.triangles = tris;

                Vector3[] normals = new Vector3[4]
                {
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward,
                    -Vector3.forward
                };
                mesh.normals = normals;

                Vector2[] uv = new Vector2[4]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
                mesh.uv = uv;
                meshFilter.mesh = mesh;
            }

            // Set up Browser component
            tvBrowser = gameObject.AddComponent<Browser>();
            tvBrowser.Resize(BROWSER_VIDEO_WIDTH, BROWSER_VIDEO_HEIGHT);
            tvBrowser.onLoad += OnPageLoaded;

            // Set up Browser input (simulating mouse touches)
            browserPointer = gameObject.AddComponent<PointerUIMesh>();
            browserPointer.enableTouchInput = false;
            browserPointer.enableMouseInput = false;
            browserPointer.onHandlePointers += OnHandlePointers;

            // Set up outdoor triggers
            GameObject indoorTrigger1 = new GameObject("IndoorTrigger1");
            indoorTrigger1.layer = LayerMask.NameToLayer("Ignore Raycast");
            indoorTrigger1.transform.position = new Vector3(-33.2f, 25.2f, 154f);
            indoorTrigger1.transform.localScale = new Vector3(0.5f, 3f, 3.5f);
            indoorTrigger1.AddComponent<BoxCollider>().isTrigger = true;
            indoorTrigger1.AddComponent<Rigidbody>().isKinematic = true;
            indoorTrigger1.AddComponent<BrowserOutdoorTrigger>().isGoingOutdoors = false;

            GameObject indoorTrigger2 = new GameObject("IndoorTrigger2");
            indoorTrigger2.layer = LayerMask.NameToLayer("Ignore Raycast");
            indoorTrigger2.transform.position = new Vector3(-59f, 20f, 154f);
            indoorTrigger2.transform.localScale = new Vector3(1f, 5f, 3.5f);
            indoorTrigger2.AddComponent<BoxCollider>().isTrigger = true;
            indoorTrigger2.AddComponent<Rigidbody>().isKinematic = true;
            indoorTrigger2.AddComponent<BrowserOutdoorTrigger>().isGoingOutdoors = false;

            GameObject outdoorTrigger1 = new GameObject("OutdoorTrigger1");
            outdoorTrigger1.layer = LayerMask.NameToLayer("Ignore Raycast");
            outdoorTrigger1.transform.position = new Vector3(-31.8f, 25.2f, 151.8f);
            outdoorTrigger1.transform.localScale = new Vector3(1.8f, 3f, 0.5f);
            outdoorTrigger1.AddComponent<BoxCollider>().isTrigger = true;
            outdoorTrigger1.AddComponent<Rigidbody>().isKinematic = true;
            outdoorTrigger1.AddComponent<BrowserOutdoorTrigger>().isGoingOutdoors = true;

            GameObject outdoorTrigger2 = new GameObject("OutdoorTrigger2");
            outdoorTrigger2.layer = LayerMask.NameToLayer("Ignore Raycast");
            outdoorTrigger2.transform.position = new Vector3(-62f, 20f, 154f);
            outdoorTrigger2.transform.localScale = new Vector3(1f, 5f, 3.5f);
            outdoorTrigger2.AddComponent<BoxCollider>().isTrigger = true;
            outdoorTrigger2.AddComponent<Rigidbody>().isKinematic = true;
            outdoorTrigger2.AddComponent<BrowserOutdoorTrigger>().isGoingOutdoors = true;
        }

        void Update()
        {
            // Send volume+pan+filter information back to Chromium instance so the audio sounds natural
            if (filtersApplied)
            {
                // Not necessary to do every frame
                if (Time.frameCount % 4 == 0)
                {
                    Camera desiredCamera = Camera.main;
                    Vector3 posDifference = gameObject.transform.position - desiredCamera.transform.position;

                    // Scale volume along a curve instead of linearly by squaring the scaled volume
                    float volInWorldScaled = Mathf.Pow(
                        Mathf.Clamp01(1-(posDifference.magnitude / VOLUME_MAX_DIST)) * volume,
                        2f);

                    if (playerIsOutdoors)
                    {
                        volInWorldScaled *= OUTDOOR_VOLUME_SCALE;
                    }

                    // Maps perfectly to panning values in the AudioContext
                    float panRight = Vector3.Dot(desiredCamera.transform.right, posDifference.normalized);

                    tvBrowser.EvalJS(
$@"target.filter.frequency.value = {(playerIsOutdoors ? OUTDOOR_FILTER_FREQ : 24000)};
target.creategain.gain.value = {volInWorldScaled};
target.panner.pan.value = {panRight * BROWSER_PAN_MAX};").Done();
                }
            }
        }

        void OnDestroy()
        {
            // If we're the instance, null it out
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion
        #region Public functions

        public void OpenURL(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                link = BROWSER_EMPTY_PAGE;
            }

            tvBrowser.Url = link;

            browserPageEmpty = (link == BROWSER_EMPTY_PAGE);
            filtersApplied = false;
            elementsHidden = false;
        }

        public void PlayPauseBrowser()
        {
            isPressingPlayPause = true;
        }

        public void StopBrowser()
        {
            OpenURL(BROWSER_EMPTY_PAGE);
        }

        public void SetVolume(float newVol)
        {
            volume = Mathf.Clamp01(newVol);
        }

        public async void TrySkipVideo()
        {
            // Need to force focus first
            browserPointer.ForceKeyboardHasFocus(true);

            // Explicit key press/release sequence
            tvBrowser.PressKey(KeyCode.LeftShift, KeyAction.Press);
            tvBrowser.PressKey(KeyCode.N, KeyAction.Press);
            tvBrowser.PressKey(KeyCode.LeftShift, KeyAction.Release);
            tvBrowser.PressKey(KeyCode.N, KeyAction.Release);

            // Wait a frame before releasing focus
            await Task.Delay((int)(Time.deltaTime * 1000));

            browserPointer.ForceKeyboardHasFocus(false);
        }

        public void OutdoorTriggerEntered(bool isGoingOutdoors)
        {
            playerIsOutdoors = isGoingOutdoors;
        }

        #endregion
        #region Internal logic functions

        private void OnPageLoaded(JSONNode jsonNode)
        {
            if (!browserPageEmpty && !elementsHidden)
            {
                StartCoroutine(HideElements());
            }
        }

        private void OnHandlePointers()
        {
            if (isPressingPlayPause)
            {
                isPressingPlayPause = false;

                // LMAO THIS IS SO FUCKING SCUFFED
                // Send a mouse click exactly 60% to the right and 66% to the top of the screen in screen-space. yea
                // TODO replace this with position3D and Browser's world position + offset... please
                browserPointer.FeedPointerState(new PointerUIBase.PointerState()
                {
                    id = 1,
                    is2D = true,
                    position2D = new Vector2(Screen.width * 0.6f, Screen.height * 0.66f),
                    activeButtons = MouseButton.Left, // Click down
                    scrollDelta = Vector2.zero
                });
                browserPointer.FeedPointerState(new PointerUIBase.PointerState()
                {
                    id = 1,
                    is2D = true,
                    position2D = new Vector2(Screen.width * 0.6f, Screen.height * 0.66f),
                    activeButtons = 0, // Click up
                    scrollDelta = Vector2.zero
                });

                // Apply AudioContext filters if we haven't already
                if (!filtersApplied)
                {
                    StartCoroutine(ApplyFiltersLate());
                }
            }
        }

        IEnumerator HideElements()
        {
            // I literally cannot get a better way to wait until elements are available, it's a pain in the ass
            float fuckingShitTimer = 0f;
            while (fuckingShitTimer < 1f)
            {
                tvBrowser.EvalJS(
@"document.getElementsByClassName('ytp-chrome-bottom')[0].style.visibility = 'hidden';
document.getElementsByClassName('ytp-gradient-bottom')[0].style.visibility = 'hidden';
document.getElementsByClassName('ytp-pause-overlay')[0].style.visibility = 'hidden';
document.getElementsByClassName('ytp-copylink-button')[0].style.visibility = 'hidden';").Catch(null);

                fuckingShitTimer += Time.deltaTime;
                yield return null;
            }

            elementsHidden = true;
        }

        IEnumerator ApplyFiltersLate()
        {
            // Wait a few frames for page to register user input
            yield return null;
            yield return null;

            // Set up AudioContext for proper volume and panning capabilities
            tvBrowser.EvalJS(
@"var target = document.querySelector('video');
target.audiocontext = new AudioContext();
target.creategain = target.audiocontext.createGain();
target.panner = target.audiocontext.createStereoPanner();
target.filter = target.audiocontext.createBiquadFilter();
target.source = target.audiocontext.createMediaElementSource(target);
target.source.connect(target.creategain);
target.creategain.connect(target.filter);
target.filter.connect(target.panner);
target.panner.connect(target.audiocontext.destination);").Done();

            filtersApplied = true;
        }

        #endregion
    }
}
