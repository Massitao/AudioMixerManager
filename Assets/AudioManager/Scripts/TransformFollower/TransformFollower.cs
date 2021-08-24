using System.Collections;
using UnityEngine;

namespace Massitao.TransformFollowerTool
{
    public class TransformFollower : MonoBehaviour
    {
        [Header("Follow Parameters")]
        private Transform transformToFollow;
        private Vector3 transformOffset;
        private Coroutine followCoroutine;

        private void OnDisable()
        {
            transformToFollow = null;
        }

        #region Set Transform To Follow
        private void SetTransformToFollow(Transform newTransform)
        {
            transformToFollow = newTransform;
            transformOffset = Vector3.zero;
        }
        private void SetTransformToFollow(Transform newTransform, Vector3 newOffset)
        {
            transformToFollow = newTransform;
            transformOffset = newOffset - transformToFollow.localPosition;
        }
        #endregion

        #region Follow Behaviour
        public void StartFollow(Transform newTransform)
        {
            SetTransformToFollow(newTransform);

            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }

            followCoroutine = StartCoroutine(FollowBehaviour());
        }
        public void StartFollow(Transform newTransform, Vector3 newOffset)
        {
            SetTransformToFollow(newTransform, newOffset);

            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }

            followCoroutine = StartCoroutine(FollowBehaviour());
        }

        private IEnumerator FollowBehaviour()
        {
            while (transformToFollow != null)
            {
                gameObject.transform.localPosition = transformToFollow.TransformPoint(transformOffset);
                gameObject.transform.localRotation = transformToFollow.rotation;
                yield return null;
            }

            transformToFollow = null;

            yield break;
        }

        public void StopFollow()
        {
            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }

            followCoroutine = null;
            transformToFollow = null;
        }
        #endregion
    }
}