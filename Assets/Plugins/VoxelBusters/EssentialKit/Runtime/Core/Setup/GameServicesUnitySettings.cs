using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public partial class GameServicesUnitySettings : SettingsPropertyGroup
    {
        #region Nested types

        [SerializeField, FormerlySerializedAs("m_leaderboardMetaArray")]
        [Tooltip ("Array contains information of the leaderboards used within the game.")]
        public      LeaderboardDefinition[]     m_leaderboards;

        [SerializeField]
        [Tooltip ("Array contains information of the achievements used within the game.")]
        public      AchievementDefinition[]     m_achievements;

        [SerializeField]
        [Tooltip ("If enabled, a banner is displayed when an achievement is completed (iOS).")]
        private     bool                        m_showAchievementCompletionBanner;

        [SerializeField]
        [Tooltip("Android specific settings.")]
        private     AndroidPlatformProperties   m_androidProperties;

        #endregion

        #region Properties

        public LeaderboardDefinition[] Leaderboards => m_leaderboards;

        public AchievementDefinition[] Achievements => m_achievements;

        public bool ShowAchievementCompletionBanner => m_showAchievementCompletionBanner;

        public AndroidPlatformProperties AndroidProperties => m_androidProperties;

        #endregion

        #region Constructors

        public GameServicesUnitySettings(bool isEnabled = true, bool initializeOnStart = true,
            LeaderboardDefinition[] leaderboards = null, AchievementDefinition[] achievements = null,
            bool showAchievementCompletionBanner = true, AndroidPlatformProperties androidProperties = null)
            : base(isEnabled: isEnabled, name: NativeFeatureType.kGameServices)
        {
            // set default values
            m_leaderboards                      = leaderboards ?? new LeaderboardDefinition[0];
            m_achievements                      = achievements ?? new AchievementDefinition[0];
            m_showAchievementCompletionBanner   = showAchievementCompletionBanner;
            m_androidProperties                 = androidProperties ?? new AndroidPlatformProperties();
        }

        #endregion
    }
}