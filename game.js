import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js';

const root=document.querySelector('#game');
const speedEl=document.querySelector('#speed');
const lapEl=document.querySelector('#lap');
const timeEl=document.querySelector('#time');
const count=document.querySelector('#countdownText');
const touch=document.querySelector('#touch');
const gearEl=document.querySelector('#gear');
const rpmEl=document.querySelector('#rpm');
const aeroEl=document.querySelector('#aero');
const batteryEl=document.querySelector('#battery');
const tyreEl=document.querySelector('#tyre');
const restartBtn=document.querySelector('#restart');
const aeroBtn=document.querySelector('#aeroBtn');
const overdriveBtn=document.querySelector('#overdriveBtn');
const cameraBtn=document.querySelector('#cameraBtn');
const cameraLabel=document.querySelector('#cameraLabel');

document.documentElement.style.background='#071018';
document.body.style.background='#071018';
try{screen.orientation?.lock?.('landscape').catch(()=>{});}catch(e){}

const scene=new THREE.Scene();
scene.background=new THREE.Color(0x8fb3c8);
scene.fog=new THREE.Fog(0x8fb3c8,90,340);

const camera=new THREE.PerspectiveCamera(58,innerWidth/innerHeight,.1,700);
const renderer=new THREE.WebGLRenderer({antialias:true});
renderer.setPixelRatio(Math.min(devicePixelRatio,1.8));
renderer.setSize(innerWidth,innerHeight);
renderer.outputColorSpace=THREE.SRGBColorSpace;
root.appendChild(renderer.domElement);

scene.add(new THREE.HemisphereLight(0xe9f7ff,0x26391d,2.2));
const sun=new THREE.DirectionalLight(0xffffff,2.4);
sun.position.set(80,120,50);
scene.add(sun);

const points=[[-70,-25],[-55,-60],[-5,-72],[45,-55],[72,-15],[62,30],[30,62],[-15,72],[-58,55],[-76,15]];
const curve=new THREE.CatmullRomCurve3(points.map(p=>new THREE.Vector3(p[0],0,p[1])),true);
const N=280,L=curve.getLength(),W=13,pts=[],dirs=[];
for(let i=0;i<N;i++){
  const p=curve.getPointAt(i/N),q=curve.getPointAt((i+1)/N);
  pts.push(p);dirs.push(q.sub(p).normalize());
}
function strip(w,y,mat){
  const pos=[],idx=[];
  for(let i=0;i<N;i++){
    const p=pts[i],d=dirs[i],s=new THREE.Vector3(-d.z,0,d.x);
    const a=p.clone().addScaledVector(s,w/2),b=p.clone().addScaledVector(s,-w/2);
    a.y=b.y=y;pos.push(a.x,a.y,a.z,b.x,b.y,b.z);
  }
  for(let i=0;i<N;i++){const j=i*2,k=((i+1)%N)*2;idx.push(j,k,k+1,j,k+1,j+1);}
  const g=new THREE.BufferGeometry();
  g.setAttribute('position',new THREE.Float32BufferAttribute(pos,3));
  g.setIndex(idx);g.computeVertexNormals();scene.add(new THREE.Mesh(g,mat));
}
strip(W+2,.01,new THREE.MeshStandardMaterial({color:0xc82438,roughness:.8}));
strip(W,.03,new THREE.MeshStandardMaterial({color:0x262a2f,roughness:1}));
const ground=new THREE.Mesh(new THREE.PlaneGeometry(700,700),new THREE.MeshStandardMaterial({color:0x35652f,roughness:1}));
ground.rotation.x=-Math.PI/2;ground.position.y=-.05;scene.add(ground);

