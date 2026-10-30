import re,struct,glob,sys
src=open('/mnt/user-data/attachments/db19104a-d315-54a9-99cf-21b89f9d650b/779bafcd-AC4BFSP_2_.asl',errors='ignore').read()
IDS={}
for m in re.finditer(r'\{(0x[0-9A-Fa-f]+),\s*"([^"]+)"\}',src):
    IDS[int(m.group(1),16)]=m.group(2)
if __name__=='__main__':
    print(len(IDS))
    for f in sorted(glob.glob('out/*.dec')):
        d=open(f,'rb').read()
        print(f,len(d))
        for i,n in IDS.items():
            b5=struct.pack('<Q',i)[:5]
            hits=[m.start() for m in re.finditer(re.escape(b5),d)]
            b4=struct.pack('<Q',i)[:4]
            h4=[m.start() for m in re.finditer(re.escape(b4),d)]
            if hits or h4: print('  %x %-30s 5B:%s 4B:%d'%(i,n,[hex(h) for h in hits][:6],len(h4)))
