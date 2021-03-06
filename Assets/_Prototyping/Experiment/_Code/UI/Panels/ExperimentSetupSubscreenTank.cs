using System;
using UnityEngine;
using BeauUtil;
using UnityEngine.UI;
using TMPro;

namespace ProtoAqua.Experiment
{
    public class ExperimentSetupSubscreenTank : ExperimentSetupSubscreen
    {
        #region Inspector

        [SerializeField] private ToggleGroup m_ToggleGroup = null;
        [SerializeField] private Button m_NextButton = null;
        [SerializeField] private TMP_Text m_Label = null;

        #endregion // Inspector

        [NonSerialized] private ExperimentSettings m_CachedSettings;
        [NonSerialized] private SetupToggleButton[] m_CachedButtons;

        [NonSerialized] private ExperimentSetupData m_CachedData;

        [NonSerialized] public Action OnSelectContinue;

        protected override void Awake()
        {
            m_CachedSettings = Services.Tweaks.Get<ExperimentSettings>();
            m_CachedButtons = m_ToggleGroup.GetComponentsInChildren<SetupToggleButton>();
            for(int i = 0; i < m_CachedButtons.Length; ++i)
            {
                m_CachedButtons[i].Toggle.group = m_ToggleGroup;
                m_CachedButtons[i].Toggle.onValueChanged.AddListener((b) => UpdateFromSelection());
            }

            m_NextButton.onClick.AddListener(() => OnSelectContinue?.Invoke());

            UpdateButtons();
        }

        public override void SetData(ExperimentSetupData inData)
        {
            base.SetData(inData);
            m_CachedData = inData;
        }

        public override void Refresh()
        {
            base.Refresh();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var allTankTypes = m_CachedSettings.AllNonEmptyTanks();
            var noneTankType = m_CachedSettings.GetTank(TankType.None);

            int buttonIdx = 0;
            foreach(var tankType in allTankTypes)
            {
                if (buttonIdx >= m_CachedButtons.Length)
                    break;

                var button = m_CachedButtons[buttonIdx];
                
                if (Services.Data.CheckConditions(tankType.Condition))
                {
                    button.Load((int) tankType.Tank, tankType.Icon, true);
                }
                else
                {
                    button.Load((int) noneTankType.Tank, noneTankType.Icon, false);
                }

                ++buttonIdx;
            }

            for(; buttonIdx < m_CachedButtons.Length; ++buttonIdx)
            {
                m_CachedButtons[buttonIdx].Load((int) noneTankType.Tank, noneTankType.Icon, false);
            }
        }

        private void UpdateDisplay(TankType inTankType)
        {
            var def = m_CachedSettings.GetTank(inTankType);
            m_Label.text = Services.Loc.Localize(def.LabelId);
            m_NextButton.interactable = inTankType != TankType.None;

            Services.Data.SetVariable(ExperimentVars.SetupPanelTankType, inTankType.ToString());
        }
    
        private void UpdateFromSelection()
        {
            Toggle active = m_ToggleGroup.ActiveToggle();
            if (active != null)
            {
                m_CachedData.Tank = (TankType) active.GetComponent<SetupToggleButton>().Id.AsInt();
            }
            else
            {
                m_CachedData.Tank = TankType.None;
            }

            UpdateDisplay(m_CachedData.Tank);
        }

        protected override void OnShowComplete(bool inbInstant)
        {
            base.OnShowComplete(inbInstant);
            UpdateDisplay(m_CachedData.Tank);
        }
    }
}