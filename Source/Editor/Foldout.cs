using UnityEngine;
using UnityEngine.UI;

namespace SignalSimulatorYT.Shared
{
    public class Foldout : MonoBehaviour
    {
        public const string FOLDOUT_DISABLED = "▸";
        public const string FOLDOUT_ENABLED  = "▾";
        public const float CONTENT_OFFSET_X = 30f;
        public const float CONTENT_OFFSET_Y = -4f;

        // Inspector-assigned stuff
        public bool startFoldedOut;

        public Button foldoutButton;
        public Text foldoutButtonTriangle;
        public GameObject foldoutPlaceholder;
        public RectTransform foldoutContentRect;

        [Range(0.0001f, 1f)]
        public float movementInterpolation;

        // Private (state)
        private bool isFoldedOut;

        void Awake()
        {
            if (foldoutButton)
            {
                foldoutButton.onClick.AddListener(OnFoldoutPressed);
            }

            if (startFoldedOut)
            {
                isFoldedOut = true;

                if (foldoutContentRect)
                {
                    foldoutContentRect.anchoredPosition = new Vector2(CONTENT_OFFSET_X, CONTENT_OFFSET_Y);
                }
            }
            else
            {
                isFoldedOut = false;

                if (foldoutContentRect)
                {
                    foldoutContentRect.anchoredPosition = new Vector2(CONTENT_OFFSET_X, CONTENT_OFFSET_Y + foldoutContentRect.sizeDelta.y);
                }
            }

            if (foldoutButtonTriangle)
            {
                foldoutButtonTriangle.text = isFoldedOut ? FOLDOUT_ENABLED : FOLDOUT_DISABLED;
            }
        }

        void FixedUpdate()
        {
            if (foldoutContentRect)
            {
                float targetY = 0f;

                if (isFoldedOut)
                {
                    targetY = CONTENT_OFFSET_Y;
                }
                else
                {
                    targetY = CONTENT_OFFSET_Y + foldoutContentRect.sizeDelta.y;
                }

                if (Mathf.Abs(foldoutContentRect.anchoredPosition.y - targetY) > 0.1f)
                {
                    foldoutContentRect.anchoredPosition = new Vector2(
                            CONTENT_OFFSET_X,
                            Mathf.Lerp(foldoutContentRect.anchoredPosition.y, targetY, movementInterpolation));
                
                    if (Mathf.Abs(foldoutContentRect.anchoredPosition.y - targetY) <= 0.1f)
                    {
                        foldoutContentRect.anchoredPosition = new Vector2(CONTENT_OFFSET_X, targetY);
                    }
                }
            }
        }

        private void OnFoldoutPressed()
        {
            isFoldedOut = !isFoldedOut;

            if (foldoutPlaceholder)
            {
                foldoutPlaceholder.SetActive(!isFoldedOut);
            }
            if (foldoutButtonTriangle)
            {
                foldoutButtonTriangle.text = isFoldedOut ? FOLDOUT_ENABLED : FOLDOUT_DISABLED;
            }
        }
    }
}
