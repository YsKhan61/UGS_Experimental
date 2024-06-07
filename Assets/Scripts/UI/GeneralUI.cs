using System;
using TMPro;
using UnityEngine;


public class GeneralUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _nameInputField;

    private AuthServiceFacade _authServiceFacade;

    private void Awake()
    {
        _authServiceFacade = new AuthServiceFacade();
        _ = _authServiceFacade.InitializeAndSubscribeToUnityServicesAsync();
    }

    private void OnEnable()
    {
        _authServiceFacade.OnAuthServiceSignedIn += OnAuthServiceSignedIn;
        _authServiceFacade.OnAuthServiceSignedOut += OnAuthServiceSignedOut;
    }

    private void OnDisable()
    {
        _authServiceFacade.OnAuthServiceSignedIn -= OnAuthServiceSignedIn;
        _authServiceFacade.OnAuthServiceSignedOut -= OnAuthServiceSignedOut;
    }

    private void OnDestroy()
    {
        _authServiceFacade.UnsubscribeFromEvents();
    }

    public void SignInWithUnityPlayerAccount()
    {
        SignInWithUnityPlayerAccountAsync();
    }

    public void SignInAnnonymously()
    {
        SignInAnnonymouslyAsync();
    }

    public void LinkWithUnityPlayerAccount()
    {
        _authServiceFacade.LinkWithUnityPlayerAccountAsync();
    }

    public void UnlinkFromUnityPlayerAccount()
    {
        UnlinkFromUniyPlayerAccount();
    }

    public void UpdatePlayerName()
    {
        _authServiceFacade.UpdatePlayerName(_nameInputField.text);
    }

    public void SignOut()
    {
        _authServiceFacade.SignOut(true);
        _authServiceFacade.ClearCachedSessionToken();
    }

    private void SignInWithUnityPlayerAccountAsync()
    {
        _authServiceFacade.LinkAccount = false;
        // await _authServiceFacade.SignInWithUnityPlayerAccountAsync();
        _authServiceFacade.SignInWithUnityPlayerAccount();
    }

    private async void UnlinkFromUniyPlayerAccount()
    {
        await _authServiceFacade.UnlinkUnityPlayerAccountAsync();
        // _authServiceFacade.SignOutUnityPlayerAccount();
    }

    private async void SignInAnnonymouslyAsync()
    {
        await _authServiceFacade.SignInAnnonymouslyAsync();
    }

    private async void OnAuthServiceSignedIn()
    {
        string playerName = await _authServiceFacade.GetPlayerName();
        _nameInputField.text = playerName;
    }

    private void OnAuthServiceSignedOut()
    {
        _nameInputField.text = "";
    }
}
