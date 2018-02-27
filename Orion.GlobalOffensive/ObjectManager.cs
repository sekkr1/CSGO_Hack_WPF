﻿// Copyright (C) 2015 aevitas
// See the file LICENSE for copying permission.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Orion.Common;
using Orion.GlobalOffensive.Objects;
using Orion.GlobalOffensive.Patchables;

namespace Orion.GlobalOffensive
{
    /// <summary>
    ///     Manages entities within the game world.
    /// </summary>
    public class ObjectManager : NativeObject
	{
		private readonly ILog _log = Log.Get();
		// Exposed through a read-only list, users of the API won't be able to change what's going on in game anyway.
		private readonly List<BaseEntity> _players = new List<BaseEntity>();
		private readonly int _ticksPerSecond;
		private TimeSpan _lastUpdate = TimeSpan.Zero;


		/// <summary>
		///     Initializes a new instance of the <see cref="ObjectManager" /> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="capacity">The capacity.</param>
		/// <param name="ticksPerSecond">The ticks per second.</param>
		public ObjectManager(IntPtr baseAddress, int capacity, int ticksPerSecond = 10) : base(baseAddress)
		{
			_ticksPerSecond = ticksPerSecond;

			_log.Info($"ObjectManager initialized. Capacity = {capacity}, TPS = {ticksPerSecond}");
		}

		/// <summary>
		///     Gets the current valid objects in the game world.
		/// </summary>
		public IReadOnlyList<BaseEntity> Players => _players.Where(p => p.IsValid).ToList();

        internal LocalPlayer LocalPlayer { get; private set; }

		/// <summary>
		///     Updates the ObjectManager, obtaining all player entities from the game and adding them to the Players list.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		///     Can not update the ObjectManager when it's not properly initialized!
		///     Are you sure BaseAddress is valid?
		/// </exception>
		public void Update()
		{
			if (!IsValid)
				throw new InvalidOperationException(
					"Can not update the ObjectManager when it's not properly initialized! Are you sure BaseAddress is valid?");

			var timeStamp = MonotonicTimer.GetTimeStamp();
			// Throttle the updates a little - entities won't be changing that frequently.
			// Realistically we don't need to call this very often at all, as we only keep references to the actual
			// entities in the game, and only resolve their members when they're actually required.
			if (timeStamp - _lastUpdate < TimeSpan.FromMilliseconds(1000.0/_ticksPerSecond))
				return;

            _lastUpdate = timeStamp;
            if (!Orion.Client.InGame)
				// No point in updating if we're not in game - we'd end up reading garbage.
				// Do set the last update time though, we especially don't want to tick too often in menu.
				return;

			// Prevent duplicate entries - more efficient would be maintaining a dictionary and updating entities.
			// Then again, this is significantly less code, and performance wise not too big an impact. Leave it be for now,
			// but consider updating this in the future.
			_players.Clear();

            IntPtr localPlayerPtr = Orion.Memory.Read<IntPtr>(Orion.ClientBase + (int)BaseOffsets.LocalPlayer);

            LocalPlayer = new LocalPlayer(localPlayerPtr);

            var dwAddress = BaseAddress;
            EntityEntry entry;
            while ((int)dwAddress != 0)
            {
                entry = Orion.Memory.Read<EntityEntry>(dwAddress);
                dwAddress = entry.m_pNext;
                if (entry.m_pNext == entry.m_pPrevious)
                    break;
                if ((int)entry.m_pEntity == 0)
                    continue;
                _players.Add(new BaseEntity(entry.m_pEntity));
            }

            Trace.WriteLine($"[EntityManager] Update complete. {Players.Count(s => s.IsValid)} valid entries found.");


			_lastUpdate = timeStamp;
		}

		private IntPtr GetEntityPtr(int index)
		{
			// ptr = entityList + (idx * size)
			return Orion.Memory.Read<IntPtr>(BaseAddress + index*(int) StaticOffsets.EntitySize);
		}

		/// <summary>
		///     Gets the player with the specified ID, and null if that player doesn't exist.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public BaseEntity GetPlayerById(int id)
		{
			if (_players.Count < id)
				return null;

			return Players.FirstOrDefault(p => p.Id == id);
		}

        public BaseEntity GetPlayerByAddress(IntPtr baseAddress)
        {
            return Players.FirstOrDefault(p => p.BaseAddress == baseAddress);
        }

    }
}
