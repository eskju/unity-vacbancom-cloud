using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class VacRequest : MonoBehaviour 
{
	// GameObjects defined at runtime
	private bool objectsFound = false;
	private Text _text1;
	private Text _text2;
	private Text _text3;
	private Text _text4;
	private Text _progress1_label;
	private Text _progress2_label;
	private Text _progress3_label;
	private Text _progress4_label;
	private RectTransform _progress1_bar;
	private RectTransform _progress2_bar;
	private RectTransform _progress3_bar;
	private RectTransform _progress4_bar;

	// temp stats
	public int requestsVacBanCom = 0;
	public int requestsSteam = 0;
	public float totalRequestTime = 0;
	public int addedSteamIds = 0;

	// temp string containing steamids
	private string returnString = "";

	// temp stack vars
	public int stackSize = -1;
	public int stackCompleted = 0;
	public string stackAction;


	/**
	 * Automatically start...
	 */
	void Start( )
	{
		// request new stack
		StartCoroutine( "RequestNextStack" );
	}

	/**
	 * Automatically start when view becomes visible...
	 * - find GameObjects and store references
	 */
	void OnEnable( )
	{
		// find GameObjects and store references
		this._text1 = GameObject.Find( "1. Value" ).GetComponent<Text>();
		this._text2 = GameObject.Find( "2. Value" ).GetComponent<Text>();
		this._text3 = GameObject.Find( "3. Value" ).GetComponent<Text>();
		this._text4 = GameObject.Find( "4. Value" ).GetComponent<Text>();

		this._progress1_label = GameObject.Find( "ProgressLabel 1" ).GetComponent<Text>();
		this._progress2_label = GameObject.Find( "ProgressLabel 2" ).GetComponent<Text>();
		this._progress3_label = GameObject.Find( "ProgressLabel 3" ).GetComponent<Text>();
		this._progress4_label = GameObject.Find( "ProgressLabel 4" ).GetComponent<Text>();

		this._progress1_bar = GameObject.Find( "Progress 1" ).GetComponent<RectTransform>();
		this._progress2_bar = GameObject.Find( "Progress 2" ).GetComponent<RectTransform>();
		this._progress3_bar = GameObject.Find( "Progress 3" ).GetComponent<RectTransform>();
		this._progress4_bar = GameObject.Find( "Progress 4" ).GetComponent<RectTransform>();

		this.objectsFound = true;
	}


	/**
	 * Update every frame...
	 * - wait for GameObjects to be found
	 * -> check if stack is empty
	 * --> request new stack
	 * -> check if stack is completed
	 * --> send found steamids
	 */
	void Update( )
	{
		// wait for GameObjects to be found
		if( this.objectsFound )
		{
			// check if stack is empty
			if( this.stackSize == -1 )
			{
				// prevent multiple stack requests
				this.stackSize = -2;

				// clear temp string
				this.returnString = "";
			}

			// check if stack is completed
			if( this.stackCompleted == this.stackSize )
			{
				// mark stack as empty
				this.stackSize = -1;
				this.stackCompleted = 0;

				// send data to VAC-BAN.com
				switch( this.stackAction )
				{
					case "data":
						// send found steamids
						StartCoroutine( "PostData", VacConfig.VACBAN_POST_DATA );
						break;

					case "vac":
						// send found steamids
						StartCoroutine( "PostData", VacConfig.VACBAN_POST_VAC );
						break;

					case "friends":
						// send found steamids
						StartCoroutine( "PostData", VacConfig.VACBAN_POST_FRIENDS );
						break;

					case "stats":
						// send found steamids
						StartCoroutine( "PostData", VacConfig.VACBAN_POST_STATS );
						break;

					default:
						Debug.Log( "unknown action: " + this.stackAction );
						break;
				}
			}
		}
	}


	/**
	 * Request new stack...
	 * - request stack data
	 * - parse JSON
	 * - start requests
	 */
	IEnumerator RequestNextStack( )
	{
		// request stack data
		WWW www = new WWW( VacConfig.VACBAN_HOST + VacConfig.VACBAN_GET_STACK + "?steamid=" + PlayerPrefs.GetString( "steam_id" ) );
		yield return www;

		try
		{
			// parse JSON
			var N = JSON.Parse( www.text );
			this.stackAction = N["action"].Value;
			this.stackSize = N["ids"].AsArray.Count;

			// loop steamids
			for( int i = 0; i < N["ids"].AsArray.Count; i++ )
			{
				// start requests
				switch( this.stackAction )
				{
					case "data":
						StartCoroutine( "SteamRequestData", N["ids"].AsArray[i].Value );
						break;

					case "vac":
						StartCoroutine( "SteamRequestVac", N["ids"].AsArray[i].Value );
						break;

					case "friends":
						StartCoroutine( "SteamRequestFriends", N["ids"].AsArray[i].Value );
						break;

					case "stats":
						StartCoroutine( "SteamRequestStats", N["ids"].AsArray[i].Value );
						break;

					default:
						Debug.Log( "unknown action: " + this.stackAction );
						break;
				}
			}

		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}



		this.requestsVacBanCom++;
		// request type and ids from vac-ban.com
	}


	/**
	 * Request data...
	 * - request stack data
	 * - parse JSON
	 * - add data to temp string
	 */
	IEnumerator SteamRequestData( string steamids )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// request data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_DATA + "?key=" + PlayerPrefs.GetString( "steam_api_key" ) + "&steamids=" + steamids );
		yield return www;

		try
		{
			// parseJSON
			var N = JSON.Parse( www.text );

			for( int i = 0; i < N["response"].AsObject["players"].AsArray.Count; i++ )
			{
				// add found steamids to temp string
				string tmpString = "";
				tmpString += N["response"].AsObject["players"][i].AsObject["steamid"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["communityvisibilitystate"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["profilestate"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["personaname"].Value.Replace( ",", "" ) + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["lastlogoff"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["profileurl"].Value.Replace( "http://steamcommunity.com/", "" ) + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["avatar"].Value.Replace( "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/", "" ) + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["personastate"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["realname"].Value.Replace( ",", "" ) + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["primaryclanid"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["timecreated"].Value + ",";
				tmpString += N["response"].AsObject["players"][i].AsObject["personastateflags"].Value + ",";

				this.returnString += tmpString + "\n";
			}

		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}

		// update temp vars
		this.totalRequestTime += Time.realtimeSinceStartup - timeStart;
		this.requestsSteam++;
		this.stackCompleted++;
	}


	/**
	 * Request data...
	 * - request stack data
	 * - parse JSON
	 * - add data to temp string
	 */
	IEnumerator SteamRequestVac( string steamids )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// request data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_VAC + "?key=" + PlayerPrefs.GetString( "steam_api_key" ) + "&steamids=" + steamids );
		yield return www;

		try
		{
			// parseJSON
			var N = JSON.Parse( www.text );

			for( int i = 0; i < N["players"].AsArray.Count; i++ )
			{
				// add found steamids to temp string
				string tmpString = "";
				tmpString += N["players"][i].AsObject["SteamId"].Value + ",";
				tmpString += ( N["players"][i].AsObject["CommunityBanned"].AsBool ? 1 : 0 ) + ",";
				tmpString += ( N["players"][i].AsObject["VACBanned"].AsBool ? 1 : 0 ) + ",";
				tmpString += N["players"][i].AsObject["NumberOfVACBans"].Value + ",";
				tmpString += N["players"][i].AsObject["DaysSinceLastBan"].Value + ",";
				tmpString += N["players"][i].AsObject["NumberOfGameBans"].Value + ",";
				tmpString += N["players"][i].AsObject["EconomyBan"].Value + ",";

				this.returnString += tmpString + "\n";
			}

		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}

		// update temp vars
		this.totalRequestTime += Time.realtimeSinceStartup - timeStart;
		this.requestsSteam++;
		this.stackCompleted++;
	}


	/**
	 * Request data...
	 * - request stack data
	 * - parse JSON
	 * - add found steamids to temp string
	 */
	IEnumerator SteamRequestFriends( string steamid )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// request data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_FRIENDS + "?key=" + PlayerPrefs.GetString( "steam_api_key" ) + "&steamid=" + steamid + "&relationship=friend" );
		yield return www;

		try
		{
			this.returnString += steamid + ":";

			// parseJSON
			var N = JSON.Parse( www.text );

			for( int i = 0; i < N["friendslist"].AsObject["friends"].AsArray.Count; i++ )
			{
				// add found steamids to temp string
				this.returnString += N["friendslist"].AsObject["friends"][i].AsObject["steamid"].Value + ",";
			}

			this.returnString += "\n";

		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}

		// update temp vars
		this.totalRequestTime += Time.realtimeSinceStartup - timeStart;
		this.requestsSteam++;
		this.stackCompleted++;
	}


	/**
	 * Request data...
	 * - request stack data
	 * - parse JSON
	 * - add data to temp string
	 */
	IEnumerator SteamRequestStats( string steamid )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// request data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_DATA + "?appid=730&key=" + PlayerPrefs.GetString( "steam_api_key" ) + "&steamid=" + steamid );
		yield return www;

		try
		{
			// parseJSON
			var N = JSON.Parse( www.text );
			// @TODO: handle data
		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}

		// update temp vars
		this.totalRequestTime += Time.realtimeSinceStartup - timeStart;
		this.requestsSteam++;
		this.stackCompleted++;
	}


	/**
	 * Send data to VAC-BAN.com API...
	 * - create form object
	 * - send POST request
	 * - parse JSON
	 */
	IEnumerator PostData( string url )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// create form object
		var form = new WWWForm( );
		form.AddField( "steamid", PlayerPrefs.GetString( "steam_id" ) );
		form.AddField( "data", this.returnString );

		// send POST request
		var www = new WWW( VacConfig.VACBAN_HOST + url, form );
		yield return www;

		try
		{
			// parse JSON
			var N = JSON.Parse( www.text );

			// add numer of new profiles to temp var
			this.addedSteamIds += N["updated"].AsInt;

			// update temp vars
			this.totalRequestTime += Time.realtimeSinceStartup - timeStart;

			// update UI: stats
			this._text1.text = this.requestsSteam.ToString( );
			this._text2.text = this.addedSteamIds.ToString( );
			this._text3.text = ( Mathf.Round (this.totalRequestTime / this.requestsSteam * 100) / 100 ).ToString( );
			this._text4.text = this.stackAction.ToUpper( );

			// update UI: progress labels
			this._progress1_label.text = N["stats"].AsObject["data"].AsObject["done"].Value + " / " + N["stats"].AsObject["data"].AsObject["total"].Value + " (" + N["stats"].AsObject["data"].AsObject["progress"].Value + "%)";
			this._progress2_label.text = N["stats"].AsObject["vac"].AsObject["done"].Value + " / " + N["stats"].AsObject["vac"].AsObject["total"].Value + " (" + N["stats"].AsObject["vac"].AsObject["progress"].Value + "%)";
			this._progress3_label.text = N["stats"].AsObject["friends"].AsObject["done"].Value + " / " + N["stats"].AsObject["friends"].AsObject["total"].Value + " (" + N["stats"].AsObject["friends"].AsObject["progress"].Value + "%)";
			this._progress4_label.text = N["stats"].AsObject["stats"].AsObject["done"].Value + " / " + N["stats"].AsObject["stats"].AsObject["total"].Value + " (" + N["stats"].AsObject["stats"].AsObject["progress"].Value + "%)";

			// update UI: progress bars
			this._progress1_bar.offsetMax = new Vector2( -2.72f * ( 100 - N["stats"].AsObject["data"].AsObject["progress"].AsFloat ), this._progress1_bar.offsetMax.y );
			this._progress2_bar.offsetMax = new Vector2( -2.72f * ( 100 - N["stats"].AsObject["vac"].AsObject["progress"].AsFloat ), this._progress2_bar.offsetMax.y );
			this._progress3_bar.offsetMax = new Vector2( -2.72f * ( 100 - N["stats"].AsObject["friends"].AsObject["progress"].AsFloat ), this._progress3_bar.offsetMax.y );
			this._progress4_bar.offsetMax = new Vector2( -2.72f * ( 100 - N["stats"].AsObject["stats"].AsObject["progress"].AsFloat ), this._progress4_bar.offsetMax.y );

			// request new stack
			StartCoroutine( "RequestNextStack" );
		}
		catch( Exception e )
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
			Debug.Log( www.text );

			// request new stack
			StartCoroutine( "RequestNextStack" );
		}
	}
}
