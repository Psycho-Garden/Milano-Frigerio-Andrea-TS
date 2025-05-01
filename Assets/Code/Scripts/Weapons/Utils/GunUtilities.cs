using AF.TS.Utils;
using DG.Tweening;
using UnityEngine;

namespace AF.TS.Weapons
{
    public static class GunUtilities
    {

        public static void Flash(Transform transform, string prefabName, Vector3 position, Vector3 rotation)
        {
            ServiceLocator.Get<ObjectPooler>().Get(prefabName, 0.1f)
                    .transform.SetPositionAndRotation(
                    transform.TransformPoint(position),
                    transform.rotation * Quaternion.Euler(rotation)
                    );
        }

        public static void AnimateSlideFull(Transform slide, float distance, float duration)
        {
            float originalZ = slide.localPosition.x;
            float targetZ = originalZ + distance;

            DOTween.Sequence()
                .Append(slide.DOLocalMoveX(targetZ, duration * 0.5f).SetEase(Ease.OutQuad))
                .Append(slide.DOLocalMoveX(originalZ, duration * 0.5f).SetEase(Ease.InQuad));
        }

        public static void AnimateSlide(Transform slide, float distance, float duration)
        {
            float originalZ = slide.localPosition.x;
            float targetZ = originalZ + distance;

            DOTween.Sequence()
                .Append(slide.DOLocalMoveX(targetZ, duration * 0.5f).SetEase(Ease.OutQuad));
        }

        public static void EjectCasing(Transform transform, string prefabName, Vector3 position, Vector3 rotation)
        {
            if (transform == null) return; // Ensure the source transform is valid

            var pooler = ServiceLocator.Get<ObjectPooler>();
            GameObject casing = pooler.Get(prefabName);

            if (casing == null) return; // Ensure the casing prefab was successfully retrieved

            Transform casingTransform = casing.transform;

            // Kill existing tweens (if any) on the casingTransform
            casingTransform.DOKill();

            // Set initial position and rotation
            Vector3 ejectPosition = transform.TransformPoint(position);
            Quaternion ejectRotation = transform.rotation * Quaternion.Euler(rotation);
            casingTransform.SetPositionAndRotation(ejectPosition, ejectRotation);

            // Calculate target position and movement direction
            Vector3 ejectDir = transform.right + transform.up * 0.5f + Random.insideUnitSphere * 0.2f;
            float distance = Random.Range(0.2f, 0.5f);
            Vector3 targetPos = ejectPosition + ejectDir * distance;

            float arcHeight = Random.Range(0.1f, 0.3f);
            float duration = 0.5f;

            Vector3 startPos = casingTransform.position;
            float t = 0f;

            // Animate casing movement with an arc trajectory
            var moveTween = DOTween.To(() => t, x => t = x, 1f, duration)
                .SetTarget(casingTransform)
                .OnUpdate(() =>
                {
                    if (casingTransform == null) return;

                    Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
                    float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
                    casingTransform.position = flatPos + Vector3.up * heightOffset;
                })
                .OnComplete(() =>
                {
                    if (casing != null)
                    {
                        casingTransform.DOKill(); // Ensure tweens are killed before returning to pool
                        pooler.ReturnToPool(casing);
                    }
                });

            // Random rotation animation
            var rotateTween = casingTransform
                .DORotate(new Vector3(360, 360, 360), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad)
                .SetTarget(casingTransform);
        }


        public static void TrySound(AudioSource audioSource, AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip);
        }

        [System.Obsolete("Use Shoot2 instead", true)]
        public static Bullet Shoot(Transform transform, string prefabName, Vector3 position, Vector3 rotation)
        {
            GameObject bullet = ServiceLocator.Get<ObjectPooler>().Get(prefabName);
            bullet.transform.SetPositionAndRotation(
                transform.TransformPoint(position),
                transform.rotation * Quaternion.Euler(rotation)
                );

            return bullet.GetComponent<Bullet>();
        }

        public static void Shoot2(Transform transform, string prefabName, Vector3 position, Vector3 rotation)
        {
            GameObject bullet = ServiceLocator.Get<ObjectPooler>().Get(prefabName);

            bullet.transform.SetPositionAndRotation(
                transform.TransformPoint(position),
                transform.rotation * Quaternion.Euler(rotation)
                );

            bullet.GetComponent<NewBullet>().Init();
        }
    }
}

namespace AD.TS.UI.HUD
{
}
