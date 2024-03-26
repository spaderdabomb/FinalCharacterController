using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace GinjaGaming.FinalCharacterController
{
    public static class CharacterControllerUtils
    {
        public static Vector3 GetCharacterControllerNormal(CharacterController characterController, LayerMask layerMask = default)
        {

            Vector3 normal = Vector3.up;
            RaycastHit hit;
            Vector3 transformCenter = characterController.transform.position + characterController.center;

            Transform transform = characterController.transform;
            Vector3 center = transform.position + Vector3.up * characterController.height / 2;

            RaycastHit hit2;
            if (Physics.Raycast(center, Vector3.down, out hit2, characterController.height / 2f + 0.2f, layerMask))
            {
                normal = hit2.normal;
                return normal;
            }


            if (Physics.SphereCast(transformCenter, characterController.radius, Vector3.down, out hit, characterController.center.y + 0.4f, layerMask))
            {
                normal = hit.normal;
                return normal;
            }

            return normal;
        }
    }
}
