using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace HardwareDiagnostics
{
    public class SmartData
    {
        readonly Dictionary<SmartAttributeType, SmartAttribute> attributes;
        readonly ushort structureVersion;

        public SmartData(byte[] arrVendorSpecific)
        {
            attributes = new Dictionary<SmartAttributeType, SmartAttribute>();
            
            // Get all the Types and Values of the Attributes within the VendorSpecific byte[]
            // Skip the first 2 bytes as they do not contain attribute information (just Version Info)
            for (int offset = 2; offset < arrVendorSpecific.Length; ) {
                var a = FromBytes<SmartAttribute>(arrVendorSpecific, ref offset, 12);
                // Attribute values 0x00, 0xfe, 0xff are invalid
                if (a.AttributeType != 0x00 && (byte)a.AttributeType != 0xfe && (byte)a.AttributeType != 0xff) {
                    attributes[a.AttributeType] = a;
                }
            }

            // Doesn't mean we shouldn't capture that version info SON!
            structureVersion = (ushort)(arrVendorSpecific[0] * 256 + arrVendorSpecific[1]);
        }

        public ushort StructureVersion
        {
            get {
                return this.structureVersion;
            }
        }

        public SmartAttribute this[SmartAttributeType v]
        {
            get {
                return this.attributes[v];
            }
        }

        public IEnumerable<SmartAttribute> Attributes
        {
            get {
                return this.attributes.Values;
            }
        }

        static T FromBytes<T>(byte[] bytearray, ref int offset, int count)
        {
            IntPtr ptr = IntPtr.Zero;

            try {
                ptr = Marshal.AllocHGlobal(count);
                Marshal.Copy(bytearray, offset, ptr, count);
                offset += count;
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            } finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }
}
