#pragma warning disable 0649

using System.Collections;
using ROOT.Scripts.Controllers;
using UnityEngine;
using Watermelon;

namespace Watermelon.JellyMerge
{
    public class CellBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Transform graphicsHolderTransform;

        private Animator animator;
        private IPool simpleCubePool;
        private IPool jellyCubePool;
        private JellyBehaviour jellyRef;
        private SimpleJellyBehaviour simpleJellyRef;

        private GraphicsType graphicsType;

        private bool disableAfterMove = false;
        private bool wasMoved = false;
        private bool showHitParticle = false;

        public bool locked = false;
        private int hideParameter;

        public enum GraphicsType
        {
            Physical,
            Simple,
        }

        private ColorId colorId;
        public ColorId ColorID
        {
            get { return colorId; }
        }

        private bool isBorder;
        public bool IsBorder
        {
            get { return isBorder; }
        }

        public bool IsMovable
        {
            get { return colorId != ColorId.None && !isBorder; }
        }

        public Vector2Int IndexPosition
        {
            get { return new Vector2Int((int)transformRef.position.x, (int)transformRef.position.z); }
        }

        public Vector3 Position
        {
            get { return transformRef.position; }
        }

        private static float animationTime = 0.2f;
        public static float AnimationTime
        {
            get { return animationTime; }
        }

        protected Transform transformRef;
        public Transform TF => transformRef ??= transform;

        public void Merge(ColorId id)
        {
            colorId = id;
            // change visual
        }

        protected void Awake()
        {
            transformRef = transform;
            animator = GetComponent<Animator>();

            hideParameter = Animator.StringToHash("Hide");
        }

        public void Init(ColorId cellColor, GraphicsType graphicsType)
        {
            disableAfterMove = false;
            transformRef.localScale = Vector3.one;

            colorId = cellColor;

            InitPools();
            InitGraphics(graphicsType);
        }

        private void InitPools()
        {
            simpleCubePool = PoolManager.GetPoolByName("SimpleCube");
            //jellyCubePool = PoolManager.GetPoolByName("JellyCube");
        }

        public void InitGraphics(GraphicsType graphicsType)
        {
            this.graphicsType = graphicsType;

            if (graphicsType == GraphicsType.Simple)
            {
                simpleJellyRef = simpleCubePool.GetPooledObject().SetPosition(transformRef.position).GetComponent<SimpleJellyBehaviour>();
                simpleJellyRef.Init(graphicsHolderTransform, colorId);
            }
            else
            {
                jellyRef = jellyCubePool.GetPooledObject().SetPosition(transformRef.position).GetComponent<JellyBehaviour>();
                jellyRef.Init(graphicsHolderTransform, colorId);
            }
        }

        public void Move(Vector2Int vector, bool disableAfterMove)
        {
            this.disableAfterMove = disableAfterMove;
            wasMoved = true;

            if (vector != Vector2Int.zero)
            {
                transformRef.DOMove(transformRef.position + new Vector3(vector.x, 0f, vector.y), animationTime).SetEasing(Ease.Type.SineIn).OnComplete(() => OnMovementComplete());

                if (graphicsType == GraphicsType.Simple)
                {
                    // calculating movement strength for apropriate animations blending depend on movement speed (distance)
                    float strength = Mathf.Clamp(vector.magnitude, 0f, 4f) / 5f * Random.Range(0.85f, 1.15f);
                    simpleJellyRef.PlayMoveAnimation(vector, strength);
                }
            }
            else
            {
                OnMovementComplete();
            }
        }

        public void OnMovementComplete()
        {
            if (disableAfterMove)
            {
                Disable();
            }
            else
            {
                transformRef.position = new Vector3(Mathf.RoundToInt(transformRef.position.x), 0f, Mathf.RoundToInt(transformRef.position.z));
            }

            wasMoved = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!disableAfterMove)
            {
                if (wasMoved)
                {
                    showHitParticle = true;
                }
                else
                {
                   // ShowHitParticle();
                }
            }
        }

        private void ShowHitParticle()
        {
            ParticleCase particle = ParticlesController.PlayParticle("HitParticle").SetPosition(transformRef.position);
            
             PlaceHitParticle(particle.ParticleSystem.transform, GamePlayController.Instance.LastMove);
            
             particle.ParticleSystem.GetComponent<ParticleSetuper>().SetMaterial(ColorsController.Instance.GetMaterial(colorId));
        }

        public void PlayHitAnimation(Vector2Int moveDirection)
        {
            StartCoroutine(HitAnimationCoroutine(moveDirection));
        }

        private IEnumerator HitAnimationCoroutine(Vector2Int moveDirection)
        {
            yield return new WaitForSeconds(animationTime * 0.9f);

            if (showHitParticle)
            {
                ShowHitParticle();
            }
            yield return new WaitForSeconds(animationTime * 0.1f);

            if (graphicsType == GraphicsType.Simple)
            {
                simpleJellyRef?.Bounce();
            }
            else
            {
                jellyRef?.Bounce();
            }
        }

        private void PlaceHitParticle(Transform particle1, Vector2Int moveDirection)
        {
            particle1.position = transformRef.position.SetY(0.465f) + new Vector3(moveDirection.x, 0, moveDirection.y) * -0.4f;

            if (moveDirection == Vector2Int.up)
            {
                particle1.localEulerAngles = new Vector3(0f, 0f, 0f);
            }
            else if (moveDirection == Vector2Int.right)
            {
                particle1.localEulerAngles = new Vector3(0f, 90f, 0f);
            }
            else if (moveDirection == Vector2Int.down)
            {
                particle1.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
            else if (moveDirection == Vector2Int.left)
            {
                particle1.localEulerAngles = new Vector3(0f, 270f, 0f);
            }
        }

        public void Hide()
        {
            animator.SetTrigger(hideParameter);
        }

        public void ShowDisappearedParticle()
        {
            ParticleCase particle = ParticlesController.PlayParticle("DisappearingParticle").SetPosition(transformRef.position);
            ParticleSystem particleSystem = particle.ParticleSystem;
            particleSystem.transform.eulerAngles = Vector3.right * 90f;

            ParticleSetuper particleSetuper = particleSystem.GetComponent<ParticleSetuper>();
            particleSetuper.SetMaterial(ColorsController.Instance.GetMaterial(colorId));
        }

        public void OnHideAnimationExit()
        {
            Disable();
        }

        private void Disable()
        {
            //LevelController.OnCellDeactivated();

            if (simpleJellyRef != null)
            {
                simpleJellyRef.Disable();
            }

            if (jellyRef != null)
            {
                jellyRef.Disable();
            }

            simpleJellyRef = null;
            jellyRef = null;

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (simpleJellyRef != null)
            {
                simpleJellyRef.Disable();
                simpleJellyRef = null;
            }

            if (jellyRef != null)
            {
                jellyRef.Disable();
                jellyRef = null;
            }
        }
    }
}