using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using ProtoAudio;
using BeauRoutine;
using System.Collections;
using BeauPools;
using BeauUtil.Variants;
using BeauRoutine.Extensions;

namespace ProtoAqua.Experiment
{
    public class KelpActor : ActorModule
    {
        #region Inspector

        [Header("Height")]
        [SerializeField] private FloatRange m_Height = new FloatRange(6);
        [SerializeField] private Transform m_HeightOffset = null;
        [SerializeField] private SpriteRenderer m_SpineRenderer = null;
        [SerializeField] private Transform m_HeightCapOffset = null;
        
        #endregion // Inspector

        [NonSerialized] private KelpLeaf[] m_AllLeaves = null;

        private void OnCreate()
        {
            float height = m_Height.Generate(RNG.Instance);
            m_SpineRenderer.flipX = RNG.Instance.NextBool();
            m_SpineRenderer.flipY = RNG.Instance.NextBool();
            
            Vector2 size = m_SpineRenderer.size;
            size.y = height;
            m_SpineRenderer.size = size;

            m_HeightOffset.SetPosition(height * 0.5f, Axis.Y, Space.Self);
            m_HeightCapOffset.SetPosition(height * 0.5f + 1, Axis.Y, Space.Self);

            float facing = RNG.Instance.Choose(-1, 1);
            for(int i = 0, leafCount = m_AllLeaves.Length; i < leafCount; ++i)
            {
                float lerp = (i + 0.5f + RNG.Instance.NextFloat(-0.1f, 0.1f)) / leafCount;
                float leafHeight = height * lerp;

                m_AllLeaves[i].Initialize(leafHeight, facing, Actor.Config);
                facing = -facing;
            }
        }

        public override void OnConstruct()
        {
            base.OnConstruct();

            Actor.Callbacks.OnCreate = OnCreate;

            m_AllLeaves = GetComponentsInChildren<KelpLeaf>(true);
        }
    }
}