/* 2026 vehicle rig: original, livery-neutral and reference-inspired. */
const mats={
 body:new THREE.MeshStandardMaterial({color:0x9fa4a7,metalness:.76,roughness:.24}),
 carbon:new THREE.MeshStandardMaterial({color:0x0d1116,metalness:.32,roughness:.58}),
 tyre:new THREE.MeshStandardMaterial({color:0x090909,roughness:.98}),
 glass:new THREE.MeshStandardMaterial({color:0x06131d,metalness:.2,roughness:.1}),
 trim:new THREE.MeshStandardMaterial({color:0x4d555b,metalness:.72,roughness:.3}),
 accent:new THREE.MeshStandardMaterial({color:0xb9282f,metalness:.4,roughness:.3}),
white:new THREE.MeshStandardMaterial({color:0xe9edf0,metalness:.15,roughness:.32})
};
class ActiveAero{
 constructor(front,rear){this.front=front;this.rear=rear;this.xMode=false;this.target=0}
 setXMode(v){this.xMode=v;this.target=v?1:0}
 update(dt){const a=1-Math.exp(-dt*9),t=this.target;this.front.children.forEach((o,i)=>o.rotation.x=THREE.MathUtils.lerp(o.rotation.x,(i?-.18:-.10)*t,a));this.rear.children.forEach((o,i)=>o.rotation.x=THREE.MathUtils.lerp(o.rotation.x,(i?-.32:-.24)*t,a))}
}
class PowerUnit{
 constructor(){this.energy=4;this.maxEnergy=4;this.overtake=false;this.rpm=0;this.gear=1}
 update(dt,throttle,speed){this.rpm=THREE.MathUtils.lerp(this.rpm,throttle?8000+speed*65:1800+speed*25,1-Math.exp(-dt*7));if(speed>70&&this.gear<8)this.gear++;if(speed<35&&this.gear>1)this.gear--;const use=(throttle?.035:0)*dt*(this.overtake?2.2:1);this.energy=Math.max(0,this.energy-use);if(!throttle)this.energy=Math.min(this.maxEnergy,this.energy+.018*dt);if(this.overtake&&this.energy<.12)this.overtake=false}
}
class TireModel{
 constructor(){this.wear=1;this.temp=72}
 update(dt,speed,steer,brake,throttle){const slip=Math.min(1,Math.abs(steer)*speed/95+(brake?.14:0));this.temp=THREE.MathUtils.clamp(this.temp+(slip*2.8+(brake?2:0))*dt-(this.temp-72)*.018*dt,45,125);this.wear=Math.max(.35,this.wear-(slip*.00055+Math.max(0,this.temp-105)*.000012)*dt)}
}
const car=new THREE.Group();car.name='F12026_Vehicle';
const powerUnit=new PowerUnit(),tireModel=new TireModel(),frontWing=new THREE.Group(),rearWing=new THREE.Group(),wheelMeshes=[];
function box(name,s,p,m){const o=new THREE.Mesh(new THREE.BoxGeometry(...s),m);o.name=name;o.position.set(...p);car.add(o);return o}
function sphere(name,s,p,m){const o=new THREE.Mesh(new THREE.SphereGeometry(1,28,14),m);o.name=name;o.scale.set(...s);o.position.set(...p);car.add(o);return o}
function beam(name,a,b,r,m){const A=new THREE.Vector3(...a),B=new THREE.Vector3(...b),d=B.clone().sub(A),o=new THREE.Mesh(new THREE.CylinderGeometry(r,r,d.length,10),m);o.name=name;o.position.copy(A.add(B).multiplyScalar(.5));o.quaternion.setFromUnitVectors(new THREE.Vector3(0,1,0),d.normalize());car.add(o);return o}
function wing(parent,name,w,y,z,d,m){const o=new THREE.Mesh(new THREE.BoxGeometry(w,.065,d),m);o.name=name;o.position.set(0,y,z);parent.add(o)}
/* Chassis / sidepods / floor */
box('Chassis',[.92,.28,2.48],[0,.40,.02],mats.body);sphere('FrontMonocoque',[.40,.16,.80],[0,.44,.90],mats.body);sphere('LongNose',[.215,.105,.82],[0,.42,1.65],mats.body);box('NoseTip',[.15,.07,.32],[0,.36,2.34],mats.carbon);box('Floor',[1.75,.10,3.78],[0,.18,-.02],mats.carbon);
sphere('SidepodL',[.30,.18,.76],[-.56,.48,.02],mats.body);sphere('SidepodR',[.30,.18,.76],[.56,.48,.02],mats.body);box('InletL',[.12,.15,.50],[-.79,.53,.36],mats.carbon);box('InletR',[.12,.15,.50],[.79,.53,.36],mats.carbon);box('FloorEdgeL',[.07,.06,1.42],[-.70,.27,-.10],mats.carbon);box('FloorEdgeR',[.07,.06,1.42],[.70,.27,-.10],mats.carbon);
sphere('EngineCover',[.35,.20,.72],[0,.61,-.64],mats.body);sphere('RearBody',[.46,.17,.60],[0,.49,-1.17],mats.body);box('Diffuser',[1.28,.12,.56],[0,.27,-1.48],mats.carbon);box('RearSpine',[.18,.18,.72],[0,.73,-.87],mats.body);
/* Cockpit / driver / dashboard / halo */
box('CockpitOpening',[.47,.09,.78],[0,.62,.27],mats.carbon);sphere('DriverHead',[.13,.22,.13],[0,.76,.02],mats.accent);box('DigitalDashboard',[.34,.08,.08],[0,.73,.52],mats.glass);box('SteeringWheel',[.22,.035,.16],[0,.69,.54],mats.carbon);
beam('HaloCenter',[0,.64,.43],[0,.94,.08],.045,mats.carbon);beam('HaloL',[0,.94,.08],[-.30,.91,.17],.045,mats.carbon);beam('HaloR',[0,.94,.08],[.30,.91,.17],.045,mats.carbon);beam('HaloTop',[-.30,.91,.17],[.30,.91,.17],.045,mats.carbon);box('CameraPod',[.07,.07,.11],[0,1,.84],mats.carbon);
/* Active front wing: two-element visual assembly */
frontWing.position.set(0,.30,2.28);car.add(frontWing);wing(frontWing,'FrontFlapA',1.84,0,0,.34,mats.carbon);wing(frontWing,'FrontFlapB',1.62,.08,.03,.27,mats.body);box('FrontEndplateL',[.07,.27,.42],[-.94,.38,2.28],mats.carbon);box('FrontEndplateR',[.07,.27,.42],[.94,.38,2.28],mats.carbon);
/* Active rear wing: three-element assembly */
rearWing.position.set(0,1.02,-1.60);car.add(rearWing);wing(rearWing,'RearFlapA',1.64,0,0,.25,mats.carbon);wing(rearWing,'RearFlapB',1.50,.17,.02,.20,mats.body);wing(rearWing,'RearFlapC',1.54,-.16,.03,.17,mats.carbon);box('RearEndplateL',[.07,.70,.12],[-.81,0,0],mats.carbon);box('RearEndplateR',[.07,.70,.12],[.81,0,0],mats.carbon);beam('RearSupportL',[-.20,.56,-1.43],[-.20,1.02,-1.60],.035,mats.carbon);beam('RearSupportR',[.20,.56,-1.43],[.20,1.02,-1.60],.035,mats.carbon);
/* Four independent wheel assemblies + suspension visuals */
function buildWheel(side,z,front){const x=side*.84,g=new THREE.Group();g.name=(front?'Front':'Rear')+(side<0?'Left':'Right')+'Wheel';g.position.set(x,.36,z);car.add(g);const t=new THREE.Mesh(new THREE.CylinderGeometry(.37,.37,.19,28),mats.tyre);t.rotation.z=Math.PI/2;g.add(t);const hub=new THREE.Mesh(new THREE.CylinderGeometry(.13,.13,.205,18),mats.trim);hub.rotation.z=Math.PI/2;g.add(hub);beam(g.name+'Upper',[side*.37,.50,z+(front?-.15:.15)],[x,.45,z],.026,mats.carbon);beam(g.name+'Lower',[side*.32,.30,z+(front?.15:-.15)],[x,.34,z],.026,mats.carbon);wheelMeshes.push(g)}
buildWheel(-1,1.18,true);buildWheel(1,1.18,true);buildWheel(-1,-1.18,false);buildWheel(1,-1.18,false);
scene.add(car);
const activeAero=new ActiveAero(frontWing,rearWing);
/* Race + input + simplified AI */
const key={left:false,right:false,throttle:false,brake:false};
document.querySelectorAll('[data-key]').forEach(b=>{
 const k=b.dataset.key;
 b.onpointerdown=e=>{e.preventDefault();key[k]=true};
 b.onpointerup=b.onpointercancel=b.onpointerleave=()=>key[k]=false;
});
addEventListener('keydown',e=>{if(e.key==='ArrowLeft'||e.key==='a')key.left=true;if(e.key==='ArrowRight'||e.key==='d')key.right=true;if(e.key==='ArrowUp'||e.key==='w')key.throttle=true;if(e.key==='ArrowDown'||e.key==='s')key.brake=true;if(e.key.toLowerCase()==='x')activeAero.setXMode(true);if(e.key.toLowerCase()==='z')activeAero.setXMode(false);if(e.code==='Space')powerUnit.overtake=true});
addEventListener('keyup',e=>{if(e.key==='ArrowLeft'||e.key==='a')key.left=false;if(e.key==='ArrowRight'||e.key==='d')key.right=false;if(e.key==='ArrowUp'||e.key==='w')key.throttle=false;if(e.key==='ArrowDown'||e.key==='s')key.brake=false;if(e.code==='Space')powerUnit.overtake=false});
aeroBtn.onclick=()=>activeAero.setXMode(!activeAero.xMode);
overdriveBtn.onpointerdown=()=>powerUnit.overtake=true;
overdriveBtn.onpointerup=overdriveBtn.onpointercancel=()=>powerUnit.overtake=false;

