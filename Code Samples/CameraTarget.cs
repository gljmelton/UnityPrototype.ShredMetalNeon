using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour {

    public float maxPlayerDistance;
    public float weight;

    private bool inPlayerRange = false;

    void Update()
    {
        //Check to see how far away this projectile is from the player. If they're within range add them to the camera target.
        float distanceToPlayer = Vector3.Distance(GameData.playerCharacter.transform.position, gameObject.transform.position);

        if (!inPlayerRange && distanceToPlayer <= maxPlayerDistance)
        {
            CameraLookTarget.instance.AddObject(this);
            inPlayerRange = true;
        }
        else if (inPlayerRange && distanceToPlayer > maxPlayerDistance)
        {
            CameraLookTarget.instance.RemoveObject(this);
            inPlayerRange = false;
        }
    }

}
