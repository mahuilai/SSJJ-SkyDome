using System;
using UnityEngine;

namespace DotNetDetour
{
    // Token: 0x02000006 RID: 6
    public class LDasm
    {
        // Token: 0x06000015 RID: 21 RVA: 0x00002547 File Offset: 0x00000747
        private static byte cflags(byte op)
        {
            return LDasm.flags_table[(int)op];
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002550 File Offset: 0x00000750
        private static byte cflags_ex(byte op)
        {
            return LDasm.flags_table_ex[(int)op];
        }

        // Token: 0x06000017 RID: 23 RVA: 0x0000255C File Offset: 0x0000075C
        // 计算最小字节数
        public unsafe static uint SizeofMinNumByte(void* code, int size)
        {
            if (!LDasm.IsAndroidARM())
            {
                uint num = 0U;
                LDasm.ldasm_data ldasm_data = default(LDasm.ldasm_data);
                bool flag = IntPtr.Size == 8;
                uint num2;
                do
                {
                    num2 = LDasm.ldasm(code, ldasm_data, flag);
                    byte* ptr = (byte*)code + ldasm_data.opcd_offset;
                    num += num2;
                    if ((ulong)num >= (ulong)((long)size) || (num2 == 1U && *ptr == 204))
                    {
                        break;
                    }
                    code = (void*)((byte*)code + (ulong)num2);
                }
                while (num2 > 0U);
                return num;
            }
            if (LDasm.IsIL2CPP())
            {
                return LDasm.CalcARMThumbMinLen(code, size);
            }
            return (uint)((size + 3) / 4 * 4);
        }

        // Token: 0x06000018 RID: 24 RVA: 0x000025D2 File Offset: 0x000007D2
        public static bool IsAndroidARM()
        {
            return SystemInfo.operatingSystem.Contains("Android") && SystemInfo.processorType.Contains("ARM");
        }

        // Token: 0x06000019 RID: 25 RVA: 0x000025F6 File Offset: 0x000007F6
        public static bool IsiOS()
        {
            return SystemInfo.operatingSystem.ToLower().Contains("ios");
        }

        // Token: 0x0600001A RID: 26 RVA: 0x0000260C File Offset: 0x0000080C
        public static bool IsIL2CPP()
        {
            bool flag = false;
            try
            {
                byte[] ilasByteArray = typeof(LDasm).GetMethod("IsIL2CPP").GetMethodBody().GetILAsByteArray();
                if (ilasByteArray == null || ilasByteArray.Length == 0)
                {
                    flag = true;
                }
            }
            catch
            {
                flag = true;
            }
            return flag;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x0000265C File Offset: 0x0000085C
        // 计算ARM Thumb最小长度
        public unsafe static uint CalcARMThumbMinLen(void* code, int size)
        {
            uint num = 0U;
            ushort* ptr = (ushort*)code;
            while ((ulong)num < (ulong)((long)size))
            {
                if (((*ptr >> 13) & 3) == 3)
                {
                    ptr += 2;
                    num += 4U;
                }
                else
                {
                    ptr++;
                    num += 2U;
                }
            }
            return num;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00002698 File Offset: 0x00000898
        // 反汇编单条指令
        private unsafe static uint ldasm(void* code, LDasm.ldasm_data ld, bool is64)
        {
            byte* ptr = (byte*)code;
            byte b4;
            byte b3;
            byte b2;
            byte b = (b2 = (b3 = (b4 = 0)));
            if (new IntPtr(code) == IntPtr.Zero)
            {
                return 0U;
            }
            while ((LDasm.cflags(*ptr) & 128) != 0)
            {
                if (*ptr == 102)
                {
                    b3 = 1;
                }
                if (*ptr == 103)
                {
                    b4 = 1;
                }
                ptr++;
                b2 = (byte)(b2 + 1);
                ld.flags |= 2;
                if (b2 == 15)
                {
                    ld.flags |= 1;
                    return (uint)b2;
                }
            }
            if (is64 && *ptr >> 4 == 4)
            {
                ld.rex = *ptr;
                b = (byte)((ld.rex >> 3) & 1);
                ld.flags |= 4;
                ptr++;
                b2 = (byte)(b2 + 1);
            }
            if (is64 && *ptr >> 4 == 4)
            {
                ld.flags |= 1;
                return (uint)(b2 + 1);
            }
            ld.opcd_offset = (byte)((long)((byte*)ptr - (byte*)code));
            ld.opcd_size = 1;
            byte b5 = *(ptr++);
            b2 = (byte)(b2 + 1);
            byte b6;
            if (b5 == 15)
            {
                b5 = *(ptr++);
                b2 = (byte)(b2 + 1);
                ld.opcd_size = (byte)(ld.opcd_size + 1);
                b6 = LDasm.cflags_ex(b5);
                if ((b6 & 128) != 0)
                {
                    ld.flags |= 1;
                    return (uint)b2;
                }
                if ((b6 & 16) != 0)
                {
                    b5 = *(ptr++);
                    b2 = (byte)(b2 + 1);
                    ld.opcd_size = (byte)(ld.opcd_size + 1);
                }
            }
            else
            {
                b6 = LDasm.cflags(b5);
                if (b5 >= 160 && b5 <= 163)
                {
                    b3 = b4;
                }
            }
            if ((b6 & 64) != 0)
            {
                byte b7 = (byte)(*ptr >> 6);
                byte b8 = (byte)((*ptr & 56) >> 3);
                byte b9 = (byte)(*ptr & 7);
                ld.modrm = *(ptr++);
                b2 = (byte)(b2 + 1);
                ld.flags |= 8;
                if (b5 == 246 && (b8 == 0 || b8 == 1))
                {
                    b6 |= 1;
                }
                if (b5 == 247 && (b8 == 0 || b8 == 1))
                {
                    b6 |= 8;
                }
                if (b7 != 3 && b9 == 4 && (is64 || b4 == 0))
                {
                    ld.sib = *(ptr++);
                    b2 = (byte)(b2 + 1);
                    ld.flags |= 16;
                    if ((ld.sib & 7) == 5 && b7 == 0)
                    {
                        ld.disp_size = 4;
                    }
                }
                switch (b7)
                {
                    case 0:
                        if (is64)
                        {
                            if (b9 == 5)
                            {
                                ld.disp_size = 4;
                                if (is64)
                                {
                                    ld.flags |= 128;
                                }
                            }
                        }
                        else if (b4 != 0)
                        {
                            if (b9 == 6)
                            {
                                ld.disp_size = 2;
                            }
                        }
                        else if (b9 == 5)
                        {
                            ld.disp_size = 4;
                        }
                        break;
                    case 1:
                        ld.disp_size = 1;
                        break;
                    case 2:
                        if (is64)
                        {
                            ld.disp_size = 4;
                        }
                        else if (b4 != 0)
                        {
                            ld.disp_size = 2;
                        }
                        else
                        {
                            ld.disp_size = 4;
                        }
                        break;
                }
                if (ld.disp_size > 0)
                {
                    ld.disp_offset = (byte)((long)((byte*)ptr - (byte*)code));
                    ptr += ld.disp_size;
                    b2 = (byte)(b2 + ld.disp_size);
                    ld.flags |= 32;
                }
            }
            if (b != 0 && (b6 & 8) != 0)
            {
                ld.imm_size = 8;
            }
            else if ((b6 & 4) != 0 || (b6 & 8) != 0)
            {
                ld.imm_size = (byte)(4 - ((int)b3 << 1));
            }
            ld.imm_size = (byte)(ld.imm_size + (b6 & 3));
            if (ld.imm_size != 0)
            {
                b2 = (byte)(b2 + ld.imm_size);
                ld.imm_offset = (byte)((long)((byte*)ptr - (byte*)code));
                ld.flags |= 64;
                if ((b6 & 32) != 0)
                {
                    ld.flags |= 128;
                }
            }
            if (b2 > 15)
            {
                ld.flags |= 1;
            }
            return (uint)b2;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x000029FB File Offset: 0x00000BFB
        public LDasm()
        {
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00002A03 File Offset: 0x00000C03
        // Note: this type is marked as 'beforefieldinit'.
        static LDasm()
        {
        }

        // Token: 0x04000012 RID: 18
        private const int F_INVALID = 1;

        // Token: 0x04000013 RID: 19
        private const int F_PREFIX = 2;

        // Token: 0x04000014 RID: 20
        private const int F_REX = 4;

        // Token: 0x04000015 RID: 21
        private const int F_MODRM = 8;

        // Token: 0x04000016 RID: 22
        private const int F_SIB = 16;

        // Token: 0x04000017 RID: 23
        private const int F_DISP = 32;

        // Token: 0x04000018 RID: 24
        private const int F_IMM = 64;

        // Token: 0x04000019 RID: 25
        private const int F_RELATIVE = 128;

        // Token: 0x0400001A RID: 26
        private const int OP_NONE = 0;

        // Token: 0x0400001B RID: 27
        private const int OP_INVALID = 128;

        // Token: 0x0400001C RID: 28
        private const int OP_DATA_I8 = 1;

        // Token: 0x0400001D RID: 29
        private const int OP_DATA_I16 = 2;

        // Token: 0x0400001E RID: 30
        private const int OP_DATA_I16_I32 = 4;

        // Token: 0x0400001F RID: 31
        private const int OP_DATA_I16_I32_I64 = 8;

        // Token: 0x04000020 RID: 32
        private const int OP_EXTENDED = 16;

        // Token: 0x04000021 RID: 33
        private const int OP_RELATIVE = 32;

        // Token: 0x04000022 RID: 34
        private const int OP_MODRM = 64;

        // Token: 0x04000023 RID: 35
        private const int OP_PREFIX = 128;

        // Token: 0x04000024 RID: 36
        private static byte[] flags_table = new byte[]
        {
            64, 64, 64, 64, 1, 4, 0, 0, 64, 64,
            64, 64, 1, 4, 0, 0, 64, 64, 64, 64,
            1, 4, 0, 0, 64, 64, 64, 64, 1, 4,
            0, 0, 64, 64, 64, 64, 1, 4, 128, 0,
            64, 64, 64, 64, 1, 4, 128, 0, 64, 64,
            64, 64, 1, 4, 128, 0, 64, 64, 64, 64,
            1, 4, 128, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 64, 64,
            128, 128, 128, 128, 4, 68, 1, 65, 0, 0,
            0, 0, 33, 33, 33, 33, 33, 33, 33, 33,
            33, 33, 33, 33, 33, 33, 33, 33, 65, 68,
            65, 65, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 6, 0, 0, 0, 0, 0,
            1, 8, 1, 8, 0, 0, 0, 0, 1, 4,
            0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
            1, 1, 1, 1, 8, 8, 8, 8, 8, 8,
            8, 8, 65, 65, 2, 0, 64, 64, 65, 68,
            3, 0, 2, 0, 0, 1, 0, 0, 64, 64,
            64, 64, 1, 1, 0, 0, 64, 64, 64, 64,
            64, 64, 64, 64, 33, 33, 33, 33, 1, 1,
            1, 1, 36, 36, 6, 33, 0, 0, 0, 0,
            128, 0, 128, 128, 0, 0, 64, 64, 0, 0,
            0, 0, 0, 0, 64, 64
        };

        // Token: 0x04000025 RID: 37
        private static byte[] flags_table_ex = new byte[]
        {
            64, 64, 64, 64, 128, 0, 0, 0, 0, 0,
            128, 0, 128, 64, 128, 65, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 128, 128, 128, 128, 128,
            128, 0, 64, 64, 64, 64, 80, 128, 64, 128,
            64, 64, 64, 64, 64, 64, 64, 64, 0, 0,
            0, 0, 0, 0, 128, 0, 80, 128, 81, 128,
            128, 128, 128, 128, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 65, 65, 65, 65, 64, 64, 64, 0,
            64, 64, 128, 128, 64, 64, 64, 64, 36, 36,
            36, 36, 36, 36, 36, 36, 36, 36, 36, 36,
            36, 36, 36, 36, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            0, 0, 0, 64, 65, 64, 128, 128, 0, 0,
            0, 64, 65, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 65, 64, 64, 64,
            64, 64, 64, 64, 65, 64, 65, 65, 65, 64,
            0, 0, 0, 0, 0, 0, 0, 0, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
            64, 64, 64, 64, 64, 128
        };

        // Token: 0x02000024 RID: 36
        private struct ldasm_data
        {
            public byte flags;
            public byte rex;
            public byte modrm;
            public byte sib;
            public byte opcd_offset;
            public byte opcd_size;
            public byte disp_offset;
            public byte dwSize; // alias for compatibility if needed
            public byte disp_size;
            public byte imm_offset;
            public byte imm_size;
        }
    }
}
