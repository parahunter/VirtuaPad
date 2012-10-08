using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour 
{
	private enum SelectionState{small,medium,large};
	private SelectionState mSelectionState = SelectionState.large;
	private SelectionState mLastSelectionState = SelectionState.large;
	
	public GUISkin guiSkin;
	
	public AudioClip clockClip;
		
	public float timeBetweenAvatarDisables = 0.1f;
	public float timeBetweenAvatarEnables = 0.05f;
	
	public int secondsPerLevel = 60;
	public int  turnCountDownRedAt = 10;
	
	public string[] mediumLevels;
	public string[] largeLevels;
	
	public bool shuffle = false;
	
	private int mCountDown;
	
	private Rect mRectCountDown;
	
	private bool mShowCountDown;
	
	private AvatarManager mAvatarManager;
	
	private static LevelManager mLevelManager = null;
	
	private HighscoreGui mHighscoreGui;
	
	public bool inResearchMode = false;
	private bool researchModeContinue = false;
	private bool showResearchMode = false;
		
	// Use this for initialization
	void Start () 
	{
		if(mLevelManager == null)
		{
			mRectCountDown = new Rect(Screen.width*0.3f, 0, Screen.width*0.4f, Screen.height*0.1f);
			mAvatarManager = GetComponent<AvatarManager>();
			mHighscoreGui = GetComponent<HighscoreGui>();
			
			print(Application.loadedLevelName);
			if(Application.loadedLevelName.Contains("Presentation") == false)
			{
				StartCoroutine("DoCountDown");
			}
			DontDestroyOnLoad(this);
			mLevelManager = this;
		}
		else
			Destroy(gameObject);
	}
	
	void Update()
	{	
		//if(Input.GetKeyDown(KeyCode.F1))
		//	mSelectionState = SelectionState.small;
		if(Input.GetKeyDown(KeyCode.F2))
		{
			mSelectionState = SelectionState.medium;
			researchModeContinue = true;
			ChangeLevel();
		}
		else if(Input.GetKeyDown(KeyCode.F3))
		{	
			researchModeContinue = true;
			mSelectionState = SelectionState.large;
			ChangeLevel();
		}
		
		//if(Input.GetKeyDown(KeyCode.R))
		//	inResearchMode = !inResearchMode;
		
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
		
		if(Input.GetKeyDown(KeyCode.Space))
		{	
			researchModeContinue = true;
		}
	}
	
	void OnGUI()
	{
		GUI.skin = guiSkin;
		
		if(mShowCountDown)
		{
			
			if(mCountDown <= turnCountDownRedAt)
			{	
				GUI.color = Color.red;
			}	
			GUI.Label( mRectCountDown, "time left: " + mCountDown);	
		}
		
		if(showResearchMode)
			GUI.Box(new Rect(Screen.width*0.1f, Screen.height*0.4f, Screen.width*0.8f, Screen.height*0.2f), "Game Paused! fill out the next part of the questionnaire");
	}
	
	private IEnumerator DoCountDown()
	{
		mShowCountDown = true;
		//float startTime = Time.time;
		mCountDown = secondsPerLevel;
		
		audio.clip = clockClip;
		
		while(mCountDown > 0)
		{
			yield return new WaitForSeconds(1.0f);
			mCountDown--;
			
			if(turnCountDownRedAt >= mCountDown)
			{
				audio.volume = 0.1f + 0.9f*(turnCountDownRedAt - mCountDown) /turnCountDownRedAt;
				audio.Play();
			}
		}
		
		mShowCountDown = false;
		
		yield return StartCoroutine( mAvatarManager.DisableAvatarsForLevelTransition(timeBetweenAvatarDisables));
		yield return StartCoroutine(mHighscoreGui.ShowHighScore());
		
		while(!researchModeContinue && inResearchMode)
		{
			showResearchMode = true;
			yield return 0;
		}
		
		ChangeLevel();
	}
	
	private bool mHasLoadedLevel = false;
	void OnLevelWasLoaded()
	{
		researchModeContinue = false;
		showResearchMode = false;
		
		print("level loaded test");
		
		if(!Application.loadedLevelName.Contains("Presentation") && mLevelManager == this)
		{
			print(Application.loadedLevelName);
			mHasLoadedLevel = true;
			
			StopCoroutine("DoCountDown");
			StartCoroutine("DoCountDown");
			
			mAvatarManager = GetComponent<AvatarManager>();
			StartCoroutine(mAvatarManager.EnableAvatarsForLevelTransition(timeBetweenAvatarEnables));
		}	
	}
	
	private int nextLevel = 1;
	void ChangeLevel()
	{
		if(mSelectionState != mLastSelectionState)
		{
			nextLevel = 1;
			mLastSelectionState = mSelectionState;
		}
		
		int actualLevel = 0;
		if(!shuffle)
		{
			switch(mSelectionState)
			{
			case SelectionState.small:
				break;
			case SelectionState.medium:
				actualLevel = nextLevel % mediumLevels.Length;
				break;
			case SelectionState.large:
				print("went to big level");
				actualLevel = nextLevel % largeLevels.Length;
				break;
			}
			
			nextLevel++;	
		}
		else
		{
			switch(mSelectionState)
			{
			case SelectionState.small:
				break;
			case SelectionState.medium:
				actualLevel = Random.Range(0,mediumLevels.Length - 1);	
				break;
			case SelectionState.large:	
				actualLevel = Random.Range(0,largeLevels.Length - 1);	
				break;
			}
			
		}
		switch(mSelectionState)
		{
		case SelectionState.small:
			break;
		case SelectionState.medium:
			Application.LoadLevel(mediumLevels[actualLevel]);
			break;
		case SelectionState.large:
			Application.LoadLevel(largeLevels[actualLevel]);
			break;
		}
	}
	
	
}
