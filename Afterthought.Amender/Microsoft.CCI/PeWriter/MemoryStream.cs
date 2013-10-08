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

namespace Microsoft.Cci.WriterUtilities {
  public sealed class MemoryStream {

    public MemoryStream() {
      this.Buffer = new byte[64];
    }

    public MemoryStream(uint initialSize)
      //^ requires initialSize > 0;
    {
      this.Buffer = new byte[initialSize];
    }

    public byte[] Buffer;
    //^ invariant Buffer.LongLength > 0;

    private void Grow(byte[] myBuffer, uint n, uint m)
      //^ requires n > 0;
    {
      ulong n2 = n*2;
      if (n2 == 0) n2 = 16;
      while (m >= n2) n2 = n2*2;
      byte[] newBuffer = this.Buffer = new byte[n2];
      for (int i = 0; i < n; i++)
        newBuffer[i] = myBuffer[i];
    }

    public uint Length;

    public uint Position {
      get {
        return this.position;
      }
      set {
        byte[] myBuffer = this.Buffer;
#if COMPACTFX
        uint n = (uint)myBuffer.Length;
#else
        uint n = (uint)myBuffer.LongLength;
#endif
        if (value >= n) this.Grow(myBuffer, n, value);
        if (value > this.Length) this.Length = value;
        this.position = value;
      }
    }
    private uint position;

    public byte[] ToArray() {
      uint n = this.Length;
      byte[] source = this.Buffer;
      if (source.Length == n) return this.Buffer;
      byte[] result = new byte[n];
      for (int i = 0; i < n; i++)
        result[i] = source[i];
      return result;
    }

    public void Write(byte[] buffer, uint index, uint count) {
      uint p = this.position;
      this.Position = p + count;
      byte[] myBuffer = this.Buffer;
      for (uint i = 0, j = p, k = index; i < count; i++)
        myBuffer[j++] = buffer[k++];
    }

    public void WriteTo(MemoryStream stream) {
      stream.Write(this.Buffer, 0, this.Length);
    }

    public void WriteTo(System.IO.Stream stream) {
      stream.Write(this.Buffer, 0, (int)this.Length);
    }
  }
}
