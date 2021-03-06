using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using BeauRoutine.Extensions;
using TMPro;
using System.Collections;
using System;
using BeauUtil.Tags;
using ProtoAqua.Scripting;
using BeauUtil;

namespace ProtoAqua
{
    public class DialogPanel : BasePanel
    {
        #region Types

        private enum LineEndBehavior
        {
            WaitForInput,
            WaitFixedDuration
        }

        private struct TypingState
        {
            public string SpeakerName;
            public float Speed;
            public string VisibleText;
            public string TypeSFX;

            public bool AutoContinue;
            public float SkipHoldTimer;
            public bool SkipHeld;

            public void ResetFull()
            {
                SpeakerName = string.Empty;
                Speed = 1;
                VisibleText = null;
                TypeSFX = null;

                ResetTemp();
            }

            public void ResetTemp()
            {
                SkipHoldTimer = 0;
                SkipHeld = false;
                AutoContinue = false;
            }
        }

        #endregion // Types

        #region Inspector

        [SerializeField] private string m_StyleId = null;
        
        [Header("Behavior")]

        [SerializeField] private string m_DefaultTypeSFX = "text_type";
        [SerializeField] private float m_SpeedUpThreshold = 0.25f;
        [SerializeField] private LineEndBehavior m_EndBehavior = LineEndBehavior.WaitForInput;
        [SerializeField] private float m_NonAutoWaitTimer = 2;

        [Header("Speaker")]
        
        [SerializeField] private RectTransform m_SpeakerContainer = null;
        [SerializeField] private TMP_Text m_SpeakerLabel = null;
        [SerializeField] private Graphic m_SpeakerLabelBG = null;

        [Header("Text")]

        [SerializeField] private LayoutGroup m_TextLayout = null;
        [SerializeField] private CanvasGroup m_TextContainer = null;
        [SerializeField] private TMP_Text m_TextDisplay = null;

        [Header("Button")]

        [SerializeField] private RectTransform m_ButtonContainer = null;
        [SerializeField] private Button m_Button = null;
        [SerializeField] private CanvasGroup m_ButtonGroup = null;

        #endregion // Inspector

        [NonSerialized] private TypingState m_CurrentState;
        [NonSerialized] private Routine m_BoxAnim;
        [NonSerialized] private Routine m_FadeAnim;
        [NonSerialized] private TagStringEventHandler m_EventHandler;

        [NonSerialized] private BaseInputLayer m_Input;
        [NonSerialized] private Routine m_RebuildRoutine;

        public StringHash32 StyleId() { return m_StyleId; }

        #region BasePanel

        protected override void Start()
        {
            base.Start();

            m_Input = BaseInputLayer.Find(this);
        }

        protected override void OnHideComplete(bool inbInstant)
        {
            m_CurrentState.ResetFull();

            if (m_SpeakerLabel)
                m_SpeakerLabel.SetText(string.Empty);
            
            m_TextDisplay.SetText(string.Empty);
            if (m_SpeakerContainer)
                m_SpeakerContainer.gameObject.SetActive(false);
            
            if (m_ButtonContainer)
                m_ButtonContainer.gameObject.SetActive(false);
        }

        #endregion // BasePanel

        #region Event Handlers

