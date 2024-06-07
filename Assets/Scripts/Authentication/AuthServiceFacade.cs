using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

public enum AccountType
{
    GuestAccount,
    UnityPlayerAccount,
    None
}

/// <summary>
/// This struct is used to store the access token and session token.
/// </summary>
public struct UserTokens
{
    public string AccessToken;
    public string SessionToken;
}

public class AuthServiceFacade
{
    public AccountType AccountType { get; private set; }

    /// <summary>
    /// Generate the initialization options for Unity Services
    /// </summary>
    /// <param name="profileName">the profile name to be set in the options</param>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/profile-management</remarks>
    public InitializationOptions GenerateInitializationOption(string profileName)
    {
        InitializationOptions initializationOptions = new();
        initializationOptions.SetProfile(profileName);
        return initializationOptions;
    }

    /// <summary>
    /// Initialize Unity Services and subscribe to the events
    /// </summary>
    /// <param name="options">The sign-in options</param>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/get-started</remarks>
    public async Task InitializeAndSubscribeToUnityServicesAsync(InitializationOptions options = default)
    {
        try
        {
            await UnityServices.InitializeAsync(options);
            SubscribeToAuthenticationEvents();
            AccountType = AccountType.None;
        }
        catch (Exception e)
        {
            Debug.LogError("Unity Services initialization failed: " + e.Message);
        }
    }

