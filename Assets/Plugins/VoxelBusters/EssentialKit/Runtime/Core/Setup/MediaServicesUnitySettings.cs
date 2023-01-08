using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public class MediaServicesUnitySettings : SettingsPropertyGroup
    {
        #region Fields

        [SerializeField]
        [Tooltip ("If enabled, permission required to access camera will be added.")]
        private     bool                    m_usesCamera;

        [SerializeField]
        [Tooltip ("If enabled, permission required to access gallery will be added.")]
        private     bool                    m_usesGallery;

        [SerializeField]
        [Tooltip ("If enabled, permission required to save file to gallery will be added.")]
        private     bool                    m_savesFilesToGallery;

        #endregion

        #region Properties

        public bool UsesCamera => m_usesCamera;

        public bool UsesGallery => m_usesGallery;

        public bool SavesFilesToGallery => m_savesFilesToGallery;

        #endregion

        #region Constructors

        public MediaServicesUnitySettings(bool isEnabled = true, bool usesCamera = true,
            bool usesGallery = true, bool savesFilesToGallery = true)
            : base(isEnabled: isEnabled, name: NativeFeatureType.kMediaServices)
        {
            // set properties
            m_usesCamera            = usesCamera;
            m_usesGallery           = usesGallery;
            m_savesFilesToGallery   = savesFilesToGallery;
        }

        #endregion
    }
}