export const TITLES=['hello-dinosaurs','little-bridge','rocket-moon','fairy-garden','princess-star','mermaid-shell'];
export function fresh(){return {version:1,focus:0,players:Array.from({length:4},()=>({auto:false,words:true,last:TITLES[0],books:{}}))};}
export function valid(s,catalog){
 if(!s||s.version!==1||!Number.isInteger(s.focus)||s.focus<0||s.focus>3||!Array.isArray(s.players)||s.players.length!==4)return false;
 return s.players.every(p=>p&&typeof p.auto==='boolean'&&typeof p.words==='boolean'&&TITLES.includes(p.last)&&p.books&&typeof p.books==='object'&&!Array.isArray(p.books)&&Object.entries(p.books).every(([id,b])=>{
  const book=catalog.find(c=>c.id===id);return book&&b&&Number.isInteger(b.revision)&&b.revision>0&&Number.isInteger(b.page)&&b.page>=0&&b.page<book.pages.length&&Number.isFinite(b.seconds)&&b.seconds>=0&&b.seconds<=3600;
 }));
}
export function bookmark(s,player,book){
 const old=s.players[player].books[book.id];
 if(!old||old.revision!==book.revision)s.players[player].books[book.id]={revision:book.revision,page:0,seconds:0};
 return s.players[player].books[book.id];
}
export function turn(s,player,book,delta){
 const b=bookmark(s,player,book),next=Math.max(0,Math.min(book.pages.length-1,b.page+delta));
 if(next===b.page)return false;b.page=next;b.seconds=0;return true;
}
// Every pause, page, title or player change invalidates a pending media start.
export class PlaybackIntent{
 constructor(){this.generation=0;this.kind=null;}
 stop(){this.generation++;this.kind=null;}
 start(kind){this.stop();this.kind=kind;return this.generation;}
 accepts(token){return this.kind!==null&&token===this.generation;}
}