        private TagStringEventHandler GetHandler()
        {
            if (m_EventHandler == null)
            {
                m_EventHandler = new TagStringEventHandler();

                m_EventHandler.Register(ScriptEvents.Dialog.Auto, () => m_CurrentState.AutoContinue = true);
                m_EventHandler.Register(ScriptEvents.Dialog.Clear, () => m_TextDisplay.SetText(string.Empty));
                m_EventHandler.Register(ScriptEvents.Dialog.InputContinue, () => WaitForInput());
                m_EventHandler.Register(ScriptEvents.Dialog.SetTypeSFX, (e, o) => m_CurrentState.TypeSFX = e.StringArgument);
                m_EventHandler.Register(ScriptEvents.Dialog.Speaker, (e, o) => SetSpeaker(e.StringArgument));
                m_EventHandler.Register(ScriptEvents.Dialog.Speed, (e, o) => {
                    m_CurrentState.Speed = e.IsClosing ? 1 : e.Argument0.AsFloat();
                    return Routine.WaitSeconds(0.15f);
                });
                m_EventHandler.Register(ScriptEvents.Dialog.Target, (e, o) => SetTarget(e.StringArgument));
                m_EventHandler.Register(ScriptEvents.Global.Wait, (e, o) => Routine.WaitSeconds(e.Argument0.AsFloat() * GetSkipMultiplier()));
            }

            return m_EventHandler;
        }

        
        private void SetTarget(string inTarget)
        {
            if (string.IsNullOrEmpty(inTarget))
            {
                m_CurrentState.TypeSFX = null;
                SetSpeaker(null);
                return;
            }

            m_CurrentState.TypeSFX = "text_type_" + inTarget;
            if (!Services.Audio.HasEvent(m_CurrentState.TypeSFX))
            {
                Debug.LogErrorFormat("[DialogPanel] No type sfx located for '{0}'", m_CurrentState.TypeSFX);
                m_CurrentState.TypeSFX = null;
            }

            if (inTarget == "kevin")
            {
                SetSpeaker("Kevin, Your Science Familiar");
            }
            else if (inTarget == "player")
            {
                SetSpeaker(Services.Data.CurrentCharacterName());
            }
            else if (inTarget == "mechanic")
            {
                SetSpeaker("Jan, The Mechanic");
            }
            else if (inTarget == "radio")
            {
                SetSpeaker("Radio");
            }
            else
            {
                SetSpeaker("???");
            }
        }

        private bool SetSpeaker(string inSpeaker)
        {
            if (m_CurrentState.SpeakerName == inSpeaker)
                return false;

            m_CurrentState.SpeakerName = inSpeaker;

            if (m_SpeakerContainer)
            {
                if (string.IsNullOrEmpty(inSpeaker))
                {
                    m_SpeakerContainer.gameObject.SetActive(false);
                    m_SpeakerLabel.SetText(string.Empty);
                }
                else
                {
                    m_SpeakerLabel.SetText(inSpeaker);
                    m_SpeakerContainer.gameObject.SetActive(true);
                }
            }

            return true;
        }

        #endregion // Event Handlers

        #region Scripting

        /// <summary>
        /// Preps for a line of dialog.
        /// </summary>
        public TagStringEventHandler PrepLine(TagString inLine, TagStringEventHandler inParentHandler)
        {
            m_CurrentState.ResetTemp();

            if (inLine.RichText.Length > 0)
            {
                m_TextDisplay.SetText(inLine.RichText);
                m_CurrentState.VisibleText = inLine.VisibleText;
                m_TextDisplay.maxVisibleCharacters = 0;

                UpdateSkipHeld();

                RebuildLayout();

                TagStringEventHandler handler = GetHandler();
                handler.Base = inParentHandler;
                return handler;
            }

            return inParentHandler;
        }

        /// <summary>
        /// Updates the dialog mid-line.
        /// </summary>
        public void UpdateInput()
        {
            UpdateSkipHeld();
        }