    /// <summary>
    /// Sign in annonmously
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/use-anon-sign-in</remarks>
    public async Task SignInAnnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            AccountType = AccountType.GuestAccount;
        }
        catch (Exception e)
        {
            Debug.LogError("Sign in anonymously failed: " + e.Message);
        }
    }


    /// <summary>
    /// Signing out resets the access token and player ID. 
    /// The Authentication service preserves the session token to allow the player to sign in to the same account in the future.
    /// If you want to sign in to a different account, clear your session token or switch profiles while you are signed out.
    /// </summary>
    /// <param name="clearCredentials">clear playerID, access token, session token</param>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/sign-out</remarks>
    public void SignOut(bool clearCredentials = false)
    {
        switch (AccountType)
        {
            case AccountType.UnityPlayerAccount:
                PlayerAccountService.Instance.SignOut();
                break;
        }

        AuthenticationService.Instance.SignOut(clearCredentials);
        AccountType = AccountType.None;
    }

    /// <summary>
    /// DeleteAccountAsync() only deletes the player’s Unity Authentication account.
    /// Upon such a deletion request, you must delete all associated player data connected to the player’s Unity Authentication account and other UGS services you use.
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/delete-accounts</remarks>
    public async void DeleteAccount()
    {
        try
        {
            await AuthenticationService.Instance.DeleteAccountAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Delete account failed: " + e.Message);
        }
    }

    /// <summary>
    /// Sign in the cached user account if the session token exists
    /// </summary>
    /// <remarks></remarks>
    public async Task SignInCachedUserAccount()
    {
        // Check if a cached player already exists by checking if the session token exists
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            // if not, then do nothing
            return;
        }

        try
        {
            await SignInAnnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Sign in cached user account failed: " + e.Message);
        }
    }

    public async void SignUserWithCustomTokenWithAutoRefresh()
    {
        try
        {
            if (IsSessionTokenExist())
            {
                await SignInCachedUserAccount();
            }
            else
            {
                UserTokens userTokens = new();
                AuthenticationService.Instance.ProcessAuthenticationTokens(userTokens.AccessToken, userTokens.SessionToken);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Sign in with custom token failed: " + e.Message);
        }
    }

    /// <summary>
    /// Get the player name from the Authentication service
    /// </summary>
    public async Task<string> GetPlayerName()
    {
        if (!IsAuthenticationServiceAuthorized())
            return null;

        string playerName = AuthenticationService.Instance.PlayerName;
        if (!string.IsNullOrEmpty(playerName))
        {
            return playerName;
        }

        try
        {
            playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            Debug.Log("Player name: " + playerName);
            return playerName;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get player name: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Update the player name
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/player-name-management</remarks>
    public async void UpdatePlayerName(string name)
    {
        if (!IsAuthenticationServiceSignedIn())
            return;

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name.RemoveWhiteSpaceAndLimitLength(50));
            Debug.Log("Player name updated successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to update player name: " + e.Message);
        }
    }

    /// <summary>
    /// Clear the cached session token for the current profile.
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/session-token-management</remarks>
    public void ClearCachedSessionToken()
    {
        if (IsSessionTokenExist())
        {
            AuthenticationService.Instance.ClearSessionToken();
        }
    }

    /// <summary>
    /// To verify if a session token is currently cached for the current profile
    /// </summary>
    /// <returns></returns>
    public bool IsSessionTokenExist()
    {
        return AuthenticationService.Instance.SessionTokenExists;
    }

    /// <summary>
    /// To verify if the player is signed in and authorized
    /// </summary>
    /// <returns></returns>
    public bool IsAuthenticationServiceAuthorized()
    {
        return AuthenticationService.Instance.IsAuthorized;
    }

    /// <summary>
    /// To verify if the player is signed in
    /// </summary>
    public bool IsAuthenticationServiceSignedIn()
    {
        return AuthenticationService.Instance.IsSignedIn;
    }

    /// <summary>
    /// Get the access token from the Authentication service
    /// </summary>
    public string GetAuthenticationServiceAccessToken()
    {
        string accessToken = AuthenticationService.Instance.AccessToken;
        Debug.Log("Access token: " + accessToken);
        return accessToken;
    }

    /// <summary>
    /// Players must be signed out to switch the current profile. 
    /// Use AuthenticationService.Instance.SwitchProfile(profileName). 
    /// The profile name only supports alphanumeric values, `-`, `_` and has a maximum length of 30 characters
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/profile-management</remarks>
    public void SwitchProfile(string profileName)
    {
        AuthenticationService.Instance.SwitchProfile(profileName.FilterStringToLetterDigitDashUnderscoreMaxLength(30));
    }

    /// <summary>
    /// Get the current profile name
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/profile-management</remarks>
    public string GetCurrentProfile()
    {
        return AuthenticationService.Instance.Profile;
    }

    /// <summary>
    /// Check the authentication states
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/authentication-session</remarks>
    public void CheckAuthenticationStates()
    {
        Debug.Log("IsSignedIn: " + AuthenticationService.Instance.IsSignedIn);
        Debug.Log("IsAuthorized: " + AuthenticationService.Instance.IsAuthorized);
        Debug.Log($"Is Expired: " + AuthenticationService.Instance.IsExpired);
        Debug.Log("IsSessionTokenExist: " + IsSessionTokenExist());
    }


    /// <summary>
    /// fetch the current player information. This is useful for identity management.
    /// </summary>
    /// <returns>This return the player’s creation time and the linked identities</returns>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/player-info</remarks>
    public async Task<PlayerInfo> GetPlayerInfo()
    {
        try
        {
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            LogPlayerInfo(playerInfo);
            return playerInfo;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to get player info: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Is the Player Account Service signed in
    /// </summary>
    public bool IsPlayerAccountServiceSignedIn()
    {
        return PlayerAccountService.Instance.IsSignedIn;
    }

    /// <summary>
    /// Initializes the Player Account Service
    /// </summary>
    public async Task InitializePlayerAccountService()
    {
        if (IsPlayerAccountServiceSignedIn())
            return;

        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to initialize Player Account Service: " + e.Message);
        }
    }

    public string GetPlayerAccountAccessToken()
    {
        return PlayerAccountService.Instance.AccessToken;
    }

    /// <summary>
    /// Create a new Unity Authentication player with the Unity Player Accounts credentials.
    /// Sign in an existing player using the Unity Player Accounts credentials.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/unity-player-accounts</remarks>
    public async Task SignInWithUnityPlayerAccountAsync()
    {
        if (IsPlayerAccountServiceSignedIn())
        {
            Debug.Log("Player Account Service is already signed in");
            return;
        }

        try
        {
            await InitializePlayerAccountService();
        }
        catch (Exception e)
        {
            Debug.LogError("Sign in with Unity Player Account failed: " + e.Message);
        }
    }

    /// <summary>
    /// Link the current player account with the Unity Player Accounts credentials.
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/unity-player-accounts</remarks>
    public async Task LinkWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
        }
        catch (Exception e)
        {
            Debug.LogError("Link with Unity Player Account failed: " + e.Message);
        }
    }

    /// <summary>
    /// Unlink the current player account from the Unity Player Accounts credentials.
    /// </summary>
    /// <remarks>https://docs.unity.com/ugs/en-us/manual/authentication/manual/unity-player-accounts</remarks>
    public async Task UnlinkUnityAsync()
    {
        try
        {
            await AuthenticationService.Instance.UnlinkUnityAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Unlink Unity Player Account failed: " + e.Message);
        }
    }

    public void SubscribeToAuthenticationEvents()
    {
        AuthenticationService.Instance.SignedIn += AuthServiceSignedIn;
        AuthenticationService.Instance.SignInFailed += AuthServiceSignedInFailed;
        AuthenticationService.Instance.SignedOut += AuthServiceSignedOut;

        PlayerAccountService.Instance.SignedIn += LinkPlayerAccountWithAuthentication;
        PlayerAccountService.Instance.SignedOut += PlayerAcountSignedOut;
    }

    public void UnSubscribeFromEvents()
    {
        AuthenticationService.Instance.SignedIn -= AuthServiceSignedIn;
        AuthenticationService.Instance.SignInFailed -= AuthServiceSignedInFailed;
        AuthenticationService.Instance.SignedOut -= AuthServiceSignedOut;

        PlayerAccountService.Instance.SignedIn -= LinkPlayerAccountWithAuthentication;
        PlayerAccountService.Instance.SignedOut -= PlayerAcountSignedOut;
    }


    private void AuthServiceSignedIn()
    {
        Debug.Log("AuthServiceFacade: Signed in");
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
    }

    private void AuthServiceSignedInFailed(RequestFailedException e)
    {
        Debug.Log("AuthServiceFacade: Signed in failed " + e.Message);
    }

    private void AuthServiceSignedOut()
    {
        Debug.Log("AuthServiceFacade: Signed out");
    }

    private async void LinkPlayerAccountWithAuthentication()
    {
        Debug.Log("Player Account Service Signed in with Access Token: " + GetPlayerAccountAccessToken());

        // now connect the Unity Player Account with the Unity Authentication
        if (IsPlayerAccountServiceSignedIn())
        {
            string accessToken = GetPlayerAccountAccessToken();
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            AccountType = AccountType.UnityPlayerAccount;
        }
    }

    private void PlayerAcountSignedOut()
    {
        Debug.Log("Player Account Service Signed out");
    }

    private void LogPlayerInfo(PlayerInfo playerInfo)
    {
        Debug.Log("Player ID: " + playerInfo.Id);
        Debug.Log("Player creation time: " + playerInfo.CreatedAt);

        List<Identity> identities = playerInfo.Identities;
        if (identities == null || identities.Count == 0)
        {
            Debug.Log("Player has no linked identities");
            return;
        }

        Debug.Log("Player linked identities: ");
        foreach (var identity in identities)
        {
            Debug.Log("Identity Type: " + identity.TypeId);
            Debug.Log("Identity ID: " + identity.UserId);
        }
    }
}
