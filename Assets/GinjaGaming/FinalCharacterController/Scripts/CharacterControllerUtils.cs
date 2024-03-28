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
            Vector3 center = characterController.transform.position + characterController.center;
            float distance = characterController.height / 2f + characterController.stepOffset + 0.1f;

            Debug.DrawLine(center, center + Vector3.down * distance, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(center, Vector3.down, out hit, distance, layerMask))
            {
                normal = hit.normal;
                return normal;
            }


            if (Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layerMask))
            {
                normal = hit.normal;
                Debug.Log("sphere check");
                return normal;
            }

            return normal;
        }
    }
}
