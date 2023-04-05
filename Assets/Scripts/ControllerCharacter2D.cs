using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ControllerCharacter2D : MonoBehaviour
{
	[SerializeField] float speed;
	[SerializeField] float turnRate;
	[SerializeField] float jumpHeight;
	[SerializeField] float hitForce;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;

	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}
	void Update()
	{
		// check if the character is on the ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, 0.01f, groundLayerMask) != null;

		// get direction input
		Vector2 direction = Vector2.zero;
		direction.x = Input.GetAxis("Horizontal");
		
		velocity.x = direction.x * speed;

		// set velocity
		if (onGround)
		{
			if (velocity.y < 0) velocity.y = 0;
			if (Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(jumpHeight * - 2 * Physics.gravity.y);
				StartCoroutine(DoubleJump());
			}
		}
		velocity.y += Physics.gravity.y * Time.deltaTime;

		// move character
		rb.velocity = velocity;

		// rotate character to face direction of movement (velocity)
	}

	IEnumerator DoubleJump()
	{ 
		yield return new WaitForSeconds(0.01f);
		while (velocity.y > 0)
		{
			if (Input.GetButtonDown("Jump"))
			{
				//velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
				break;
			}
			yield return null;
		}
	}
}
