using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public delegate void debugToUnity(string message);

public delegate void addClientToUnity(byte id);
public delegate void removeClientsFromUnity(byte[] ids);

public class VirtuaPadServer<VPClientType> where VPClientType : VPClientBaseClass, new()
{
	private const int numOfAccVectors = 10;
	
	private const byte tagNewId = 0;
    private const byte tagWantsToJoin    = 1;
    private const byte tagJoinAccepted   = 2;
	private const byte tagKillCount		 = 3;
	
	private const byte tagServerShutdown = 255;
	
	/// <summary>
	/// holds the clients in Dictionary, their ID's are used to fetch them
	/// </summary>
	public Dictionary<byte, VPClientType> clients = new Dictionary<byte, VPClientType>();
	private const int maxNumOfClients = 255;
	private Stack<byte> freeIDs = new Stack<byte>();
		
	private Dictionary<byte, Thread> tcpListenThreads = new Dictionary<byte, Thread>();
	private Dictionary<byte, Thread> tcpWriteThreads = new Dictionary<byte, Thread>();
	
	//members related to tcp 
	private TcpListener tcpListener;
	private Thread tcpThread;
	private bool tcpOn = true;
	private IPAddress localAddr;

	private Dictionary<byte, ServerTcpEventSubscriber> tcpEventSubscribers = new Dictionary<byte, ServerTcpEventSubscriber>();
	
	private bool mAutoJoinClients = false;

	//members related to udp
	private UdpClient udpClient;
	private IPEndPoint udpRemoteIpEndPoint;
	private Thread udpThread;
	private bool udpOn = true;
	//private int udpPort;
	
	private Thread serverBroadcastIpThread;
	private bool broadcastIpOn = true;	
	
	private bool removeClientsOn = true;
	private long maxTicsBetweenPackets = 5000000;
	private Thread removeIdleClientsThread;
	
	//call this to add avatars(game object that represents clients) to unity
	public event addClientToUnity addClientToUnity;
	//call this to remove avatars from unity
	public event removeClientsFromUnity removeClientsFromUnity;
	
	/// <summary>
	/// Constructs a new server
	/// </summary>
	/// <param name="tcpPort">
	/// port for incoming TCP connections <see cref="System.Int32"/>
	/// </param>
	/// <param name="udpPort">
	/// port for incoming UDP datagrams <see cref="System.Int32"/>
	public VirtuaPadServer( int tcpPort, int udpPort)
	{
		for(byte i = 0 ; i <= maxNumOfClients - 1 ; i++) //make sure all ID's are free
			freeIDs.Push(i);
		
		try
		{
			//TCP setup
			localAddr = IPAddress.Parse("192.168.40.135");
			tcpListener = new TcpListener(localAddr, tcpPort);
			tcpListener.Start();
		}
		catch(SocketException e)
		{
			//TCP setup
			localAddr = IPAddress.Parse("192.168.1.100");
			tcpListener = new TcpListener(localAddr, tcpPort);
			tcpListener.Start();
		}
		catch
		{
			tcpListener.Stop();
			tcpListener = null;
		}
		
		if(tcpListener != null)
		{
			tcpThread = new Thread( new ThreadStart( TcpListenForClients ) );
			tcpThread.Name = "TCP Listen tread";
			tcpThread.Priority = System.Threading.ThreadPriority.AboveNormal;
			tcpThread.Start();
			
			udpClient = new UdpClient( udpPort );
			udpClient.Client.ReceiveTimeout = 0;
			udpClient.Client.ReceiveBufferSize = 12288;
			udpRemoteIpEndPoint = new IPEndPoint( IPAddress.Any, udpPort );
			
			
			udpThread = new Thread( new ThreadStart( UdpListen ) );
			udpThread.Name = "UDP listen thread";
			udpThread.Priority = System.Threading.ThreadPriority.Highest;
			udpThread.Start();
			
			/*
			serverBroadcastIpThread = new Thread(new ThreadStart( BroadcastServerIp ));
			serverBroadcastIpThread.Name = "UDP broadcast server IP thread";
			serverBroadcastIpThread.Start();
			*/
			
			removeIdleClientsThread = new Thread(new ThreadStart( CheckForIdleClients ));
			removeIdleClientsThread.Name = "Remove idle clients thread";
			removeIdleClientsThread.Start();
		}
	}
	
	public void AddTcpEventSubscriber(byte tag, ServerTcpEventSubscriber subscriber)
	{
		tcpEventSubscribers.Add(tag, subscriber);	
	}
	
	public void RemoveTcpEventSubscriber(byte tag)
	{
		tcpEventSubscribers.Remove(tag);	
	}
	
