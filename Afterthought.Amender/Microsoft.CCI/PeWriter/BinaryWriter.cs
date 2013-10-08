//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------

using System.Diagnostics.Contracts;

namespace Microsoft.Cci.WriterUtilities {
  public sealed class BinaryWriter {

    public BinaryWriter(MemoryStream output) {
      Contract.Requires(output != null);

      this.baseStream = output;
    }

    public BinaryWriter(MemoryStream output, bool unicode) {
      Contract.Requires(output != null);

      this.baseStream = output;
      this.UTF8 = !unicode;
    }

    [ContractInvariantMethod]
    private void ObjectInvariant() {
      Contract.Invariant(this.baseStream != null);
    }


    public MemoryStream BaseStream {
      get {
        Contract.Ensures(Contract.Result<MemoryStream>() != null);
        return this.baseStream;
      }
    }
    MemoryStream baseStream;

    private bool UTF8 = true;

    public void Align(uint alignment) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      while (i % alignment > 0) {
        m.Buffer[i++] = 0;
        m.Position = i;
      }
    }

    public void WriteBool(bool value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position = i+1;
      m.Buffer[i] = (byte)(value ? 1 : 0);
    }

    public void WriteByte(byte value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position = i+1;
      m.Buffer[i] = value;
    }

    public void WriteSbyte(sbyte value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position = i+1;
      m.Buffer[i] = (byte)value;
    }

    public void WriteBytes(byte[] buffer) {
      if (buffer == null) return;
      this.BaseStream.Write(buffer, 0, (uint)buffer.Length);
    }

    //public void WriteChar(char ch) {
    //  MemoryStream m = this.BaseStream;
    //  uint i = m.Position;
    //  if (this.UTF8) {
    //    if (ch < 0x80) {
    //      m.Position = i+1;
    //      m.Buffer[i] = (byte)ch;
    //    } else
    //      this.WriteChars(new char[] { ch });
    //  } else {
    //    m.Position = i+2;
    //    byte[] buffer = m.Buffer;
    //    buffer[i++] = (byte)ch;
    //    buffer[i] = (byte)(ch >> 8);
    //  }
    //}

