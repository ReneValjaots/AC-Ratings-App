using System.IO;
using System.Text;

namespace Ac.Ratings.Services.Acd;

public class ReadAheadBinaryReader : IDisposable {
    protected const bool IsLittleEndian = true;

    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private int _left, _total;
    private long? _length;

    public long Length => _length ?? (_length = _stream.Length).Value;
    public Stream BaseStream => _baseSteam ?? (_baseSteam = new InnerStream(this));
    private Stream _baseSteam;

    public long Position {
        get => _stream.Position - _left;
        set => Seek(value, SeekOrigin.Begin);
    }

    private class InnerStream : Stream {
        private readonly ReadAheadBinaryReader _parent;

        public InnerStream(ReadAheadBinaryReader parent) {
            _parent = parent;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) {
            return _parent.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _parent.ReadBytes(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _parent.Length;

        public override long Position {
            get => _parent.Position;
            set => _parent.Seek(value, SeekOrigin.Begin);
        }
    }

    public byte ReadByte() {
        Require(1);
        return NextByte();
    }

    private byte NextByte() {
        return _buffer[_total - _left--];
    }

    public int ReadBytes(byte[] destination, int offset, int count) {
        if (_left >= count) {
            Array.Copy(_buffer, GetPosAndMove(count), destination, offset, count);
            return count;
        }

        var buffer = _left;
        Array.Copy(_buffer, _total - _left, destination, offset, buffer);
        _left = _total = 0;
        return buffer + _stream.Read(destination, offset + buffer, count - buffer);
    }


    public long Seek(long offset, SeekOrigin seekOrigin) {
        if (_left == 0) {
            // return _stream.Seek(offset, seekOrigin);
        }

        var current = Position;
        long target;

        switch (seekOrigin) {
            case SeekOrigin.Begin:
                target = offset;
                break;
            case SeekOrigin.End:
                target = Length + offset;
                break;
            case SeekOrigin.Current:
                target = current + offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(seekOrigin), seekOrigin, null);
        }

        var delta = target - current;
        if (delta == 0) return current;

        if (delta > 0) {
            if (delta <= _left) {
                _left -= (int)delta;
                return target;
            }
        }
        else {
            var leftBackwards = _total - _left;
            if (-delta <= leftBackwards) {
                _left -= (int)delta;
                return target;
            }
        }

        _left = 0;
        return _stream.Seek(target, SeekOrigin.Begin);
    }

    public void Dispose() {
        _stream.Dispose();
    }

    public int ReadInt32() {
        var pos = GetPosAndMove(4);
        return ToInt32(_buffer, pos);
    }

    protected static unsafe int ToInt32(byte[] value, int startIndex) {
        fixed (byte* pbyte = &value[startIndex]) {
            if (startIndex % 4 == 0) return *(int*)pbyte;
            return BitConverter.IsLittleEndian
                ? *pbyte | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24)
                : (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | *(pbyte + 3);
        }
    }

    private int GetPosAndMove(int count) {
        Require(count);
        var p = _total - _left;
        _left -= count;
        return p;
    }

    public void Skip(int count) {
        if (_left >= count) {
            GetPosAndMove(count);
        }
        else {
            _stream.Seek(Position + count, SeekOrigin.Begin);
            _left = 0;
        }
    }

    private void Require(int count) {
        if (_left >= count) {
            return;
        }

        if (_left > 0) {
            Array.Copy(_buffer, _total - _left, _buffer, 0, _left);
        }

        if (_left < 0) _left = 0;

        var leftToFill = _buffer.Length - _left;
        _left += _stream.Read(_buffer, _left, leftToFill);
        _total = _left;

        if (_left < count) throw new EndOfStreamException("Unexpected end");
    }

    public ReadAheadBinaryReader(string filename, int bufferSize = 2048) {
        _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8196);
        _buffer = new byte[bufferSize];
        _left = 0;
    }

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public string ReadString() {
        var length = ReadInt32();
        return length > _buffer.Length
            ? Encoding.GetString(ReadBytes(length), 0, length)
            : Encoding.GetString(_buffer, GetPosAndMove(length), length);
    }

    public byte[] ReadBytes(int count) {
        var result = new byte[count];
        ReadBytesTo(result, 0, count);
        return result;
    }

    public void ReadBytesTo(byte[] destination, int offset, int count) {
        if (_left >= count) {
            Array.Copy(_buffer, GetPosAndMove(count), destination, offset, count);
        }
        else {
            Array.Copy(_buffer, _total - _left, destination, offset, _left);
            offset += _left;
            count -= _left;

            var read = _stream.Read(destination, offset, count);
            if (read != count) throw new EndOfStreamException("Unexpected end");

            _left = _total = 0;
        }
    }


    public ReadAheadBinaryReader(Stream stream, int bufferSize = 2048) {
        _stream = stream;
        _buffer = new byte[bufferSize];
        _left = 0;
    }
}