	private void TcpListenForClients()
	{
		Debug.Log("TCP thread started");
		/*
		try
		{
			tcpListener.Start();
			//tcpListener.Server.LingerState.Enabled = true;
			//tcpListener.Server.LingerState.LingerTime = 0;
			//tcpListener.Server.ReceiveTimeout = 2000;
			//tcpListener.Server.ReceiveTimeout = 
		}
		catch(Exception e)
		{
			Debug.Log(e.ToString());	
		}
		Debug.Log("TCP started");
		*/
		while( tcpOn )
		{
			try
			{
				Debug.Log("TCP loop");
				//blocks until a client has connected to the server
	  			
				TcpClient tcpClient = this.tcpListener.AcceptTcpClient();
				//set the new tcp client to use no delay = faster sending but lower packet efficiency
				
				tcpClient.NoDelay = true;
					
				VPClientType newVPClient = this.AddClient(tcpClient);
				newVPClient.connectionState = VPClientBaseClass.ClientConnectionStates.connected;
				
				//create a thread to handle listening
				//with connected client
				Thread clientListenThread = new Thread(new ParameterizedThreadStart(ClientTcpListen));
				clientListenThread.Name = "tcpListenTrhead" + newVPClient.ID.ToString();
				//add it to the dictionary of tcp client threads
				tcpListenThreads.Add(newVPClient.ID, clientListenThread);
				//start the client
				clientListenThread.Start(newVPClient);
				
				//next create a thread to handle writing
				Thread clientWriteThread = new Thread(new ParameterizedThreadStart(ClientTcpSend));
				clientWriteThread.Name = "tcpWriteThread" + newVPClient.ID.ToString();
				tcpWriteThreads.Add(newVPClient.ID,clientWriteThread);
				clientWriteThread.Start(newVPClient);
				
				//now we can send ID to client
				SendIdToClient(newVPClient.ID);
			}
			catch(ThreadAbortException)
			{
				Debug.Log("aborted from inside tcp thread");
				tcpOn = false;
				break;
			}
			catch(Exception e)
			{
				Debug.Log(e.Message);
				tcpOn = false;
				break;	
			}
			
			Debug.Log("tcp loop running");
		}
		Debug.Log("TCP thread ended");
	}
	
	private void SendIdToClient(byte id)
	{
		byte[] newIdData = new byte[1];
		newIdData[0] = id;
					
		SendToClient(id, tagNewId, newIdData, 1);
	}
	
	private void ClientTcpListen(object client)
	{
		VPClientType virtuaPadClient = (VPClientType)client;
		NetworkStream clientStream = virtuaPadClient.tcpClient.GetStream();
		
		byte[] message = new byte[1024];
		int bytesRead;
		
		while (tcpOn)
		{
			bytesRead = 0;
			
			try
			{
				//blocks until a client sends a message
				bytesRead = clientStream.Read(message, 0, 1024);
			}
			catch
			{
				//a socket error has occured
				break;
			}
			
			if (bytesRead == 0)
			{
				//the client has disconnected from the server
				break;
			}
			
			//message has successfully been received
			
			RecieveFromClient(virtuaPadClient.ID, message, bytesRead);
			//System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
		}
		
		Debug.Log("client " + virtuaPadClient.ID + " tcp listen thread stopped");
	}
	
	private void ClientTcpSend(object client)
	{
		VPClientType virtuaPadClient = (VPClientType)client;
		NetworkStream clientStream = virtuaPadClient.tcpClient.GetStream();
	
		bool runThread = true;
		
		while(virtuaPadClient.connectionState != VPClientBaseClass.ClientConnectionStates.disconnected && runThread)
		{
			try
			{
				virtuaPadClient.autoEvent.WaitOne();
				
				while(virtuaPadClient.messagesToSend.Count > 0 && runThread)
				{
					lock(virtuaPadClient)
					{
						while(virtuaPadClient.messagesToSend.Count > 0 && runThread)
						{
							
							byte[] message = virtuaPadClient.messagesToSend.Dequeue();
							clientStream.Write(message, 0, message.Length);
							
						}
					}
				}
			}
			catch
			{
				runThread = false;
				break;
			}
		}
	}
	
	public void AllowClientsToJoinGame()
	{
		if(!mAutoJoinClients)
		{
			mAutoJoinClients = true;
			
			lock(clients)
			{
				foreach(KeyValuePair<byte,VPClientType> client in clients)
				{
					if(client.Value.connectionState == VPClientBaseClass.ClientConnectionStates.tryingToJoin)
						JoinClient(client.Key);
				}
			}
		}
	}
	
