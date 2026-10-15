using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private static void ValidateBirthdayParty(BirthdayState b)
        {
            if(b.giftId==null || b.giftActor==null || !KeepyRules.Finite(b.giftAt) || b.giftAt< -1 || b.giftAt>b.clock || b.giftCursor<0 ||
                b.stickerX==null || b.stickerY==null || b.stickerX.Length!=b.stickers.Length || b.stickerY.Length!=b.stickers.Length ||
                b.stickers.Any(k=>k>25) || b.stickerX.Any(v=>!KeepyRules.Finite(v) || v<0 || v>1) || b.stickerY.Any(v=>!KeepyRules.Finite(v) || v<0 || v>1) ||
                b.props.Any(p=>p.content<0 || p.content>=HulkBirthday.Contents.Length) ||
                b.giftAt>=0 && !b.props.Any(p=>p.id==b.giftId && p.location=="unwrapping" && p.holder==b.giftActor && !p.opened) ||
                b.props.Any(p=>p.location=="unwrapping" && (b.giftAt<0 || p.id!=b.giftId)))throw new InvalidOperationException("Invalid shared birthday activity.");
        }
        private static void CancelBirthdayUnwrap(BirthdayState b)
        {
            if(b.giftAt<0)return;
            var prop=b.props.FirstOrDefault(p=>p.id==b.giftId);
            if(prop!=null){prop.location=prop.holder==""?"ground":"held";prop.version++;}
            var member=b.members.FirstOrDefault(m=>m.actor==b.giftActor);
            if(member!=null && member.station=="unwrap")member.station=member.seat>=0?"sit":"";
            b.giftAt=-1;b.giftId="";b.giftActor="";
        }
        private bool BirthdayPartyOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            var b=state.birthday;outcome=null;
            if(c.value=="leave-queue"){LeaveBirthdayVendor(player.id);return true;}
            if(c.value=="queue"){
                if(b.vendorQueue.Contains(player.id))return true;
                if(held!=null || member.station!=""){outcome="hands-busy";return true;}
                if(b.vendorQueue.Length>=4){outcome="queue-is-full";return true;}
                // Arrival and reservation are separate. Another family member
                // may take the predicted spot while this request is in flight;
                // reaching the shared queue area is enough to reserve a place.
                bool nearby=Enumerable.Range(0,4).Any(i=>Math.Abs(player.x-HulkBirthday.QueueX[i])<=InteractionRadius && Math.Abs(player.y-HulkBirthday.QueueY[i])<=InteractionRadius);
                if(!nearby){outcome="come-to-the-squirrel-stall";return true;}
                b.vendorQueue=b.vendorQueue.Append(player.id).ToArray();member.station="queue";BirthdayPose(member,"Idle");PositionBirthdayQueue();return true;
            }
            if((member.station=="unwrap" || member.station=="eat" || member.station=="drink") && !(c.value=="station" && c.target=="")){outcome="finish-or-walk-away";return true;}
            if(c.value=="wrap"){
                if(held==null || held.kind!="gift" || !held.opened){outcome="hold-an-opened-present";return true;}
                // Explicit family replay control: reuse this bounded object,
                // rather than accumulating unlimited loose gift contents.
                held.opened=false;held.content=0;held.version++;return true;
            }
            if(c.value=="unwrap"){
                var hulk=b.cast[0];string recipient=hulk.controller==""?hulk.id:hulk.controller;
                var recipientPlayer=state.players.FirstOrDefault(p=>p.id==recipient);
                float hx=recipientPlayer?.x??hulk.x,hy=recipientPlayer?.y??hulk.y;
                if(held==null || held.kind!="gift" || held.opened || b.giftAt>=0){outcome="bring-one-wrapped-present";return true;}
                if(Math.Abs(player.x-hx)>InteractionRadius || Math.Abs(player.y-hy)>InteractionRadius){outcome="bring-present-to-hulk";return true;}
                if(b.props.Any(p=>p.holder==recipient && p!=held)){outcome="hulks-hands-are-busy";return true;}
                var hulkMember=b.members.FirstOrDefault(m=>m.actor==recipient);
                if(hulkMember!=null && hulkMember.station!="" && hulkMember.station!="sit" && hulkMember.station!="sleep"){outcome="hulk-is-playing";return true;}
                held.holder=recipient;held.location="unwrapping";held.version++;b.giftAt=b.clock;b.giftActor=recipient;b.giftId=held.id;
                if(hulkMember!=null){hulkMember.station="unwrap";BirthdayPose(hulkMember,"Unwrap");}
                return true;
            }
            if(c.value=="eat" || c.value=="drink"){
                bool edible=held!=null && (c.value=="drink"?held.kind=="drink":held.kind=="food" || held.kind=="gift" && held.opened && held.content==0);
                if(!edible){outcome="hold-a-snack-first";return true;}
                member.station=c.value;BirthdayPose(member,c.value);return true;
            }
            if(c.value=="serve" || c.value=="pour"){
                if(!HulkBirthday.NearCake(player.x,player.y,InteractionRadius)){outcome="come-to-the-cake-table";return true;}
                if(held!=null || member.station!=""){outcome="hands-busy";return true;}
                if(c.value=="serve" && !b.cakeReady){outcome="decorate-the-cake-first";return true;}
                var portion=b.props.FirstOrDefault(p=>p.kind==(c.value=="serve"?"food":"drink") && p.location=="stock" && p.holder=="");
                if(portion==null){outcome="all-four-portions-are-out";return true;}
                portion.holder=player.id;portion.location="held";portion.style=b.cakeBase;portion.version++;return true;
            }
            if(c.value.StartsWith("cake-",StringComparison.Ordinal)){
                if(!HulkBirthday.NearCake(player.x,player.y,InteractionRadius)){outcome="come-to-the-cake-table";return true;}
                if(held!=null || member.station!=""){outcome="hands-busy";return true;}
                if(c.value=="cake-base"){
                    if(!int.TryParse(c.item,out var choice) || choice<0 || choice>2){outcome="choose-a-cake-base";return true;}
                    b.cakeBase=choice;b.cakeReady=false;return true;
                }
                if(c.value=="cake-sticker"){
                    if(!int.TryParse(c.item,out var choice) || choice<0 || choice>25 || !KeepyRules.Finite(c.x) || !KeepyRules.Finite(c.y) || c.x<.12f || c.x>.88f || c.y<.22f || c.y>.78f){outcome="put-decoration-on-the-cake";return true;}
                    if(b.stickers.Length>=32){outcome="cake-is-full";return true;}
                    b.stickers=b.stickers.Append(choice).ToArray();b.stickerX=b.stickerX.Append(c.x).ToArray();b.stickerY=b.stickerY.Append(c.y).ToArray();b.cakeReady=false;return true;
                }
                if(c.value=="cake-undo"){
                    int count=Math.Max(0,b.stickers.Length-1);b.stickers=b.stickers.Take(count).ToArray();b.stickerX=b.stickerX.Take(count).ToArray();b.stickerY=b.stickerY.Take(count).ToArray();b.cakeReady=false;return true;
                }
                if(c.value=="cake-clear"){b.stickers=Array.Empty<int>();b.stickerX=Array.Empty<float>();b.stickerY=Array.Empty<float>();b.cakeReady=false;return true;}
                if(c.value=="cake-finish"){
                    if(b.stickers.Length==0){outcome="add-a-decoration";return true;}b.cakeReady=true;return true;
                }
                if(c.value=="cake-celebrate"){
                    if(!b.cakeReady){outcome="finish-the-cake-first";return true;}
                    b.celebrations++;b.reaction="cake-happy";b.reactionAt=b.clock;return true;
                }
                outcome="choose-a-cake-action";return true;
            }
            return false;
        }
        private bool AdvanceBirthdayParty()
        {
            var b=state.birthday;bool changed=false;
            if(b.giftAt>=0 && b.clock-b.giftAt>=3){
                var gift=b.props.Single(p=>p.id==b.giftId);
                if(gift.fake){
                    CancelBirthdayUnwrap(b);gift.opened=false;gift.holder="";gift.location="stock";gift.fakeAt=-1;gift.version++;
                    b.reaction="negative";b.reactionAt=b.clock;changed=true;
                }else {
                int choice=gift.fake?4:b.giftCursor++%HulkBirthday.Contents.Length;
                // The source limits cat/pug gifts to one each. Preserve that
                // object budget across repeated wrapping and shared sessions.
                if(choice>=6 && b.props.Any(p=>p!=gift && p.opened && p.content==choice))choice=0;
                gift.opened=true;gift.content=choice;gift.version++;
                b.reaction=choice==2 || choice==3 || choice==4?"negative":"happy";b.reactionAt=b.clock;
                CancelBirthdayUnwrap(b);changed=true;
                }
            }
            foreach(var member in b.members.Where(m=>m.station=="eat" || m.station=="drink")){
                if(b.clock-member.actionAt<2.5)continue;
                var food=b.props.SingleOrDefault(p=>p.holder==member.actor);
                if(food!=null){food.holder="";food.opened=false;food.location="stock";food.version++;}
                member.station=member.seat>=0?"sit":"";BirthdayPose(member,"Happy");changed=true;
            }
            return changed;
        }
        private static void ValidateBirthdayVendor(SoloSnapshot snapshot)
        {
            var b=snapshot.birthday;
            if(b.vendorQueue==null || b.vendorQueue.Length>4 || b.vendorQueue.Distinct().Count()!=b.vendorQueue.Length || b.vendorActor==null || b.vendorItem==null || b.vendorCount<0 ||
                new[]{b.vendorAt,b.vendorLast}.Any(t=>!KeepyRules.Finite(t) || t< -1 || t>b.clock) ||
                b.vendorQueue.Any(id=>!b.members.Any(m=>m.actor==id && m.station=="queue") || !snapshot.players.Any(p=>p.id==id && p.zone==HulkBirthday.Zone)) ||
                b.members.Any(m=>m.station=="queue" && !b.vendorQueue.Contains(m.actor)) ||
                b.vendorAt>=0 && (b.vendorQueue.Length==0 || b.vendorQueue[0]!=b.vendorActor || !b.props.Any(p=>p.id==b.vendorItem && p.kind=="food" && p.holder=="" && p.location=="counter")) ||
                b.props.Count(p=>p.location=="counter")!=(b.vendorAt>=0?1:0))throw new InvalidOperationException("Invalid birthday vendor queue.");
        }
        private static void ResetBirthdayVendor(BirthdayState b)
        {
            foreach(var prop in b.props.Where(p=>p.location=="counter")){prop.location="stock";prop.version++;}
            b.vendorQueue=Array.Empty<string>();b.vendorAt=-1;b.vendorActor="";b.vendorItem="";
        }
        private void PositionBirthdayQueue()
        {
            var b=state.birthday;
            for(int i=0;i<b.vendorQueue.Length;i++){
                var player=state.players.Single(p=>p.id==b.vendorQueue[i]);player.x=HulkBirthday.QueueX[i];player.y=HulkBirthday.QueueY[i];
            }
        }
        private void LeaveBirthdayVendor(string actor)
        {
            var b=state.birthday;if(!b.vendorQueue.Contains(actor))return;
            if(b.vendorActor==actor){
                var reserved=b.props.FirstOrDefault(p=>p.id==b.vendorItem);
                if(reserved!=null && reserved.location=="counter"){reserved.location="stock";reserved.version++;}
                b.vendorAt=-1;b.vendorActor="";b.vendorItem="";
            }
            b.vendorQueue=b.vendorQueue.Where(id=>id!=actor).ToArray();var member=b.members.Single(m=>m.actor==actor);member.station=member.seat>=0?"sit":"";BirthdayPose(member,"Idle");PositionBirthdayQueue();
        }
        private bool AdvanceBirthdayVendor(double seconds,string[] active)
        {
            var b=state.birthday;bool changed=false;
            foreach(var id in b.vendorQueue.ToArray())if(!state.players.Any(p=>p.id==id && p.zone==HulkBirthday.Zone) || active!=null && !active.Contains(id)){LeaveBirthdayVendor(id);changed=true;}
            if(b.vendorAt>=0 && b.clock-b.vendorAt>=HulkBirthday.VendorServeSeconds){
                var food=b.props.Single(p=>p.id==b.vendorItem);food.holder=b.vendorActor;food.location="held";food.version++;
                string done=b.vendorActor;b.vendorAt=-1;b.vendorActor="";b.vendorItem="";b.vendorLast=b.clock;b.vendorCount++;
                LeaveBirthdayVendor(done);
                var member=b.members.Single(m=>m.actor==done);int place=Array.IndexOf(b.members,member);
                member.exitX=4567+place%2*100;member.exitY=35+place/2*110;member.station="vendor-exit";member.actionAt=b.clock;changed=true;
            }
            if(b.vendorAt<0 && b.vendorQueue.Length>0 && (b.vendorLast<0 || b.clock-b.vendorLast>=.5)){
                var food=b.props.FirstOrDefault(p=>p.kind=="food" && p.location=="stock" && p.holder=="");
                if(food!=null){
                    food.style=b.vendorCount%2;food.location="counter";food.x=4499;food.y=127;food.version++;
                    b.vendorItem=food.id;b.vendorActor=b.vendorQueue[0];b.vendorAt=b.clock;changed=true;
                }
            }
            foreach(var member in b.members.Where(m=>m.station=="vendor-exit")){
                if(b.clock-member.actionAt<1d/3)continue;
                var player=state.players.Single(p=>p.id==member.actor);
                double dx=member.exitX-player.x,dy=member.exitY-player.y,distance=Math.Sqrt(dx*dx+dy*dy),step=Math.Min(distance,Walking.Speed*seconds);
                if(distance>.001){player.x+=(float)(dx/distance*step);player.y+=(float)(dy/distance*step);changed=true;}
                if(distance<=step+.01){member.station=member.seat>=0?"sit":"";BirthdayPose(member,"Idle");changed=true;}
            }
            return changed;
        }
    }
}
