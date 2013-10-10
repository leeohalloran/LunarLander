using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Component of Level1 scenes GuiScripts empty gameobject
/// </summary>
public class GuiInGame : MonoBehaviour
{
	public GameObject nguiControls;
	public GameObject nguiMenu;
	public GameObject nguiMenuTopButton;
	public GameObject nguiMenuSoundButton;
	public AudioClip WinClip;
	public AudioClip LoseClip;
	public UILabel ScoreLevel;
	public UILabel ScoreTotal;
	public UISlider FuelGauge;
	
	/// <summary>
	/// TODO: fix the bug described in this summary
	/// This property is being used to try to trap a bug that occurs on the load of a second game level
	/// repo:
	/// 1. open Level1
	/// 2. hit esc to bring up the menu
	/// 3. go to main menu
	/// 4. start a new game
	/// 5. OBSERVE: FuelGauge has a value when called from start()
	/// 6. press a thrusters button
	/// 7. OBSERVE: FuelGauge is now null when called from UpdateFuelMeter()
	/// </summary>
	private UISlider FuelGugeProperty
	{
		get
		{
			return FuelGauge;
		}
		set
		{
			FuelGauge = value;
		}
	}
	
	private State game;
	private float fuelGaugeMaxWidth;
	private float fuelMax;
	private bool isMenuDisplayed = false;
	
	private string toggleSoundLabel
	{
		get
		{
			return "Turn Sound " + (Globals.IsSoundOn ? "Off" : "On");
		}
	}
	
	void Start ()
	{
		toggleSound();
		game = Globals.Game;
		game.CurrentModeChanged += HandleGameModeChanged;
		game.FuelRemainingChanged += HandleFuelRemainingChanged;
		
		//Fuel
		game.FuelRemaining = fuelMax = 400;
		fuelGaugeMaxWidth = FuelGugeProperty.foreground.localScale.x;
	}

	void HandleGameModeChanged(object sender, EventArgs<Mode> e)
	{
		//Debug.Log("Gamemode is now " + e.Data.ToString() + " == " + game.CurrentMode.ToString());
		
		//show menu
		if (isMenuDisplayed == false && game.CurrentMode != Mode.InGame)
		{
			displayGui(nguiMenu);
			changeButton(nguiMenuSoundButton, toggleSoundLabel);
			if (game.CurrentMode == Mode.Paused)
			{
				changeButton(nguiMenuTopButton, "Resume Game", "OnClickResume");
			}
			else if (game.CurrentMode == Mode.Win)
			{
				changeButton(nguiMenuTopButton, "Next Level", "OnClickNextLevel");
			}
			else if (game.CurrentMode == Mode.Lose)
			{
				changeButton(nguiMenuTopButton, "Retry Level", "OnClickRetry");
			}
		}
		
		//show space ship controls HUD
		else if (game.CurrentMode == Mode.InGame)
		{
			displayGui(nguiControls);
		}
	}
	
	void Update () {
		if (Input.GetKeyDown("escape"))
		{
			Time.timeScale = 0;
			game.CurrentMode = Mode.Paused;
		}
	}
	
	#region [ Button events ]
	public void OnClickResume()
	{
		Time.timeScale = 1;
		game.CurrentMode = Mode.InGame;
	}
	
	public void OnClickNextLevel()
	{
		Time.timeScale = 1;
		game.CurrentMode = Mode.InGame;
		Application.LoadLevel(Application.loadedLevel+1);
	}
	
	public void OnClickRetry()
	{
		Time.timeScale = 1;
		Globals.Game = new State(Mode.InGame);
		Application.LoadLevel(Application.loadedLevel);
	}
	
	public void OnClickToggleSound()
	{
		Globals.IsSoundOn = !Globals.IsSoundOn;
		changeButton(nguiMenuSoundButton, toggleSoundLabel);
		toggleSound();
	}
	
	public void OnClickMainMenu()
	{
		Time.timeScale = 1;
		Application.LoadLevel(0);
	}
	
	public void OnClickQuit()
	{
		Application.Quit();
	}
	#endregion [ Button events ]
	
	public void UpdateScore(int levelScore)
	{
		ScoreLevel.text = "Level Score:" + levelScore;
	}
	
