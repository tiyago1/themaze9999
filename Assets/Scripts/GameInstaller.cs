using Zenject;

namespace Maze
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameOver>().OptionalSubscriber();
            // Container.DeclareSignal<AStartCompleted>().OptionalSubscriber();
            Container.DeclareSignal<MazeGenerateFinished>().OptionalSubscriber();
        }
    }
}