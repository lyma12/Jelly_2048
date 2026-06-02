using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.JellyMerge
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private static CameraController instance;

        [Header("Settings")]
        public float oneUnitMoveTime = 0.1f;

        [Header("References")]
        public Transform backPlaneTransform;

        private Camera cameraRef;
        private Transform transformRef;

        [Header("Padding")]
        [SerializeField] private float boardPadding = 1.5f;

        public static Vector2 FrustrumSize
        {
            get
            {
                float tiltRad = instance.transformRef.eulerAngles.x * Mathf.Deg2Rad;
                float dist    = tiltRad > 0.01f
                                ? instance.transformRef.position.y / Mathf.Sin(tiltRad)
                                : instance.transformRef.position.y;

                float frustumHeight = 2.0f * dist * Mathf.Tan(instance.cameraRef.fieldOfView * 0.5f * Mathf.Deg2Rad);
                float frustumWidth  = frustumHeight * instance.cameraRef.aspect;

                return new Vector2(frustumWidth, frustumHeight);
            }
        }

#if UNITY_EDITOR
        private Vector3 levelCenterCached;
        private Vector2 levelSizeCached;
#endif

        private void Awake()
        {
            instance = this;
            transformRef = transform;
            cameraRef = GetComponent<Camera>();
        }

        public static void Init(Vector3 levelCenter, Vector2 boardWorldSize, bool smoothMovement = false)
        {
            instance.InitCamera(levelCenter, boardWorldSize, smoothMovement);
        }

        private void InitCamera(Vector3 levelCenter, Vector2 boardWorldSize, bool smoothMovement = false)
        {
            float halfFov = cameraRef.fieldOfView * 0.5f * Mathf.Deg2Rad;

            // height cần để board vừa theo chiều ngang (giới hạn bởi aspect)
            float hForWidth = (boardWorldSize.x + boardPadding) * 0.5f
                              / (Mathf.Tan(halfFov) * cameraRef.aspect);

            // height cần để board vừa theo chiều dọc màn hình
            float hForDepth = (boardWorldSize.y + boardPadding) * 0.5f
                              / Mathf.Tan(halfFov);

            float cameraHeight = Mathf.Max(hForWidth, hForDepth);

            float tiltRad = transformRef.eulerAngles.x * Mathf.Deg2Rad;
            float zOffset = tiltRad > 0.01f
                ? cameraHeight / Mathf.Tan(tiltRad)
                : 0f;

            Vector3 position = levelCenter.SetY(cameraHeight).AddToZ(-zOffset);
            float animationTime = 0;

            if (smoothMovement)
            {
                float moveDistance = (transform.position - position).magnitude;

                if (moveDistance != 0)
                {
                    animationTime = moveDistance * oneUnitMoveTime;
                    transform.DOMove(position, animationTime);
                }
            }
            else
            {
                transform.position = position;
            }

            //backplane setup
            backPlaneTransform.position = levelCenter;
            
            Vector3 scale = new Vector3(boardWorldSize.x, boardWorldSize.y, 1);
            
            if (smoothMovement)
                backPlaneTransform.DOScale(scale, animationTime);
            else
                backPlaneTransform.localScale = scale;

#if UNITY_EDITOR
            instance.levelCenterCached = levelCenter;
            instance.levelSizeCached = boardWorldSize;
#endif
        }

    }
}