using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController
{
    public static class CharacterControllerUtils
    {
        public static Vector3 GetCharacterControllerNormal(CharacterController characterController, LayerMask layerMask = default)
        {
            Vector3 normal = Vector3.up;
            Vector3 center = characterController.transform.position + characterController.center;
            float distance = characterController.height / 2f + characterController.stepOffset + 0.1f;

            RaycastHit hit;
            if (Physics.Raycast(center, Vector3.down, out hit, distance + characterController.radius / 2f, layerMask))
            {
                return hit.normal;
            }

            if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask))
            {
                return hit.normal;
            }

            return normal;
        }

        public static bool IsOverlapCapsule(CapsuleCollider capsuleCollider, LayerMask layerMask = default)
        {
            Vector3 center = capsuleCollider.transform.position + capsuleCollider.center;
            Vector3 heightOffset = Vector3.up * (capsuleCollider.height * 0.5f - capsuleCollider.radius);
            Vector3 bottom = center - heightOffset;
            Vector3 top = center + heightOffset;

            Collider[] colliders = Physics.OverlapCapsule(bottom, top, capsuleCollider.radius, layerMask);

            return colliders.Length > 0;
        }
    }
}
