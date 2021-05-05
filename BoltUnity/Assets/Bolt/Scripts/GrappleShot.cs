using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrappleShot : MonoBehaviour
{
	[HideInInspector] [SerializeField] private Rigidbody body;

	public event Action<Collision> Collided;
	public Rigidbody Body => body;

	private void OnValidate()
	{
		body = GetComponent<Rigidbody>();
		if (GetComponent<Collider>() == null)
			Debug.LogWarning("no collider present on " + name);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Collided?.Invoke(collision);
	}
}
