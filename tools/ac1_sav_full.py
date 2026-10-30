"""AC1 .sav strict parser/writer. Layout confirmed against the DX9 binary (notes 10.1).
Usage: python ac1_sav_full.py file.sav   -> parses to EOF and checks a byte-exact round trip."""
import sys
SZ={0:1,1:1,2:1,3:1,4:2,5:2,6:4,7:4,8:8,9:8,10:4,11:8,12:12,13:16,14:16,15:36,16:64,17:4,18:4,20:4,21:4,
    25:4,26:4,27:4,28:4,30:257,31:257,32:514,33:1028,34:2056,35:1028,36:4100,37:1040,38:1040}
def parse(b):
    p=0
    def u(n):
        nonlocal p
        v=int.from_bytes(b[p:p+n],'big');p+=n;return v
    h={'size':u(4),'magic':u(4),'ver':u(2),'n':u(4)};objs=[]
    for _ in range(h['n']):
        o={'cls':u(4),'props':[]}
        for _ in range(u(4)):
            fl=u(1);c=u(4);o['props'].append((fl,[(u(4),u(2)) for _ in range(c)]))
        o['meta']=[(u(4),u(4)) for _ in range(u(4))];o['vsz']=u(4);o['vals']=[]
        for tw,key in o['meta']:
            t=(tw>>16)&0x3f
            if t in(29,24):
                et=(tw>>23)&0x3f;n=u(4);o['vals'].append(b[p:p+n*SZ[et]]);p+=n*SZ[et]
            elif t==23:
                et=(tw>>23)&0x3f;n=(tw&0x7fff)*SZ[et];o['vals'].append(b[p:p+n]);p+=n
            else:
                o['vals'].append(b[p:p+SZ[t]]);p+=SZ[t]
        objs.append(o)
    assert p==len(b)==h['size'],(p,len(b),h['size'])
    return h,objs
def write(h,objs):
    o=bytearray()
    def u(v,n):o.extend(v.to_bytes(n,'big'))
    u(0,4);u(h['magic'],4);u(h['ver'],2);u(len(objs),4)
    for ob in objs:
        u(ob['cls'],4);u(len(ob['props']),4)
        for fl,r in ob['props']:
            u(fl,1);u(len(r),4)
            for i,s in r:u(i,4);u(s,2)
        u(len(ob['meta']),4)
        for tw,k in ob['meta']:u(tw,4);u(k,4)
        u(ob['vsz'],4)
        for v,(tw,_) in zip(ob['vals'],ob['meta']):
            if ((tw>>16)&0x3f) in(29,24):u(len(v)//SZ[(tw>>23)&0x3f],4)
            o.extend(v)
    o[0:4]=len(o).to_bytes(4,'big');return bytes(o)
if __name__=='__main__':
    b=open(sys.argv[1],'rb').read();h,objs=parse(b)
    print(len(objs),'objects, round trip exact:',write(h,objs)==b)
