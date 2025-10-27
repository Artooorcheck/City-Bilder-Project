using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CityBuilder.Application.Events;
using CityBuilder.Application.Interfaces;
using CityBuilder.Application.UseCases;
using CityBuilder.Domain;
using CityBuilder.Infrastructure.Config;
using CityBuilder.Presentation.View;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using CityBuilder.Application.Services;

namespace CityBuilder.Presentation.Presenters
{
    public sealed class CityInteractionPresenter : MonoBehaviour
    {
        private readonly Dictionary<Guid, BuildingView> _views = new();
        private readonly List<IDisposable> _subscriptions = new();

        private IBuildingCatalog _catalog;
        private ICityRepository _cityRepository;
        private IEconomyRepository _economyRepository;
        private PlaceBuildingUseCase _placeBuildingUseCase;
        private MoveBuildingUseCase _moveBuildingUseCase;
        private RemoveBuildingUseCase _removeBuildingUseCase;
        private UpgradeBuildingUseCase _upgradeBuildingUseCase;
        private SaveGameUseCase _saveGameUseCase;
        private LoadGameUseCase _loadGameUseCase;
        private GameplaySettings _gameplaySettings;
        private EconomyTickService _economyTickService;
        private ISubscriber<BuildingPlacedEvent> _buildingPlacedSubscriber;
        private ISubscriber<BuildingRemovedEvent> _buildingRemovedSubscriber;
        private ISubscriber<BuildingMovedEvent> _buildingMovedSubscriber;
        private ISubscriber<BuildingUpgradedEvent> _buildingUpgradedSubscriber;
        private ISubscriber<EconomyChangedEvent> _economyChangedSubscriber;
        private ISubscriber<NotEnoughGoldEvent> _notEnoughSubscriber;
        private ISubscriber<GameSavedEvent> _savedSubscriber;
        private ISubscriber<GameLoadedEvent> _loadedSubscriber;

        private GridView _gridView;
        private BuildingGhostView _ghostView;
        private HudPresenter _hudPresenter;

        private string _placingBuildingTypeId;
        private Guid? _selectedBuildingId;
        private bool _moveMode;
        private int _rotation;
        private IDisposable _incomeSubscription;

        [Inject]
        public void Construct(
            IBuildingCatalog catalog,
            ICityRepository cityRepository,
            IEconomyRepository economyRepository,
            PlaceBuildingUseCase placeBuildingUseCase,
            MoveBuildingUseCase moveBuildingUseCase,
            RemoveBuildingUseCase removeBuildingUseCase,
            UpgradeBuildingUseCase upgradeBuildingUseCase,
            SaveGameUseCase saveGameUseCase,
            LoadGameUseCase loadGameUseCase,
            GameplaySettings gameplaySettings,
            EconomyTickService economyTickService,
            ISubscriber<BuildingPlacedEvent> buildingPlacedSubscriber,
            ISubscriber<BuildingRemovedEvent> buildingRemovedSubscriber,
            ISubscriber<BuildingMovedEvent> buildingMovedSubscriber,
            ISubscriber<BuildingUpgradedEvent> buildingUpgradedSubscriber,
            ISubscriber<EconomyChangedEvent> economyChangedSubscriber,
            ISubscriber<NotEnoughGoldEvent> notEnoughSubscriber,
            ISubscriber<GameSavedEvent> savedSubscriber,
            ISubscriber<GameLoadedEvent> loadedSubscriber)
        {
            _catalog = catalog;
            _cityRepository = cityRepository;
            _economyRepository = economyRepository;
            _placeBuildingUseCase = placeBuildingUseCase;
            _moveBuildingUseCase = moveBuildingUseCase;
            _removeBuildingUseCase = removeBuildingUseCase;
            _upgradeBuildingUseCase = upgradeBuildingUseCase;
            _saveGameUseCase = saveGameUseCase;
            _loadGameUseCase = loadGameUseCase;
            _gameplaySettings = gameplaySettings;
            _economyTickService = economyTickService;
            _buildingPlacedSubscriber = buildingPlacedSubscriber;
            _buildingRemovedSubscriber = buildingRemovedSubscriber;
            _buildingMovedSubscriber = buildingMovedSubscriber;
            _buildingUpgradedSubscriber = buildingUpgradedSubscriber;
            _economyChangedSubscriber = economyChangedSubscriber;
            _notEnoughSubscriber = notEnoughSubscriber;
            _savedSubscriber = savedSubscriber;
            _loadedSubscriber = loadedSubscriber;
        }