const aiCars=[];
function makeAI(i){
 const g=new THREE.Group(),m=i%2?mats.accent:mats.white;
 const b=new THREE.Mesh(new THREE.BoxGeometry(.95,.23,2.05),m);b.position.y=.35;g.add(b);
 const w=new THREE.Mesh(new THREE.BoxGeometry(1.45,.07,.25),mats.carbon);w.position.set(0,.30,-1.02);g.add(w);
 for(const x of[-.55,.55])for(const z of[-.70,.70]){const t=new THREE.Mesh(new THREE.CylinderGeometry(.27,.27,.14,16),mats.tyre);t.rotation.z=Math.PI/2;t.position.set(x,.29,z);g.add(t)}
 g.userData={progress:(i+1)*.025,offset:(i-1.5)*1.65,speed:56+i*1.5};
 scene.add(g);return g;
}
for(let i=0;i<4;i++)aiCars.push(makeAI(i));

let run=false,v=0,offset=0,progress=0,last=.99,laps=0,start=0,finish=false;
function fmt(ms){const m=Math.floor(ms/60000),sec=Math.floor(ms/1000)%60,mm=Math.floor(ms)%1000;return String(m).padStart(2,'0')+':'+String(sec).padStart(2,'0')+'.'+String(mm).padStart(3,'0')}
function place(){
 const p=curve.getPointAt(progress),d=curve.getTangentAt(progress),r=new THREE.Vector3(d.z,0,-d.x);
 p.addScaledVector(r,offset);car.position.copy(p);car.position.y=.15;car.rotation.y=Math.atan2(d.x,d.z);
}
function placeAI(a,dt){
 a.userData.speed+=(58-a.userData.speed)*dt*.7;
 a.userData.progress=(a.userData.progress+a.userData.speed*dt/L)%1;
 const p=curve.getPointAt(a.userData.progress),d=curve.getTangentAt(a.userData.progress),r=new THREE.Vector3(d.z,0,-d.x);
 p.addScaledVector(r,a.userData.offset);a.position.copy(p);a.position.y=.12;a.rotation.y=Math.atan2(d.x,d.z);
}
place();

