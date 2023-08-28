using UnityEngine;

namespace Extensions {
    public static class TransformExt {
        public static Transform Clear(this Transform transform) {
            foreach (Transform child in transform) Object.Destroy(child.gameObject);
            return transform;
        }
    }
}