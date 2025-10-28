using System.Collections;
using Serilog;
using UnityEngine;
using UnityEngine.UIElements;

namespace CityBuilder.Presentation.Presenters
{
    public sealed class HudPresenter : MonoBehaviour
    {
        private UIDocument _document;
        private PanelSettings _panelSettings;
        private Label _goldLabel;
        private Label _incomeLabel;
        private Label _selectionTitle;
        private Label _selectionLevel;
        private Label _notificationLabel;
        private Button _houseButton;
        private Button _farmButton;
        private Button _mineButton;
        private Button _upgradeButton;
        private Button _moveButton;
        private Button _deleteButton;
        private Button _saveButton;
        private Button _loadButton;
        private CityInteractionPresenter _presenter;
        private Coroutine _notificationRoutine;

        private void Awake()
        {
            _document = FindAnyObjectByType<UIDocument>();
            _document.sortingOrder = 100;
            var tree = Resources.Load<VisualTreeAsset>("UI/Hud");
            if (tree == null)
            {
                Log.Error("[HudPresenter.Awake] HUD visual tree not found in Resources/UI.");
                return;
            }

            _document.visualTreeAsset = tree;
            var root = _document.rootVisualElement;
            _goldLabel = root.Q<Label>("GoldLabel");
            _incomeLabel = root.Q<Label>("IncomeLabel");
            _selectionTitle = root.Q<Label>("SelectionTitle");
            _selectionLevel = root.Q<Label>("SelectionLevel");
            _notificationLabel = root.Q<Label>("NotificationLabel");
            _houseButton = root.Q<Button>("HouseButton");
            _farmButton = root.Q<Button>("FarmButton");
            _mineButton = root.Q<Button>("MineButton");
            _upgradeButton = root.Q<Button>("UpgradeButton");
            _moveButton = root.Q<Button>("MoveButton");
            _deleteButton = root.Q<Button>("DeleteButton");
            _saveButton = root.Q<Button>("SaveButton");
            _loadButton = root.Q<Button>("LoadButton");
        }

        /// <summary>
        /// Binds UI controls to presenter commands.
        /// </summary>
        /// <param name="presenter">Interaction presenter responsible for gameplay actions.</param>
        public void Initialize(CityInteractionPresenter presenter)
        {
            _presenter = presenter;
            if (_houseButton != null)
            {
                _houseButton.clicked += () => _presenter.SelectBuildingType("House");
            }

            if (_farmButton != null)
            {
                _farmButton.clicked += () => _presenter.SelectBuildingType("Farm");
            }

            if (_mineButton != null)
            {
                _mineButton.clicked += () => _presenter.SelectBuildingType("Mine");
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.clicked += _presenter.UpgradeSelectedBuilding;
            }

            if (_moveButton != null)
            {
                _moveButton.clicked += _presenter.ToggleMoveMode;
            }

            if (_deleteButton != null)
            {
                _deleteButton.clicked += _presenter.RemoveSelectedBuilding;
            }

            if (_saveButton != null)
            {
                _saveButton.clicked += _presenter.SaveGame;
            }

            if (_loadButton != null)
            {
                _loadButton.clicked += _presenter.LoadGame;
            }
        }

        /// <summary>
        /// Updates button labels for the available building catalog entries.
        /// </summary>
        /// <param name="house">Display name for the house building.</param>
        /// <param name="farm">Display name for the farm building.</param>
        /// <param name="mine">Display name for the mine building.</param>
        public void SetBuildingNames(string house, string farm, string mine)
        {
            if (_houseButton != null)
            {
                _houseButton.text = $"{house}";
            }

            if (_farmButton != null)
            {
                _farmButton.text = $"{farm}";
            }

            if (_mineButton != null)
            {
                _mineButton.text = $"{mine}";
            }
        }

        /// <summary>
        /// Displays the current gold balance.
        /// </summary>
        /// <param name="gold">The player's gold amount.</param>
        public void SetGold(int gold)
        {
            if (_goldLabel != null)
            {
                _goldLabel.text = $"Gold: {gold}";
            }
        }

        /// <summary>
        /// Displays the passive income rate.
        /// </summary>
        /// <param name="income">Income amount generated per tick.</param>
        /// <param name="tickSeconds">Tick duration in seconds.</param>
        public void SetIncome(int income, float tickSeconds)
        {
            if (_incomeLabel != null)
            {
                _incomeLabel.text = tickSeconds <= 0f
                    ? $"Income: +{income}"
                    : $"Income: +{income}/{tickSeconds:0}s";
            }
        }

        /// <summary>
        /// Updates the selection details of the currently highlighted building.
        /// </summary>
        /// <param name="title">Building display name.</param>
        /// <param name="level">Building level.</param>
        /// <param name="canUpgrade">Indicates if upgrade is permitted.</param>
        /// <param name="canMove">Indicates if move/delete actions are permitted.</param>
        public void UpdateSelection(string title, int level, bool canUpgrade, bool canMove)
        {
            if (_selectionTitle != null)
            {
                _selectionTitle.text = title;
            }

            if (_selectionLevel != null)
            {
                _selectionLevel.text = level > 0 ? $"Level: {level}" : "Level: -";
            }

            _upgradeButton?.SetEnabled(canUpgrade);
            _moveButton?.SetEnabled(canMove);
            _deleteButton?.SetEnabled(canMove);
        }

        /// <summary>
        /// Shows a temporary notification message to the player.
        /// </summary>
        /// <param name="message">Notification text.</param>
        public void ShowNotification(string message)
        {
            if (_notificationLabel == null)
            {
                return;
            }

            _notificationLabel.text = message;
            if (_notificationRoutine != null)
            {
                StopCoroutine(_notificationRoutine);
            }

            _notificationRoutine = StartCoroutine(ClearNotification());
        }

        private IEnumerator ClearNotification()
        {
            yield return new WaitForSeconds(2f);
            if (_notificationLabel != null)
            {
                _notificationLabel.text = string.Empty;
            }
        }
    }
}
