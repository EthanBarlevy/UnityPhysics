using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class AIController2D : MonoBehaviour
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
	[Header("AI")]
	[SerializeField] Transform[] waypoints;
	[SerializeField] float sightDistance = 1;


	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	float groundAngle = 0;
	Transform targetWaypoint = null;

	enum State
	{ 
		IDLE,
		PATROL,
		CHASE,
		ATTACK
	}

	State state = State.IDLE;
	float stateTimer = 0;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		Vector2 direction = Vector2.zero;

		// update ai
		switch (state)
		{
			case State.IDLE:
				if (CanSeePlayer()) state = State.CHASE;
				stateTimer += Time.deltaTime;
				if (stateTimer > 1)
				{
					SetNewWaypointTarget();
					state = State.PATROL;
				} 
				break;
			case State.PATROL:
				if (CanSeePlayer()) state = State.CHASE;
				direction.x = Mathf.Sign(targetWaypoint.position.x - transform.position.x);
				float dx = Mathf.Abs(targetWaypoint.position.x - transform.position.x);
				if (dx <= 0.25f)
				{
					stateTimer = 0;
					state = State.PATROL;
				}
				break;
			case State.CHASE:

				break;
			case State.ATTACK:
				break;
			default:
				break;
		}

		// check if the character is on the ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

		
		velocity.x = direction.x * speed;

		// set velocity
		if (onGround)
		{
			if (velocity.y < 0) velocity.y = 0;
		}
		float gravityMultiplier = 1;
		velocity.y += Physics.gravity.y * Time.deltaTime;

		// move character
		rb.velocity = velocity;

		// flip character to face direction of movement (velocity)
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		// update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
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

	private void Flip()
	{ 
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	private void SetNewWaypointTarget()
	{
		Transform waypoint = null;
		do
		{
			waypoint = waypoints[Random.Range(0, waypoints.Length)];
		}
		while (waypoint == targetWaypoint);
		targetWaypoint = waypoint;
	}

	private bool CanSeePlayer()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * sightDistance);
		Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * sightDistance);

		return hit.collider != null && hit.collider.gameObject.CompareTag("Player");
	}
}
