public class GameInteractor
{
    private PlayerData _data;
    private GameRepository _repository;

    public GameInteractor(GameRepository repository)
    {
        _repository = repository;
        _data = new PlayerData(); // Дефолтные данные
    }

    public PlayerData Data => _data;

    public void SaveGame(PlayerData currentData)
    {
        _data = currentData;
        _repository.Save(_data);
    }

    public void LoadGame()
    {
        var loadedData = _repository.Load();
        if (loadedData != null) _data = loadedData;
    }
}