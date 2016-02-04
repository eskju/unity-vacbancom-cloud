/**
 * @author Chris Witte <info@cwdesigns.de>
 * @copyright VAC-BAN.com <www.vac-ban.com>
 */

using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class VacAuthentication : MonoBehaviour 
{
	// GameObjects defined by hierarchy
	public GameObject _errorMsg;

	// GameObjects defined at runtime
	public InputField _inputSteamProfile;
	public InputField _inputSteamApiKey;

	// player vars for runtime
	public string steamid;
	public string personaname;
	public string avatar;

	// temp check results
	private bool profileOk = false;
	private bool apiKeyOk = false;


	/**
	 * Automatically start...
	 * - find GameObjects and store references
	 * - load player preferences
	 * - check for updates (check version)
	 */
	void Start( )
	{
		GetComponent<VacViews>().ShowView( GetComponent<VacViews>().VIEW_AUTHENTICATE );

		// find GameObjects and store references
		this._inputSteamProfile = GameObject.Find( "Steam ID" ).GetComponent<InputField>();
		this._inputSteamApiKey = GameObject.Find( "Steam API Key" ).GetComponent<InputField>();
		GameObject.Find ("Version").GetComponent<Text> ().text = "v" + VacConfig.VERSION;

		// load player preferences
		this._inputSteamProfile.text = PlayerPrefs.GetString( "steam_id" );
		this._inputSteamApiKey.text = PlayerPrefs.GetString( "steam_api_key" );

		// check for updates (check version)
		StartCoroutine( "CheckVersion" );
	}


	/**
	 * Update every frame...
	 * - check authentication
	 * -> if okay, show dashboard view
	 * -> disable authentication view
	 */
	void Update( )
	{
		// check authentication
		if( this.apiKeyOk && this.profileOk ) 
		{
			// show dashboard view
			// disable authentication view
			GetComponent<VacViews>( ).ShowView( GetComponent<VacViews>( ).VIEW_DASHBOARD );
			GetComponent<VacAuthentication>( ).enabled = false;
		}
	}


	/**
	 * Check authentication
	 * - reset result checks
	 * - hide error layer
	 * - start authentication checks
	 */
	public void CheckAuthentication( )
	{
		// reset result checks
		this.apiKeyOk = false;
		this.profileOk = false;

		// hide error layer
		this._errorMsg.SetActive( false );

		// start authentication checks
		StartCoroutine( "CheckProfile" );
		StartCoroutine( "CheckApiKey" );
	}


	/**
	 * Start download for update
	 */
	public void OpenDownloadUrl( )
	{
		Application.OpenURL( "http://www.vac-ban.com/api/cloud/demo.zip" );
	}


	/**
	 * Check Version
	 * - request current app version via API
	 * - parse JSON
	 * - compare versions
	 * -> version outdated? show update view
	 */
	IEnumerator CheckVersion( )
	{
		// request current app version via API
		WWW www = new WWW( VacConfig.VACBAN_HOST + VacConfig.VACBAN_CHECK_VERSION );
		yield return www;
		
		try
		{
			// parse JSON
			var N = JSON.Parse( www.text );

			// compare versions
			if( N["version"].Value != VacConfig.VERSION )
			{
				// version outdated, show update view
				GetComponent<VacViews>( ).ShowView( GetComponent<VacViews>( ).VIEW_UPDATE );
			}
		}
		catch( Exception e ) 
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}
	}


	/**
	 * Check SteamID/profile
	 * - request profile data
	 * - parse JSON
	 * -> steamid exists? save data to player preferences
	 * -> steamid does not exist? show error layer
	 */
	IEnumerator CheckProfile( )
	{
		// request profile data
		WWW www = new WWW( VacConfig.VACBAN_HOST + VacConfig.VACBAN_CHECK_PROFILE + "?url=" + WWW.EscapeURL( this._inputSteamProfile.text ) );
		yield return www;

		try
		{
			// parse JSON
			var N = JSON.Parse( www.text );

			// steamid exists?
			if( N["success"].AsBool )
			{
				// save data to player preferences
				this.steamid = N["steamid"].Value;
				this.personaname = N["personaname"].Value;
				this.avatar = N["avatar"].Value;
				this.profileOk = true;
				
				PlayerPrefs.SetString( "steam_id", this.steamid );
			}
			else
			{
				// show error layer
				this._errorMsg.SetActive( true );
			}
		}
		catch( Exception e ) 
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
		}
	}


	/**
	 * Check Steam web API key
	 * - request dummy profile data
	 * - parse JSON
	 * - count items in response
	 * -> count equals expectation (= 1)? save data to player preferences
	 * -> count does not equal expectation (!= 1)? show error layer
	 */
	IEnumerator CheckApiKey( )
	{
		// request dummy profile data
		WWW www = new WWW( VacConfig.STEAM_HOST + VacConfig.STEAM_API_DATA + "?key=" + WWW.EscapeURL( this._inputSteamApiKey.text ) + "&steamids=76561197980468695" );
		yield return www;
		
		try
		{
			// parse JSON
			var N = JSON.Parse( www.text );

			// count equals expectation (= 1)?
			if( N["response"].AsObject["players"].AsArray.Count == 1 )
			{
				// save data to player preferences
				PlayerPrefs.SetString( "steam_api_key", this._inputSteamApiKey.text );
				this.apiKeyOk = true;
			}
			else
			{
				// show error layer
				this._errorMsg.SetActive( true );
			}
		}
		catch( Exception e ) 
		{
			// TBD: Errorhandler
			Debug.Log( e.Message );
			this._errorMsg.SetActive( true );
		}
	}
}
