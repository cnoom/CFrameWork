using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        #region 本地坐标

        public static Vector3 LocalPosition(this Transform transform) => transform.localPosition;
        public static float LocalPositionX(this Transform transform) => transform.localPosition.x;
        public static float LocalPositionY(this Transform transform) => transform.localPosition.y;
        public static float LocalPositionZ(this Transform transform) => transform.localPosition.z;

        public static Transform SetLocalPosition(this Transform transform, Vector3 pos)
        {
            transform.localPosition = pos;
            return transform;
        }

        public static Transform SetLocalPositionX(this Transform transform, float x)
            => transform.SetLocalPosition(new Vector3(x, transform.localPosition.y, transform.localPosition.z));

        public static Transform SetLocalPositionY(this Transform transform, float y)
            => transform.SetLocalPosition(new Vector3(transform.localPosition.x, y, transform.localPosition.z));

        public static Transform SetLocalPositionZ(this Transform transform, float z)
            => transform.SetLocalPosition(new Vector3(transform.localPosition.x, transform.localPosition.y, z));

        public static Transform AddLocalPositionX(this Transform transform, float x)
            => transform.SetLocalPositionX(transform.localPosition.x + x);

        public static Transform AddLocalPositionY(this Transform transform, float y)
            => transform.SetLocalPositionY(transform.localPosition.y + y);

        public static Transform AddLocalPositionZ(this Transform transform, float z)
            => transform.SetLocalPositionZ(transform.localPosition.z + z);

        #endregion

        #region 世界坐标

        public static Vector3 Position(this Transform transform) => transform.position;
        public static float PositionX(this Transform transform) => transform.position.x;
        public static float PositionY(this Transform transform) => transform.position.y;
        public static float PositionZ(this Transform transform) => transform.position.z;

        public static Transform SetPosition(this Transform transform, Vector3 pos)
        {
            transform.position = pos;
            return transform;
        }

        public static Transform SetPositionX(this Transform transform, float x)
            => transform.SetPosition(new Vector3(x, transform.position.y, transform.position.z));

        public static Transform SetPositionY(this Transform transform, float y)
            => transform.SetPosition(new Vector3(transform.position.x, y, transform.position.z));

        public static Transform SetPositionZ(this Transform transform, float z)
            => transform.SetPosition(new Vector3(transform.position.x, transform.position.y, z));

        public static Transform AddPositionX(this Transform transform, float x)
            => transform.SetPositionX(transform.position.x + x);

        public static Transform AddPositionY(this Transform transform, float y)
            => transform.SetPositionY(transform.position.y + y);

        public static Transform AddPositionZ(this Transform transform, float z)
            => transform.SetPositionZ(transform.position.z + z);

        #endregion

        #region 缩放设置

        public static Vector3 LocalScale(this Transform transform) => transform.localScale;
        public static float LocalScaleX(this Transform transform) => transform.localScale.x;
        public static float LocalScaleY(this Transform transform) => transform.localScale.y;
        public static float LocalScaleZ(this Transform transform) => transform.localScale.z;

        public static Transform SetLocalScaleX(this Transform transform, float x)
            => transform.SetLocalScale(new Vector3(x, transform.localScale.y, transform.localScale.z));

        public static Transform SetLocalScaleY(this Transform transform, float y)
            => transform.SetLocalScale(new Vector3(transform.localScale.x, y, transform.localScale.z));

        public static Transform SetLocalScaleZ(this Transform transform, float z)
            => transform.SetLocalScale(new Vector3(transform.localScale.x, transform.localScale.y, z));

        private static Transform SetLocalScale(this Transform transform, Vector3 scale)
        {
            transform.localScale = scale;
            return transform;
        }

        public static Transform SetLocalEulerAnglesX(this Transform transform, float x)
            => transform.SetLocalEulerAngles(new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z));

        public static Transform SetLocalEulerAnglesY(this Transform transform, float y)
            => transform.SetLocalEulerAngles(new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z));

        public static Transform SetLocalEulerAnglesZ(this Transform transform, float z)
            => transform.SetLocalEulerAngles(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z));

        private static Transform SetLocalEulerAngles(this Transform transform, Vector3 angles)
        {
            transform.localEulerAngles = angles;
            return transform;
        }

        #endregion


        //============== 重置操作 ==============//
        public static Transform ResetLocalPosition(this Transform transform)
            => transform.SetLocalPosition(Vector3.zero);

        public static Transform ResetLocalScale(this Transform transform)
            => transform.SetLocalScale(Vector3.one);
    }
}