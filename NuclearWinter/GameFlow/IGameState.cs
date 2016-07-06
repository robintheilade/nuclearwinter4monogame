namespace NuclearWinter.GameFlow
{
    public interface IGameState<out T> where T : NuclearGame
    {
        void Start();
        void Stop();
        void OnActivated();
        void OnExiting();
        bool UpdateFadeIn(float elapsedTime);
        void DrawFadeIn();
        bool UpdateFadeOut(float elapsedTime);
        void DrawFadeOut();
        void Update(float elapsedTime);
        void Draw();

    }
}
