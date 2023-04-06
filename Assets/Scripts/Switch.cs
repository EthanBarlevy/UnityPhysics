using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] GameObject light;
	private void OnTriggerEnter(Collider other)
	{
		light.SetActive(true);
	}
}
