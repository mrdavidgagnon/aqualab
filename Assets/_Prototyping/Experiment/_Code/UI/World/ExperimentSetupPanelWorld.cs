using UnityEngine;
using ProtoCP;
using System;
using BeauRoutine;

namespace ProtoAqua.Experiment
{
    public class ExperimentSetupPanelWorld : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private PointerListener m_Proxy = null;
        [SerializeField] private SpriteAnimator m_Animation = null;

        [SerializeField] private Vector3 m_ExperimentOffset = default(Vector3);

        #endregion // Inspector

        [NonSerialized] private Vector3 m_OriginalPosition;

        #region Unity Events

        private void Awake()
        {
            m_Proxy.onClick.AddListener((e) => Services.Events.Dispatch(ExperimentEvents.SetupPanelOn));

            Services.Events.Register(ExperimentEvents.SetupPanelOn, OnPanelOn, this)
                .Register(ExperimentEvents.SetupPanelOff, OnPanelOff, this)
                .Register(ExperimentEvents.ExperimentBegin, OnExperimentBegin, this)
                .Register(ExperimentEvents.ExperimentTeardown, OnExperimentTeardown, this);

            m_OriginalPosition = transform.localPosition;
        }

        private void OnDestroy()
        {
            Services.Events?.DeregisterAll(this);
        }

        #endregion // Unity Events

        private void OnPanelOn()
        {
            m_Animation.Pause();
        }

        private void OnPanelOff()
        {
            m_Animation.Restart();
        }

        private void OnExperimentBegin()
        {
            Routine.Start(this, transform.MoveTo(m_OriginalPosition + m_ExperimentOffset, 0.5f, Axis.XYZ, Space.Self).Ease(Curve.CubeOut));
        }

        private void OnExperimentTeardown()
        {
            transform.localPosition = m_OriginalPosition;
        }
    }
}