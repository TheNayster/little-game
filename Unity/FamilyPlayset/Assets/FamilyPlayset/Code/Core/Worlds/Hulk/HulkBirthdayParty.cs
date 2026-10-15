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
            if(member!=null && member.station=="unwrap")member.station="";
            b.giftAt=-1;b.giftId="";b.giftActor="";
        }
        private bool BirthdayPartyOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            var b=state.birthday;outcome=null;
            bool Near(float x)=>Math.Abs(player.x-x)<=InteractionRadius && Math.Abs(player.y-200)<=InteractionRadius;
            if(member.station=="unwrap" || member.station=="eat" || member.station=="drink"){outcome="finish-or-walk-away";return true;}
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
                if(hulkMember!=null && hulkMember.station!=""){outcome="hulk-is-playing";return true;}
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
                if(!Near(2400)){outcome="come-to-the-cake-table";return true;}
                if(held!=null || member.station!=""){outcome="hands-busy";return true;}
                if(c.value=="serve" && !b.cakeReady){outcome="decorate-the-cake-first";return true;}
                var portion=b.props.FirstOrDefault(p=>p.kind==(c.value=="serve"?"food":"drink") && p.location=="stock" && p.holder=="");
                if(portion==null){outcome="all-four-portions-are-out";return true;}
                portion.holder=player.id;portion.location="held";portion.style=b.cakeBase;portion.version++;return true;
            }
            if(c.value.StartsWith("cake-",StringComparison.Ordinal)){
                if(!Near(2400)){outcome="come-to-the-cake-table";return true;}
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
                int choice=gift.fake?4:b.giftCursor++%HulkBirthday.Contents.Length;
                // The source limits cat/pug gifts to one each. Preserve that
                // object budget across repeated wrapping and shared sessions.
                if(choice>=6 && b.props.Any(p=>p!=gift && p.opened && p.content==choice))choice=0;
                gift.opened=true;gift.content=choice;gift.version++;
                b.reaction=choice==2 || choice==3 || choice==4?"negative":"happy";b.reactionAt=b.clock;
                CancelBirthdayUnwrap(b);changed=true;
            }
            foreach(var member in b.members.Where(m=>m.station=="eat" || m.station=="drink")){
                if(b.clock-member.actionAt<2.5)continue;
                var food=b.props.SingleOrDefault(p=>p.holder==member.actor);
                if(food!=null){food.holder="";food.opened=false;food.location="stock";food.version++;}
                member.station="";BirthdayPose(member,"Happy");changed=true;
            }
            return changed;
        }
    }
}
