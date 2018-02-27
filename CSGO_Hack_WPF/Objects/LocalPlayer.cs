using System;
using System.Numerics;
using CSGO_Hack_WPF.SDK;

namespace CSGO_Hack_WPF.Objects
{
    public class LocalPlayer : Player
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalPlayer" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        internal LocalPlayer(IntPtr baseAddress) : base(baseAddress) { }

        /// <summary>
        ///     Gets the view matrix of the local player.
        /// </summary>
        /// <value>
        ///     The view matrix.
        /// </value>
        public Matrix4x4 ViewMatrix => Core.Memory.Read<Matrix4x4>(Core.ClientBase + Offsets.Misc.ViewMatrix);

        /// <summary>
        ///     Gets the player ID for the player currently under the player's crosshair, and 0 if none.
        /// </summary>
        public int CrosshairId => ReadField<int>(Offsets.LocalPlayer.CrosshairId);

        public int Velocity => (int)new Vector2(Core.LocalPlayer.VecVelocity.X, Core.LocalPlayer.VecVelocity.Y).Length();

        /// <summary>
        ///     Gets the target the local player is currently aiming at, or null if none.
        /// </summary>
        public Player Target => CrosshairId <= 0 ? null : Core.Objects.GetPlayerById(CrosshairId);
    }
}