using System.IO;

namespace Ac.Ratings.Services.Acd {
    internal static class Lzf {
        private static readonly long[] HashTable = new long[Hsize];

        private const uint Hlog = 14;
        private const uint Hsize = 1 << 14;
        private const uint MaxLit = 1 << 5;
        private const uint MaxOff = 1 << 13;
        private const uint MaxRef = (1 << 8) + (1 << 3);

        public static int Compress(byte[] input, int inputLength, byte[] output, int outputLength) {
            lock (HashTable) {
                Array.Clear(HashTable, 0, (int)Hsize);

                uint iidx = 0, oidx = 0, hval = (uint)((input[iidx] << 8) | input[iidx + 1]);
                var lit = 0;

                for (;;) {
                    if (iidx < inputLength - 2) {
                        hval = (hval << 8) | input[iidx + 2];
                        long hslot = (hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1);
                        var reference = HashTable[hslot];
                        HashTable[hslot] = iidx;

                        long off;
                        if ((off = iidx - reference - 1) < MaxOff && iidx + 4 < inputLength && reference > 0 && input[reference + 0] == input[iidx + 0]
                            && input[reference + 1] == input[iidx + 1] && input[reference + 2] == input[iidx + 2]) {
                            /* match found at *reference++ */
                            uint len = 2;
                            var maxlen = (uint)inputLength - iidx - len;
                            maxlen = maxlen > MaxRef ? MaxRef : maxlen;

                            if (oidx + lit + 1 + 3 >= outputLength) return 0;

                            do {
                                len++;
                            } while (len < maxlen && input[reference + len] == input[iidx + len]);

                            if (lit != 0) {
                                output[oidx++] = (byte)(lit - 1);
                                lit = -lit;
                                do {
                                    output[oidx++] = input[iidx + lit];
                                } while (++lit != 0);
                            }

                            len -= 2;
                            iidx++;

                            if (len < 7) {
                                output[oidx++] = (byte)((off >> 8) + (len << 5));
                            }
                            else {
                                output[oidx++] = (byte)((off >> 8) + (7 << 5));
                                output[oidx++] = (byte)(len - 7);
                            }

                            output[oidx++] = (byte)off;

                            iidx += len - 1;
                            hval = (uint)((input[iidx] << 8) | input[iidx + 1]);

                            hval = (hval << 8) | input[iidx + 2];
                            HashTable[(hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1)] = iidx;
                            iidx++;

                            hval = (hval << 8) | input[iidx + 2];
                            HashTable[(hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1)] = iidx;
                            iidx++;
                            continue;
                        }
                    }
                    else if (iidx == inputLength) break;

                    /* one more literal byte we must copy */
                    lit++;
                    iidx++;

                    if (lit == MaxLit) {
                        if (oidx + 1 + MaxLit >= outputLength) return 0;

                        output[oidx++] = (byte)(MaxLit - 1);
                        lit = -lit;
                        do {
                            output[oidx++] = input[iidx + lit];
                        } while (++lit != 0);
                    }
                }

                if (lit != 0) {
                    if (oidx + lit + 1 >= outputLength) return 0;
                    output[oidx++] = (byte)(lit - 1);
                    lit = -lit;
                    do {
                        output[oidx++] = input[iidx + lit];
                    } while (++lit != 0);
                }

                return (int)oidx;
            }
        }

