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
        _authServiceFacade.UnSubscribeFromEvents();
    }

    public void SignInWithUnityPlayerAccount()
    {
        SignInWithUnityPlayerAccountAsync();
    }

    public void SignInAnnonymously()
    {
        SignInAnnonymouslyAsync();
    }

    public void SignOut()
    {
        _authServiceFacade.SignOut(true);
    }

    private async void SignInWithUnityPlayerAccountAsync()
    {
        await _authServiceFacade.SignInWithUnityPlayerAccountAsync();
    }

    private async void SignInAnnonymouslyAsync()
    {
        await _authServiceFacade.SignInAnnonymouslyAsync();
    }
}
