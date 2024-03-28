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

            if (Physics.Raycast(transformCenter, Vector3.down, out hit, characterController.center.y + 0.1f, layerMask))
            {
                normal = hit.normal;
            }

            return normal;
        }
    }
}