        public void Initialize(GridView gridView, BuildingGhostView ghostView, HudPresenter hudPresenter)
        {
            _gridView = gridView;
            _ghostView = ghostView;
            _hudPresenter = hudPresenter;
            if (_cityRepository == null || _gameplaySettings == null)
            {
                return;
            }

            _gridView.Initialize(_cityRepository.City.Width, _cityRepository.City.Height);
            ConfigureHud();
            RebuildAllViews();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            foreach (var disposable in _subscriptions)
            {
                disposable.Dispose();
            }

            _subscriptions.Clear();
            _incomeSubscription?.Dispose();
            _incomeSubscription = null;
        }

        private void SubscribeEvents()
        {
            if (_buildingPlacedSubscriber != null)
            {
                _subscriptions.Add(_buildingPlacedSubscriber.Subscribe(OnBuildingPlaced));
            }

            if (_buildingRemovedSubscriber != null)
            {
                _subscriptions.Add(_buildingRemovedSubscriber.Subscribe(OnBuildingRemoved));
            }

            if (_buildingMovedSubscriber != null)
            {
                _subscriptions.Add(_buildingMovedSubscriber.Subscribe(OnBuildingMoved));
            }

            if (_buildingUpgradedSubscriber != null)
            {
                _subscriptions.Add(_buildingUpgradedSubscriber.Subscribe(OnBuildingUpgraded));
            }

            if (_economyChangedSubscriber != null)
            {
                _subscriptions.Add(_economyChangedSubscriber.Subscribe(evt => _hudPresenter?.SetGold(evt.Gold)));
            }

            if (_notEnoughSubscriber != null)
            {
                _subscriptions.Add(_notEnoughSubscriber.Subscribe(evt => _hudPresenter?.ShowNotification($"Need {evt.Required} gold")));
            }

            if (_savedSubscriber != null)
            {
                _subscriptions.Add(_savedSubscriber.Subscribe(evt => _hudPresenter?.ShowNotification($"Saved to {evt.Path}")));
            }

            if (_loadedSubscriber != null)
            {
                _subscriptions.Add(_loadedSubscriber.Subscribe(_ =>
                {
                    RebuildAllViews();
                    DeselectBuilding();
                }));
            }

            if (_economyTickService != null)
            {
                _incomeSubscription = _economyTickService.IncomeStream.Subscribe(income =>
                {
                    if (income > 0)
                    {
                        _hudPresenter?.ShowNotification($"+{income} gold");
                    }
                });
            }
        }

        private void ConfigureHud()
        {
            if (_hudPresenter == null || _catalog == null || _economyRepository == null || _gameplaySettings == null)
            {
                return;
            }

            var definitions = _catalog.All.ToList();
            if (definitions.Count >= 3)
            {
                _hudPresenter.SetBuildingNames(definitions[0].DisplayName, definitions[1].DisplayName, definitions[2].DisplayName);
            }

            _hudPresenter.SetGold(_economyRepository.Economy.Gold);
            UpdateIncomeDisplay();
            _hudPresenter.UpdateSelection("No building selected", -1, false, false);
        }

        private void Update()
        {
            if (_gridView == null || _ghostView == null)
            {
                return;
            }

            HandleHotkeys();
            UpdatePointer();
        }

        private void HandleHotkeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectBuildingType("House");
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectBuildingType("Farm");
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SelectBuildingType("Mine");
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                _rotation = (_rotation + 90) % 360;
            }

