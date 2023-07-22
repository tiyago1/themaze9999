using Zenject;
using Zenject.Asteroids;

namespace Maze
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameOver>();
            Container.Bind<ShipStateFactory>().AsSingle();
        }
    }
}