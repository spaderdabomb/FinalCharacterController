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
            RaycastHit hit;
            Vector3 transformCenter = characterController.transform.position + characterController.center;
            
            if (Physics.SphereCast(transformCenter,
                                   characterController.radius,
                                   Vector3.down,
                                   out hit,
                                   characterController.center.y + 0.1f,
                                   layerMask))
            {
                normal = Vector3.Angle(hit.normal, Vector3.up) <= characterController.slopeLimit ? hit.normal : Vector3.up;
                normal = new Vector3(0f, Mathf.Round(normal.y * 100f)/100f, Mathf.Round(normal.z * 100f) / 100f).normalized;
            }

            return normal;
        }
    }
}
