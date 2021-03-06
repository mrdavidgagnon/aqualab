using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using ProtoAudio;
using BeauRoutine;
using System.Collections;
using BeauPools;
using System.Collections.Generic;

namespace ProtoAqua.Experiment
{
    public class ActorNavHelper : MonoBehaviour
    {
        [SerializeField] private float m_SpawnHeight = 8;
        [SerializeField] private BoxCollider2D m_Region = null;
        [NonSerialized] private Transform m_Transform;

        private void Awake()
        {
            this.CacheComponent(ref m_Transform);
        }

        public Vector2 GetSpawnOffset()
        {
            return new Vector2(0, m_SpawnHeight);
        }

        public Vector2 GetFloorSpawnTarget(float inSideOffset, float inFloorOffset)
        {
            Rect r = Rect();
            Vector2 left = new Vector2(r.xMin + inSideOffset, r.yMin + inFloorOffset);
            Vector2 right = new Vector2(r.xMax - inSideOffset, left.y);
            return RNG.Instance.NextVector2(left, right);
        }

        public Vector2 GetRandomSwimTarget(float inSideOffset, float inFloorOffset, float inCeilingOffset)
        {
            Rect r = Rect();
            Vector2 left = new Vector2(r.xMin + inSideOffset, r.yMin + inFloorOffset);
            Vector2 right = new Vector2(r.xMax - inSideOffset, r.yMax - inCeilingOffset);
            return RNG.Instance.NextVector2(left, right);
        }

        private Rect Rect()
        {
            Rect rect = new Rect();
            rect.size = m_Region.size;
            rect.center = (Vector2) m_Transform.position + m_Region.offset;
            return rect;
        }
    }
}