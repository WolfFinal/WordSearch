using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    public partial class GameServicesUnitySettings
    {
        [Serializable]
        public class AndroidPlatformProperties 
        {
            #region Fields

            [SerializeField]
            [Tooltip ("Your application id in Google Play services.")]
            private     string      m_playServicesApplicationId;

            [Header("External Server Control")]
            [SerializeField]
            [Tooltip("Your Server Client Id for getting external authentication credentials (Make sure its from a web app)")]
            private     string      m_serverClientId;

            [Header("Additional Scopes")]
            [SerializeField]
            [Tooltip("If enabled, requests profile scope usefule for external server authentication")]
            private     bool        m_needsProfileScope;

            [SerializeField]
            [Tooltip("If enabled, requests email scope for getting player's email")]
            private     bool        m_needsEmailScope;

            [Tooltip("Text formats used to derive completed achievement description. Note: Achievement title will be inserted in place of token \'#\'.")]
            private     string[]    m_achievedDescriptionFormats;

            [Header("Extra Settings")]
            [SerializeField]
            [Tooltip("If enabled, alert dialog is shown automatically on error(for ex: signin failure).")]
            private     bool        m_showErrorDialogs;

            [SerializeField]
            [Tooltip("If enabled, popups from google play services will be displayed at top center. Else, bottom center.")]
            private     bool        m_displayPopupsAtTop;

            #endregion

            #region Properties

            public string PlayServicesApplicationId => PropertyHelper.GetValueOrDefault(m_playServicesApplicationId);
        
            public string ServerClientId => PropertyHelper.GetValueOrDefault(m_serverClientId);

            public string[] AchievedDescriptionFormats => m_achievedDescriptionFormats;

            public bool ShowErrorDialogs => m_showErrorDialogs;

            public bool DisplayPopupsAtTop => m_displayPopupsAtTop;

            public bool NeedsProfileScope => m_needsProfileScope;

            public bool NeedsEmailScope => m_needsEmailScope;

            #endregion

            #region Constructors

            public AndroidPlatformProperties(string playServicesApplicationId = null, string serverClientId = null,
                string[] achievedDescriptionFormats = null, bool showErrorDialogs = true,
                bool displayPopupsAtTop = true, bool needsProfileScope = false, bool needsEmailScope = false)
            {
                // set properties
                m_playServicesApplicationId     = playServicesApplicationId;
                m_serverClientId                = serverClientId;
                m_achievedDescriptionFormats    = achievedDescriptionFormats ?? new string[] 
                {
                    "Awesome! Achievement # completed."
                };
                m_showErrorDialogs              = showErrorDialogs;
                m_displayPopupsAtTop            = displayPopupsAtTop;
                m_needsProfileScope             = needsProfileScope;
                m_needsEmailScope               = needsEmailScope;
            }

            #endregion
        }
    }
}