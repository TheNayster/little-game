"""Authoritative original book scripts; export runtime JSON and local audio jobs.

Art/audio are separate reviewed assets. Running this never generates or approves them.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import json
from pathlib import Path

ROOT=PROJECT_ROOT
BOOKS=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/Books'

# Name, a concise visible fact, original narration, illustrative call direction.
DINOSAURS=[
('Tyrannosaurus rex','Two strong legs and tiny arms.','Tyrannosaurus rex walked on two strong legs. Its arms were small. Can you find them?','One low, friendly animal rumble with a breathy crocodile-like exhale. A gentle imaginary large dinosaur call, close and dry, quiet background.'),
('Triceratops','Three horns. One, two, three!','Triceratops had three horns and a wide frill. Can you count the horns? One, two, three!','Two soft resonant animal grunts, warm and rounded, a calm large herbivore, close and dry, quiet background.'),
('Stegosaurus','Tall plates along its back.','Stegosaurus had tall plates along its back and spikes on its tail. Point to a plate, then a spike.','A soft low animal coo followed by a little breathy snuffle, gentle imaginary herbivore, quiet background.'),
('Brachiosaurus','Front legs longer than back legs.','Brachiosaurus had a long neck. Its front legs were longer than its back legs. Imagine reaching high into a leafy tree.','A gentle deep hollow coo from a very large calm animal, slow breath, warm rumbling resonance, quiet background.'),
('Diplodocus','A very long, slender tail.','Diplodocus had a long neck and a very long, slender tail. Follow the tail with your finger. Where does it end?','Soft airy animal hum and a light breath, a relaxed giant herbivore, quiet background.'),
('Ankylosaurus','Armour and a tail club.','Ankylosaurus wore bony armour. Its tail ended in a heavy club. Can you spot the club in the picture?','A short rounded low animal grunt, then gentle snuffling, a calm armoured imaginary dinosaur, quiet background.'),
('Brontosaurus','A long neck and sturdy legs.','Brontosaurus was a giant plant eater with a long neck and sturdy legs. Imagine its slow steps through the ferns.','A deep peaceful resonant animal bellow, very soft, short and rounded, large gentle herbivore, quiet background.'),
('Spinosaurus','A tall sail and a long snout.','Spinosaurus had a tall sail on its back and a long snout. It ate fish. Can you trace the shape of its sail?','A soft crocodilian animal chuff and breathy rumble near a quiet river, gentle and curious, no loud roar.'),
('Pteranodon','A flying reptile, not a dinosaur.','Pteranodon was a flying reptile, not a dinosaur. It had a long beak and wide wings. Spread your arms like wings!','Gentle seabird-like chirrup followed by two soft leathery wing flaps, an imaginary flying reptile, quiet background.'),
('Quetzalcoatlus','Another amazing flying reptile.','Quetzalcoatlus was another flying reptile. It had a long neck and an enormous wingspan. Imagine the shadow of those wings.','One soft hollow bird-like croak with slow leathery wing flaps, a gentle large flying reptile, quiet background.'),
('Parasaurolophus','A curved crest on its head.','Parasaurolophus had a long curved crest on its head. Scientists think its crest may have helped it make sounds. What do you imagine?','One gentle low hollow resonant horn-like animal call, soft and breathy, an imaginative crested dinosaur, quiet background.'),
('Velociraptor','A small dinosaur with feathers.','Velociraptor was much smaller than the giants in this book. It had feathers. Look for the feathers on its arms and tail.','Two soft curious bird-like clucks and a small chirp, gentle feathered animal, close and dry, quiet background.'),
]

# Eight illustrated pages per story. Each page has one deliberate meaningful
# sound hotspot; illustrations never auto-trigger speech or gate page turns.
STORIES=[
('little-bridge','Big Trucks, Little Bridge',[
('A bridge for everyone','Milo visits a muddy stream.','Milo wanted to visit his friend across the stream. The old stepping stones were under water. We need a little bridge, he said.','A curious child in a yellow hard hat and red boots stands beside a stream; friendly orange digger and blue dump truck nearby.'),
('Look, then plan','Where can the bridge go?','First, the builders looked carefully. They chose firm ground on both banks. Milo drew a bridge with a wide path and strong sides.','Same child draws a simple bridge plan on paper; two banks of stream and safe clear work area.'),
('Dig a steady base','The digger scoops the earth.','The orange digger scooped a little earth. Scoop, lift, and set it down. The workers made a steady place for each end of the bridge.','Orange digger scoops earth at one stream bank; child watches well outside work zone.'),
('Deliver the stones','Tip, tumble, stop!','The blue truck brought stones. Its bed lifted slowly. The stones tumbled into a tidy pile. That is enough, said Milo.','Blue dump truck tips gravel into a tidy pile beside stream, same child at safe distance.'),
('Lift the beams','Up slowly. Down gently.','The crane lifted a strong beam. Everyone stood clear. Up slowly, across carefully, and down gently. The beam joined the two banks.','Small yellow crane carefully places wooden beam across stream; child behind low safety fence.'),
('Build the path','A smooth path and sturdy rails.','The builders added boards and sturdy rails. They checked for wobbles and sharp edges. A smooth path helps wheels and little feet.','Nearly complete wooden bridge, workers attach rails, smooth boards, no dangerous child work.'),
('Time to check','The grown-up checks it first.','A grown-up builder checked the finished bridge. Strong, steady, and ready! Milo waited for the all-clear, then walked across with his friend.','Adult builder inspects completed bridge while two children wait, friendly trucks behind.'),
('Across together','A bridge for little feet and wheels.','Now friends could visit in muddy weather. A little wagon rolled across too. Milo waved to the builders. We made a way for everyone!','Two friends and little wagon cross finished bridge with rails; orange digger blue truck yellow crane celebrate quietly.')],
 'Soft construction vehicle engine idling and a short gravel tumble, gentle toy-like outdoor work sounds, no voices.'),
('rocket-moon','Rocket to the Moon',[
('Our moon adventure','A pretend mission begins.','Ari and Pip packed for a pretend moon adventure. Helmets, gloves, and a picnic! Their rocket waited under the stars. Where shall we go first?','Child Ari with curly dark hair in white teal spacesuit, small round orange robot Pip, red white rocket, starry launch pad.'),
('Count down','Three, two, one… lift off!','Ari checked the seat belts. Pip checked the map. Three, two, one, lift off! Their pretend rocket rose into the sparkling night.','Same rocket lifts gently from launch pad with soft orange plume; Ari and Pip visible in round window.'),
('Hello, Earth','Our home looks blue and white.','Through the window, Earth looked blue and white. That is our home, said Ari. Pip drew a little heart beside it on the map.','Ari and orange robot look through rocket window at blue white Earth, dark space.'),
('A gentle landing','The moon has no air to breathe.','They landed gently. The real moon has no air to breathe, so astronauts need spacesuits. Ari and Pip kept their pretend helmets closed.','Rocket landed on grey moon; child in sealed helmet and small robot step down ladder, Earth distant.'),
('Moon steps','Hop, pause, look!','Moon gravity is weaker than Earth\'s. Ari imagined a long slow hop. Hop, pause, look! Pip followed a trail of round footprints.','Child in sealed spacesuit makes small playful hop on moon, robot and footprints, rocket nearby.'),
('What made that crater?','A bowl-shaped hollow in the ground.','They found a crater, a big hollow in the ground. Space rocks made many moon craters. Ari looked from a safe spot and sketched its shape.','Ari and robot observe rounded crater from safe flat edge, sketch pad, rocket in distance.'),
('A message home','We are coming back!','Inside the rocket, Ari sent a message home. We saw craters and imagined moon hops! Pip saved their pictures. Now it was time to return.','Child and orange robot inside warm rocket cabin press blue radio button, moon visible through window.'),
('Home under the stars','What would you explore next?','Their pretend mission ended with a picnic at home. The moon shone above them. Next time, said Ari, let us imagine another adventure.','Ari without spacesuit and small orange toy robot on garden picnic blanket, cardboard rocket, moon overhead.')],
 'A soft science fiction rocket whoosh with a gentle electronic radio bleep, playful calm space adventure, no voices or music.'),
('fairy-garden','The Fairy Garden Surprise',[
('A tiny garden','Lila finds a drooping flower.','Lila the fairy found a flower with a drooping head. Hello, little flower, she whispered. What would help you feel better today?','Tiny fairy Lila with brown skin dark curly bob mint dress translucent wings beside drooping pink flower in lush garden.'),
('Look at the soil','The soil feels dry.','Lila touched the soil gently. It felt dry. She fetched her little watering can. Plants need water, but not a great big flood.','Same fairy kneels beside dry soil and pink flower, blue tiny watering can.'),
('A little drink','Pour slowly around the roots.','Drip, drop, drip. Lila poured a little water around the roots. A ladybird rested on a leaf and watched the droplets shine.','Fairy pours small water droplets at flower roots, red ladybird on leaf, morning light.'),
('Make room for light','Leaves reach toward the sunshine.','A fallen leaf was covering the small plant. Lila moved it aside. Now the green leaves could reach toward the warm sunshine.','Fairy lifts fallen leaf from young flower, warm sun reaches leaves, ladybird nearby.'),
('Waiting is part of growing','Good things take time.','Lila waited. She watched the clouds and listened to the garden. Growing takes time. She could care for the flower without hurrying it.','Fairy sits on smooth stone beside recovering flower, soft clouds and calm garden.'),
('A visitor arrives','A butterfly finds the flower.','On another morning, the flower stood taller. A butterfly fluttered by. It settled for a moment, then danced away between the leaves.','Upright pink flower, yellow butterfly lands gently, fairy watches smiling.'),
('The garden surprise','A new little bud!','Look! A new little bud had appeared. Lila smiled. Water, light, and patient care had helped the garden grow. What a lovely surprise.','Fairy discovers tiny new closed pink bud beside open flower, blue watering can, ladybird.'),
('A place to share','Who might visit next?','Lila left room for all her garden visitors. The flower, the butterfly, and the tiny ladybird each belonged. Who might visit your garden?','Thriving flower patch, fairy butterfly ladybird and snail together, inviting warm illustrated garden.')],
 'Gentle garden birds chirping with a little water trickle and soft leaves rustling, peaceful close outdoor sounds.'),
('princess-star','The Princess and the Lost Star',[
('A light in the courtyard','Nora finds a little lantern.','Princess Nora found a star-shaped lantern in the courtyard. It had rolled away from the evening parade. Someone must be looking for you, she said.','Princess Nora with warm brown skin long dark braid lavender practical dress and small gold crown finds glowing star paper lantern in courtyard.'),
('Ask a friend','Have you seen this star?','Nora asked the gardener. Have you seen this star? The gardener shook her head, then pointed toward the busy village square. Let us ask together.','Same princess shows star paper lantern to friendly woman gardener near castle gate, flower beds.'),
('Follow the ribbon','A golden ribbon on the path.','A little golden ribbon lay on the path. It matched the lantern\'s ribbon. Nora followed it carefully, keeping the lantern safe in both hands.','Princess carrying star paper lantern follows golden ribbon along cobbled village path.'),
('Help along the way','A basket has tipped over.','Beside the bakery, a basket had tipped over. Nora stopped to help gather the wrapped rolls. Thank you, said the baker. Kindness is never a detour.','Princess and smiling baker gather wrapped bread rolls from clean paving; star lantern safe on bench.'),
('The missing lantern','A small child looks worried.','In the square, a child was searching beside the parade cart. My star is missing, he said. Nora knelt down. Is this the one you made?','Princess kneels to show star paper lantern to worried young child beside decorated parade cart.'),
('Found at last','The star belongs to the parade.','That is my star! The child smiled. Nora helped tie its ribbon onto the cart. Together they checked that the knot was secure.','Child and princess attach glowing star paper lantern to low parade cart with ribbon, smiling villagers.'),
('An invitation','Walk with us, Nora!','Walk with us, said the child. Nora joined the parade. There were big lanterns and tiny lanterns, each made by someone in the village.','Princess child gardener baker walk together beside small cart, varied glowing paper lanterns at dusk.'),
('Everyone brings a little light','Helping made the evening brighter.','The lanterns glowed as evening fell. Nora had not found a real star from the sky. She had found a chance to help, and new friends to share the light.','Wide welcoming village evening lantern parade, princess friends star paper lantern central, real stars overhead.')],
 'A delicate short magical shimmer made of soft tiny glass bell chimes, warm and gentle, no tune or voices.'),
('mermaid-shell','The Mermaid and the Rainbow Shell',[
('A colour in the sand','Mira spots a rainbow shell.','Mira the mermaid spotted a shell with rainbow colours. It rested in the sand beside a swaying sea plant. What a beautiful little home, she said.','Mermaid Mira with copper skin dark curly hair turquoise tail coral top finds pearly rainbow spiral shell on sandy seabed.'),
('Look before you lift','Is someone living inside?','Mira looked closely before touching it. A tiny claw peeked out. Someone was already living inside! The shell belonged to a little hermit crab.','Same mermaid carefully observes small hermit crab claw peeking from rainbow spiral shell, gentle clear underwater scene.'),
('A wobbly path','The crab is trying to reach a rock.','The little crab wanted to reach a sheltered rock. But a loose piece of litter lay across the sand. Mira knew litter did not belong in the sea.','Small hermit crab in rainbow shell faces discarded plain plastic loop on sand; mermaid notices, safe no entanglement.'),
('Clear the way','Mira puts the litter in her basket.','Mira carefully lifted the loose litter into her basket. She would take it to the shore for proper disposal. Now the sandy path was clear.','Mermaid places loose plastic loop into woven collection basket, crab path now clear, underwater plants.'),
('Travel together','Slow is a good speed for a small crab.','Mira swam beside the crab. She did not hurry him or carry his shell away. Slow was a good speed for such small legs.','Mermaid swims slowly beside tiny hermit crab walking in rainbow shell toward sheltered rock.'),
('A sheltered home','A calm place beside the rock.','At last, the crab reached a calm spot beside the rock. Little fish drifted past. Mira waved gently and gave her new neighbour plenty of room.','Rainbow-shell crab sheltered beside smooth rock with small fish, mermaid waves at respectful distance.'),
('The best treasure','A living sea to care for.','Mira left the rainbow shell with its owner. The best treasure was a living sea to care for, full of creatures with homes of their own.','Mermaid admires thriving reef from distance, hermit crab in shell at rock, diverse gentle fish and sea plants.'),
('What will you notice?','Look closely. Care gently.','Mira swam home with her basket of litter. Next time she saw a pretty shell, she would look closely first. What might you notice by the sea?','Mermaid at shallow shore brings basket of litter to collection point, clean beach, shell crab visible safely below water.')],
 'Soft underwater bubbles rising with a gentle water swish, calm close aquatic sound, no voices or music.'),
]

def main():
    # The user requested the original six drafts on the phone for review before
    # choosing replacements. Preserve that preview separately from final scope.
    rects=json.loads((ROOT/'SourceArt/Home/Books/art-rects.json').read_text(encoding='utf-8'))
    voices=[];effects=[];library=[]
    dinosaur_calls=json.loads((ROOT/'SourceAudio/Home/Books/dinosaur-call-jobs-2026-09-30.json').read_text())
    def export(book):
        book['artRects']=rects[book['id']]
        folder=BOOKS/book['id'];folder.mkdir(parents=True,exist_ok=True)
        (folder/'content.json').write_text(json.dumps(book,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
        library.append(dict(id=book['id'],title=book['title'],pages=len(book['pages'])))
        for i,page in enumerate(book['pages']):
            voices.append(dict(text=page['speech'],output=f'SourceAudio/Home/Books/{book["id"]}/page-{i}.wav'))
        for i,name in enumerate(book['names']):
            voices.append(dict(text=name+'.',output=f'SourceAudio/Home/Books/{book["id"]}/name-{i}.wav'))
    pages=[dict(title='Hello, Dinosaurs!',caption='Twelve prehistoric animals to meet.',speech='Hello, dinosaur explorers! Let us meet twelve amazing prehistoric animals. Tap a picture to hear its name. The sound button plays an imagined animal call.',species=-1)]
    for i,(name,caption,speech,prompt) in enumerate(DINOSAURS):
        pages.append(dict(title=name,caption=caption,speech=speech,species=i))
        effects.append(dinosaur_calls[i])
    pages.append(dict(title='Who will you choose?',caption='Tap an animal to hear its name again.',speech='Which animal would you like to meet again? Some lived millions of years apart. Their real voices are a mystery, so our sounds are made for imagination.',species=-1))
    export(dict(id='hello-dinosaurs',revision=2,title='Hello, Dinosaurs!',locale='en-US',kind='dinosaurs',pages=pages,names=[v[0] for v in DINOSAURS],notes='Original text. Illustrative colours and imaginative calls; no claim animals coexisted. Pterosaurs are flying reptiles.'))
    for id,title,story,prompt in STORIES:
        export(dict(id=id,title=title,revision=1,locale='en-US',kind='story',names=[],pages=[dict(title=a,caption=b,speech=c,art=d,species=-1) for a,b,c,d in story],notes='Original story and illustrations; all family profiles can read it.'))
        effects.append(dict(prompt=prompt,output=f'SourceAudio/Home/Books/{id}/effect-0.wav'))
    BOOKS.mkdir(exist_ok=True,parents=True);(BOOKS/'catalog.json').write_text(json.dumps(dict(books=library),indent=2)+'\n')
    out=ROOT/'SourceAudio/Home/Books';out.mkdir(exist_ok=True,parents=True)
    (out/'narration-jobs.json').write_text(json.dumps(voices,indent=2)+'\n')
    (out/'effect-jobs.json').write_text(json.dumps(effects,indent=2)+'\n')
    print(f'{len(library)} books, {sum(b["pages"] for b in library)} pages, {len(voices)} narration jobs, {len(effects)} effects')

if __name__=='__main__':main()
