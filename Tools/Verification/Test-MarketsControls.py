"""Real native Markets touches shared by gesture and activity verification.

All coordinates come from currently visible inspect.controls Button hitboxes.
No method invokes a button callback or bypasses pointer event routing.
"""
import time
from shared_garden_runtime import read,wait,require


class MarketsTouches:
    def __init__(self,client):
        self.client=client
        self.serial=70

    def control(self,name):
        return next((c for c in self.client.input('inspect')['controls'] if c['name']==name and c['enabled']),None)

    def center(self,name):
        self.ensure(name)
        control=wait(lambda:self.control(name),'visible enabled '+name,20)
        r=control['bounds']
        require(r['width']>0 and r['height']>0,'Empty native hitbox '+name)
        return r['x']+r['width']/2,r['y']+r['height']/2

    def ensure(self,name):
        """Open the actual pictured drawer when its requested child is hidden."""
        makeover=('Markets Body','Markets Mane','Markets Saddle','Markets Tail','Markets rotate makeover left','Markets rotate makeover right','Markets close makeover')
        if name not in makeover and self.control('Markets close makeover'):
            self.client.input('touchButton',text='Markets close makeover')
            wait(lambda:not self.control('Markets close makeover'),'closed prior makeover drawer',10)
        if self.control(name):return
        opener=('Markets open places' if name.startswith('Markets visit ') else
                'Markets open family tools' if name in ('Markets tool Put down','Markets tool Give','Markets tool Leave','Markets tool More items') else
                'Markets open makeover' if name in ('Markets Body','Markets Mane','Markets Saddle','Markets Tail') else
                'Markets open songs' if name.startswith('Markets Song ') else None)
        if opener:
            wait(lambda:self.control(opener),'drawer opener '+opener,20)
            self.client.input('touchButton',text=opener)
            wait(lambda:self.control(name),'opened drawer target '+name,20)

    def receipt_files(self):
        return set(self.client.out.glob('reply-*.json'))

    def receipt(self,before,label):
        reply=wait(lambda:next((v for p in self.client.out.glob('reply-*.json') if p not in before and (v:=read(p)) and v['outcome']!='stale-revision'),None),'acknowledged '+label,25)
        require(reply['accepted'],label+': '+reply['outcome'])
        return reply

    def tap(self,name,acknowledge=True):
        self.center(name)
        before=self.receipt_files()
        self.client.input('touchButton',text=name)
        return self.receipt(before,name) if acknowledge else None

    def drag(self,source,target,acknowledge=True,steps=8):
        """Press a pictured source, cross native drag threshold, release on target."""
        start=self.center(source);end=self.center(target)
        require(start!=end,'Drag source and target coincide')
        before=self.receipt_files();self.serial+=1;finger=self.serial
        self.client.input('touch-begin',role='screen',x=start[0],y=start[1],finger=finger)
        try:
            time.sleep(.1)
            for i in range(1,steps+1):
                t=i/steps
                self.client.input('touch-move',role='screen',x=start[0]+(end[0]-start[0])*t,y=start[1]+(end[1]-start[1])*t,finger=finger)
                time.sleep(.045)
        finally:
            self.client.input('touch-end',role='screen',x=end[0],y=end[1],finger=finger)
        return self.receipt(before,source+' -> '+target) if acknowledge else None

    def swipe(self,name,acknowledge=True):
        """Sweep across one inspected instrument control without clicking it."""
        control=wait(lambda:self.control(name),'visible instrument '+name,20);r=control['bounds']
        start=(r['x']+r['width']*.2,r['y']+r['height']*.5)
        end=(r['x']+r['width']*.8,r['y']+r['height']*.5)
        before=self.receipt_files();self.serial+=1;finger=self.serial
        self.client.input('touch-begin',role='screen',x=start[0],y=start[1],finger=finger)
        try:
            for i in range(1,9):
                self.client.input('touch-move',role='screen',x=start[0]+(end[0]-start[0])*i/8,y=start[1],finger=finger)
                time.sleep(.045)
        finally:
            self.client.input('touch-end',role='screen',x=end[0],y=end[1],finger=finger)
        return self.receipt(before,name+' swipe') if acknowledge else None
