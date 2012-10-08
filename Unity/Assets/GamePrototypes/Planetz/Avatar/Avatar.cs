using UnityEngine;
using System.Collections;

public class Avatar : MonoBehaviour 
{
	public int health = 1000;
	public int maxHealth = 2000;
	
	public float deadTime = 3.0f;
	public float involnurableTime = 1.0f;
	public float involnurableBlinkTime = 0.1f;
	
	public float fadeInTime = 1.0f;
	public float fadeInScaleRatio = 2.0f;
	
	private AvatarManager mAvatarManager;
	private SpawnPointManager mSpawnManager;
	
	private Transform mAvatarGraphicsMain;
	private Transform mAvatarGraphicsOverlay;
	private AvatarPickup mAvatarPickup;
	
	public Transform avatarDeath;
	
	public Color avatarColor;
	
	private byte id;
	public byte Id
	{
		set{id = value;}
		get{return id;}
	}
	
	private bool mIsDead = false;
	public bool IsDead
	{
		get{return mIsDead;}
	}
		
	private bool mIsInvolnurable = false;
	public bool IsInvolnurable
	{
		get{return mIsInvolnurable;}	
	}
	
	private bool mIsDisabled = false;
	public bool IsDisabled
	{
		get{return mIsDisabled;}	
	}
		
	private bool isRespawning = false;
	
	private Vector3 mGraphicsOriginalScale;
	// Use this for initialization
	void Start () 
	{
		mSpawnManager = GameObject.Find("AvatarSpawnManager").GetComponent<SpawnPointManager>();
		mAvatarManager = GameObject.Find("ServerObject").GetComponent<AvatarManager>();
			
		mAvatarGraphicsMain = transform.Find("GraphicsMain");
		mAvatarGraphicsOverlay = transform.Find("GraphicsOverlay");
		
		mAvatarPickup = GetComponent<AvatarPickup>();
		
		mGraphicsOriginalScale = mAvatarGraphicsMain.localScale;
	
		StartCoroutine("Respawn", true);
		
		DontDestroyOnLoad(gameObject);
	}
	
	public void TakeDamage(int damage, int fromWhom)
	{
		if(fromWhom == id)
			return;
		
		if(mAvatarPickup.CurrentShield != null)
		{
			mAvatarPickup.CurrentShield.TakeShieldDamage(damage, fromWhom);
			return;
		}
		
		if(fromWhom >= 0 && !mIsInvolnurable && !IsDead)
			mAvatarManager.AddScoreToClient(this.id, (byte)fromWhom);
		
		if(!mIsInvolnurable && !IsDead)
			StartCoroutine(DieCoroutine());
	}
	
	public void DisableAvatar()
	{
		transform.position = new Vector3(10000,10000,10000);
		rigidbody.velocity = Vector3.zero;
		mIsDisabled = true;	
	}
	
	/*
	void Update()
	{
		
		if(Input.GetKeyDown(KeyCode.K))
			StartCoroutine(DieCoroutine());	
	}*/
	
	private IEnumerator DieCoroutine()
	{
		mIsDead = true;
		
		float startTime = Time.time;
		
		mAvatarManager.VibrateClient(id, (byte)2);
		mAvatarManager.DisableInputFromClient(id, true);
		
		float t;
		while((t = ((Time.time - startTime) / fadeInTime)) < 1.0f)
		{
			Vector3 newScale = Vector3.Lerp(mGraphicsOriginalScale, Vector3.zero, t);
			
			mAvatarGraphicsMain.localScale = newScale;
			mAvatarGraphicsOverlay.localScale = newScale;
			yield return 0;
		}
		
		Transform death = (Transform) Instantiate(avatarDeath, transform.position, transform.rotation);
		DisableAvatar();
		
		yield return new WaitForSeconds(deadTime);
		
		if(mAvatarManager.respawnAvatars && !isRespawning)
			StartCoroutine("Respawn", true);
	}

	void OnLevelWasLoaded()
	{
		
		mSpawnManager = GameObject.Find("AvatarSpawnManager").GetComponent<SpawnPointManager>();
	}
	
	public IEnumerator Respawn(bool atRandomPoint = true)
	{
		print("start of respawn");
		mIsDisabled = false;
		mIsDead = false;
		mIsInvolnurable = true;
		isRespawning = true;
		
		if(atRandomPoint)
			transform.position = mSpawnManager.GetRandomSpawnPoint();
		else
			transform.position = mSpawnManager.GetNextSpawnPoint();
		
		transform.rotation = Quaternion.identity;
		
		yield return StartCoroutine(PopOutAndIn());
		
		float startTime = Time.time;
		
		while(startTime + involnurableTime > Time.time)
		{
			mAvatarGraphicsMain.renderer.enabled = !mAvatarGraphicsMain.renderer.enabled;
			mAvatarGraphicsOverlay.renderer.enabled = !mAvatarGraphicsOverlay.renderer.enabled;
		
			yield return new WaitForSeconds(involnurableBlinkTime);
		}
		
		mAvatarGraphicsMain.renderer.enabled = true;
		mAvatarGraphicsOverlay.renderer.enabled = true;
		
		isRespawning = false;
		mIsInvolnurable = false;
	}
	
	public void MakeVolnurable()
	{
		if(mIsInvolnurable)
		{
			StopCoroutine("Respawn");
			isRespawning = false;
			
			print("making volnurable");
			
			mAvatarGraphicsMain.renderer.enabled = true;
			mAvatarGraphicsOverlay.renderer.enabled = true;
		
			mIsInvolnurable = false;
		}
	}
	
	private IEnumerator PopOutAndIn()
	{
		yield return new WaitForEndOfFrame();
		
		mIsDisabled = true;
		mAvatarGraphicsMain.collider.enabled = false;
		rigidbody.isKinematic = true;
		
		mAvatarManager.VibrateClient(id, (byte)1);
		
		Vector3 bigScale = new Vector3(mGraphicsOriginalScale.x*fadeInScaleRatio, mGraphicsOriginalScale.y*fadeInScaleRatio,mGraphicsOriginalScale.z*fadeInScaleRatio);
		
		float startTime = Time.time;
		
		
		float t;
		while((t = ((Time.time - startTime) / fadeInTime)) < 1.0f)
		{
			Vector3 newScale = Vector3.Lerp(Vector3.zero,bigScale, t);
			mAvatarGraphicsMain.localScale = newScale;
			mAvatarGraphicsOverlay.localScale = newScale;
			
			yield return 0;
		}
		
		startTime = Time.time;
		
		while( (t = ((Time.time - startTime) / fadeInTime) )< 1.0f)
		{
			Vector3 newScale = Vector3.Lerp(bigScale,mGraphicsOriginalScale, t);
			mAvatarGraphicsMain.localScale = newScale;
			mAvatarGraphicsOverlay.localScale = newScale;
			yield return 0;
		}
		
		mAvatarGraphicsMain.collider.enabled = true;
		rigidbody.isKinematic = false;
		
		mIsDisabled = false;
		mAvatarManager.DisableInputFromClient(id, false);
		
		mAvatarGraphicsMain.localScale = mGraphicsOriginalScale;
		mAvatarGraphicsOverlay.localScale = mGraphicsOriginalScale;
	}
}
