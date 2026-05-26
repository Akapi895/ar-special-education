using UnityEngine;

namespace Features.Activities
{
    /// <summary>
    /// Adds a lightweight idle animation to AR learning objects.
    /// </summary>
    public class ARAnimalPresentation : MonoBehaviour
    {
        [SerializeField] private bool useBuiltInAnimation = true;
        [SerializeField] private float bobAmplitude = 0f;
        [SerializeField] private float bobFrequency = 1.45f;
        [SerializeField] private float turnAmplitude = 7f;
        [SerializeField] private Vector2 actionIntervalRange = new Vector2(1.8f, 3.8f);
        [SerializeField] private float actionCrossFadeDuration = 0.16f;
        [SerializeField] private float hopHeight = 0f;
        [SerializeField] private float hopFrequency = 0.55f;
        [SerializeField] private float scalePulse = 0.025f;
        [SerializeField] private bool preventAirborneAnimations = true;

        private Quaternion baseLocalRotation;
        private Vector3 appliedBobOffset;
        private Vector3 baseLocalScale;
        private float phaseOffset;
        private bool usingBuiltInAnimation;
        private Animator animator;
        private int baseLayerIndex;
        private int shapeLayerIndex;
        private float nextActionTime;
        private float nextExpressionTime;

        [Header("Wandering Behavior")]
        [SerializeField] private bool enableWandering = false;
        [SerializeField] private float wanderRadius = 0.38f;
        [SerializeField] private float wanderSpeed = 0.2f;
        [SerializeField] private float wanderRotateSpeed = 5.0f;
        [SerializeField] private float minWanderWaitTime = 2.0f;
        [SerializeField] private float maxWanderWaitTime = 5.0f;
        [SerializeField] private bool snapWanderToGround;
        [SerializeField] private float groundProbeHeight = 1.5f;
        [SerializeField] private float groundProbeDistance = 3.5f;
        [SerializeField] private float groundOffset;
        [SerializeField] private float groundRenderClearance = 0.035f;
        [SerializeField] private float minimumGroundNormalY = 0.65f;

        private Vector3 originLocalPos;
        private Vector3 currentBaseLocalPos;
        private Vector3 targetLocalPos;
        private Quaternion currentBaseLocalRot;
        private bool isMoving;
        private float wanderWaitTimer;
        private bool useFixedWanderArea;
        private Vector3 fixedWanderCenterLocal;
        private float fixedWanderRadius;
        private bool hasGroundWorldY;
        private float groundWorldY;

        private static readonly string[] MainActionStates =
        {
            "Idle_A", "Idle_B", "Idle_C", "Eat", "Sit", "Spin", "Walk"
        };

        private static readonly string[] ExpressionStates =
        {
            "Eyes_Blink", "Eyes_Happy", "Eyes_Excited", "Eyes_LookUp", "Eyes_LookDown",
            "Eyes_LookIn", "Eyes_LookOut", "Eyes_Squint"
        };

        private static readonly string[] AirborneKeywords =
        {
            "fly", "flying", "flight", "glide", "hover", "soar", "swim", "floating", "float"
        };

        private void OnEnable()
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
            ResetBasePose();
        }

        private void OnDisable()
        {
            RemoveAppliedBobOffset();
        }

