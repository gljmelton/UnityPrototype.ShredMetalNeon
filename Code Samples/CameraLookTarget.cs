using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookTarget : MonoBehaviour {

    public static CameraLookTarget instance;

    public CameraTarget player;
    public List<CameraTarget> lookTargets;

    private Vector3 targetPos;
    public float lookWeight;

	// Use this for initialization
	void Start () {
        //Set up the singleton instance
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        lookWeight = player.weight;
	}
	
	// Update is called once per frame
	void Update () {
        targetPos = player.transform.position;

        if (!lookTargets.Count.Equals(0)) {
            for (int i = 0; i < lookTargets.Count; i++)
            {
                //Nudge the cameras target toward a weighted average position between the targets and the player.
                targetPos += (lookTargets[i].weight / lookWeight) * (lookTargets[i].transform.position - player.transform.position);
            }
        }

        //Set the position of the look target.
        transform.position = targetPos;
	}

    //Add a new object to the list of targets that should be kept in view.
    public void AddObject(CameraTarget obj) {
        lookTargets.Add(obj);
        lookWeight += obj.weight;
    }

    //Remove an object from the list of targets that should be kept in view.
    public void RemoveObject(CameraTarget obj) {
        lookTargets.Remove(obj);
        lookWeight -= obj.weight;
    }
}