	public void Win(int levelScore)
	{
		if (Globals.IsSoundOn)
		{
			audio.clip = WinClip;
			audio.Play();
		}
		Time.timeScale = 0;
		game.CurrentMode = Mode.Win;
		PlayerPrefs.SetInt(PlayerPrefKey.Level, Application.loadedLevel+1);
		int totalScore = levelScore;
		if (PlayerPrefs.HasKey(PlayerPrefKey.TotalScore))
		{
			totalScore = PlayerPrefs.GetInt(PlayerPrefKey.TotalScore) + levelScore;
			PlayerPrefs.SetInt(PlayerPrefKey.TotalScore, totalScore);
		}
		else
		{
			PlayerPrefs.SetInt(PlayerPrefKey.TotalScore, totalScore);
		}
		PlayerPrefs.Save();
		
		ScoreTotal.text = "Total Score:" + totalScore;
	}
	
	public void Lose()
	{
		Action afterExplosion = () => 
		{
			if (Globals.IsSoundOn)
			{
				audio.clip = LoseClip;
				audio.Play();
			}
			Time.timeScale = 0;
			game.CurrentMode = Mode.Lose;
		};
		
		//Refactor: try to move this call into PlayerShip.cs, tried couldn't figure out why Coroutine wouldn't work
		StartCoroutine(yieldForExplosion(afterExplosion));
	}
	
	/// <summary> Gives 3 seconds for the explosion animation to play.  </summary>
	private IEnumerator yieldForExplosion(Action afterExplosion)
	{
		yield return new WaitForSeconds(3);
		afterExplosion();
	}
	
	private void displayGui(GameObject primary)
	{
		if (primary == nguiMenu)
		{
			NGUITools.SetActive(nguiMenu,true);
			
			//SetActive() line below causes exception "!IsActive () && !m_RunInEditMode"
			//NGUITools.SetActive(nguiControls,false);
			
			isMenuDisplayed = true;
		}
		else
		{
			NGUITools.SetActive(nguiMenu,false);
			
			//deactivated this line due to "!IsActive () && !m_RunInEditMode" error above
			//NGUITools.SetActive(nguiControls,true);
			
			isMenuDisplayed = false;
		}
	}
	
	private void changeButton(GameObject button, string text, string actionName = null)
	{
		UILabel label = button.GetComponentInChildren(typeof(UILabel)) as UILabel;
		label.text = text;
		
		if (actionName != null)
		{
			UIButtonMessage buttonMessage = button.GetComponent("UIButtonMessage") as UIButtonMessage;
			buttonMessage.functionName = actionName;
		}
	}
	
	private void toggleSound()
	{
		bool isSoundOn = Globals.IsSoundOn;
				
		//toggle thruster sounds
		var buttonsWithSound = nguiControls.transform.GetComponentsInChildren<UIButtonSound>();
		foreach (UIButtonSound buttonSound in buttonsWithSound) {
			buttonSound.enabled = isSoundOn;
		}
		
		//toggle explosion sounds
//		var explosions = Globals.PlayerShip.GetComponent<PlayerKeyboard>().shipExplosions;
//		foreach (GameObject explosion in explosions) {
//			var sound = explosion.GetComponent<DetonatorSound>();
//			sound.on = isSoundOn;
//			sound.maxVolume = 0; //isSoundOn ? 1 : 
//			sound.enabled = isSoundOn;
//		}
	}
	
	#region [ Fuel ]
	void HandleFuelRemainingChanged (object sender, EventArgs<float> e)
	{
		if (e.Data <= 0.01)
		{
			//disable buttons "Left Thrusters" & "Right Thrusters" otherwise they will still make the thruster audio
			foreach (GameObject test in GameObject.FindGameObjectsWithTag("HudButton"))
				test.SetActive(false);
			this.Lose();
		}
		else
		{
			UpdateFuelMeter(e.Data/fuelMax);
		}
	}
	
	public void UpdateFuelMeter(float toPercent)
	{
		//Debug.Log(toPercent + " of " + fuelGaugeMaxWidth + " in " + FuelGauge.foreground.localScale.ToString());
		
		//Update FuelGauge width
		FuelGugeProperty.foreground.localScale = new Vector3(
			fuelGaugeMaxWidth * toPercent,
			FuelGugeProperty.foreground.localScale.y,
			FuelGugeProperty.foreground.localScale.z);
		
		//Update FuelGauge color
		UISprite sliderSprite = FuelGugeProperty.foreground.GetComponent<UISprite>();		
		if (sliderSprite != null)
		{
			sliderSprite.color = Color.Lerp(Color.red, Color.green, toPercent);
		}
	}
	#endregion [ Fuel ]
}