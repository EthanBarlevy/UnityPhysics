using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button : MonoBehaviour
{
	[SerializeField] GameObject ragdoll;
	[SerializeField] Transform place;
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Domino"))
		{
			Instantiate(ragdoll, place.position, place.rotation);
		}
	}
}