async function startRace(){
 run=false;finish=false;v=0;offset=0;progress=0;last=.99;laps=0;powerUnit.energy=4;powerUnit.gear=1;tireModel.wear=1;tireModel.temp=72;activeAero.setXMode(false);place();
 aiCars.forEach((a,i)=>{a.userData.progress=(i+1)*.025;a.userData.offset=(i-1.5)*1.65});
 for(const n of['3','2','1']){count.textContent=n;await new Promise(x=>setTimeout(x,600))}
 count.textContent='GO!';run=true;start=performance.now();setTimeout(()=>count.textContent='',450);
}
let cameraMode=0;
if(cameraBtn) cameraBtn.onclick=()=>{cameraMode=(cameraMode+1)%2;cameraBtn.textContent=cameraMode?'CAM · COCKPIT':'CAM · CHASE';cameraLabel.textContent=cameraMode?'COCKPIT CAMERA':'CHASE CAMERA'};
restartBtn.onclick=startRace;startRace();

const clock=new THREE.Clock();
function animate(){
 requestAnimationFrame(animate);
 const dt=Math.min(clock.getDelta(),.04),now=performance.now();
 if(run&&!finish){
  const throttle=key.throttle,brake=key.brake;
  const target=throttle?42:0;
  v+=(target-v*.32-(brake?58:0))*dt;
  v=Math.max(0,Math.min(82,v));
  const steer=(key.right?1:0)-(key.left?1:0);
  offset+=steer*v*.070*dt;
  offset=Math.max(-W/2-3,Math.min(W/2+3,offset));
  if(Math.abs(offset)>W/2)v*=.985;
  const previous=progress;progress=(progress+v*dt/L)%1;
  if(previous>.8&&progress<.2){laps++;if(laps>=3){finish=true;run=false;count.textContent='FINISH'}}
  powerUnit.update(dt,throttle,v);tireModel.update(dt,v,steer,brake,throttle);activeAero.update(dt);
  wheelMeshes.forEach(w=>w.rotation.y+=v*dt*2.2);
  place();aiCars.forEach(a=>placeAI(a,dt));
  speedEl.textContent=Math.round(v*3.6)+' KM/H';
  gearEl.textContent=powerUnit.gear;
  rpmEl.textContent=Math.round(powerUnit.rpm)+' RPM';
  lapEl.textContent=Math.min(laps+1,3)+' / 3 LAP';
  timeEl.textContent=fmt(now-start);
  aeroEl.textContent=activeAero.xMode?'X-MODE · LOW DRAG':'Z-MODE · HIGH DOWNFORCE';
  batteryEl.textContent='ERS '+Math.round(powerUnit.energy/powerUnit.maxEnergy*100)+'%'+(powerUnit.overtake?' · OVERRIDE':'');
  tyreEl.textContent='TYRE '+Math.round(tireModel.wear*100)+'% · '+Math.round(tireModel.temp)+'°C';
  aeroBtn.textContent=activeAero.xMode?'AERO · X':'AERO · Z';
 }
 const f=new THREE.Vector3(Math.sin(car.rotation.y),0,Math.cos(car.rotation.y));
  const chaseTarget=car.position.clone().addScaledVector(f,-11).setY(5.8);
  const cockpitTarget=car.position.clone().addScaledVector(f,.18).setY(1.03);
  const target=cameraMode?cockpitTarget:chaseTarget;
  camera.position.lerp(target,1-Math.exp(-dt*5));
  if(cameraMode){
    const look=car.position.clone().addScaledVector(f,3.2);look.y=1.02;camera.lookAt(look);
  }else camera.lookAt(car.position.x,car.position.y+.55,car.position.z+1);
 renderer.render(scene,camera);
}
animate();

addEventListener('resize',()=>{
  camera.aspect=innerWidth/innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(innerWidth,innerHeight);
});
