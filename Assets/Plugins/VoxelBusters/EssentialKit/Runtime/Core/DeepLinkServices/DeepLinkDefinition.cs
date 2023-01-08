using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public class DeepLinkDefinition
    {
        #region Fields
        
        [SerializeField, DefaultValue("identifier")]
        private     string                      m_identifier;

        [SerializeField, DefaultValue("applinks")]
        private     string                      m_serviceType;

        [SerializeField]
        private     string                      m_scheme;

        [SerializeField]
        private     string                      m_host;

        [SerializeField]
        private     string                      m_path;

        #endregion

        #region Properties

        public string Identifier => PropertyHelper.GetValueOrDefault(
            instance: this,
            fieldAccess: (field) => field.m_identifier,
            value: m_identifier);

        public string ServiceType => PropertyHelper.GetValueOrDefault(
            instance: this,
            fieldAccess: (field) => field.m_serviceType,
            value: m_serviceType);

        public string Scheme => PropertyHelper.GetValueOrDefault(m_scheme); 

        public string Host => PropertyHelper.GetValueOrDefault(m_host);

        public string Path => PropertyHelper.GetValueOrDefault(m_path);

        #endregion

        #region Constructors
        
        public DeepLinkDefinition(string identifier = null, string serviceType = null,
            string scheme = null, string host = null,
            string path = null)
        {
            // set properties
            m_identifier    = PropertyHelper.GetValueOrDefault(
                instance: this,
                fieldAccess: (field) => field.m_identifier,
                value: identifier);
            m_serviceType   = PropertyHelper.GetValueOrDefault(
                instance: this,
                fieldAccess: (field) => field.m_serviceType,
                value: serviceType);
            m_scheme        = scheme;
            m_host          = host;
            m_path          = path;
        }

        #endregion
    }
}