        private void Update()
        {
            if (enableWandering)
            {
                UpdateWandering();
            }

            if (usingBuiltInAnimation)
            {
                UpdateBuiltInAnimation();
                if (enableWandering)
                {
                    transform.localPosition = currentBaseLocalPos;
                    transform.localRotation = currentBaseLocalRot;
                }
                return;
            }

            float wave = Mathf.Sin(Time.time * bobFrequency + phaseOffset);
            float hopWave = Mathf.Max(0f, Mathf.Sin(Time.time * hopFrequency + phaseOffset));
            Vector3 nextBobOffset = Vector3.up * (wave * bobAmplitude + hopWave * hopWave * hopHeight);

            if (enableWandering)
            {
                transform.localPosition = currentBaseLocalPos + nextBobOffset;
                transform.localRotation = currentBaseLocalRot * Quaternion.Euler(0f, wave * turnAmplitude, Mathf.Sin(Time.time * 0.9f + phaseOffset) * 2f);
            }
            else
            {
                transform.localPosition += nextBobOffset - appliedBobOffset;
                appliedBobOffset = nextBobOffset;
                transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, wave * turnAmplitude, Mathf.Sin(Time.time * 0.9f + phaseOffset) * 2f);
            }
            transform.localScale = baseLocalScale * (1f + Mathf.Max(0f, wave) * scalePulse);
        }

        private void LateUpdate()
        {
            CorrectRenderableGroundPenetration();
        }

        public void ResetBasePose()
        {
            RemoveAppliedBobOffset();
            baseLocalRotation = transform.localRotation;
            baseLocalScale = transform.localScale;

            originLocalPos = SnapLocalPositionToGround(ClampLocalPositionToWanderArea(transform.localPosition));
            transform.localPosition = originLocalPos;
            currentBaseLocalPos = originLocalPos;
            currentBaseLocalRot = baseLocalRotation;
            isMoving = false;
            wanderWaitTimer = Random.Range(0.5f, minWanderWaitTime);

            usingBuiltInAnimation = TryInitializeBuiltInAnimation();
        }

        private bool TryInitializeBuiltInAnimation()
        {
            if (!useBuiltInAnimation)
            {
                return false;
            }

            animator = GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                if (preventAirborneAnimations && IsAirborneClipName(gameObject.name))
                {
                    animator.enabled = false;
                    return false;
                }

                animator.applyRootMotion = false;
                baseLayerIndex = 0;
                shapeLayerIndex = animator.layerCount > 1 ? 1 : -1;
                if (shapeLayerIndex >= 0)
                {
                    animator.SetLayerWeight(shapeLayerIndex, 1f);
                }

                if (!PlayRandomAvailableState(MainActionStates, baseLayerIndex, 0f))
                {
                    animator.enabled = false;
                    return false;
                }

                if (shapeLayerIndex >= 0)
                {
                    PlayRandomAvailableState(ExpressionStates, shapeLayerIndex, 0f);
                }

                ScheduleNextBuiltInAction();
                return true;
            }

            Animation legacyAnimation = GetComponentInChildren<Animation>();
            if (legacyAnimation == null)
            {
                return false;
            }

            AnimationState preferredState = null;
            foreach (AnimationState state in legacyAnimation)
            {
                if (state == null)
                {
                    continue;
                }

                if (preventAirborneAnimations && IsAirborneClipName(state.name))
                {
                    continue;
                }

                if (preferredState == null || IsPreferredClipName(state.name))
                {
                    preferredState = state;
                }

                if (IsIdleClipName(state.name))
                {
                    preferredState = state;
                    break;
                }
            }

            if (preferredState == null)
            {
                legacyAnimation.enabled = false;
                return false;
            }

            preferredState.wrapMode = WrapMode.Loop;
            preferredState.time = Random.value * preferredState.length;
            legacyAnimation.Play(preferredState.name);
            return true;
        }

        private void UpdateBuiltInAnimation()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                usingBuiltInAnimation = false;
                return;
            }

            if (Time.time >= nextActionTime)
            {
                if (!(enableWandering && isMoving))
                {
                    if (!PlayRandomAvailableState(MainActionStates, baseLayerIndex, actionCrossFadeDuration))
                    {
                        usingBuiltInAnimation = false;
                        return;
                    }

                    ScheduleNextBuiltInAction();
                }
                else
                {
                    // Postpone random idle actions during wandering movement
                    nextActionTime = Time.time + 1.0f;
                }
            }

            if (shapeLayerIndex >= 0 && Time.time >= nextExpressionTime)
            {
                PlayRandomAvailableState(ExpressionStates, shapeLayerIndex, actionCrossFadeDuration);
                nextExpressionTime = Time.time + Random.Range(1.2f, 2.8f);
            }
        }

        private void ScheduleNextBuiltInAction()
        {
            nextActionTime = Time.time + Random.Range(actionIntervalRange.x, actionIntervalRange.y);
            nextExpressionTime = Time.time + Random.Range(0.6f, 1.6f);
        }

        private bool PlayRandomAvailableState(string[] stateNames, int layerIndex, float fadeDuration)
        {
            if (animator == null || layerIndex < 0 || stateNames == null || stateNames.Length == 0)
            {
                return false;
            }

            int startIndex = Random.Range(0, stateNames.Length);
            for (int i = 0; i < stateNames.Length; i++)
            {
                string stateName = stateNames[(startIndex + i) % stateNames.Length];
                if (preventAirborneAnimations && IsAirborneClipName(stateName))
                {
                    continue;
                }

                int stateHash = Animator.StringToHash(stateName);
                if (!animator.HasState(layerIndex, stateHash))
                {
                    continue;
                }

                float normalizedStartTime = IsIdleClipName(stateName) ? Random.value : 0f;
                if (fadeDuration > 0f)
                {
                    animator.CrossFade(stateHash, fadeDuration, layerIndex, normalizedStartTime);
                }
                else
                {
                    animator.Play(stateHash, layerIndex, normalizedStartTime);
                }

                return true;
            }

            return false;
        }

        private static AnimationClip FindPreferredClip(AnimationClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            AnimationClip fallback = null;
            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null)
                {
                    continue;
                }

                if (fallback == null || IsPreferredClipName(clip.name))
                {
                    fallback = clip;
                }

                if (IsIdleClipName(clip.name))
                {
                    return clip;
                }
            }

            return fallback;
        }

        private static bool IsIdleClipName(string clipName)
        {
            return !string.IsNullOrEmpty(clipName)
                && clipName.ToLowerInvariant().Contains("idle");
        }

        private static bool IsPreferredClipName(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return false;
            }

            string lower = clipName.ToLowerInvariant();
            return lower.Contains("idle")
                || lower.Contains("bounce")
                || lower.Contains("jump")
                || lower.Contains("walk");
        }

        private static bool IsAirborneClipName(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return false;
            }

            string lower = clipName.ToLowerInvariant();
            for (int i = 0; i < AirborneKeywords.Length; i++)
            {
                if (lower.Contains(AirborneKeywords[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveAppliedBobOffset()
        {
            if (appliedBobOffset == Vector3.zero)
            {
                return;
            }

            transform.localPosition -= appliedBobOffset;
            appliedBobOffset = Vector3.zero;
        }

        public void SetWandering(bool enable)
        {
            enableWandering = enable;
        }

        public void ConfigureWandering(bool enable, float radius, float speed, float minWaitTime, float maxWaitTime, bool snapToGround)
        {
            enableWandering = enable;
            wanderRadius = Mathf.Max(0f, radius);
            wanderSpeed = Mathf.Max(0.01f, speed);
            minWanderWaitTime = Mathf.Max(0f, minWaitTime);
            maxWanderWaitTime = Mathf.Max(minWanderWaitTime, maxWaitTime);
            snapWanderToGround = snapToGround;
        }

        public void ConfigureFixedWanderArea(Vector3 centerLocalPosition, float radius)
        {
            useFixedWanderArea = true;
            fixedWanderCenterLocal = centerLocalPosition;
            fixedWanderRadius = Mathf.Max(0.1f, radius);
            wanderRadius = fixedWanderRadius;
        }

        private void UpdateWandering()
        {
            if (!enableWandering)
            {
                currentBaseLocalPos = originLocalPos;
                return;
            }

            if (isMoving)
            {
                Vector3 toTarget = targetLocalPos - currentBaseLocalPos;
                toTarget.y = 0f; // wander horizontally only
                float dist = toTarget.magnitude;

                if (dist > 0.015f)
                {
                    float moveStep = wanderSpeed * Time.deltaTime;
                    currentBaseLocalPos = Vector3.MoveTowards(currentBaseLocalPos, targetLocalPos, moveStep);
                    currentBaseLocalPos = SnapLocalPositionToGround(ClampLocalPositionToWanderArea(currentBaseLocalPos));

                    if (toTarget.sqrMagnitude > 0.0001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(toTarget, Vector3.up);
                        currentBaseLocalRot = Quaternion.Slerp(currentBaseLocalRot, targetRot, wanderRotateSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    isMoving = false;
                    wanderWaitTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);

                    if (usingBuiltInAnimation && animator != null)
                    {
                        PlayRandomAvailableState(new[] { "Idle_A", "Idle_B", "Idle_C", "Bounce", "Sit" }, baseLayerIndex, actionCrossFadeDuration);
                    }
                }
            }
            else
            {
                wanderWaitTimer -= Time.deltaTime;
                if (wanderWaitTimer <= 0f)
                {
                    targetLocalPos = SnapLocalPositionToGround(GetRandomWanderTargetLocal());
                    isMoving = true;

                    if (usingBuiltInAnimation && animator != null)
                    {
                        if (!PlayRandomAvailableState(new[] { "Walk", "Idle_A", "Idle_B", "Idle_C" }, baseLayerIndex, actionCrossFadeDuration))
                        {
                            PlayRandomAvailableState(MainActionStates, baseLayerIndex, actionCrossFadeDuration);
                        }
                    }
                }
            }
        }

        private Vector3 GetRandomWanderTargetLocal()
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 center = useFixedWanderArea ? fixedWanderCenterLocal : originLocalPos;
            Vector3 target = center + new Vector3(randomCircle.x, 0f, randomCircle.y);
            target.y = originLocalPos.y;
            return ClampLocalPositionToWanderArea(target);
        }

        private Vector3 ClampLocalPositionToWanderArea(Vector3 localPosition)
        {
            if (!useFixedWanderArea)
            {
                return localPosition;
            }

            Vector2 offset = new Vector2(
                localPosition.x - fixedWanderCenterLocal.x,
                localPosition.z - fixedWanderCenterLocal.z);

            if (offset.sqrMagnitude <= fixedWanderRadius * fixedWanderRadius)
            {
                return localPosition;
            }

            Vector2 clamped = offset.normalized * fixedWanderRadius;
            return new Vector3(
                fixedWanderCenterLocal.x + clamped.x,
                localPosition.y,
                fixedWanderCenterLocal.z + clamped.y);
        }

        private Vector3 SnapLocalPositionToGround(Vector3 localPosition)
        {
            if (!snapWanderToGround)
            {
                return localPosition;
            }

            Transform parent = transform.parent;
            Vector3 worldPosition = parent != null ? parent.TransformPoint(localPosition) : localPosition;
            Vector3 rayOrigin = worldPosition + Vector3.up * groundProbeHeight;
            float rayDistance = groundProbeHeight + groundProbeDistance;
            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                rayDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            bool foundGround = false;
            RaycastHit bestHit = default;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (!IsValidGroundHit(hit)
                    || hit.distance >= bestDistance)
                {
                    continue;
                }

                bestHit = hit;
                bestDistance = hit.distance;
                foundGround = true;
            }

            if (!foundGround)
            {
                return localPosition;
            }

            groundWorldY = bestHit.point.y;
            hasGroundWorldY = true;
            worldPosition.y = groundWorldY + groundOffset + groundRenderClearance - GetRenderableBottomOffsetFromRoot();
            return parent != null ? parent.InverseTransformPoint(worldPosition) : worldPosition;
        }

        private bool IsValidGroundHit(RaycastHit hit)
        {
            if (hit.collider == null
                || hit.collider.isTrigger
                || IsSelfHit(hit.collider)
                || IsOtherAnimalHit(hit.collider)
                || hit.normal.y < minimumGroundNormalY)
            {
                return false;
            }

            return true;
        }

        private bool IsSelfHit(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return false;
            }

            return hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform);
        }

        private bool IsOtherAnimalHit(Collider hitCollider)
        {
            ARAnimalPresentation animal = hitCollider != null
                ? hitCollider.GetComponentInParent<ARAnimalPresentation>()
                : null;
            return animal != null && animal != this;
        }

        private void CorrectRenderableGroundPenetration()
        {
            if (!snapWanderToGround || !hasGroundWorldY)
            {
                return;
            }

            if (!TryGetRenderableBounds(out Bounds bounds))
            {
                return;
            }

            float targetMinY = groundWorldY + groundOffset + groundRenderClearance;
            float penetration = targetMinY - bounds.min.y;
            if (penetration <= 0.001f)
            {
                return;
            }

            transform.position += Vector3.up * penetration;
            SyncBaseLocalYToCurrentTransform();
        }

        private void SyncBaseLocalYToCurrentTransform()
        {
            float localY = transform.localPosition.y;
            originLocalPos.y = localY;
            currentBaseLocalPos.y = localY;
            targetLocalPos.y = localY;
            fixedWanderCenterLocal.y = localY;
        }

        private float GetRenderableBottomOffsetFromRoot()
        {
            if (!TryGetRenderableBounds(out Bounds bounds))
            {
                return 0f;
            }

            float offset = bounds.min.y - transform.position.y;
            if (float.IsNaN(offset) || float.IsInfinity(offset))
            {
                return 0f;
            }

            return Mathf.Clamp(offset, -2f, 2f);
        }

        private bool TryGetRenderableBounds(out Bounds bounds)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            bool hasBounds = false;
            bounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || renderer is LineRenderer)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            return true;
        }
    }
}
