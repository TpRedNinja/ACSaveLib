"""AC4 Black Flag .save reader: XOR-decrypted, LZO-chunked, CRC32-keyed property stream."""
import struct, zlib, re
from lzallright import LZOCompressor
_lzo = LZOCompressor()
SIG = bytes.fromhex('33aafb5799fa0410')
PLAIN = bytes.fromhex('23000000acdbfe00') + b'\0' * 8
def crc(n): return zlib.crc32(n.encode())
K = {n: crc(n) for n in ['Mission','HighestSynchronization','TimePlayed','BriefingDisplayedOnce','BriefingAccepted',
     'MissionStepSaveData','MissionConstraintSaveData','Step','IsCheckpoint','Succeeded','ConstraintID',
     'PlayerSelected','IsRevealed','BestRunSucceeded','CurrentRunState','ConditionData','ConditionID','MetaValue']}
def decrypt(d):
    n = len(d); enc = d[552:568]
    # key index = (i - n) mod 16
    key = bytes(enc[j] ^ PLAIN[j] for j in range(16))
    kk = [key[(552 + j - n) % 16 and 0] for j in range(0)]  # placeholder
    # rotate so that key index (i-n)%16 maps to key bytes
    kb = [0] * 16
    for j in range(16): kb[(552 + j - n) % 16] = enc[j] ^ PLAIN[j]
    out = bytearray(d)
    for i in range(552, n): out[i] ^= kb[(i - n) % 16]
    return bytes(out)
def unpack(d):
    p = 576; segs = []
    while p < len(d):
        if d[p:p+8] == SIG:
            q = p + 15; blk = []
            while q + 13 <= len(d) and d[q] == 1:
                q += 1; cs, us, ck = struct.unpack_from('<III', d, q)
                u = _lzo.decompress(d[q+12:q+12+cs], us); assert len(u) == us
                blk.append(u); q += 12 + cs
            segs.append(b''.join(blk)); p = q
        else:
            n = d.find(SIG, p)
            if n < 0: break
            p = n
    return segs[-1]   # last container = full property stream
def _items(d, p, end):
    """yield (key, typ, value_bytes, L, pos) for typed items; arrays yield ('arr',...)"""
    while p + 4 <= end:
        L = struct.unpack_from('<I', d, p)[0]
        if L == 0: p += 1; continue
        if L < 13 or p + 4 + L > end: return
        b = d[p+4:p+4+L]; key = struct.unpack_from('<I', b, 0)[0]; typ = struct.unpack_from('<H', b, 10)[0]
        if typ in (0x0a1d, 0x0a9d) and b[8:10] == b'\0\0':
            cnt = struct.unpack_from('<I', b, 14)[0]; elems = []; q = p + 22; aend = p + 4 + L
            for _ in range(cnt):
                while q < aend and d[q] == 0: q += 1
                if q + 12 > aend: break
                k2, sz = struct.unpack_from('<II', d, q); L2 = struct.unpack_from('<I', d, q+8)[0]
                elems.append(list(_items(d, q+12, q+12+L2))); q += 8 + sz
            yield key, 'arr', elems, L, p; p = aend; continue
        if not (b[8:10] == b'\0\0' and b[12] == 0x0b and len(b) - 13 <= 12):
            yield key, 'raw', b, L, p; p += 4 + L; continue
        yield key, typ, b[13:], L, p; p += 4 + L
def _num(v, typ): return int.from_bytes(v[:8] if typ in (0x12, 0x09) else v, 'little')
def parse_missions(stream):
    res = []; kS = struct.pack('<I', K['HighestSynchronization'])
    for m in re.finditer(re.escape(kS), stream):
        p = m.start() - 4 - 25          # id item sits right before the sync item
        if p < 0: continue
        L = struct.unpack_from('<I', stream, p)[0]
        if L != 21 or struct.unpack_from('<H', stream, p+14)[0] != 0x12: continue
        items = list(_items(stream, p, min(len(stream), p + 20000)))
        ms = {'id': _num(items[0][2], 0x12), 'sync': 0, 'time': 0, 'briefing_displayed': False, 'briefing_accepted': False, 'steps': [], 'constraints': []}
        for key, typ, val, _, _ in items[1:]:
            if typ == 0x12 and _ != 21: break
            if key == K['HighestSynchronization']: ms['sync'] = _num(val, typ)
            elif key == K['TimePlayed']: ms['time'] = _num(val, typ)
            elif key == K['BriefingDisplayedOnce']: ms['briefing_displayed'] = bool(val[0])
            elif key == K['BriefingAccepted']: ms['briefing_accepted'] = bool(val[0])
            elif key == K['MissionStepSaveData'] and typ == 'arr':
                for e in val:
                    st = {}
                    for k2, t2, v2, _, _ in e:
                        if k2 == K['Step']: st['id'] = _num(v2, t2)
                        elif k2 == K['IsCheckpoint']: st['checkpoint'] = bool(v2[0])
                        elif k2 == K['Succeeded']: st['succeeded'] = bool(v2[0])
                    ms['steps'].append(st)
            elif key == K['MissionConstraintSaveData'] and typ == 'arr':
                for e in val:
                    c = {}
                    for k2, t2, v2, _, _ in e:
                        if k2 == K['ConstraintID']: c['id'] = _num(v2, t2)
                        elif k2 == K['PlayerSelected']: c['selected'] = bool(v2[0])
                        elif k2 == K['IsRevealed']: c['revealed'] = bool(v2[0])
                        elif k2 == K['BestRunSucceeded']: c['best_run_succeeded'] = bool(v2[0])
                        elif k2 == K['CurrentRunState']: c['state'] = struct.unpack_from('<I', v2, 0)[0]
                    ms['constraints'].append(c)
        ms['completed'] = ms['sync'] >= 100
        res.append(ms)
    return res
