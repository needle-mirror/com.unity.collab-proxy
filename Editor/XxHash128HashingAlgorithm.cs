using System;
using System.Buffers.Binary;
using System.IO;

using Unity.Collections;
using Unity.Mathematics;

using Codice.Utils;
using Codice.Utils.Buffers;

namespace Unity.PlasticSCM.Editor
{
    internal class XxHash128HashingAlgorithm : IHashingAlgorithm
    {
        public byte[] Hash => mHashValue;

        public XxHash128HashingAlgorithm(FlexibleBufferPool bufferPool)
        {
            mBufferPool = bufferPool;
        }

        public byte[] ComputeHash(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            return ComputeHash(buffer, 0, buffer.Length);
        }

        public unsafe byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            Initialize();

            if (count > 0)
            {
                fixed (byte* bufferPtr = &buffer[offset])
                {
                    mStreamingState.Update(bufferPtr, count);
                }
            }

            mHashValue = GetCurrentHashAndResetState(mStreamingState);
            return mHashValue;
        }

        public unsafe byte[] ComputeHash(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            Initialize();

            const int MB_4 = 4 * 1024 * 1024;

            byte[] buffer = mBufferPool.TakeBuffer(
                inputStream.Length > MB_4 ? MB_4 : (int)inputStream.Length, 0);
            try
            {
                fixed (byte* bufferPtr = buffer)
                {
                    int bytesRead;
                    while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        mStreamingState.Update(bufferPtr, bytesRead);
                    }
                }
            }
            finally
            {
                mBufferPool.ReturnBuffer(buffer);
            }

            mHashValue = GetCurrentHashAndResetState(mStreamingState);
            return mHashValue;
        }

        public unsafe int TransformBlock(
            byte[] inputBuffer,
            int inputOffset,
            int inputCount,
            byte[] outputBuffer,
            int outputOffset)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException(nameof(inputBuffer));

            if (inputOffset < 0 || inputOffset > inputBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(inputOffset));

            if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(inputCount));

            if (inputCount > 0)
            {
                fixed (byte* bufferPtr = &inputBuffer[inputOffset])
                {
                    mStreamingState.Update(bufferPtr, inputCount);
                }
            }

            // Copy input to output if provided (supports null and in-place)
            if (outputBuffer == null)
                return inputCount;

            if (outputOffset < 0 || outputOffset > outputBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(outputOffset));

            if (outputOffset + inputCount > outputBuffer.Length)
                throw new ArgumentException("Output buffer too small");

            if (inputCount > 0)
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);

            return inputCount;
        }

        public unsafe byte[] TransformFinalBlock(
            byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
                throw new ArgumentNullException(nameof(inputBuffer));

            if (inputOffset < 0 || inputOffset > inputBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(inputOffset));

            if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(inputCount));

            if (inputCount > 0)
            {
                fixed (byte* bufferPtr = &inputBuffer[inputOffset])
                {
                    mStreamingState.Update(bufferPtr, inputCount);
                }
            }

            mHashValue = GetCurrentHashAndResetState(mStreamingState);

            // Return copy of input (HashAlgorithm contract)
            byte[] result = new byte[inputCount];

            if (inputCount > 0)
                Buffer.BlockCopy(inputBuffer, inputOffset, result, 0, inputCount);

            return result;
        }

        public void Initialize()
        {
            mStreamingState.Reset(isHash64: false, seed: 0);
            mHashValue = Array.Empty<byte>();
        }

        public void Dispose()
        {
            // No need
        }

        static unsafe byte[] GetCurrentHashAndResetState(xxHash3.StreamingState streamingState)
        {
            uint4 hash = streamingState.DigestHash128();
            byte[] result = new byte[16];
            fixed (byte* resultPtr = result)
            {
                // Unity's uint4 lays down components in memory as [x, y, z, w] (lowest to highest address),
                // which is little-endian component order.
                // To match the conventional big-endian hash digest representation
                // (as used by System.IO.Hashing), reverse both the component order
                // (w first, x last) and the byte order within each component (via ReverseEndianness).
                ((uint*)resultPtr)[0] = BinaryPrimitives.ReverseEndianness(hash.w);
                ((uint*)resultPtr)[1] = BinaryPrimitives.ReverseEndianness(hash.z);
                ((uint*)resultPtr)[2] = BinaryPrimitives.ReverseEndianness(hash.y);
                ((uint*)resultPtr)[3] = BinaryPrimitives.ReverseEndianness(hash.x);
            }

            streamingState.Reset(isHash64: false, seed: 0);
            return result;
        }

        xxHash3.StreamingState mStreamingState = new(isHash64: false, seed: 0);
        byte[] mHashValue = Array.Empty<byte>();

        readonly FlexibleBufferPool mBufferPool;
    }

    internal class XxHash128HashingAlgorithmFactory : IXxHash128HashingAlgorithmFactory
    {
        public XxHash128HashingAlgorithmFactory(FlexibleBufferPool bufferPool)
        {
            mBufferPool = bufferPool;
        }

        public IHashingAlgorithm Build()
        {
            return new XxHash128HashingAlgorithm(mBufferPool);
        }

        readonly FlexibleBufferPool mBufferPool;
    }
}
