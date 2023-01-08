using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public class CloudServicesUnitySettings : SettingsPropertyGroup
    {
        #region Fields

        [Tooltip("If enabled, data is synchronized automatically on launch.")]
        [SerializeField]
        private     bool            m_synchronizeOnLoad;
        
        [Tooltip("Time interval (in seconds) between consecutive sync calls.")]
        [SerializeField]
        private     int             m_syncInterval;

        #endregion

        #region Properties

        public bool SynchronizeOnLoad => m_synchronizeOnLoad;

        public int SyncInterval => m_syncInterval;

        #endregion

        #region Constructors

        public CloudServicesUnitySettings(bool isEnabled = true, bool synchronizeOnLoad = false,
            int syncInterval = 60) 
            : base(isEnabled: isEnabled, name: NativeFeatureType.kCloudServices)
        { 
            // set properties
            m_synchronizeOnLoad = synchronizeOnLoad;
            m_syncInterval      = syncInterval;
        }

        #endregion
    }
}