	public void RecieveFromClient(byte id, byte[] data, int lengthOfData)
	{
		//Debug.Log("got tcp message form " + id + " with tag " + data[0]);
		
		byte tag = data[0];
		if(tag == tagWantsToJoin)
		{
			lock(clients)
			{
				//Debug.Log("client " + id + "has state " + clients[id].connectionState);
				
				if(mAutoJoinClients && clients[id].connectionState == VPClientBaseClass.ClientConnectionStates.connected)
				{
					JoinClient(id);
				}
				else if(clients[id].connectionState == VPClientBaseClass.ClientConnectionStates.connected)
				{
					//Debug.Log("client " + id + " is on hold");
					clients[id].connectionState = VPClientBaseClass.ClientConnectionStates.tryingToJoin;
				}
			}
		}
		else if(tcpEventSubscribers.ContainsKey(tag))
		{
			tcpEventSubscribers[tag].Recieve(id, data, lengthOfData);	
		}
		else
		{
			Debug.LogError("recieved tag " + tag + " I do not know how to handle!");
		}
	}
	
	private void JoinClient(byte id)
	{
		lock(clients)
		{
			clients[id].connectionState = VPClientBaseClass.ClientConnectionStates.joined;		
		}
		
		Debug.Log("server join with id " + id);
		addClientToUnity(id);
		byte[] returnMessage = new byte[1];
		returnMessage[0] = tagJoinAccepted;
		SendToClient(id, tagJoinAccepted, returnMessage, 1);
	}
		
	public void SendToClient(byte id, byte tag, byte[] dataToSend, byte lengthOfData)
	{
		byte[] buffer = new byte[lengthOfData + 2];
		
		buffer[0] = (byte)(lengthOfData + 1);
		buffer[1] = tag;
		
		for(int i = 0 ; i < lengthOfData ; i++)
		{
			buffer[i + 2] = dataToSend[i];	
		}
					
		lock(clients[id])
		{
			clients[id].messagesToSend.Enqueue(buffer);
			clients[id].autoEvent.Set();
		}
	}
	
	public void UdpListen()
	{
		while( udpOn )
		{
			try
			{
				// recieve udp packet
				byte[] packet = udpClient.Receive( ref udpRemoteIpEndPoint );
				//if we recieved a packet
				if( packet.Length > 0 )
				{
					//update the client
					UpdateClient(packet);					
				}
			}
			catch
			{
				udpOn = false;
				break;
			}
		}
	}
	
	private void BroadcastServerIp()
	{
		Debug.Log("broadcast server ip thread started");
		
		string message = "VPServerIp:" + GetLocalIp();
		
		//UdpClient serverUdpClient = new UdpClient();
	
		ASCIIEncoding encoder = new ASCIIEncoding();
		
		byte[] data = encoder.GetBytes(message);
		
		while(broadcastIpOn)
		{
			udpClient.Send(data, message.Length, "255.255.255.255", 60005);
			
			
			Thread.Sleep(1000);
		}
		Debug.Log("broadcast server ip thread ended");
	}
	
	private int removeIdleClientsDelay = 4000;
	private void CheckForIdleClients()
	{
		//while the flag removeclients is on continue to remove clients
		//when the removeClientsOn flag is set to false the thread will terminate
		while(removeClientsOn)
		{
			//delay thread execution for some time
			//this is done to reduce the overhead of this operation so other
			//processen can run
			Thread.Sleep(removeIdleClientsDelay);
			//get an array of bytes with clients we have not recieved a udp
			//package from in some time and remove them from the server
			byte[] removedClients = RemoveDisconnectedClients();
			//afterwards they are removed from unity
			removeClientsFromUnity(removedClients);
		}
	}
	
	private void UpdateClient(byte[] packet)
	{
		byte currentId = packet[0];
		
		lock(clients)
		{
			try
			{
				VPClientType client = clients[currentId];
				
				lock(client)
				{
					client.UnpackUdpData(packet);
					client.lastPacketTime = DateTime.Now.Ticks;
			
					clients[currentId] = client;
				}
			}
			catch ( KeyNotFoundException e)
			{
				Debug.Log( e.ToString() + " it seems a client is still sending");
			}
		}
	}
	
	private string BytesToString( byte[] packet, int start, int length )
	{

		StringBuilder sb = new StringBuilder();
			
		for( int i=0; i<packet.Length; i++ ) 
		{
			sb.Append( (char) packet[ i ] );
		}
		
		return sb.ToString();
	}
	
