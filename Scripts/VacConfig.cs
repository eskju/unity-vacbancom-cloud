using UnityEngine;
using System.Collections;

public static class VacConfig
{
	// app version
	// should be increased with each build
	public static string VERSION = "0.1.00";

	// VAC-BAN.com API urls
	public static string VACBAN_HOST = "http://www.vac-ban.com/api/cloud/";
	public static string VACBAN_CHECK_VERSION = "checkVersion.php";
	public static string VACBAN_CHECK_PROFILE = "checkProfile.php";
	public static string VACBAN_CHECK_APIKEY = "checkApiKey.php";
	public static string VACBAN_GET_STACK = "getStack.php";
	public static string VACBAN_POST_STEAMIDS = "postSteamids.php";

	// Steam API urls
	public static string STEAM_HOST = "http://api.steampowered.com/";
	public static string STEAM_API_FRIENDS = "ISteamUser/GetFriendList/v0001/";
	public static string STEAM_API_DATA = "ISteamUser/GetPlayerSummaries/v0002/";

	// allowed requests per day & Steam web API key
	public static int MAX_REQUESTS_PER_DAY = 100000;
}
