using UnityEngine;
using UnityEngine.UI;

namespace LivingPixels.Runtime.Views
{

    [RequireComponent(typeof(GridLayoutGroup))]
    public class DynamicGridLayout : MonoBehaviour
    {
        [SerializeField]
        private GridLayoutGroup _elementsHolderLayout;
        private RectTransform _elementsHolderTransform;


        private void Awake()
        {
            Init();
        }

        //call this externally if instantiated dynamically
        public void Init()
        {
            _elementsHolderTransform = GetComponent<RectTransform>();
            _elementsHolderLayout = GetComponent<GridLayoutGroup>();
        }

        /// <summary>
        /// optional setup if we object-pool the shit out of the game
        /// </summary>
        /// <param name="verticalPadding"></param>
        /// <param name="horizontalPadding"></param>
        /// <param name="spacing"></param>
        public void SetSpacing(int verticalPadding = 5, int horizontalPadding = 5, float spacing = 8)
        {
            _elementsHolderLayout.spacing = new Vector2(spacing, spacing);
            _elementsHolderLayout.padding = new RectOffset(verticalPadding, verticalPadding, horizontalPadding, horizontalPadding);
        }

        public void AutoAdjustCellSize(GameObject itemPrefab)
        {
            AutoAdjustCellSize(itemPrefab.GetComponent<RectTransform>());
        }

        public void AutoAdjustCellSize(RectTransform itemPrefab)
        {
            _elementsHolderLayout.cellSize = itemPrefab.sizeDelta;
            AdjustGridLayout(itemPrefab.sizeDelta.x);
        }

        /// <summary>
        /// Call this if tickets are already instantiated
        /// </summary>
        public void AutoAdjustCellSize()
        {
            if (transform.childCount == 0) return;

            // Get the first child's size as a reference
            RectTransform firstChild = transform.GetChild(0).GetComponent<RectTransform>();
            if (firstChild != null)
            {
                _elementsHolderLayout.cellSize = firstChild.sizeDelta;
            }
            AdjustGridLayout(firstChild.sizeDelta.x);
        }

        private void AdjustGridLayout(float ticketWidth)
        {
            // Get the available width of the parent (excluding padding)
            float availableWidth = _elementsHolderTransform.rect.width - _elementsHolderLayout.padding.left - _elementsHolderLayout.padding.right;

            // Calculate how many columns can fit within the available width
            int columns = Mathf.FloorToInt((availableWidth + _elementsHolderLayout.spacing.x) / (ticketWidth + _elementsHolderLayout.spacing.x));
            columns = Mathf.Max(1, columns); // Ensure at least one column

            // Update GridLayoutGroup settings
            _elementsHolderLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _elementsHolderLayout.constraintCount = columns;
        }
        
    }
}