	public void Stop()
	{
		Debug.Log("stop called");
		/*
		broadcastIpOn = false;
		serverBroadcastIpThread.Abort();
		*/
		
		udpOn = false;
		
		if( udpClient != null )
		{	
			using(udpClient)
			{
				//udpClient.Client.Shutdown(SocketShutdown.Both);
				//udpClient.Client.Close();
				udpClient.Close();
			}
		}
		
		if( udpThread != null)
		{
			Debug.Log("trying to join udp thread");
			udpThread.Join(100);
			
            if (udpThread.IsAlive)
			{
				Debug.Log("aborting udp thread");
    	        udpThread.Abort();
			}
		}
								
		tcpOn = false;
		
		if(tcpListener != null)
		{
			//tcpListener.Server.Shutdown(SocketShutdown.Both);
			//tcpListener.Server.Close();
			tcpListener.Stop();
		}
		
		if(tcpThread != null)
		{
			Debug.Log("trying to join tcp thread");
			tcpThread.Join(100);
			
			if(tcpThread.IsAlive)
			{
				Debug.Log("aborting tcp thread");
				tcpThread.Abort();				
			}
		}		
				
		removeClientsOn = false;
		if(removeIdleClientsThread != null)
		{
			removeIdleClientsThread.Abort();
			removeIdleClientsThread.Join();
		}
		
		Debug.Log("remove thread aborted");
				
		List<byte> keys = new List<byte>();
		lock(clients)
		{
			foreach(KeyValuePair<byte, VPClientType> client in clients)
			{
				keys.Add(client.Key);	
			}
		}
		
		foreach(byte key in keys)
		{
			RemoveClient(key);
		}
		
 		Debug.Log("stop ended");
	}
	
	public VPClientType AddClient(TcpClient tcpClient)
	{
		//get next available ID
		byte newID = AssignID();
		
		if(newID == (byte)maxNumOfClients) //if there is no available ID
			throw new InvalidOperationException("ERROR: Failed to add client - server is full!");
			//return newID; //Notify By returning MaxID
		
		return AddClient(newID, tcpClient);
	}
	
	public VPClientType AddClient(byte newID, TcpClient tcpClient)
	{
		lock(clients)
		{
			VPClientType newClient = new VPClientType(); //make new client
			newClient.ID = newID; //set ID
			newClient.lastPacketTime = DateTime.Now.Ticks; //set timestamp
			newClient.color = ColorAssigner.AssignColor(newID);
			newClient.connectionState = VPClientBaseClass.ClientConnectionStates.connected;
			
			newClient.tcpClient = tcpClient;
			
			clients.Add(newID,newClient);
		}
		
		return clients[newID];
	}
	
	public byte[] RemoveDisconnectedClients()
	{
		List<byte> idsFreed = new List<byte>();
		
		lock(clients)
		{
			foreach(VPClientType client in clients.Values)
			{
				long deltaTime = DateTime.Now.Ticks - client.lastPacketTime;
				//Debug.Log("client : " + client.ID + " deltaTime " + deltaTime);
				if(deltaTime > maxTicsBetweenPackets)
				{
					Debug.Log("removed client " + client.ID);
					idsFreed.Add(client.ID);
				}		             
			}
			
			foreach(byte idFreed in idsFreed)
			{
				RemoveClient(idFreed);	
			}
		}
		
		return idsFreed.ToArray();
	}
	
	private void RemoveClient(byte id)
	{
		
		lock(clients)
		{
			lock(clients[id])
			{ 
				clients[id].Close();
				clients[id].autoEvent.Set();
				clients.Remove(id); //remove client
			}
		}
		
		lock(tcpListenThreads)
		{
			lock(tcpListenThreads[id])
			{
				tcpListenThreads[id].Join(100);
				
				if(tcpListenThreads[id].IsAlive)
				{
					Debug.Log("aborting tcp thread");
					tcpListenThreads[id].Abort();				
				}
				
				tcpListenThreads.Remove(id);
			}
		}
		
		lock(tcpWriteThreads)
		{
			lock(tcpWriteThreads[id])
			{
				
				tcpWriteThreads[id].Abort();
				tcpWriteThreads[id].Join();
				tcpWriteThreads.Remove(id);
			}
		}
				
		FreeID(id); //free ID
	}
		
	private byte AssignID()
	{
		if(freeIDs.Count > 0) //if there is a free ID
		{
			lock(freeIDs)
			{
				return freeIDs.Pop(); //return it and remove it from the stack
			}
		}
		
		return (byte)maxNumOfClients; //otherwise return maxNumOfClients to indicate we have reached the max number of clients...as if that will EVER HAPPEN!!!

	}

	private void FreeID(byte idToFree)
	{
		lock(freeIDs)
		{
			freeIDs.Push(idToFree);
		}
	}
	
	public string GetLocalIp()
	{
		IPHostEntry host;
		string localIP = "";
		host = Dns.GetHostEntry(Dns.GetHostName());
		
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily.ToString() == "InterNetwork")
			{
				localIP = ip.ToString();
			}
		}
		
		return localIP;
	}
}