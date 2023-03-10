using System.Collections;
using System.Collections.Generic;
using HyperCasual.Gameplay;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A class representing a Spawnable object.
    /// If a GameObject tagged "Player" collides
    /// with this object, the "Player" GameObject
    /// will be destroyed.
    /// </summary>
    /*[ExecuteInEditMode]
    [RequireComponent(typeof(Collider))]*/
    public class Obstacle : Spawnable
    {
        [SerializeField]
        SoundID m_Sound = SoundID.None;

        [SerializeField]
        GameObject m_VFX;

        const string k_PlayerTag = "Player";

        public ObstacleHitEvent m_Event;

        Renderer[] m_Renderers;

        /// <summary>
        /// Reset the Obstacle to its initial state. Called
        /// when a level is restarted by the GameManager.
        /// </summary>
        public override void ResetSpawnable()
        {
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_Renderers[i].enabled = true;
            }

            ResetColliderSize();
        }

        protected override void Awake()
        {
            base.Awake();

            m_Renderers = gameObject.GetComponents<Renderer>();
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(k_PlayerTag))
            {
                Collide(col);
            }
        }

        void Collide(Collider col)
        {
            if (m_Event != null)
            {
                m_Event.Raise();
            }

            AudioManager.Instance.PlayEffect(m_Sound);
            PlayVFX(col.gameObject.transform.position, m_VFX);

            ChangeColliderSize();
            PlayerController.Instance.RemoveCharacter(col.gameObject);
            
        }

        protected virtual void ChangeColliderSize()
        {
            // override this in Explosive.cs to change collider size on impact.
        }

        protected virtual void ResetColliderSize()
        {
            // override this in Explosive.cs to reset size on reload.
        }

        virtual public void PlayVFX(Vector3 ImpactPosition, GameObject VFX)
        {
            if (m_VFX != null)
            {
                Destroy(Instantiate(VFX, new Vector3(ImpactPosition.x, ImpactPosition.y + .5f, ImpactPosition.z), Quaternion.identity), 2.5f);
            }
            else
                return;
        }
    }
}