        /// <summary>
        /// Finishes the current line.
        /// </summary>
        public IEnumerator CompleteLine()
        {
            if (IsShowing() && !string.IsNullOrEmpty(m_CurrentState.VisibleText))
            {
                switch(m_EndBehavior)
                {
                    case LineEndBehavior.WaitForInput:
                        {
                            if (!m_CurrentState.AutoContinue)
                                return WaitForInput();
                            break;
                        }

                    case LineEndBehavior.WaitFixedDuration:
                        {
                            if (!m_CurrentState.AutoContinue)
                                return Routine.WaitSeconds(m_NonAutoWaitTimer);
                            break;
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// Completes the current sequence.
        /// </summary>
        public void CompleteSequence()
        {
            if (IsShowing())
            {
                Hide();
            }
        }

        #endregion // Scripting

        #region Typing

        /// <summary>
        /// Types out a certain number of characters.
        /// </summary>
        public IEnumerator TypeLine(TagTextData inText)
        {
            if (m_TextDisplay.maxVisibleCharacters == 0)
            {
                if (!IsShowing())
                    Show();
                else
                    m_BoxAnim.Replace(this, Pulse());

                RebuildLayout();
            }

            float timeThisFrame = Routine.DeltaTime;
            float delay = -timeThisFrame;
            bool bPlayType = false;

            int charsToShow = (int) inText.VisibleCharacterCount;
            while(charsToShow > 0)
            {
                while(delay > 0)
                {
                    if (bPlayType)
                    {
                        bPlayType = false;
                        PlayTypingSound();
                    }
                    yield return null;
                    timeThisFrame = Routine.DeltaTime;
                    delay -= timeThisFrame;
                    UpdateInput();
                }

                m_TextDisplay.maxVisibleCharacters++;

                char shownChar = m_CurrentState.VisibleText[m_TextDisplay.maxVisibleCharacters - 1];
                switch(shownChar)
                {
                    case ' ':
                        delay += GetDelay(0.05f);
                        break;

                    case '.':
                    case '!':
                    case '?':
                        delay += GetDelay(0.2f);
                        break;
                    
                    case ',':
                        delay += GetDelay(0.08f);
                        break;

                    default:
                        if (char.IsLetterOrDigit(shownChar))
                        {
                            bPlayType = true;
                        }
                        delay += GetDelay(0.03f);
                        break;
                }

                --charsToShow;
            }

            if (bPlayType)
            {
                PlayTypingSound();
            }
        }

        private void UpdateSkipHeld()
        {
            if (m_EndBehavior != LineEndBehavior.WaitForInput)
                return;
            
            if (m_Input.Device.MouseDown(0))
            {
                m_CurrentState.SkipHoldTimer += Routine.DeltaTime;
                m_CurrentState.SkipHeld = m_CurrentState.SkipHoldTimer >= m_SpeedUpThreshold;
            }
            else
            {
                m_CurrentState.SkipHoldTimer = 0;
                m_CurrentState.SkipHeld = false;
            }
        }

        #endregion // Typing

        private float GetSkipMultiplier()
        {
            return m_CurrentState.SkipHeld ? 0.25f : 1;
        }

        private void PlayTypingSound()
        {
            string typeSfx = string.IsNullOrEmpty(m_CurrentState.TypeSFX) ? m_DefaultTypeSFX : m_CurrentState.TypeSFX;
            Services.Audio.PostEvent(typeSfx);
        }

        private float GetDelay(float inBase)
        {
            if (m_CurrentState.Speed == 0)
                return 0;

            return inBase / m_CurrentState.Speed * GetSkipMultiplier();
        }

        #region Coroutines

        private IEnumerator Pulse()
        {
            yield return Routine.Inline(m_RootTransform.ScaleTo(1.03f, 0.04f).RevertOnCancel().Yoyo());
        }

        private IEnumerator WaitForInput()
        {
            m_ButtonContainer.gameObject.SetActive(true);
            yield return Routine.Race(
                m_Button == null ? null : m_Button.onClick.WaitForInvoke(),
                Routine.WaitCondition(() => m_Input.Device.MousePressed(0) || m_Input.Device.KeyPressed(KeyCode.Space))
            );
            m_ButtonContainer.gameObject.SetActive(false);
        }

        #endregion // Coroutines

        #region BasePanel

        protected override IEnumerator TransitionToShow()
        {
            if (!m_RootTransform.gameObject.activeSelf)
            {
                m_RootGroup.alpha = 0;
                m_RootTransform.SetScale(0.5f);
                m_RootTransform.gameObject.SetActive(true);
            }

            yield return Routine.Combine(
                m_RootGroup.FadeTo(1, 0.2f),
                m_RootTransform.ScaleTo(1f, 0.2f).Ease(Curve.BackOut)
            );
        }

        protected override IEnumerator TransitionToHide()
        {
            yield return Routine.Combine(
                m_RootGroup.FadeTo(0, 0.15f),
                m_RootTransform.ScaleTo(0.5f, 0.15f).Ease(Curve.CubeIn)
            );

            m_RootTransform.gameObject.SetActive(false);
        }

        protected override void OnShow(bool inbInstant)
        {
            base.OnShow(inbInstant);

            RebuildLayout();
        }

        private void RebuildLayout()
        {
            m_RebuildRoutine.Replace(this, RebuildDelayed());
        }

        private IEnumerator RebuildDelayed()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) m_TextLayout.transform);
        }
    
        #endregion // BasePanel
    }
}