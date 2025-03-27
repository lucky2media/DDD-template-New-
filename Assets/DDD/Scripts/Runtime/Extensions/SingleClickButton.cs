
using System.Collections;

namespace UnityEngine.UI
{

    /// <summary>
    /// A Button that prevents accidental double-clicks by implementing a cooldown period.
    /// Can be used as a direct replacement for Unity's UI.Button.
    /// </summary>
    [AddComponentMenu("UI/Debounced Button")]
    public class SingleClickButton : Button
    {
        [Tooltip("Cooldown duration in seconds before the button can be clicked again")] [SerializeField]
        private float cooldownDuration = 0.2f;

        [Tooltip("Whether to disable the button visually during cooldown")] [SerializeField]
        private bool disableVisualsDuringCooldown = true;

        private bool isInCooldown;
        private Coroutine cooldownCoroutine;

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(HandleClick);
        }

        protected override void OnDestroy()
        {
            onClick.RemoveListener(HandleClick);
            base.OnDestroy();
        }

        private void HandleClick()
        {
            if (!isInCooldown)
            {
                if (cooldownCoroutine != null)
                {
                    StopCoroutine(cooldownCoroutine);
                }

                cooldownCoroutine = StartCoroutine(StartCooldown());
            }
        }

        private IEnumerator StartCooldown()
        {
            isInCooldown = true;

            if (disableVisualsDuringCooldown)
            {
                DoStateTransition(SelectionState.Disabled, true);
            }

            yield return new WaitForSeconds(cooldownDuration);

            if (disableVisualsDuringCooldown)
            {
                DoStateTransition(currentSelectionState, true);
            }

            isInCooldown = false;
            cooldownCoroutine = null;
        }

        /// <summary>
        /// Override of the Button's press handler to implement debouncing
        /// </summary>
        public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!isInCooldown)
            {
                base.OnPointerClick(eventData);
            }
        }

        /// <summary>
        /// Manually reset the cooldown state.
        /// </summary>
        public void ResetCooldown()
        {
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = null;
            }

            isInCooldown = false;

            if (disableVisualsDuringCooldown)
            {
                DoStateTransition(currentSelectionState, true);
            }
        }

        /// <summary>
        /// Get the current cooldown state
        /// </summary>
        public bool IsInCooldown => isInCooldown;

        /// <summary>
        /// Set or get the cooldown duration at runtime
        /// </summary>
        public float CooldownDuration
        {
            get => cooldownDuration;
            set => cooldownDuration = Mathf.Max(0f, value);
        }
    }
}