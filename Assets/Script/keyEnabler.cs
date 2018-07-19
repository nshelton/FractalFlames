using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyEnabler : MonoBehaviour {

	[SerializeField]
	private KeyCode m_key;

	[SerializeField]
	private GameObject	m_target;

	void Start () {
		m_target.SetActive(false);
	}
	
	void Update () {
		if (Input.GetKeyDown(m_key))
		{
			m_target.SetActive(!m_target.activeInHierarchy);
		}
	}
}