def load(path):
    d = open(path, 'rb').read()
    return parse_missions(unpack(decrypt(d)))

# ---------- hierarchical reader (preferred) ----------
def _hdr_obj(b, L):
    return L >= 39 and b[8:10] == b'\0\0' and b[12] == 0x0b and b[4:8] == b[27:31] and struct.unpack_from('<H', b, 10)[0] in (0x13, 0x16)
def parse_tree(d, p, end):
    """-> list of (key, kind, payload): kind 'val' (int, type), 'obj' (children), 'arr' (list of element-children lists), 'raw'"""
    out = []
    while p + 4 <= end:
        L = struct.unpack_from('<I', d, p)[0]
        if L == 0: p += 1; continue
        if L < 13 or p + 4 + L > end: break
        b = d[p+4:p+4+L]; key = struct.unpack_from('<I', b, 0)[0]; typ = struct.unpack_from('<H', b, 10)[0]
        if typ in (0x0a1d, 0x0a9d) and b[8:10] == b'\0\0':
            cnt = struct.unpack_from('<I', b, 14)[0]; q = p + 22; aend = p + 4 + L; elems = []
            for _ in range(cnt):
                while q < aend and d[q] == 0: q += 1
                if q + 12 > aend: break
                k2, sz = struct.unpack_from('<II', d, q); L2 = struct.unpack_from('<I', d, q+8)[0]
                elems.append((k2, parse_tree(d, q+12, q+12+L2))); q += 8 + sz
            out.append((key, 'arr', elems)); p = aend; continue
        if _hdr_obj(b, L):
            out.append((key, 'obj', parse_tree(d, p+4+39, p+4+L))); p += 4 + L; continue
        if b[8:10] == b'\0\0' and b[12] == 0x0b and L - 13 <= 12:
            out.append((key, 'val', (_num(b[13:], typ), typ, b[13:]))); p += 4 + L; continue
        out.append((key, 'raw', b)); p += 4 + L
    return out
def _g(children, name):
    k = K.get(name) or crc(name)
    return [c for c in children if c[0] == k]
def parse_history(children):
    """children of a MissionHistory object -> list of groups [{'id','missions':[mission dicts]}]"""
    groups = []
    for _, kind, elems in _g(children, 'GroupSaveData'):
        for _, g in elems:
            gid = next((c[2][0] for c in g if c[0] == crc('Group') and c[1] == 'val'), None)
            ms = []
            for _, kind2, melems in _g(g, 'MissionSaveData'):
                for _, mc in melems:
                    m = {'id': 0, 'sync': 0, 'time': 0, 'briefing_displayed': False, 'briefing_accepted': False, 'steps': [], 'constraints': []}
                    for k, kd, pl in mc:
                        if k == K['Mission']: m['id'] = pl[0]
                        elif k == K['HighestSynchronization']: m['sync'] = pl[0]
                        elif k == K['TimePlayed']: m['time'] = pl[0]
                        elif k == K['BriefingDisplayedOnce']: m['briefing_displayed'] = bool(pl[0])
                        elif k == K['BriefingAccepted']: m['briefing_accepted'] = bool(pl[0])
                        elif k == K['MissionStepSaveData']:
                            for _, e in pl:
                                s = {}
                                for k2, kd2, p2 in e:
                                    if k2 == K['Step']: s['id'] = p2[0]
                                    elif k2 == K['IsCheckpoint']: s['checkpoint'] = bool(p2[0])
                                    elif k2 == K['Succeeded']: s['succeeded'] = bool(p2[0])
                                m['steps'].append(s)
                        elif k == K['MissionConstraintSaveData']:
                            for _, e in pl:
                                c = {}
                                for k2, kd2, p2 in e:
                                    if k2 == K['ConstraintID']: c['id'] = p2[0]
                                    elif k2 == K['PlayerSelected']: c['selected'] = bool(p2[0])
                                    elif k2 == K['IsRevealed']: c['revealed'] = bool(p2[0])
                                    elif k2 == K['BestRunSucceeded']: c['best_run_succeeded'] = bool(p2[0])
                                    elif k2 == K['CurrentRunState']: c['state'] = struct.unpack_from('<I', p2[2], 0)[0]
                                m['constraints'].append(c)
                    m['completed'] = m['sync'] >= 100
                    ms.append(m)
            groups.append({'id': gid, 'missions': ms})
    return groups
def load_save(path):
    d = unpack(decrypt(open(path, 'rb').read()))
    top = parse_tree(d, 26, len(d))
    res = {'raw': top}
    for name in ('NarrativeMissionHistory', 'SideMissionHistory'):
        for k, kind, ch in top:
            if k == crc(name) and kind == 'obj': res[name] = parse_history(ch)
    for name in ('CurrentMission', 'World', 'PlayerPortal'):
        for k, kind, pl in top:
            if k == crc(name) and kind == 'val': res[name] = pl[0]
    return res
