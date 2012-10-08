using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HighscoreGui : MonoBehaviour 
{
	public GUISkin guiSkin;
	
	public AudioClip applauseClip;
	
	public Texture2D highscoreBackground;
	public float timeToShowHighscore = 5.0f;
	public float timeForEachScore = 0.3f;
	
	private float mHighscoreTopMargin;	
	private float mHighscoreHorizontalMargin;
	private float mHighscoreCenterMargin;
	private float mHighscoreBottomMargin;
	private float mHighscoreEntryWidth;
	private float mHighscoreEntryHeight;
		
	private bool mShowHighscore = false;
	
	private Rect mRectScoreText;
	
	private AvatarManager mAvatarManager;
	private AudioShufler mAudioShufler;
	
	void Start()
	{
		mRectScoreText = new Rect(Screen.width*0.3f, 0, Screen.width*0.4f, Screen.height*0.1f);
				
		mAvatarManager = GetComponent<AvatarManager>();
		mAudioShufler = GetComponent<AudioShufler>();
		
		mHighscoreTopMargin = Screen.height*0.15f;
		mHighscoreHorizontalMargin = Screen.width*0.2f;
		mHighscoreCenterMargin = Screen.width*0.3f;
		mHighscoreBottomMargin = Screen.height*0.05f;
		mHighscoreEntryWidth = Screen.width*0.5f - mHighscoreHorizontalMargin - mHighscoreCenterMargin*0.5f;
		mHighscoreEntryHeight = (Screen.height - mHighscoreTopMargin - mHighscoreBottomMargin)/12;

	}
	
	private int mScorePerColumn = 12;
	void OnGUI()
	{
		GUI.skin = guiSkin;
		
		if(mShowHighscore)
		{
			GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height), highscoreBackground);
			
			GUI.Label(mRectScoreText, "Time's up! you earned:");
		
			int placementCounter = 0;
			
			for(int i = 0 ; i <= mNumOfScoresToShow ; i++)
			{
				Rect placement;
				Rect texPlacement;
								
				if(i < mScorePerColumn)
				{
					placement = new Rect(mHighscoreHorizontalMargin,
										mHighscoreTopMargin + placementCounter*mHighscoreEntryHeight, mHighscoreEntryWidth,mHighscoreEntryHeight);	
						
					texPlacement = new Rect(mHighscoreHorizontalMargin - mHighscoreEntryHeight,
										mHighscoreTopMargin + placementCounter*mHighscoreEntryHeight, mHighscoreEntryHeight,mHighscoreEntryHeight);	
					
				}
				else
				{
					placement = new Rect(mHighscoreHorizontalMargin + mHighscoreEntryWidth + mHighscoreCenterMargin,
										mHighscoreTopMargin + (placementCounter)*mHighscoreEntryHeight, mHighscoreEntryWidth,mHighscoreEntryHeight);	

					texPlacement = new Rect(mHighscoreHorizontalMargin - mHighscoreEntryHeight +  mHighscoreEntryWidth + mHighscoreCenterMargin,
										mHighscoreTopMargin + (placementCounter)*mHighscoreEntryHeight, mHighscoreEntryHeight,mHighscoreEntryHeight);	
				}
				
				int reverseCounter = highScore.Count - i - 1;
				
				GUI.color = Color.white;
				GUI.Label(placement, "   : " + highScore[reverseCounter].Value + " points");	
				GUI.color = mAvatarManager.GetAvatarColor(highScore[reverseCounter].Key);
				GUI.DrawTexture(texPlacement, mAvatarManager.GetAvatarMainTex(highScore[reverseCounter].Key));
				GUI.color = Color.white;
				GUI.DrawTexture(texPlacement,mAvatarManager.GetAvatarOverlayTex(highScore[reverseCounter].Key));
				
				if(placementCounter == mScorePerColumn - 1)
					placementCounter = 0;
				else
					placementCounter++;
			}
		}
	}
	
	private List<KeyValuePair<byte, int>> highScore;
	
	private int mNumOfScoresToShow = 0;
	public IEnumerator ShowHighScore()
	{
		highScore = mAvatarManager.GetHighScore();
		
		if(highScore.Count <= 0)
			yield return 0;
		else
		{
			mShowHighscore = true;
			audio.clip = applauseClip;
			audio.Play();
				
			for(int i = 0 ; i < highScore.Count ; i++)
			{
				mAudioShufler.Play();
				mNumOfScoresToShow = i;
				
				mAvatarManager.VibrateClient(highScore[highScore.Count - i - 1].Key, (byte)0);
				
				yield return new WaitForSeconds(timeForEachScore);
			}
					
			yield return new WaitForSeconds(timeToShowHighscore);
			mShowHighscore = false;
		}
	}
}
