using UnityEngine;
using System.Collections;

public class VacViews : MonoBehaviour 
{
	// GameObjects defined by hierarchy
	public GameObject VIEW_AUTHENTICATE;
	public GameObject VIEW_UPDATE;
	public GameObject VIEW_DASHBOARD;


	/**
	 * Automatically start...
	 * - show authentication view
	 */
	void Start( )
	{
		this.ShowView( this.VIEW_AUTHENTICATE );
	}


	/**
	 * Show view...
	 * @param GameObject view
	 */
	public void ShowView( GameObject view )
	{
		this.VIEW_AUTHENTICATE.SetActive( false );
		this.VIEW_UPDATE.SetActive( false );
		this.VIEW_DASHBOARD.SetActive( false );

		view.SetActive( true );
	}
}
