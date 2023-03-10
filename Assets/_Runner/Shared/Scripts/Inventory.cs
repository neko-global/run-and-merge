using System;
using System.Collections;
using System.Collections.Generic;
using HyperCasual.Core;
using HyperCasual.Gameplay;
using UnityEngine;

namespace HyperCasual.Runner
{
    /// <summary>
    /// A simple inventory class that listens to game events and keeps track of the amount of in-game currencies
    /// collected by the player
    /// </summary>
    public class Inventory : AbstractSingleton<Inventory>
    {
        [SerializeField]
        GenericGameEventListener m_GoldEventListener;
        [SerializeField]
        GenericGameEventListener m_KeyEventListener;
        [SerializeField]
        GenericGameEventListener m_WinEventListener;
        [SerializeField]
        GenericGameEventListener m_LoseEventListener;

        int m_TempGold;
        int m_TotalGold;
        int m_TempKeys;
        int m_TotalKeys;
        float m_TempProgression;
        float m_TotalProgression;

        /// <summary>
        /// Temporary const
        /// Users keep accumulating XP when playing the game and they're rewarded as they hit a milestone.
        /// Milestones are simply a threshold to reward users for playing the game. We need to come up with
        /// a proper formula to calculate milestone values but because we don't have a plan for the milestone
        /// rewards yet, we have simple set the value to something users can never reach. 
        /// </summary>
        const float k_MilestoneFactor = 1.2f;

        Hud m_Hud;
        LevelCompleteScreen m_LevelCompleteScreen;

        public int TempGold
        {
            get => m_TempGold;
        }

        void Start()
        {
            m_GoldEventListener.EventHandler = OnGoldPicked;
            m_KeyEventListener.EventHandler = OnKeyPicked;
            m_WinEventListener.EventHandler = OnWin;
            m_LoseEventListener.EventHandler = OnLose;

            m_TempGold = 0;
            m_TotalGold = SaveManager.Instance.Currency;
            m_TempKeys = 0;
            m_TotalKeys = SaveManager.Instance.Keys;
            m_TempProgression = 0;
            m_TotalProgression = SaveManager.Instance.XP;

            m_LevelCompleteScreen = UIManager.Instance.GetView<LevelCompleteScreen>();
            m_Hud = UIManager.Instance.GetView<Hud>();
        } 

        void OnEnable()
        {
            m_GoldEventListener.Subscribe();
            m_KeyEventListener.Subscribe();
            m_WinEventListener.Subscribe();
            m_LoseEventListener.Subscribe();
        }

        void OnDisable()
        {
            m_GoldEventListener.Unsubscribe();
            m_KeyEventListener.Unsubscribe();
            m_WinEventListener.Unsubscribe();
            m_LoseEventListener.Unsubscribe();
        }

        void OnGoldPicked()
        {
            if (m_GoldEventListener.m_Event is ItemPickedEvent goldPickedEvent)
            {
                m_TempGold += goldPickedEvent.Count;

                PlayerPrefs.SetInt("Currency", PlayerPrefs.GetInt("Currency") + goldPickedEvent.Count);

                m_Hud.GoldValue = PlayerPrefs.GetInt("Currency");
            }
            else
            {
                throw new Exception($"Invalid event type!");
            }
        }

        void OnKeyPicked()
        {
            if (m_KeyEventListener.m_Event is ItemPickedEvent keyPickedEvent)
            {
                m_TempKeys += keyPickedEvent.Count;
                //m_Hud.KeysValue = m_TempKeys;
            }
            else
            {
                throw new Exception($"Invalid event type!");
            }
        }

        void OnWin()
        {
            // Gold.
            /*m_TotalGold += m_TempGold;
            m_TempGold = 0;
            SaveManager.Instance.Currency = m_TotalGold;*/

            // Keys.
            m_TotalKeys += m_TempKeys;
            m_TempKeys = 0;
            SaveManager.Instance.Keys = m_TotalKeys;

            // EXP.
            m_TotalProgression += m_TempProgression;
            m_TempProgression = 0f;
            SaveManager.Instance.XP = m_TotalProgression;

            // Remove levelcompletescreen
            /*m_LevelCompleteScreen.GoldValue = m_TotalGold;
            m_LevelCompleteScreen.XpSlider.minValue = m_TotalProgression;
            m_LevelCompleteScreen.XpSlider.maxValue = k_MilestoneFactor * (m_TotalProgression + m_TempProgression);
            m_LevelCompleteScreen.XpValue = m_TotalProgression + m_TempProgression;

            m_LevelCompleteScreen.StarCount = m_TempKeys;*/
        }

        void OnLose()
        {
            // Gold aquired during run is saved
            /*m_TotalGold += m_TempGold;
            m_TempGold = 0;
            SaveManager.Instance.Currency = m_TotalGold;*/

            // Key aquired during run is reset on lost
            m_TempKeys = 0;

            // EXP aquired during run is saved
            m_TotalProgression += m_TempProgression;
            m_TempProgression = 0f;
            SaveManager.Instance.XP = m_TotalProgression;
        }

        void Update()
        {
            if (m_Hud.gameObject.activeSelf)
            {
                m_TempProgression += PlayerController.Instance.Speed * Time.deltaTime;
                m_Hud.Progress = m_TempProgression;
                m_Hud.SetNumberOfChar(PlayerController.Instance.Characters.Count);

                if (SequenceManager.Instance.m_CurrentLevel is LoadLevelFromDef loadLevelFromDef)
                {
                    m_Hud.Progression.MinValue = 0;
                    m_Hud.Progression.MaxValue = loadLevelFromDef.m_LevelDefinition.LevelLength;
                }
            }
        }
    }
}
