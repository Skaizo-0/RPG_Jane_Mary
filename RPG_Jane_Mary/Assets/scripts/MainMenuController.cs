using UnityEngine.SceneManagement;

public class MainMenuController
{
    private readonly MainMenuView _view;
    private readonly IAudioService _audioService;

    public MainMenuController(MainMenuView view, IAudioService audioService)
    {
        _view = view;
        _audioService = audioService;


        _view.playButton.onClick.AddListener(PlayGame);
        _view.settingsButton.onClick.AddListener(OpenSettings);
        _view.backButton.onClick.AddListener(CloseSettings);


        _view.volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void PlayGame() => SceneManager.LoadScene(2); 

    private void OpenSettings()
    {
        _view.settingsWindow.SetActive(true);
        _view.mainButtonsPanel.SetActive(false);
    }

    private void CloseSettings()
    {
        _view.settingsWindow.SetActive(false);
        _view.mainButtonsPanel.SetActive(true);
    }

    private void SetVolume(float vol) => _audioService.SetVolume(vol);


}