using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    /// <summary>
    /// This class contains the information retrieved when <see cref="GameServices.LoadPlayers(string[], EventCallback{GameServicesLoadPlayersResult})"/> operation is completed.
    /// </summary>
    public class GameServicesLoadPlayersResult
    {
        #region Properties

        /// <summary>
        /// An array of requested players.
        /// </summary>
        public IPlayer[] Players { get; private set; }

        #endregion

        #region Constructors

        internal GameServicesLoadPlayersResult(IPlayer[] players)
        {
            // set properties
            Players     = players;
        }

        #endregion
    }
}