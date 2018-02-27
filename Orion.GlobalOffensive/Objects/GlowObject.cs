using System;

namespace Orion.GlobalOffensive.Objects
{
    public class GlowObject : NativeObject
    {
        public BaseEntity entity => Orion.Objects.GetPlayerByAddress(pEntity);
        public IntPtr pEntity => ReadField<IntPtr>(0);
        public Color m_vGlowColor => ReadField<Color>(4);
        public bool m_bRenderWhenOccluded => ReadField<bool>(36);
        public bool m_bRenderWhenUnoccluded => ReadField<bool>(37);
        public bool m_bFullBloom => ReadField<bool>(38);

        public GlowObject(IntPtr baseAddress) : base(baseAddress) { }
        public void Glow(Color color, bool cham)
        {
            WriteField(4, color);
            WriteField(36, true);
            WriteField(37, false);
            WriteField(38, cham);
        }
    }//size 0x34
}
