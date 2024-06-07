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

    public void SignOut()
    {
        _authServiceFacade.SignOut(true);
        _authServiceFacade.ClearCachedSessionToken();
    }

    private async void SignInWithUnityPlayerAccountAsync()
    {
        _authServiceFacade.LinkAccount = false;
        await _authServiceFacade.SignInWithUnityPlayerAccountAsync();
    }

    private async void UnlinkFromUniyPlayerAccount()
    {
        await _authServiceFacade.UnlinkUnityPlayerAccountAsync();
        _authServiceFacade.SignOutUnityPlayerAccount();
    }

    private async void SignInAnnonymouslyAsync()
    {
        await _authServiceFacade.SignInAnnonymouslyAsync();
    }
}
