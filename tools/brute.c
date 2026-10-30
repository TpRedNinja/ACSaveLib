#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
static uint32_t T[256];
static uint32_t upd(uint32_t c,const char*s){while(*s)c=T[(c^(uint8_t)*s++)&0xFF]^(c>>8);return c;}
int nt=0; uint32_t tg[4096];
char *W[2048]; int nw=0;
static void chk(uint32_t c,const char*b){c=~c;for(int i=0;i<nt;i++)if(tg[i]==c)printf("%08x %s\n",c,b);}
int main(int argc,char**argv){
 for(uint32_t i=0;i<256;i++){uint32_t c=i;for(int k=0;k<8;k++)c=(c&1)?0xEDB88320^(c>>1):c>>1;T[i]=c;}
 FILE*f=fopen(argv[1],"r");char l[64];while(fgets(l,64,f)){tg[nt++]=strtoul(l,0,16);}fclose(f);
 f=fopen(argv[2],"r");while(fgets(l,64,f)){l[strcspn(l,"\r\n")]=0;if(*l)W[nw++]=strdup(l);}fclose(f);
 int maxd=atoi(argv[3]); char buf[256];
 for(int a=0;a<nw;a++){uint32_t ca=upd(0xFFFFFFFF,W[a]);chk(ca,W[a]);
  if(maxd<2)continue;
  for(int b=0;b<nw;b++){uint32_t cb=upd(ca,W[b]);sprintf(buf,"%s%s",W[a],W[b]);chk(cb,buf);
   if(maxd<3)continue;
   for(int c=0;c<nw;c++){uint32_t cc=upd(cb,W[c]);
    if(maxd==3||1){sprintf(buf,"%s%s%s",W[a],W[b],W[c]);chk(cc,buf);}
    if(maxd<4)continue;
    for(int d=0;d<nw;d++){uint32_t cd=upd(cc,W[d]);uint32_t x=~cd;
      for(int i=0;i<nt;i++)if(tg[i]==x){printf("%08x %s%s%s%s\n",x,W[a],W[b],W[c],W[d]);}}}}}
 return 0;}
