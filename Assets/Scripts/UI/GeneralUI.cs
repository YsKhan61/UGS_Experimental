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
        SignInAsync(AccountType.UnityPlayerAccount);
    }

    public void SignInAnnonymously()
    {
        SignInAsync(AccountType.GuestAccount);
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
        _authServiceFacade.UpdatePlayerNameAsync(_nameInputField.text);
    }

    public void SignOut()
    {
        _authServiceFacade.SignOut(true);
        _authServiceFacade.ClearCachedSessionToken();
    }

    private async void SignInAsync(AccountType accountType)
    {
        _authServiceFacade.SignOut(true);
        _authServiceFacade.ClearCachedSessionToken();
        await _authServiceFacade.InitializeAndSubscribeToUnityServicesAsync();
        _authServiceFacade.LinkAccount = false;
        await _authServiceFacade.SignInAsync(AccountType.UnityPlayerAccount);
    }

    private async void UnlinkFromUniyPlayerAccount()
    {
        await _authServiceFacade.UnlinkUnityPlayerAccountAsync();
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
