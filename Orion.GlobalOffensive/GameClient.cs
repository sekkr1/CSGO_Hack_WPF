﻿// Copyright (C) 2015 aevitas
// See the file LICENSE for copying permission.

using System;
using log4net;
using Orion.Common;
using Orion.GlobalOffensive.Objects;
using Orion.GlobalOffensive.Patchables;

namespace Orion.GlobalOffensive
{
	/// <summary>
	///     Manages the game client, and all stuff we require from it.
	/// </summary>
	public class GameClient : NativeObject
	{
		private readonly ILog _log = Log.Get();

		/// <summary>
		///     Initializes a new instance of the <see cref="GameClient" /> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		public GameClient(IntPtr baseAddress) : base(baseAddress)
		{
			_log.Info("GameClient initialized.");
		}

		/// <summary>
		///     Gets the index of the local player.
		/// </summary>
		/// <value>
		///     The index of the local player.
		/// </value>
		public int LocalPlayerIndex => ReadField<int>(StaticOffsets.LocalPlayerIndex);

		/// <summary>
		///     Gets the state of the game client.
		/// </summary>
		/// <value>
		///     The state.
		/// </value>
		public SignonState State => (SignonState) ReadField<int>(StaticOffsets.GameState);

		/// <summary>
		///     Gets a value indicating whether [in game].
		/// </summary>
		/// <value>
		///     <c>true</c> if [in game]; otherwise, <c>false</c>.
		/// </value>
		public bool InGame => State == SignonState.Full;

		/// <summary>
		///     Gets a value indicating whether [in menu].
		/// </summary>
		/// <value>
		///     <c>true</c> if [in menu]; otherwise, <c>false</c>.
		/// </value>
		public bool InMenu => State == SignonState.None;
	}
}