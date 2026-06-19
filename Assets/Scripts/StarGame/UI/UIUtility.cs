using UnityEngine;

namespace SGF.UI.Framework
{
    public static class UIUtility
    {

        /// <summary>
        /// Brings the game object to the front.
        /// </summary>
        /// <param name="go">Game Object.</param>
        public static void BringToFront(GameObject go)
        {
            BringToFront(go, true);
        }

        /// <summary>
        /// Brings the game object to the front.
        /// </summary>
        /// <param name="go">The Game Object.</param>
        /// <param name="allowReparent">Should we allow the method to change the Game Object's parent.</param>
        public static void BringToFront(GameObject go, bool allowReparent)
        {
            Transform root = null;
            Canvas canvas = FindInParents<Canvas>(go);
            if (canvas != null) root = canvas.transform;

            // If the object has a parent canvas
            if (allowReparent && root != null)
                go.transform.SetParent(root, true);

            // Set as last sibling
            go.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Finds the component in the game object's parents.
        /// </summary>
        /// <returns>The component.</returns>
        /// <param name="go">Game Object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T FindInParents<T>(GameObject go) where T : UnityEngine.Component
        {
            if (go == null)
                return null;

            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }

            return comp;
        }
    }
}
