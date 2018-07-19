using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class TransformEvent : MonoBehaviour {

	[Serializable]
    public class Vector4Event : UnityEvent<Vector4> {}

	[SerializeField]
	public  Klak.Wiring.NodeBase.Vector3Event  positionEvent;

	[SerializeField]
	public  Vector4Event  rotationEvent;

	[SerializeField]
	public  Vector4Event  vector4PositionEvent;

	void Update () {
		positionEvent.Invoke(transform.position);
		vector4PositionEvent.Invoke(new Vector4(transform.position.x , transform.position.y , transform.position.z , 0f));
		rotationEvent.Invoke(new Vector4(transform.rotation.x , transform.rotation.y , transform.rotation.z , transform.rotation.w));
	}
}
