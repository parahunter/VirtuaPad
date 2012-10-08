using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AvatarManager : MonoBehaviour 
{
	private struct AvatarScruct
	{
		public GameObject gameObject;
		public AvatarMovement movementScript;
		public AvatarShoot shootScript;
		public Avatar avatarScript;
	}
	
	public GameObject avatarPrefab;
		
	public Texture2D[] avatarMainTextures;
	public Texture2D[] avatarOverlayTextures;
	public Color[] avatarColors;
	public float maxAcc = 10.0f;
	public float accAdjustment = 5.0f;
		
	private Dictionary<byte, AvatarScruct> avatars = new Dictionary<byte, AvatarScruct>();
	//private LookupCollection scoreCollection;
	private List<KeyValuePair<byte,AvatarScruct>> scoreList;
	
	private ServerBehaviour serverBehaviour;
	private LogManager logManager;
	
	private List<byte> avatarsToRemove = new List<byte>();
		
	public bool respawnAvatars = true;
	
	public Vector3 GetAvatarPos(byte id)
	{
		Vector3 avatarPos;
		lock(avatars)
		{
			avatarPos = avatars[id].gameObject.transform.position;
		}
			
		return avatarPos;
	}
	
	public Transform GetAvatarTransform(byte id)
	{
		Transform avatarTans;
		lock(avatars)
		{
			avatarTans = avatars[id].gameObject.transform;
		}
			
		return avatarTans;
	}
	
	void Awake()
	{
		serverBehaviour = GetComponent<ServerBehaviour>();
		serverBehaviour.addAvatar += AddAvatar;
		serverBehaviour.removeAvatars += RemoveAvatars;
		
		logManager = GetComponent<LogManager>();
		//scoreCollection = new LookupCollection();
		
		StartCoroutine("ScaleAvatarsBasedOnScore");
		
		DontDestroyOnLoad(this);
	}
	
	public void AddScoreToClient(byte killedClient, byte killedByClient)
	{
		print("client " + killedClient + " was killed by " + killedByClient);
		
		if(killedClient == killedByClient)
			return;
		
		//scoreCollection[killedByClient] = (int)scoreCollection[killedByClient] + 1;
		
		lock(serverBehaviour.server.clients)
		{
			serverBehaviour.server.clients[killedByClient].AddScore();
			
			byte[] newScoreData = new byte[1];
			newScoreData[0] = serverBehaviour.server.clients[killedByClient].Score;
					
			serverBehaviour.server.SendToClient(killedByClient, 3, newScoreData, 1);
			
		}
	}
	
	public void VibrateClient(byte id, byte data)
	{
		byte[] vibrateData = new byte[1];
		vibrateData[0] = data;
					
		serverBehaviour.server.SendToClient(id, Tags.vibrateOnHit, vibrateData, 1);
	}
	
	public List<KeyValuePair<byte, int>> GetHighScore()
	{
		List<KeyValuePair<byte, int >> sortedList = new List<KeyValuePair<byte, int>>();
				
		lock(serverBehaviour.server.clients)
		{
			foreach(KeyValuePair<byte, SpaceBugzClient> client in serverBehaviour.server.clients)
			{
				KeyValuePair<byte, int> pair = new KeyValuePair<byte, int>(client.Key, client.Value.Score);
				sortedList.Add( pair );
			}
		}
		
		sortedList.Sort(
	    delegate(KeyValuePair<byte, int> firstPair,
	    KeyValuePair<byte, int> nextPair)
	    {
	        return firstPair.Value.CompareTo(nextPair.Value);
	    }
		);
		
		return sortedList;
	}	
	
	private List< KeyValuePair<byte, int> > sortedHighScore = new List<KeyValuePair<byte, int>>();
	public float timeBetweenHighScoreCalculations = 0.5f;
	private IEnumerator ScaleAvatarsBasedOnScore()
	{
		
		while(true)
		{
			yield return new WaitForSeconds(timeBetweenHighScoreCalculations);
			
			lock(serverBehaviour.server.clients)
			{
				foreach(KeyValuePair<byte, SpaceBugzClient> client in serverBehaviour.server.clients)
				{
					KeyValuePair<byte, int> pair = new KeyValuePair<byte, int>(client.Key, client.Value.Score);
					sortedHighScore.Add( pair );
				}
			}
						
			sortedHighScore.Sort(
	        delegate(KeyValuePair<byte, int> firstPair,
	        KeyValuePair<byte, int> nextPair)
	        {
	            return firstPair.Value.CompareTo(nextPair.Value);
	        }
	    	);
		}
	}
	
	public IEnumerator DisableAvatarsForLevelTransition(float timeBetweenAvatarDisables)
	{
		respawnAvatars = false;
		foreach(KeyValuePair<byte,AvatarScruct> avatar in avatars)
		{
			yield return new WaitForSeconds(timeBetweenAvatarDisables);
			avatar.Value.avatarScript.DisableAvatar();
		}
	}
	
	public IEnumerator EnableAvatarsForLevelTransition(float timeBetweenAvatarEnables)
	{
		print("reseting avatars");
		respawnAvatars = true;
		lock(serverBehaviour.server.clients)
		{
			foreach(KeyValuePair<byte,SpaceBugzClient> client in serverBehaviour.server.clients)
			{
				client.Value.ResetScore();
				
				byte[] newScoreData = new byte[1];
				newScoreData[0] = 0;
					
				serverBehaviour.server.SendToClient(client.Key, 3, newScoreData, 1);
			}
		}
		
		print("respawning");
		foreach(KeyValuePair<byte,AvatarScruct> avatar in avatars)
		{
			yield return new WaitForSeconds(timeBetweenAvatarEnables);
						
			StartCoroutine( avatar.Value.avatarScript.Respawn(false) );
		}
	}
	
	public void AddAvatar(byte id)
	{
		print("id to add avatar with " + id);
		GameObject newAvatarGameObject = (GameObject)Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity);
				
		AvatarScruct newAvatar = new AvatarScruct();
		newAvatar.avatarScript = newAvatarGameObject.GetComponent<Avatar>();
		newAvatar.avatarScript.Id = id;
		
		newAvatar.gameObject = newAvatarGameObject;
		newAvatar.movementScript = newAvatarGameObject.GetComponent<AvatarMovement>();
		newAvatar.shootScript = newAvatarGameObject.GetComponent<AvatarShoot>();
		newAvatar.shootScript.id = id;
		
		lock(avatars)
		{
			avatars.Add(id, newAvatar);
	
			if(logManager != null)
				logManager.CreateClientLog(id);
		}
		
		Renderer newAvatarRenderer = newAvatar.gameObject.transform.Find("GraphicsMain").GetComponent<Renderer>();
		newAvatarRenderer.material.mainTexture = GetAvatarMainTex(id);
		newAvatarRenderer.material.color = GetAvatarColor(id);
		newAvatar.avatarScript.avatarColor = GetAvatarColor(id);
		
		newAvatar.gameObject.transform.Find("GraphicsOverlay").renderer.material.mainTexture = GetAvatarOverlayTex(id);
			
		//scoreCollection.Add(newAvatar.avatarScript.Id, 0);
		//print("added client with id " + id);	
	}
	
	public void DisableInputFromClient(byte id, bool state)
	{
		lock(serverBehaviour.server.clients)
		{
			serverBehaviour.server.clients[id].InputDisabled = state;
		}
	}
	
	public Color GetAvatarColor(int id)
	{
		int invertedId = 254 - (int)id;
		int nextColor = invertedId % avatarColors.Length;
		return avatarColors[nextColor];	
	}
	
	public Texture2D GetAvatarOverlayTex(int id)
	{
		int invertedId = 254 - (int)id;
		
		int nextTexture = invertedId % avatarMainTextures.Length;
		
		return avatarOverlayTextures[nextTexture];
	}
	
	public Texture2D GetAvatarMainTex(int id)
	{
		int invertedId = 254 - (int)id;
		
		int nextTexture = invertedId % avatarMainTextures.Length;
		
		return avatarMainTextures[nextTexture];
	}
	
	public void RemoveAvatars(byte[] ids)
	{
		lock(avatarsToRemove)
		{
			avatarsToRemove.AddRange(ids);
		}
	}
	
	void Update()
	{
		lock(avatars)
		{
			foreach(byte id in avatarsToRemove)
			{
				if(avatars.ContainsKey(id))
				{
					//scoreCollection.Remove(avatars[id].avatarScript.Id);
					
					Destroy(avatars[id].gameObject);
					
					avatars.Remove(id);
					
					//logManager.CloseClientLog(id);
				}
			}
			
			avatarsToRemove.Clear();
		}
				
		lock(serverBehaviour.server.clients)
		{
			
			lock(avatars)
			{
				foreach(KeyValuePair<byte, SpaceBugzClient> client in serverBehaviour.server.clients)
				{
					if(avatars.ContainsKey(client.Key) && !avatars[client.Key].avatarScript.IsDisabled)
					{
						if(client.Value.shouldShoot())
						{
							avatars[client.Key].shootScript.Shoot( new Vector3(client.Value.aimVector.x, client.Value.aimVector.y, 0) );
						}
					}
				}
			}
			
		}
	}
	
	void FixedUpdate()
	{
		lock(serverBehaviour.server.clients)
		{
			lock(avatars)
			{
				foreach(KeyValuePair<byte, SpaceBugzClient> client in serverBehaviour.server.clients)
				{
					if(avatars.ContainsKey(client.Key) && !avatars[client.Key].avatarScript.IsDisabled)
					{
						Vector3 accVector = client.Value.accelerationVector;
						accVector.z = 0;
						accVector /= accAdjustment;
						
						accVector = Vector3.ClampMagnitude(accVector, maxAcc);
						
						avatars[client.Key].movementScript.ApplyMovement(accVector);
					
						if(client.Value.isJumping)
							avatars[client.Key].movementScript.ApplyJump();
					}
				}
			}
		}
	}
	
}
