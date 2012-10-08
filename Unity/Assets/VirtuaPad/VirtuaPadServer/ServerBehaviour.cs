using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public delegate void addAvatar(byte id);
public delegate void removeAvatars(byte[] ids);

public class ServerBehaviour : MonoBehaviour 
{
	public int udpPort;
	public int tcpPort;
	public VirtuaPadServer<SpaceBugzClient> server;

	public Dictionary<byte, PlayerEntity> avatarObjects = new Dictionary<byte, PlayerEntity>();
	private List<byte> avatarIDsToAdd = new List<byte>();
	
	public event addAvatar addAvatar;
	public event removeAvatars removeAvatars;
	
	private static ServerBehaviour mInstance; //server implemented as a singleton
	
	void Start () 
	{	
		//implementation of singleton pattern
		if(mInstance == null) 
		{
			mInstance = this;
			
			server = new VirtuaPadServer<SpaceBugzClient>(tcpPort, udpPort);
			
			server.addClientToUnity += QueueAvatar;
			server.removeClientsFromUnity += RemoveAvatars;
			DontDestroyOnLoad(gameObject);
			
			//connect beacon manager to server
			//server.AddTcpEventSubscriber(Tags.avatarBeacon, GetComponent<AvatarBeaconManager>());
			
			if(Application.loadedLevelName.Contains("Presentation") == false)
				server.AllowClientsToJoinGame();
		}
		else
			Destroy(gameObject);
	}
		
	void AddAvatars()
	{
		
		lock(avatarIDsToAdd)
		{
			foreach(byte newID in avatarIDsToAdd)
			{
				addAvatar(newID);
			}
			
			avatarIDsToAdd.Clear();
		}
	}
	
	void RemoveAvatars(byte[] ids)
	{
		removeAvatars( ids );	
	}
	
	private void Update()
	{
		AddAvatars();
	}
	
	private void QueueAvatar(byte id)
	{
		
		lock(avatarIDsToAdd)
		{
			avatarIDsToAdd.Add(id);
		}
	}

	void OnLevelWasLoaded()
	{
		if(mInstance == this)
			server.AllowClientsToJoinGame();
	}
	
	void OnApplicationQuit()
	{
		if(mInstance == this)
		{	
			server.Stop();	//stop server threads
		}
	}
	
}