            if (keyboard.deleteKey.wasPressedThisFrame)
            {
                RemoveSelectedBuilding();
            }
        }

        private void UpdatePointer()
        {
            var mouse = Mouse.current;
            var camera = Camera.main;
            if (mouse == null || camera == null)
            {
                return;
            }

            var ray = camera.ScreenPointToRay(mouse.position.ReadValue());
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                _gridView?.HideHighlight();
                _ghostView?.Hide();
                return;
            }

            var worldPosition = ray.GetPoint(distance);
            if (_gridView == null)
            {
                return;
            }

            if (!_gridView.TryGetGridPosition(worldPosition, out var gridPosition))
            {
                _gridView.HideHighlight();
                _ghostView?.Hide();
                return;
            }

            var canPlace = CanPlaceAt(gridPosition);
            if (_placingBuildingTypeId != null || _moveMode)
            {
                _gridView.ShowHighlight(gridPosition, canPlace);
                _ghostView?.Show(_gridView.GridToWorld(gridPosition), canPlace, _rotation);
            }
            else
            {
                _gridView.HideHighlight();
                _ghostView?.Hide();
            }

            var mouseLeft = mouse.leftButton;
            var mouseRight = mouse.rightButton;
            if (mouseLeft.wasPressedThisFrame)
            {
                if (_placingBuildingTypeId != null)
                {
                    TryPlaceBuilding(gridPosition, canPlace);
                }
                else if (_moveMode)
                {
                    TryMoveBuilding(gridPosition, canPlace);
                }
                else
                {
                    TrySelectBuilding(ray);
                }
            }
            else if (mouseRight.wasPressedThisFrame)
            {
                CancelModes();
            }
        }

        private bool CanPlaceAt(GridPosition position)
        {
            if (_cityRepository == null)
            {
                return false;
            }

            if (_moveMode && _selectedBuildingId.HasValue)
            {
                var building = _cityRepository.City.GetBuilding(_selectedBuildingId.Value);
                if (building.Position == position)
                {
                    return true;
                }
            }

            return !_cityRepository.City.IsCellOccupied(position);
        }

        private void TryPlaceBuilding(GridPosition position, bool canPlace)
        {
            if (!canPlace || _placeBuildingUseCase == null || _placingBuildingTypeId == null)
            {
                return;
            }

            if (_placeBuildingUseCase.Execute(_placingBuildingTypeId, position, _rotation, out _))
            {
                _placingBuildingTypeId = null;
                _ghostView?.Hide();
                _gridView?.HideHighlight();
                UpdateIncomeDisplay();
            }
        }

        private void TryMoveBuilding(GridPosition position, bool canPlace)
        {
            if (!canPlace || !_selectedBuildingId.HasValue || _moveBuildingUseCase == null)
            {
                return;
            }

            _moveBuildingUseCase.Execute(_selectedBuildingId.Value, position, _rotation);
            _moveMode = false;
            _ghostView?.Hide();
            _gridView?.HideHighlight();
        }

        private void TrySelectBuilding(Ray ray)
        {
            var hits = Physics.RaycastAll(ray);
            foreach (var hit in hits)
            {
                var view = hit.collider.GetComponent<BuildingView>();
                if (view != null)
                {
                    SelectBuilding(view.BuildingId);
                    return;
                }
            }

            DeselectBuilding();
        }

        private void SelectBuilding(Guid buildingId)
        {
            if (_selectedBuildingId.HasValue && _selectedBuildingId.Value == buildingId)
            {
                return;
            }

            if (_views.TryGetValue(_selectedBuildingId ?? Guid.Empty, out var previous))
            {
                previous.ResetColor();
            }

            if (_views.TryGetValue(buildingId, out var view))
            {
                _selectedBuildingId = buildingId;
                view.SetSelected(true);
                if (_hudPresenter != null && _catalog != null && _cityRepository != null)
                {
                    var building = _cityRepository.City.GetBuilding(buildingId);
                    var definition = _catalog.GetById(building.TypeId);
                    var canUpgrade = definition.TryGetNextLevel(building.Level, out _);
                    _hudPresenter.UpdateSelection(definition.DisplayName, building.Level, canUpgrade, true);
                }
            }
        }

        private void DeselectBuilding()
        {
            if (_selectedBuildingId.HasValue && _views.TryGetValue(_selectedBuildingId.Value, out var view))
            {
                view.ResetColor();
            }

            _selectedBuildingId = null;
            _moveMode = false;
            _ghostView?.Hide();
            _gridView?.HideHighlight();
            _hudPresenter?.UpdateSelection("No building selected", -1, false, false);
        }

        public void SelectBuildingType(string buildingTypeId)
        {
            if (_catalog == null)
            {
                return;
            }

            try
            {
                _ = _catalog.GetById(buildingTypeId);
            }
            catch (DomainException)
            {
                _hudPresenter?.ShowNotification("Unknown building type");
                return;
            }

            _placingBuildingTypeId = buildingTypeId;
            _moveMode = false;
            _rotation = 0;
            _hudPresenter?.ShowNotification($"Placing {buildingTypeId}");
        }

        public void UpgradeSelectedBuilding()
        {
            if (!_selectedBuildingId.HasValue || _upgradeBuildingUseCase == null)
            {
                return;
            }

            _upgradeBuildingUseCase.Execute(_selectedBuildingId.Value);
        }

        public void ToggleMoveMode()
        {
            if (!_selectedBuildingId.HasValue)
            {
                _hudPresenter?.ShowNotification("Select a building first");
                return;
            }

            _moveMode = !_moveMode;
            if (!_moveMode)
            {
                _ghostView?.Hide();
                _gridView?.HideHighlight();
            }
            else
            {
                _placingBuildingTypeId = null;
            }
        }

        public void RemoveSelectedBuilding()
        {
            if (!_selectedBuildingId.HasValue || _removeBuildingUseCase == null)
            {
                return;
            }

            _removeBuildingUseCase.Execute(_selectedBuildingId.Value);
            DeselectBuilding();
            UpdateIncomeDisplay();
        }

        public void SaveGame()
        {
            if (_saveGameUseCase == null)
            {
                return;
            }

            _saveGameUseCase.ExecuteAsync(CancellationToken.None).Forget();
        }

        public void LoadGame()
        {
            if (_loadGameUseCase == null)
            {
                return;
            }

            _loadGameUseCase.ExecuteAsync(CancellationToken.None).Forget();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            CreateOrUpdateView(evt.Building);
            UpdateIncomeDisplay();
        }

        private void OnBuildingRemoved(BuildingRemovedEvent evt)
        {
            if (_views.TryGetValue(evt.BuildingId, out var view))
            {
                Destroy(view.gameObject);
                _views.Remove(evt.BuildingId);
            }

            if (_selectedBuildingId == evt.BuildingId)
            {
                DeselectBuilding();
            }

            UpdateIncomeDisplay();
        }

        private void OnBuildingMoved(BuildingMovedEvent evt)
        {
            CreateOrUpdateView(evt.Building);
        }

        private void OnBuildingUpgraded(BuildingUpgradedEvent evt)
        {
            CreateOrUpdateView(evt.Building);
            if (_selectedBuildingId == evt.Building.Id && _hudPresenter != null && _catalog != null)
            {
                var definition = _catalog.GetById(evt.Building.TypeId);
                var canUpgrade = definition.TryGetNextLevel(evt.Building.Level, out _);
                _hudPresenter.UpdateSelection(definition.DisplayName, evt.Building.Level, canUpgrade, true);
            }

            UpdateIncomeDisplay();
        }

        private void CreateOrUpdateView(Building building)
        {
            if (_gridView == null)
            {
                return;
            }

            if (!_views.TryGetValue(building.Id, out var view))
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.transform.SetParent(transform, false);
                view = primitive.AddComponent<BuildingView>();
                _views.Add(building.Id, view);
            }

            view.transform.position = _gridView.GridToWorld(building.Position);
            view.transform.rotation = Quaternion.Euler(0f, building.Rotation, 0f);
            var color = GetColorForType(building.TypeId);
            view.Initialize(building.Id, building.TypeId, building.Level, color);
        }

        private Color GetColorForType(string buildingTypeId)
        {
            return buildingTypeId switch
            {
                "House" => new Color(0.4f, 0.8f, 1f),
                "Farm" => new Color(0.3f, 0.9f, 0.3f),
                "Mine" => new Color(0.7f, 0.6f, 0.4f),
                _ => Color.white
            };
        }

        private void RebuildAllViews()
        {
            foreach (var view in _views.Values)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _views.Clear();
            if (_cityRepository == null)
            {
                return;
            }

            foreach (var building in _cityRepository.City.Buildings)
            {
                CreateOrUpdateView(building);
            }

            UpdateIncomeDisplay();
        }

        private void UpdateIncomeDisplay()
        {
            if (_catalog == null || _cityRepository == null || _hudPresenter == null || _gameplaySettings == null)
            {
                return;
            }

            var totalIncome = 0;
            foreach (var building in _cityRepository.City.Buildings)
            {
                var definition = _catalog.GetById(building.TypeId);
                totalIncome += definition.GetLevel(building.Level).Income;
            }

            _hudPresenter.SetIncome(totalIncome, _gameplaySettings.IncomeTickSeconds);
        }

        private void CancelModes()
        {
            _placingBuildingTypeId = null;
            _moveMode = false;
            _ghostView?.Hide();
            _gridView?.HideHighlight();
        }
    }
}
