using System;

namespace SGF.NetworkRefactorV2
{
    public sealed class NetworkPacket
    {
        public ushort Command { get; }
        public byte SerializeType { get; }
        public byte[] Payload { get; }
        public bool IsRpc { get; }
        public int MessageCommand { get; }
        public ulong EntityId { get; }
        public NetworkPacket(ushort command, byte serializeType, byte[] payload) { Command = command; SerializeType = serializeType; Payload = payload; }
        public NetworkPacket(ushort command, byte serializeType, byte[] payload, int messageCommand, ulong entityId)
        {
            Command = command; SerializeType = serializeType; Payload = payload; IsRpc = true; MessageCommand = messageCommand; EntityId = entityId;
        }
    }

    public sealed class NetworkOutboundPacket
    {
        public int Command { get; }
        public byte[] Payload { get; }
        public bool Encrypt { get; }
        public NetworkOutboundPacket(int command, byte[] payload, bool encrypt) { Command = command; Payload = payload ?? throw new ArgumentNullException(nameof(payload)); Encrypt = encrypt; }
    }

    public static class PacketCodec
    {
        public const int HeaderSize = 6;
        private static readonly byte[] Key = { 253, 1, 56, 52, 62, 176, 42, 138 };

        public static byte[] Encode(int command, byte[] payload, bool encrypt)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if ((uint)command > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(command));
            byte[] body = (byte[])payload.Clone();
            if (encrypt) Transform(body, true);
            int wireLength = body.Length + 2;
            if (wireLength > 0xFFFFFF) throw new ArgumentOutOfRangeException(nameof(payload));
            var result = new byte[body.Length + HeaderSize];
            result[0] = (byte)wireLength; result[1] = (byte)(wireLength >> 8); result[2] = (byte)(wireLength >> 16);
            result[3] = encrypt ? (byte)2 : (byte)0; result[4] = (byte)command; result[5] = (byte)(command >> 8);
            Buffer.BlockCopy(body, 0, result, HeaderSize, body.Length);
            return result;
        }

        internal static void Decrypt(byte[] data) { Transform(data, false); }
        private static void Transform(byte[] data, bool encrypt)
        {
            for (int i = 0; i < data.Length; i++)
            {
                int shift = i % 7 + 1;
                if (encrypt) data[i] = (byte)(((data[i] << shift) | (data[i] >> (8 - shift))) ^ Key[i % Key.Length]);
                else { int value = data[i] ^ Key[i % Key.Length]; data[i] = (byte)((value >> shift) | (value << (8 - shift))); }
            }
        }
    }

    public sealed class PacketReader
    {
        private readonly int _maxPacketSize;
        private byte[] _buffer;
        private int _offset;
        private int _count;
        public PacketReader(int maxPacketSize)
        {
            if (maxPacketSize < PacketCodec.HeaderSize) throw new ArgumentOutOfRangeException(nameof(maxPacketSize));
            _maxPacketSize = maxPacketSize; _buffer = new byte[Math.Min(8192, maxPacketSize)];
        }

        public void Append(byte[] data, int offset, int count)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0 || count < 0 || offset + count > data.Length) throw new ArgumentOutOfRangeException();
            EnsureCapacity(count);
            Buffer.BlockCopy(data, offset, _buffer, _offset + _count, count);
            _count += count;
            ValidateHeader();
        }

        public bool TryRead(out NetworkPacket packet)
        {
            packet = null; ValidateHeader(); if (_count < PacketCodec.HeaderSize) return false;
            int length = ReadPacketLength();
            if (_count < length) return false;
            byte serializeType = _buffer[_offset + 3]; ushort command = (ushort)(_buffer[_offset + 4] | _buffer[_offset + 5] << 8);
            byte[] payload = new byte[length - PacketCodec.HeaderSize];
            Buffer.BlockCopy(_buffer, _offset + PacketCodec.HeaderSize, payload, 0, payload.Length);
            _offset += length; _count -= length; CompactIfEmpty();
            if ((serializeType & 2) != 0) PacketCodec.Decrypt(payload);
            packet = new NetworkPacket(command, serializeType, payload); return true;
        }

        private void ValidateHeader()
        {
            if (_count < PacketCodec.HeaderSize) return;
            int length = ReadPacketLength();
            if (length < PacketCodec.HeaderSize || length > _maxPacketSize) throw new InvalidOperationException("Invalid packet length.");
        }

        private int ReadPacketLength() { return (_buffer[_offset] | _buffer[_offset + 1] << 8 | _buffer[_offset + 2] << 16) + 4; }
        private void EnsureCapacity(int incoming)
        {
            if (_offset + _count + incoming <= _buffer.Length) return;
            if (_count + incoming > _maxPacketSize) throw new InvalidOperationException("Invalid packet length.");
            if (_offset > 0) { Buffer.BlockCopy(_buffer, _offset, _buffer, 0, _count); _offset = 0; }
            if (_count + incoming <= _buffer.Length) return;
            int size = _buffer.Length;
            while (size < _count + incoming) size *= 2;
            if (size > _maxPacketSize) size = _maxPacketSize;
            Array.Resize(ref _buffer, size);
        }
        private void CompactIfEmpty() { if (_count == 0) _offset = 0; }
    }

    public static class RpcCodec
    {
        public static byte[] Encode(int command, byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if ((uint)command > ushort.MaxValue || payload.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException();
            var result = new byte[payload.Length + 5]; result[0] = 14; result[1] = (byte)command; result[2] = (byte)(command >> 8);
            result[3] = (byte)payload.Length; result[4] = (byte)(payload.Length >> 8); Buffer.BlockCopy(payload, 0, result, 5, payload.Length); return result;
        }
    }
}
