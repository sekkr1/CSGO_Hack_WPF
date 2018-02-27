using System;
using System.Runtime.InteropServices;

namespace Orion.GlobalOffensive.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    struct EntityEntry
    {
        public IntPtr m_pEntity;
        public IntPtr m_SerialNumber;
        public IntPtr m_pPrevious;
        public IntPtr m_pNext;
    }
}
