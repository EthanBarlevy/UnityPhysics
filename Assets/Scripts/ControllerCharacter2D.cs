using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ControllerCharacter2D : MonoBehaviour
{
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;

	[SerializeField] float speed;
	[SerializeField] float jumpHeight;
	[SerializeField] float doubleJumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("Attack")]
	[SerializeField] Transform attackLocation;
	[SerializeField] float attackRadius;

	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		// check if the character is on the ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

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
				animator.SetTrigger("Jump");
			}
			if(Input.GetMouseButtonDown(0)) 
			{
				animator.SetTrigger("Attack");
			}
		}
		velocity.y += Physics.gravity.y * fallRateMultiplier * Time.deltaTime;

		// move character
		rb.velocity = velocity;

		// flip character to face direction of movement (velocity)
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		// update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
		animator.SetBool("Fall", !onGround && velocity.y < -0.1f);
	}

	IEnumerator DoubleJump()
	{ 
		yield return new WaitForSeconds(0.01f);
		while (velocity.y > 0)
		{
			if (Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
				break;
			}
			yield return null;
		}
	}

	private void CheckAttack()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackLocation.position, attackRadius);
		foreach (Collider2D collider in colliders)
		{
			if (collider.gameObject == gameObject) continue;

			if (collider.gameObject.TryGetComponent<IDamagable>(out var damagable))
			{
				damagable.Damage(30);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	private void Flip()
	{ 
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}
}
