using UnityEngine;
using System.Collections;

public class AvatarMovement : MonoBehaviour 
{
	public float moveForceSpace = 5.0f;
	public float moveForcePlanets = 10f;
	public float maxVelocitySpace = 3.0f;
	public float maxVelocityPlanets = 5.0f;	
	
	public float jumpForceSpace = 2.0f;
	public float jumpForcePlanets = 4.0f;
	
	public float timeBetweenJumps = 0.4f;
	
	private bool canJump = true;	
	
	private Vector3 lastInputVec;
	private AffectedByGravity gravityScript;
	
	public float allignmentSpeed = 5.0f;
	
	void Start()
	{
		gravityScript = GetComponent<AffectedByGravity>();
		
	}	
	
	void FixedUpdate()
	{
		//get entity to point in the right direction
		if(gravityScript.InWell)
			transform.up = Vector3.Slerp(transform.up, -gravityScript.Gravity.normalized, allignmentSpeed*Time.fixedDeltaTime);
		else if(lastInputVec.sqrMagnitude > 0.5f)
			transform.up = Vector3.Slerp(transform.up, lastInputVec.normalized, allignmentSpeed*Time.fixedDeltaTime);
	}
	
	public void ApplyJump()
	{
		if(canJump)
		{
			canJump = false;
			
			if(gravityScript.InWell)
			{
				rigidbody.AddForce(-gravityScript.Gravity.normalized*jumpForcePlanets, ForceMode.VelocityChange);
			}
			else
			{
				rigidbody.AddForce(lastInputVec.normalized*jumpForceSpace, ForceMode.VelocityChange);
			}
			
			StartCoroutine(jumpTimer());
		}
	}
		
	public void ApplyMovement(Vector3 inputVec)
	{
		//float dot = Vector3.Dot(transform.right, inputVec.normalized);
		
		lastInputVec = inputVec;//*Mathf.Abs(dot)*moveForce;
		
		Debug.DrawLine(transform.position, transform.position + rigidbody.velocity, Color.blue);
		
		if(gravityScript.InWell)
		{
			Vector3 parallelPartOfInputVec = lastInputVec - Vector3.Project(lastInputVec, gravityScript.Gravity);
			
			//Debug.DrawLine(transform.position, transform.position + parallelPartOfInputVec*10, Color.red);
			
			if(rigidbody.velocity.magnitude < maxVelocityPlanets)
			{ 
				
				Debug.DrawLine(transform.position, transform.position + parallelPartOfInputVec *10, Color.yellow);

				rigidbody.AddForce(parallelPartOfInputVec*moveForcePlanets, ForceMode.VelocityChange);
			} 
			/*
			else
			{
				float dot = Vector3.Dot(inputVec.normalized, -gravityScript.Gravity.normalized);
				
				if(dot < 0)
				{
					Debug.DrawLine(transform.position, transform.position + parallelPartOfInputVec *10, Color.green);

					rigidbody.AddForce( moveForcePlanets * parallelPartOfInputVec, ForceMode.VelocityChange);
				}
				else
				{
					Debug.DrawLine(transform.position, transform.position + parallelPartOfInputVec *10, Color.red);
					
					rigidbody.AddForce( moveForcePlanets * parallelPartOfInputVec * (1 - dot) , ForceMode.VelocityChange);
				}
			}		*/
		}
		else
		{
			
			if(rigidbody.velocity.magnitude < maxVelocitySpace)
			{ 
				Debug.DrawLine(transform.position, transform.position + lastInputVec*10, Color.red);
		
				rigidbody.AddForce(lastInputVec*moveForceSpace, ForceMode.VelocityChange);
			} 
			else
			{
				
				float dot = Vector3.Dot(inputVec.normalized, rigidbody.velocity.normalized);
								
				if(dot < 0)
				{
					Debug.DrawLine(transform.position, transform.position + lastInputVec*10, Color.green);

					rigidbody.AddForce( moveForceSpace * lastInputVec, ForceMode.VelocityChange);
				}
				else
				{
					rigidbody.AddForce( lastInputVec * moveForceSpace * (1 - dot) , ForceMode.VelocityChange);
				}
			}	
		}
	}
	
	IEnumerator jumpTimer()
	{
		
		yield return new WaitForSeconds(timeBetweenJumps);
		canJump = true;
	}
}
