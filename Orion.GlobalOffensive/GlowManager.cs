using System;
using System.Collections.Generic;
using System.Linq;
using Orion.GlobalOffensive.Objects;
using Orion.GlobalOffensive.Patchables;

namespace Orion.GlobalOffensive
{
    public class GlowManager : NativeObject
    {
        private readonly List<GlowObject> _glowObjects = new List<GlowObject>();
        private int objCount => ReadField<int>(StaticOffsets.glowObjectsCount);
        private readonly IntPtr objBase;
        public IReadOnlyList<GlowObject> glowObjects => _glowObjects.ToList();
        public GlowManager(IntPtr baseAddress) : base(baseAddress)
        {
            objBase = Orion.Memory.Read<IntPtr>(baseAddress);
        }
        public void Update()
        {
            if (!IsValid)
                throw new InvalidOperationException(
                    "Can not update the ObjectManager when it's not properly initialized! Are you sure BaseAddress is valid?");
            _glowObjects.Clear();
            for (var i = 0; i < objCount; i++)
            {
                _glowObjects.Add(new GlowObject(objBase + i * 0x38));
            }
        }
    }
}
