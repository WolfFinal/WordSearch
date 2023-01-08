using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using UnityEngine.Serialization;

namespace VoxelBusters.EssentialKit
{
    [Serializable]
    public partial class BillingServicesUnitySettings : SettingsPropertyGroup
    {
        #region Fields

        [SerializeField, FormerlySerializedAs("m_billingProductMetaArray")]
        [Tooltip("Array contains information of the products used in the app.")]
        private     BillingProductDefinition[]  m_products;

        [SerializeField]
        [Tooltip("If enabled, system tracks non-consummable purchase information (locally).")]
        private     bool                        m_maintainPurchaseHistory;

        [SerializeField]
        [Tooltip("If enabled, completed transactions are removed from queue automatically.")]
        private     bool                        m_autoFinishTransactions;

        [SerializeField]
        [Tooltip("If enabled, payment receipts are validated for completed transactions.")]
        private     bool                        m_verifyTransactionReceipts;

        [SerializeField]
        [Tooltip("Android specific properties.")]
        private     IosPlatformProperties       m_iosProperties;

        [SerializeField]
        [Tooltip("Android specific properties.")]
        private     AndroidPlatformProperties   m_androidProperties;

        #endregion

        #region Properties

        public BillingProductDefinition[] Products => m_products;

        public bool MaintainPurchaseHistory => m_maintainPurchaseHistory;

        public bool AutoFinishTransactions => m_autoFinishTransactions;

        public bool VerifyPaymentReceipts => m_verifyTransactionReceipts;

        public IosPlatformProperties IosProperties => m_iosProperties;

        public AndroidPlatformProperties AndroidProperties => m_androidProperties;

        #endregion

        #region Constructors

        public BillingServicesUnitySettings(bool isEnabled = true, BillingProductDefinition[] products = null, 
                                       bool maintainPurchaseHistory = true, bool autoFinishTransactions = true, 
                                       bool verifyTransactionReceipts = true, IosPlatformProperties iosProperties = null, 
                                       AndroidPlatformProperties androidProperties = null)
            : base(isEnabled: isEnabled, name: NativeFeatureType.kBillingServices)
        {
            // set properties
            m_products                      = products ?? new BillingProductDefinition[0];
            m_maintainPurchaseHistory       = maintainPurchaseHistory;
            m_autoFinishTransactions        = autoFinishTransactions;
            m_verifyTransactionReceipts     = verifyTransactionReceipts;
            m_iosProperties                 = iosProperties ?? new IosPlatformProperties();
            m_androidProperties             = androidProperties ?? new AndroidPlatformProperties();

            //
        }

        #endregion
    }
}