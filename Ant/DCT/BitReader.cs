using FrostySdk.IO;
using System;
using System.IO;

namespace AssetBankPlugin.Ant
{
    public class BitReader : NativeReader
    {
        /// <summary>
        /// Current position of the stream.
        /// </summary>
        public override long Position
        {
            get => 8 * BaseStream.Position - CurrentBitsLeft / 8;
        }

        public Endian Endianness { get; set; }

        /// <summary>
        /// Whether or not this reader has been disposed yet.
        /// </summary>
        protected bool Disposed { get; set; }
        protected bool ShouldDispose { get; set; }

        /// <summary>
        /// Length of the underlying stream.
        /// </summary>
        public override long Length => BaseStream.Length * 8;

        protected byte[] BitBuffer = new byte[0];

        protected int BitsPerSlice = 0;
        protected int CurrentBitsLeft = 0;


        protected int AlignedBitsPerSlice => BytesPerSlice * 8;
        protected int BytesPerSlice => (BitsPerSlice + 7) / 8;

        protected int CurrentByteIndexLow => (CurrentBitsLeft - 1) / 8;
        protected int CurrentByteIndexHigh => BytesPerSlice - CurrentByteIndexLow - 1;


        private bool HasReadHigh { get; set; } = false;
        private bool HasReadLow { get; set; } = false;


        /// <summary>
        /// Constructs a new binary reader with the given bit converter, reading to the given stream.
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        public BitReader(Stream stream, int bitCountPerSlice = 8, Endian endianness = Endian.Big, bool shouldDispose = false) : base(stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream isn't readable", nameof(stream));

            if (bitCountPerSlice % 8 != 0)
                throw new ArgumentException("BitReader does not support bit slices that are not 8 bit aligned");

            BitsPerSlice = bitCountPerSlice;
            BitBuffer = new byte[BytesPerSlice];
            Endianness = endianness;
            ShouldDispose = shouldDispose;
        }

        public ulong ReadUIntLow(int p_BitCount)
        {
            ulong Result = 0;
            for (var i = 0; i < p_BitCount; i++)
            {
                Result <<= 1;
                Result |= ReadLowBit() ? (ulong)1 : 0;
            }
            return Result;
        }

        public long ReadIntLow(int p_BitCount)
        {
            var s_Result = ReadUIntLow(p_BitCount);


            if ((s_Result & (ulong)1 << p_BitCount - 1) != 0)
                return (long)(s_Result | ulong.MaxValue << p_BitCount);

            return (long)s_Result;
        }

        public bool ReadLowBit()
        {
            UpdateBits();

            if (CurrentBitsLeft == 0)
                throw new Exception("Bit reader issue!");

            if (HasReadHigh)
                throw new Exception("Trying to read low bit after reading high bit. Will result in data loss. pLz fix!");
            HasReadLow = true;


            var s_BitMask = 1 << (CurrentBitsLeft - BitsPerSlice) % 8;

            bool Result = (BitBuffer[CurrentByteIndexLow] & s_BitMask) != 0;

            //m_BitBuffer[CurrentByteIndexLow] >>= 1;
            CurrentBitsLeft--;

            return Result;
        }

        public ulong ReadUIntHigh(int p_BitCount)
        {
            ulong Result = 0;
            for (var i = 0; i < p_BitCount; i++)
            {
                Result <<= 1;
                Result |= ReadHighBit() ? (ulong)1 : 0;
            }

            return Result;
        }

        public long ReadIntHigh(int p_BitCount)
        {
            var s_Result = ReadUIntHigh(p_BitCount);

            if ((s_Result & (ulong)1 << p_BitCount - 1) != 0)
                return (long)(s_Result | ulong.MaxValue << p_BitCount);

            return (long)s_Result;
        }

        public bool ReadHighBit()
        {
            UpdateBits();

            if (CurrentBitsLeft == 0)
                throw new Exception("Bit reader issue!");

            if (HasReadLow)
                throw new Exception("Trying to read high bit after reading low bit. Will result in data loss. pLz fix!");
            HasReadHigh = true;


            var s_BitMask = 1 << CurrentBitsLeft - 8 * CurrentByteIndexLow - 1;

            bool Result = (BitBuffer[CurrentByteIndexHigh] & s_BitMask) != 0;

            // not mask, so it would be disabled after this
            //m_BitBuffer[CurrentByteIndexHigh] &= (byte)~(s_BitMask);
            CurrentBitsLeft--;

            return Result;
        }

        private void UpdateBits()
        {
            CheckDisposed();

            if (CurrentBitsLeft == 0)
            {
                if (ReadInternal(BitBuffer, 0, BytesPerSlice) == 0)
                    throw new Exception("End of stream bitreader");

                if (BytesPerSlice > 1 && Endianness != Endian.Big)
                {
                    var s_TempBuffer = new byte[BytesPerSlice];

                    for (var i = 0; i < BytesPerSlice; i++)
                        s_TempBuffer[BytesPerSlice - i - 1] = BitBuffer[i];

                    s_TempBuffer.CopyTo(BitBuffer, 0);
                }

                CurrentBitsLeft = BitsPerSlice;
            }
        }

        /// <inheritdoc/>
        public new void Dispose()
        {
            base.Dispose();

            CheckDisposed();

            Disposed = true;

            if (ShouldDispose)
                BaseStream.Dispose();
        }

        public void Flush()
        {
            CheckDisposed();
            BaseStream.Flush();
        }

        public override int Read(byte[] p_Buffer, int p_Offset, int p_Count)
        {
            throw new NotImplementedException();
        }

        public void SetLength(long p_Value)
        {
            CheckDisposed();
            BaseStream.SetLength(p_Value);
        }

        /// <summary>
        /// Checks whether or not the reader has been disposed, throwing an exception if so.
        /// </summary>
        protected void CheckDisposed()
        {
            if (Disposed)
                throw new ObjectDisposedException("BitReader");
        }

        /// <summary>
        /// Read the specified number of bytes into the provided buffer, starting at the
        /// specified offset.
        /// </summary>
        /// <param name="p_Buffer">The buffer to read the data into</param>
        /// <param name="p_Offset">The offset to start writing the data at</param>
        /// <param name="p_Count">The number of bytes to read</param>
        /// <returns>The number of bytes read</returns>
        protected int ReadInternal(byte[] p_Buffer, int p_Offset, int p_Count)
        {
            CheckDisposed();
            return BaseStream.Read(p_Buffer, p_Offset, p_Count);
        }
    }
}