        public static void Compress(byte[] input, int inputLength, Stream output) {
            lock (HashTable) {
                Array.Clear(HashTable, 0, (int)Hsize);

                uint iidx = 0, hval = (uint)((input[iidx] << 8) | input[iidx + 1]);
                var lit = 0;

                for (;;) {
                    if (iidx < inputLength - 2) {
                        hval = (hval << 8) | input[iidx + 2];
                        long hslot = (hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1);
                        var reference = HashTable[hslot];
                        HashTable[hslot] = iidx;

                        long off;
                        if ((off = iidx - reference - 1) < MaxOff && iidx + 4 < inputLength && reference > 0 && input[reference + 0] == input[iidx + 0]
                            && input[reference + 1] == input[iidx + 1] && input[reference + 2] == input[iidx + 2]) {
                            /* match found at *reference++ */
                            uint len = 2;
                            var maxlen = (uint)inputLength - iidx - len;
                            maxlen = maxlen > MaxRef ? MaxRef : maxlen;

                            do {
                                len++;
                            } while (len < maxlen && input[reference + len] == input[iidx + len]);

                            if (lit != 0) {
                                output.WriteByte((byte)(lit - 1));
                                lit = -lit;
                                do {
                                    output.WriteByte(input[iidx + lit]);
                                } while (++lit != 0);
                            }

                            len -= 2;
                            iidx++;

                            if (len < 7) {
                                output.WriteByte((byte)((off >> 8) + (len << 5)));
                            }
                            else {
                                output.WriteByte((byte)((off >> 8) + (7 << 5)));
                                output.WriteByte((byte)(len - 7));
                            }

                            output.WriteByte((byte)off);

                            iidx += len - 1;
                            hval = (uint)((input[iidx] << 8) | input[iidx + 1]);

                            hval = (hval << 8) | input[iidx + 2];
                            HashTable[(hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1)] = iidx;
                            iidx++;

                            hval = (hval << 8) | input[iidx + 2];
                            HashTable[(hval ^ (hval << 5)) >> (int)(3 * 8 - Hlog - hval * 5) & (Hsize - 1)] = iidx;
                            iidx++;
                            continue;
                        }
                    }
                    else if (iidx == inputLength) break;

                    /* one more literal byte we must copy */
                    lit++;
                    iidx++;

                    if (lit == MaxLit) {
                        output.WriteByte((byte)(MaxLit - 1));
                        lit = -lit;
                        do {
                            output.WriteByte(input[iidx + lit]);
                        } while (++lit != 0);
                    }
                }

                if (lit == 0) return;

                output.WriteByte((byte)(lit - 1));
                lit = -lit;
                do {
                    output.WriteByte(input[iidx + lit]);
                } while (++lit != 0);
            }
        }

        public static byte[] Compress(byte[] input) {
            if (input.Length == 0) return new byte[0];

            using (var stream = new MemoryStream(input.Length)) {
                Compress(input, input.Length, stream);
                return stream.ToArray();
            }
        }

        public static int Decompress(byte[] input, int inputOffset, int inputLength, byte[] output, int outputLength) {
            var inputEnd = inputLength + inputOffset;
            var iidx = (uint)inputOffset;
            uint oidx = 0;

            do {
                uint ctrl = input[iidx++];

                if (ctrl < 1 << 5) {
                    /* literal run */
                    ctrl++;

                    //SET_ERRNO (E2BIG);
                    if (oidx + ctrl > outputLength) return 0;
                    do {
                        output[oidx++] = input[iidx++];
                    } while (--ctrl != 0);
                }
                else {
                    /* back reference */
                    var len = ctrl >> 5;
                    var reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);
                    if (len == 7) len += input[iidx++];
                    reference -= input[iidx++];

                    //SET_ERRNO (E2BIG);
                    if (oidx + len + 2 > outputLength) return 0;

                    //SET_ERRNO (EINVAL);
                    if (reference < 0) return 0;

                    output[oidx++] = output[reference++];
                    output[oidx++] = output[reference++];

                    do {
                        output[oidx++] = output[reference++];
                    } while (--len != 0);
                }
            } while (iidx < inputEnd);

            return (int)oidx;
        }

        public static byte[] Decompress(byte[] input, int offset, int count) {
            if (input.Length == 0) return Array.Empty<byte>();

            var result = new List<byte>(count * 2);
            var end = offset + count;

            do {
                uint ctrl = input[offset++];
                if (ctrl < 1 << 5) {
                    /* literal run */
                    ctrl++;

                    do {
                        result.Add(input[offset++]);
                    } while (--ctrl != 0);
                }
                else {
                    /* back reference */
                    var len = ctrl >> 5;
                    var reference = (int)(result.Count - ((ctrl & 0x1f) << 8) - 1);
                    if (len == 7) len += input[offset++];
                    reference -= input[offset++];

                    if (reference < 0) throw new Exception("Negative reference");

                    result.Add(result[reference++]);
                    result.Add(result[reference++]);

                    do {
                        result.Add(result[reference++]);
                    } while (--len != 0);
                }
            } while (offset < end);

            return result.ToArray();
        }
    }
}
