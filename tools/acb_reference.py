"""AC Brotherhood (and Revelations plain) .SAV reader: no encryption, LZO2A chunked containers, CRC32-keyed property stream.

Usage: python3 tools/acb_reference.py <save> [names.txt]
Prints the containers and the top-level items of the header and main property streams.
Reference only (the C# library does not read Brotherhood yet)."""
import struct, sys, zlib

SIG = bytes.fromhex('33aafb5799fa0410')


def lzo2a(src, start, length):
    """LZO2A decompressor (port of the game's codec 2 routine, ACRSP.exe sub_1C3E960).
    Returns (output bytes, input position after the end marker)."""
    ip = start
    dst = bytearray()
    bits = 0      # current flag byte(s), consumed from the low bit
    count = 0     # number of unread flag bits
    while True:
        while True:  # literals
            if count == 0:
                bits |= src[ip]; ip += 1; count = 8
            count -= 1
            if bits & 1:
                break
            dst.append(src[ip]); bits >>= 1; ip += 1
        nxt = bits >> 1
        if count == 0:
            nxt |= src[ip]; ip += 1; count = 8
        count -= 1
        if nxt & 1:  # long match
            b0, b1 = src[ip], src[ip + 1]; ip += 2
            bits = nxt >> 1
            dist = (b0 & 0x1f) | (b1 << 5)
            code = b0 >> 5
            m = len(dst) - dist
            if code:
                if dist == 0:
                    return bytes(dst), ip       # end marker
                n = code + 2
            else:
                n = 9
                while src[ip] == 0:
                    ip += 1; n += 255
                n += src[ip]; ip += 1
            for _ in range(n):
                dst.append(dst[m]); m += 1
        else:        # short match: 2 to 5 bytes, distance up to 256
            nxt >>= 1
            if count < 2:
                nxt |= src[ip] << count; ip += 1; count += 8
            n = (nxt & 3) + 2
            bits = nxt >> 2
            count -= 2
            m = len(dst) - src[ip] - 1; ip += 1
            for _ in range(n):
                dst.append(dst[m]); m += 1


def read_container(d, pos):
    """Container at pos (signature). Header: signature(8), u16 version, u8 codec id, u32 chunk size (7 bytes).
    Blocks: 01 | u32 compSize | u32 uncompSize | u32 checksum (not checked) | data."""
    codec = d[pos + 10]
    if codec != 2:
        raise ValueError('codec %d is not LZO2A' % codec)
    q = pos + 15
    out = bytearray()
    while q + 13 <= len(d) and d[q] == 1:
        cs, us, _ = struct.unpack_from('<III', d, q + 1)
        if cs == 0 or us == 0 or q + 13 + cs > len(d):
            break
        block, end = lzo2a(d, q + 13, cs)
        assert len(block) == us and end == q + 13 + cs, (len(block), us)
        out += block
        q += 13 + cs
    return bytes(out), q


def read_save(d, start=0):
    """All containers of the file, in order: [(position, uncompressed bytes)]."""
    res = []
    p = d.find(SIG, start)
    while p >= 0:
        data, q = read_container(d, p)
        res.append((p, data))
        p = d.find(SIG, q)
    return res


def top_level(stream, names):
    """Items at the top of a decoded container: 10 zero bytes, root key, 2 x u32 size, items from offset 22."""
    root = struct.unpack_from('<I', stream, 10)[0]
    items, pos = [], 22
    while pos + 4 <= len(stream):
        length = struct.unpack_from('<I', stream, pos)[0]
        if length < 13 or pos + 4 + length > len(stream):
            break
        key = struct.unpack_from('<I', stream, pos + 4)[0]
        typ = struct.unpack_from('<H', stream, pos + 14)[0]
        items.append((names.get(key, '%08x' % key), typ, length))
        pos += 4 + length
    return root, items


if __name__ == '__main__':
    data = open(sys.argv[1], 'rb').read()
    names = {}
    if len(sys.argv) > 2:
        for line in open(sys.argv[2], encoding='utf-8'):
            line = line.strip()
            if line:
                names[zlib.crc32(line.encode()) & 0xffffffff] = line
    for n, (pos, stream) in enumerate(read_save(data)):
        root, items = top_level(stream, names)
        print('container %d at 0x%x: %d bytes, root key %08x, %d top-level items' % (n, pos, len(stream), root, len(items)))
        for name, typ, length in items:
            print('   %-34s type 0x%x length %d' % (name, typ, length))
