using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    public partial class BillingProductDefinition
    {
        [Serializable]
        public class AndroidPlatformProperties
        {
            #region Fields

            [SerializeField]
            private     string              m_publicKey;

            [SerializeField]
            private     string              m_developerPayload;

            #endregion

            #region Properties

            public string PublicKey => PropertyHelper.GetValueOrDefault(m_publicKey);

            public string DeveloperPayload => PropertyHelper.GetValueOrDefault(m_developerPayload);

            #endregion

            #region Properties

            public AndroidPlatformProperties(string publicKey = null, string developerPayload = null)
            {
                // set properties
                m_publicKey         = publicKey;
                m_developerPayload  = developerPayload;
            }

            #endregion
        }
    }
}