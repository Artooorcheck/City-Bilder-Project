using CityBuilder.Infrastructure.DI;
using CityBuilder.Presentation.View;
using Serilog;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CityBuilder.Presentation.Presenters
{
    /// <summary>
    /// Creates presentation layer components and wires them with the dependency container.
    /// </summary>
    public sealed class PresentationRoot : MonoBehaviour
    {
        private IObjectResolver _resolver;

        /// <summary>
        /// Locates the lifetime scope and constructs presentation components.
        /// </summary>
        private void Awake()
        {
            var scope = LifetimeScope.Find<GameLifetimeScope>();
            if (scope == null)
            {
                Log.Error("[PresentationRoot.Awake] GameLifetimeScope not found. Presentation cannot initialize.");
                return;
            }

            _resolver = scope.Container;
            Setup();
        }

        /// <summary>
        /// Instantiates views and presenters required for gameplay UI.
        /// </summary>
        private void Setup()
        {
            if (_resolver == null)
            {
                return;
            }

            var gridObject = new GameObject("GridView");
            gridObject.transform.SetParent(transform, false);
            var gridView = gridObject.AddComponent<GridView>();
            _resolver.Inject(gridView);

            var ghostObject = new GameObject("BuildingGhost");
            ghostObject.transform.SetParent(transform, false);
            var ghostView = ghostObject.AddComponent<BuildingGhostView>();
            _resolver.Inject(ghostView);

            var hudPresenter = gameObject.AddComponent<HudPresenter>();
            _resolver.Inject(hudPresenter);

            var interactionPresenter = gameObject.AddComponent<CityInteractionPresenter>();
            _resolver.Inject(interactionPresenter);

            var cameraController = gameObject.AddComponent<CameraController>();
            _resolver.Inject(cameraController);

            cameraController.Initialize(gridView);
            hudPresenter.Initialize(interactionPresenter);
            interactionPresenter.Initialize(gridView, ghostView, hudPresenter);
        }
    }
}
