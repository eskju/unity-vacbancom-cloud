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
		this.objectsFound = true;
	}


	/**
	 * Update every frame...
	 * - wait for GameObjects to be found
	 * -> update UI output
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
			// update UI output
			this._text1.text = this.requestsSteam.ToString( );
			this._text2.text = this.addedSteamIds.ToString( );
			this._text3.text = ( Mathf.Round (this.totalRequestTime / this.requestsSteam * 100) / 100 ).ToString( );
			this._text4.text = ( VacConfig.MAX_REQUESTS_PER_DAY - this.requestsSteam ).ToString( );

			// check if stack is empty
			if( this.stackSize == -1 )
			{
				// prevent multiple stack requests
				this.stackSize = -2;

				// clear temp string
				this.returnString = "";

				// request new stack
				StartCoroutine( "RequestNextStack" );
			}

			// check if stack is completed
			if( this.stackCompleted == this.stackSize )
			{
				// mark stack as empty
				this.stackSize = -1;
				this.stackCompleted = 0;

				// send found steamids
				StartCoroutine( "PostSteamIds" );
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
			string apiAction = N["action"].Value;
			this.stackSize = N["ids"].AsArray.Count;

			// loop steamids
			for( int i = 0; i < N["ids"].AsArray.Count; i++ )
			{
				// start requests
				StartCoroutine( "DoRequest", N["ids"].AsArray[i].Value );
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
	 * - add found steamids to temp string
	 */
	IEnumerator DoRequest( string steamid )
	{
		// set temp time
		float timeStart = Time.realtimeSinceStartup;

		// request data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_FRIENDS + "?key=" + PlayerPrefs.GetString( "steam_api_key" ) + "&steamid=" + steamid + "&relationship=friend" );
		yield return www;

		try
		{
			// parseJSON
			var N = JSON.Parse( www.text );

			for( int i = 0; i < N["friendslist"].AsObject["friends"].AsArray.Count; i++ )
			{
				// add found steamids to temp string
				this.returnString += N["friendslist"].AsObject["friends"][i].AsObject["steamid"].Value + ",";
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
	 * Send SteamIDs to VAC-BAN.com API...
	 * - create form object
	 * - send POST request
	 * - parse JSON
	 */
	IEnumerator PostSteamIds( )
	{
		// create form object
		var form = new WWWForm( );
		form.AddField( "steamid", PlayerPrefs.GetString( "steam_id" ) );
		form.AddField( "steamids", this.returnString );

		// send POST request
		var www = new WWW( VacConfig.VACBAN_HOST + VacConfig.VACBAN_POST_STEAMIDS, form );
		yield return www;
	
		// parse JSON
		var N = JSON.Parse( www.text );

		// add numer of new profiles to temp var
		this.addedSteamIds += N["added"].AsInt;
	}
}