    public void WriteChars(char[] chars) {
      if (chars == null) return;
      MemoryStream m = this.BaseStream;
      uint n = (uint)chars.Length;
      uint i = m.Position;
      if (this.UTF8) {
        m.Position = i+n;
        byte[] buffer = m.Buffer;
        for (int j = 0; j < n; j++) {
          char ch = chars[j];
          if ((ch & 0x80) != 0) goto writeUTF8;
          buffer[i++] = (byte)ch;
        }
        return;
      writeUTF8:
        int ch32 = 0;
        for (uint j = n-(m.Position-i); j < n; j++) {
          char ch = chars[j];
          if (ch < 0x80) {
            m.Position = i+1;
            buffer = m.Buffer;
            buffer[i++] = (byte)ch;
          } else if (ch < 0x800) {
            m.Position = i+2;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch>>6) & 0x1F) | 0xC0);
            buffer[i] = (byte)((ch & 0x3F) | 0x80);
          } else if (0xD800 <= ch && ch <= 0xDBFF) {
            ch32 = (ch & 0x3FF) << 10;
          } else if (0xDC00 <= ch && ch <= 0xDFFF) {
            ch32 |= ch & 0x3FF;
            m.Position = i+4;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch32>>18) & 0x7) | 0xF0);
            buffer[i++] = (byte)(((ch32>>12) & 0x3F) | 0x80);
            buffer[i++] = (byte)(((ch32>>6) & 0x3F) | 0x80);
            buffer[i] = (byte)((ch32 & 0x3F) | 0x80);
          } else {
            m.Position = i+3;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch>>12) & 0xF) | 0xE0);
            buffer[i++] = (byte)(((ch>>6) & 0x3F) | 0x80);
            buffer[i] = (byte)((ch & 0x3F) | 0x80);
          }
        }
      } else {
        m.Position = i+n*2;
        byte[] buffer = m.Buffer;
        for (int j = 0; j < n; j++) {
          char ch = chars[j];
          buffer[i++] = (byte)ch;
          buffer[i++] = (byte)(ch >> 8);
        }
      }
    }

    public unsafe void WriteDouble(double value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+8;
      byte[] buffer = m.Buffer;
      byte* d = (byte*)&value;
      for (uint j = 0; j < 8; j++)
        buffer[i+j] = *(d + j);
    }

    public void WriteShort(short value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+2;
      byte[] buffer = m.Buffer;
      buffer[i++] = (byte)value;
      buffer[i] = (byte)(value >> 8);
    }

    public unsafe void WriteUshort(ushort value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+2;
      byte[] buffer = m.Buffer;
      buffer[i++] = (byte)value;
      buffer[i] = (byte)(value >> 8);
    }

    public void WriteInt(int value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+4;
      byte[] buffer = m.Buffer;
      buffer[i++] = (byte)value;
      buffer[i++] = (byte)(value >> 8);
      buffer[i++] = (byte)(value >> 16);
      buffer[i] = (byte)(value >> 24);
    }

    public void WriteUint(uint value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+4;
      byte[] buffer = m.Buffer;
      buffer[i++] = (byte)value;
      buffer[i++] = (byte)(value >> 8);
      buffer[i++] = (byte)(value >> 16);
      buffer[i] = (byte)(value >> 24);
    }

    public void WriteLong(long value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+8;
      byte[] buffer = m.Buffer;
      uint lo = (uint)value;
      uint hi = (uint)(value >> 32);
      buffer[i++] = (byte)lo;
      buffer[i++] = (byte)(lo >> 8);
      buffer[i++] = (byte)(lo >> 16);
      buffer[i++] = (byte)(lo >> 24);
      buffer[i++] = (byte)hi;
      buffer[i++] = (byte)(hi >> 8);
      buffer[i++] = (byte)(hi >> 16);
      buffer[i] = (byte)(hi >> 24);
    }

    public void WriteUlong(ulong value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+8;
      byte[] buffer = m.Buffer;
      uint lo = (uint)value;
      uint hi = (uint)(value >> 32);
      buffer[i++] = (byte)lo;
      buffer[i++] = (byte)(lo >> 8);
      buffer[i++] = (byte)(lo >> 16);
      buffer[i++] = (byte)(lo >> 24);
      buffer[i++] = (byte)hi;
      buffer[i++] = (byte)(hi >> 8);
      buffer[i++] = (byte)(hi >> 16);
      buffer[i] = (byte)(hi >> 24);
    }

    public unsafe void WriteFloat(float value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      m.Position=i+4;
      byte[] buffer = m.Buffer;
      byte* f = (byte*)&value;
      for (uint j = 0; j < 4; j++)
        buffer[i+j] = *(f + j);
    }

    public void WriteString(string str) {
      this.WriteString(str, false);
    }

    public void WriteString(string str, bool emitNullTerminator) {
      if (str == null) {
        this.WriteByte(0xff);
        return;
      }
      int n = str.Length;
      if (!emitNullTerminator) {
        if (this.UTF8)
          this.WriteCompressedUInt(GetUTF8ByteCount(str));
        else
          this.WriteCompressedUInt((uint)n*2);
      }
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      if (this.UTF8) {
        m.Position = i+(uint)n;
        byte[] buffer = m.Buffer;
        for (int j = 0; j < n; j++) {
          char ch = str[j];
          if (ch >= 0x80) goto writeUTF8;
          buffer[i++] = (byte)ch;
        }
        if (emitNullTerminator) {
          m.Position = i+1;
          buffer = m.Buffer;
          buffer[i] = 0;
        }
        return;
      writeUTF8:
        int ch32 = 0;
        for (int j = n-(int)(m.Position-i); j < n; j++) {
          char ch = str[j];
          if (ch < 0x80) {
            m.Position = i+1;
            buffer = m.Buffer;
            buffer[i++] = (byte)ch;
          } else if (ch < 0x800) {
            m.Position = i+2;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch>>6) & 0x1F) | 0xC0);
            buffer[i++] = (byte)((ch & 0x3F) | 0x80);
          } else if (0xD800 <= ch && ch <= 0xDBFF) {
            ch32 = (ch & 0x3FF) << 10;
          } else if (0xDC00 <= ch && ch <= 0xDFFF) {
            ch32 |= ch & 0x3FF;
            m.Position = i+4;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch32>>18) & 0x7) | 0xF0);
            buffer[i++] = (byte)(((ch32>>12) & 0x3F) | 0x80);
            buffer[i++] = (byte)(((ch32>>6) & 0x3F) | 0x80);
            buffer[i++] = (byte)((ch32 & 0x3F) | 0x80);
          } else {
            m.Position = i+3;
            buffer = m.Buffer;
            buffer[i++] = (byte)(((ch>>12) & 0xF) | 0xE0);
            buffer[i++] = (byte)(((ch>>6) & 0x3F) | 0x80);
            buffer[i++] = (byte)((ch & 0x3F) | 0x80);
          }
        }
        if (emitNullTerminator) {
          m.Position = i+1;
          buffer = m.Buffer;
          buffer[i] = 0;
        }
      } else {
        m.Position = i+(uint)n*2;
        byte[] buffer = m.Buffer;
        for (int j = 0; j < n; j++) {
          char ch = str[j];
          buffer[i++] = (byte)ch;
          buffer[i++] = (byte)(ch >> 8);
        }
        if (emitNullTerminator) {
          m.Position = i+2;
          buffer = m.Buffer;
          buffer[i++] = 0;
          buffer[i] = 0;
        }
      }
    }

    public void WriteCompressedFullInt(int value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      byte[] buffer = m.Buffer;
      uint d = (uint)value;
      if (d + 64 < 128) {
        buffer[i++] = (byte)(d*2 + 0);
      } else if (d + 64*128 < 128*128) {
        buffer[i++] = (byte)(d*4 + 1);
        buffer[i++] = (byte)(d >> 6);
      } else if (d + 64*128*128 < 128*128*128) {
        buffer[i++] = (byte)(d*8 + 3);
        buffer[i++] = (byte)(d >> 5);
        buffer[i++] = (byte)(d >> 13);
      } else if (d + 64*128*128*128 < 128*128*128*128) {
        buffer[i++] = (byte)(d*16 + 7);
        buffer[i++] = (byte)(d >> 4);
        buffer[i++] = (byte)(d >> 12);
        buffer[i++] = (byte)(d >> 20);
      } else {
        buffer[i++] = (byte)15;
        buffer[i++] = (byte)d;
        buffer[i++] = (byte)(d >> 8);
        buffer[i++] = (byte)(d >> 16);
        buffer[i++] = (byte)(d >> 24);
      }
      m.Position=i;
    }

    public void WriteCompressedFullUInt(uint value) {
      MemoryStream m = this.BaseStream;
      uint i = m.Position;
      byte[] buffer = m.Buffer;
      if (value < 128) {
        buffer[i++] = (byte)(value*2 + 0);
      } else if (value < 128*128) {
        buffer[i++] = (byte)(value*4 + 1);
        buffer[i++] = (byte)(value >> 6);
      } else if (value < 128*128*128) {
        buffer[i++] = (byte)(value*8 + 3);
        buffer[i++] = (byte)(value >> 5);
        buffer[i++] = (byte)(value >> 13);
      } else if (value < 128*128*128*128) {
        buffer[i++] = (byte)(value*16 + 7);
        buffer[i++] = (byte)(value >> 4);
        buffer[i++] = (byte)(value >> 12);
        buffer[i++] = (byte)(value >> 20);
      } else {
        buffer[i++] = (byte)15;
        buffer[i++] = (byte)value;
        buffer[i++] = (byte)(value >> 8);
        buffer[i++] = (byte)(value >> 16);
        buffer[i++] = (byte)(value >> 24);
      }
      m.Position=i;
    }

    public void WriteCompressedInt(int val) {
      if (val >= 0) {
        val = val << 1;
        this.WriteCompressedUInt((uint)val);
      } else {
        if (val > -0x40) {
          val = 0x40 + val;
          val = (val << 1)|1;
          this.WriteByte((byte)val);
        } else if (val >= -0x2000) {
          val = 0x2000 + val;
          val = (val << 1)|1;
          this.WriteByte((byte)((val >> 8)|0x80));
          this.WriteByte((byte)(val & 0xff));
        } else if (val >= -0x20000000) {
          val = 0x20000000 + val;
          val = (val << 1)|1;
          this.WriteByte((byte)((val >> 24)|0xc0));
          this.WriteByte((byte)((val & 0xff0000)>>16));
          this.WriteByte((byte)((val & 0xff00)>>8));
          this.WriteByte((byte)(val & 0xff));
        } else {
          //^ assume false;
        }
      }
    }

    public void WriteCompressedUInt(uint val) {
      if (val <= 0x7f)
        this.WriteByte((byte)val);
      else if (val <= 0x3fff) {
        this.WriteByte((byte)((val >> 8)|0x80));
        this.WriteByte((byte)(val & 0xff));
      } else if (val <= 0x1fffffff) {
        this.WriteByte((byte)((val >> 24)|0xc0));
        this.WriteByte((byte)((val & 0xff0000)>>16));
        this.WriteByte((byte)((val & 0xff00)>>8));
        this.WriteByte((byte)(val & 0xff));
      } else {
        //^ assume false;
      }
    }

    public static uint GetUTF8ByteCount(string str) {
      uint count = 0;
      for (int i = 0, n = str.Length; i < n; i++) {
        char ch = str[i];
        if (ch < 0x80) {
          count += 1;
        } else if (ch < 0x800) {
          count += 2;
        } else if (0xD800 <= ch && ch <= 0xDBFF) {
          count += 2;
        } else if (0xDC00 <= ch && ch <= 0xDFFF) {
          count += 2;
        } else {
          count += 3;
        }
      }
      return count;
    }

  }
}