using UnityEngine;

namespace Extensions {
    public static class TransformExt {
        public static Transform Clear(this Transform transform) {
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                Object.Destroy(child.gameObject);
            }

            return transform;
        }
    }
}