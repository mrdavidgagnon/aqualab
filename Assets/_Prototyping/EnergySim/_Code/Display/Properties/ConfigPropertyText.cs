using System;
using System.Collections;
using System.Runtime.InteropServices;
using Aqua;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProtoAqua.Energy
{
    public class ConfigPropertyText : MonoBehaviour, IPooledObject<ConfigPropertyText>
    {
        #region Types

        public struct Configuration
        {
            public string Name;

            public int MaxLength;

            public Func<string> Get;
            public Action<string> Set;
        }

        #endregion // Types

        #region Inspector
        
        [SerializeField] private IndentGroup m_Indent = null;

        [Header("Controls")]
        [SerializeField] private TMP_Text m_Label = null;
        [SerializeField] private TMP_InputField m_Input = null;

        #endregion // Inspector

        [NonSerialized] private string m_CurrentValue = string.Empty;
        [NonSerialized] private Configuration m_Configuration;

        public IndentGroup IndentGroup { get { return m_Indent; } }

        #region Unity Events

        private void Awake()
        {
            m_Input.onEndEdit.AddListener(OnTextEndEdit);
        }

        #endregion // Unity Events

        #region Configuration

        public void Configure(in Configuration inConfiguration)
        {
            m_Configuration = inConfiguration;

            m_Label.SetText(m_Configuration.Name);

            m_Input.characterLimit = m_Configuration.MaxLength;

            m_CurrentValue = m_Configuration.Get();
            UpdateText();
        }

        #endregion // Configuration

        public string Value
        {
            get { return m_CurrentValue; }
            set
            {
                TrySetValue(m_CurrentValue);
            }
        }

        public void SyncValue()
        {
            string val = m_Configuration.Get();
            m_CurrentValue = val;
            UpdateText();
        }

        #region Updates

        private void TrySetValue(string inNewValue)
        {
            string newVal = inNewValue;
            if (CheckValueChange(ref newVal))
            {
                m_CurrentValue = newVal;
                UpdateText();
                NotifyChanged();
            }
        }

        private bool CheckValueChange(ref string ioNewValue)
        {
            string oldValue = m_CurrentValue;
            if (m_Configuration.MaxLength > 0 && ioNewValue.Length > m_Configuration.MaxLength)
            {
                ioNewValue = ioNewValue.Substring(m_Configuration.MaxLength);
            }
            
            return ioNewValue != oldValue;
        }

        private void UpdateText()
        {
            m_Input.SetTextWithoutNotify(m_CurrentValue);
        }

        private void NotifyChanged()
        {
            if (m_Configuration.Set != null)
            {
                m_Configuration.Set(m_CurrentValue);
            }
        }

        #endregion // Updates

        #region Listeners

        private void OnTextEndEdit(string inText)
        {
            TrySetValue(inText);
        }

        #endregion // Listeners

        #region IPooledObject

        void IPooledObject<ConfigPropertyText>.OnAlloc()
        {
            // throw new NotImplementedException();
        }

        void IPooledObject<ConfigPropertyText>.OnConstruct(IPool<ConfigPropertyText> inPool)
        {
            // throw new NotImplementedException();
        }

        void IPooledObject<ConfigPropertyText>.OnDestruct()
        {
            // throw new NotImplementedException();
        }

        void IPooledObject<ConfigPropertyText>.OnFree()
        {
            m_Label.SetText(string.Empty);
            m_Indent.SetIndent(0);
            m_Configuration = default(Configuration);
        }

        #endregion // IPooledObject
    }
}