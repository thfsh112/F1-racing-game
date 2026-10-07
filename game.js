import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js';

const root=document.querySelector('#game');
const loadingEl=document.querySelector('#loading');
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
const ersPercent=document.querySelector('#ersPercent');
const ersFill=document.querySelector('#ersFill');
const pitLapEl=document.querySelector('#pitLap');
const pitButton=document.querySelector('#pitButton');
const compoundButtons=document.querySelectorAll('.compound');
let selectedCompound='S';
let pitLap=12;

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
if(loadingEl){loadingEl.classList.add('hidden');setTimeout(()=>loadingEl.remove(),500);}

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
const powerUnit=new PowerUnit(),tireModel=new TireModel(),frontWing=new THREE.Group(),rearWing=new THREE.Group(),wheelMeshes=[],frontWheelMeshes=[];
function box(name,s,p,m,parent=car){const o=new THREE.Mesh(new THREE.BoxGeometry(...s),m);o.name=name;o.position.set(...p);parent.add(o);return o}
function sphere(name,s,p,m,parent=car){const o=new THREE.Mesh(new THREE.SphereGeometry(1,32,16),m);o.name=name;o.scale.set(...s);o.position.set(...p);parent.add(o);return o}
function beam(name,a,b,r,m,parent=car){const A=new THREE.Vector3(...a),B=new THREE.Vector3(...b),d=B.clone().sub(A),o=new THREE.Mesh(new THREE.CylinderGeometry(r,r,d.length,12),m);o.name=name;o.position.copy(A.add(B).multiplyScalar(.5));o.quaternion.setFromUnitVectors(new THREE.Vector3(0,1,0),d.normalize());parent.add(o);return o}
function wing(parent,name,w,y,z,d,m,rot=0){const o=new THREE.Mesh(new THREE.BoxGeometry(w,.075,d),m);o.name=name;o.position.set(0,y,z);o.rotation.x=rot;parent.add(o);return o}

/* Low, wide, long modern ground-effect F1 silhouette. */
box('Floor',[2.05,.10,5.10],[0,.18,-.02],mats.carbon);
box('Chassis',[1.08,.27,2.35],[0,.48,.05],mats.body);
sphere('Nose',[.34,.16,1.62],[0,.47,1.42],mats.body);
box('NoseTip',[.20,.10,.48],[0,.40,2.34],mats.carbon);
sphere('FrontShoulder',[.60,.14,.76],[0,.50,.76],mats.body);
sphere('SidepodL',[.46,.25,.88],[-.72,.49,.00],mats.body);
sphere('SidepodR',[.46,.25,.88],[.72,.49,.00],mats.body);
box('SidepodInletL',[.24,.20,.48],[-.99,.58,.42],mats.carbon);
box('SidepodInletR',[.24,.20,.48],[.99,.58,.42],mats.carbon);
box('FloorEdgeL',[.10,.08,2.05],[-1.00,.29,-.10],mats.carbon);
box('FloorEdgeR',[.10,.08,2.05],[1.00,.29,-.10],mats.carbon);
sphere('EngineCover',[.43,.34,.92],[0,.68,-.72],mats.body);
sphere('RearBody',[.56,.20,.76],[0,.53,-1.48],mats.body);
box('RearSpine',[.18,.25,1.05],[0,.78,-1.16],mats.body);
box('Diffuser',[1.58,.18,.66],[0,.29,-2.02],mats.carbon);

/* cockpit / driver / halo */
box('CockpitOpening',[.55,.10,.98],[0,.70,.15],mats.carbon);
sphere('DriverHead',[.15,.24,.15],[0,.84,.00],mats.accent);
box('SeatBack',[.30,.22,.44],[0,.66,-.15],mats.carbon);
box('SteeringWheel',[.27,.045,.18],[0,.72,.52],mats.glass);
beam('HaloCenter',[0,.72,.42],[0,1.08,.05],.052,mats.carbon);
beam('HaloL',[0,1.08,.05],[-.34,1.04,.15],.052,mats.carbon);
beam('HaloR',[0,1.08,.05],[.34,1.04,.15],.052,mats.carbon);
beam('HaloTop',[-.34,1.04,.15],[.34,1.04,.15],.052,mats.carbon);
box('CameraPod',[.09,.09,.14],[0,1.14,.77],mats.carbon);

/* front wing: wide multi-element ground-effect shape */
frontWing.position.set(0,.30,2.63);car.add(frontWing);
wing(frontWing,'FrontMain',2.35,-.02,0,.30,mats.carbon,.08);
wing(frontWing,'FrontFlap1',2.16,.08,.04,.25,mats.body,-.05);
wing(frontWing,'FrontFlap2',1.92,.15,.06,.18,mats.carbon,-.16);
box('FrontEndplateL',[.08,.42,.52],[-1.17,.35,0],mats.carbon,frontWing);
box('FrontEndplateR',[.08,.42,.52],[1.17,.35,0],mats.carbon,frontWing);

/* rear wing: tall endplates + dual flap */
rearWing.position.set(0,1.18,-2.00);car.add(rearWing);
wing(rearWing,'RearMain',1.92,0,0,.28,mats.carbon,.02);
wing(rearWing,'RearFlap',1.76,.28,.03,.24,mats.body,-.12);
box('RearEndplateL',[.09,.82,.16],[-.96,.02,0],mats.carbon,rearWing);
box('RearEndplateR',[.09,.82,.16],[.96,.02,0],mats.carbon,rearWing);
beam('RearSupportL',[-.23,.62,-1.73],[-.23,1.18,-2.00],.042,mats.carbon);
beam('RearSupportR',[.23,.62,-1.73],[.23,1.18,-2.00],.042,mats.carbon);

/* exposed suspension + four large F1 slicks */
function buildWheel(side,z,front){
  const x=side*1.08,g=new THREE.Group();
  g.name=(front?'Front':'Rear')+(side<0?'Left':'Right')+'Wheel';
  g.position.set(x,.48,z);g.userData.front=front;car.add(g);
  const tire=new THREE.Mesh(new THREE.CylinderGeometry(.43,.43,.25,36),mats.tyre);
  tire.rotation.z=Math.PI/2;g.add(tire);
  const sidewall=new THREE.Mesh(new THREE.TorusGeometry(.36,.025,8,32),mats.trim);
  sidewall.rotation.y=Math.PI/2;g.add(sidewall);
  const hub=new THREE.Mesh(new THREE.CylinderGeometry(.14,.14,.27,20),mats.trim);
  hub.rotation.z=Math.PI/2;g.add(hub);
  g.userData.tire=tire;
  beam('UpperWishbone',[side*.42,.63,z+(front?-.22:.18)],[x,.60,z],.028,mats.carbon);
  beam('LowerWishbone',[side*.38,.34,z+(front?.22:-.18)],[x,.39,z],.030,mats.carbon);
  wheelMeshes.push(g);if(front)frontWheelMeshes.push(g);
}
buildWheel(-1,1.55,true);buildWheel(1,1.55,true);buildWheel(-1,-1.48,false);buildWheel(1,-1.48,false);
scene.add(car);
const activeAero=new ActiveAero(frontWing,rearWing);

/* Production GLB loader disabled for startup stability.
   The procedural F1 2026 vehicle is the guaranteed fallback.
   A GLB can be integrated later without blocking game startup. */
let productionModel=null;
let productionWheelMeshes=[];
let productionFrontWheels=[];
let modelLoaded=false;

/* Race + input + simplified AI */

// ===== F1 25-style unified controls / mini-map / race systems =====
const mapCanvas=document.querySelector('#trackMapCanvas');
const mapCtx=mapCanvas?.getContext('2d');
const flagStateEl=document.querySelector('#flagState');
const positionEl=document.querySelector('#position');
const inputState={steer:0,throttle:0,brake:0};
let raceFlag='GREEN';
let manualGear=false;

function deadzone(v,z=.08){
  if(Math.abs(v)<z)return 0;
  return Math.sign(v)*(Math.abs(v)-z)/(1-z);
}
function readGamepad(){
  const pads=navigator.getGamepads?navigator.getGamepads():[];
  const p=[...pads].find(x=>x&&x.connected);
  if(!p){inputState.steer=0;inputState.throttle=0;inputState.brake=0;return;}
  inputState.steer=deadzone(p.axes?.[0]||0);
  inputState.throttle=THREE.MathUtils.clamp(p.buttons?.[7]?.value||0,0,1);
  inputState.brake=THREE.MathUtils.clamp(p.buttons?.[6]?.value||0,0,1);
  if(p.buttons?.[0]?.pressed) powerUnit.overtake=true;
}
addEventListener('gamepadconnected',e=>console.log('Gamepad connected:',e.gamepad.id));
addEventListener('gamepaddisconnected',()=>console.log('Gamepad disconnected'));

function drawMiniMap(){
  if(!mapCtx)return;
  mapCtx.clearRect(0,0,150,150);
  mapCtx.fillStyle='rgba(0,0,0,.28)';mapCtx.fillRect(0,0,150,150);
  const all=points;
  let minX=Infinity,maxX=-Infinity,minZ=Infinity,maxZ=-Infinity;
  all.forEach(p=>{minX=Math.min(minX,p[0]);maxX=Math.max(maxX,p[0]);minZ=Math.min(minZ,p[1]);maxZ=Math.max(maxZ,p[1]);});
  const sx=116/(maxX-minX),sz=116/(maxZ-minZ),s=Math.min(sx,sz);
  const map=p=>({x:17+(p[0]-minX)*s,y:17+(p[1]-minZ)*s});
  mapCtx.beginPath();
  all.forEach((p,i)=>{const q=map(p);i?mapCtx.lineTo(q.x,q.y):mapCtx.moveTo(q.x,q.y);});
  mapCtx.closePath();mapCtx.strokeStyle='#707983';mapCtx.lineWidth=8;mapCtx.lineJoin='round';mapCtx.stroke();
  mapCtx.strokeStyle='#161b20';mapCtx.lineWidth=4;mapCtx.stroke();
  aiCars.forEach((a,i)=>{const q=map({x:a.position.x,z:a.position.z});mapCtx.beginPath();mapCtx.arc(q.x,q.y,3,0,Math.PI*2);mapCtx.fillStyle=i%2?'#fff':'#ffd447';mapCtx.fill();});
  const q=map({x:car.position.x,z:car.position.z});
  mapCtx.save();mapCtx.translate(q.x,q.y);mapCtx.rotate(car.rotation.y);mapCtx.beginPath();mapCtx.moveTo(0,-7);mapCtx.lineTo(4,5);mapCtx.lineTo(0,3);mapCtx.lineTo(-4,5);mapCtx.closePath();mapCtx.fillStyle=powerUnit.overtake?'#37e58c':'#e10600';mapCtx.fill();mapCtx.restore();
}
function updateRaceHUD(){
  if(flagStateEl)flagStateEl.textContent=raceFlag;
  if(positionEl)positionEl.textContent='P'+(1+aiCars.filter(a=>a.userData.progress>progress).length);
}

const key={left:false,right:false,throttle:false,brake:false};
document.querySelectorAll('[data-key]').forEach(b=>{
 const k=b.dataset.key;
 b.onpointerdown=e=>{e.preventDefault();key[k]=true};
 b.onpointerup=b.onpointercancel=b.onpointerleave=()=>key[k]=false;
});
addEventListener('keydown',e=>{if(e.key==='ArrowLeft'||e.key==='a')key.left=true;if(e.key==='ArrowRight'||e.key==='d')key.right=true;if(e.key==='ArrowUp'||e.key==='w')key.throttle=true;if(e.key==='ArrowDown'||e.key==='s')key.brake=true;if(e.key.toLowerCase()==='x')activeAero.setXMode(true);if(e.key.toLowerCase()==='z')activeAero.setXMode(false);if(e.code==='Space')powerUnit.overtake=true});
addEventListener('keyup',e=>{if(e.key==='ArrowLeft'||e.key==='a')key.left=false;if(e.key==='ArrowRight'||e.key==='d')key.right=false;if(e.key==='ArrowUp'||e.key==='w')key.throttle=false;if(e.key==='ArrowDown'||e.key==='s')key.brake=false;if(e.code==='Space')powerUnit.overtake=false});
aeroBtn.onclick=()=>activeAero.setXMode(!activeAero.xMode);
let overtakeToggle=false;
function setOvertake(v){powerUnit.overtake=v;overdriveBtn.classList.toggle('active',v)}
overdriveBtn.onclick=()=>{overtakeToggle=!overtakeToggle;setOvertake(overtakeToggle)};
overdriveBtn.onpointerdown=()=>setOvertake(true);
overdriveBtn.onpointerup=()=>{if(!overtakeToggle)setOvertake(false)};
overdriveBtn.onpointercancel=()=>{if(!overtakeToggle)setOvertake(false)};
compoundButtons.forEach(b=>b.onclick=()=>{selectedCompound=b.dataset.compound;compoundButtons.forEach(x=>x.classList.remove('active'));b.classList.add('active');tireModel.compound=selectedCompound});
if(pitButton)pitButton.onclick=()=>{pitLap=Math.max(laps+1,pitLap);pitLapEl.textContent=pitLap};
const pitMinus=document.querySelector('#pitMinus'),pitPlus=document.querySelector('#pitPlus');
if(pitMinus)pitMinus.onclick=()=>{pitLap=Math.max(1,pitLap-1);if(pitLapEl)pitLapEl.textContent=pitLap};
if(pitPlus)pitPlus.onclick=()=>{pitLap=Math.min(99,pitLap+1);if(pitLapEl)pitLapEl.textContent=pitLap};

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
  readGamepad();
  const steerInput=Math.abs(inputState.steer)>.02?inputState.steer:(key.right?1:0)-(key.left?1:0);
  const throttle=Math.max(key.throttle?1:0,inputState.throttle);
  const brake=Math.max(key.brake?1:0,inputState.brake);
  const target=throttle?42:0;
  v+=(target-v*.32-(brake?58:0))*dt;
  v=Math.max(0,Math.min(82,v));
  const steer=(key.right?1:0)-(key.left?1:0);
  offset+=steerInput*v*.070*dt;
  offset=Math.max(-W/2-3,Math.min(W/2+3,offset));
  if(Math.abs(offset)>W/2)v*=.985;
  const previous=progress;progress=(progress+v*dt/L)%1;
  if(previous>.8&&progress<.2){laps++;if(laps>=3){finish=true;run=false;count.textContent='FINISH'}}
  powerUnit.update(dt,throttle,v);tireModel.update(dt,v,steerInput,brake,throttle);activeAero.update(dt);
  wheelMeshes.forEach(w=>{w.userData.tire.rotation.x+=v*dt*2.2;});
   frontWheelMeshes.forEach(w=>w.rotation.y=THREE.MathUtils.lerp(w.rotation.y,steerInput*.42,1-Math.exp(-dt*12)));
  place();aiCars.forEach(a=>placeAI(a,dt));
  speedEl.textContent=Math.round(v*3.6)+' KM/H';
  gearEl.textContent=powerUnit.gear;
  rpmEl.textContent=Math.round(powerUnit.rpm)+' RPM';
  lapEl.textContent=Math.min(laps+1,3)+' / 3 LAP';
  timeEl.textContent=fmt(now-start);
  aeroEl.textContent=activeAero.xMode?'X-MODE · LOW DRAG':'Z-MODE · HIGH DOWNFORCE';
  batteryEl.textContent='ERS '+Math.round(powerUnit.energy/powerUnit.maxEnergy*100)+'%'+(powerUnit.overtake?' · OVERRIDE':'');
  tyreEl.textContent='TYRE '+Math.round(tireModel.wear*100)+'% · '+Math.round(tireModel.temp)+'°C';
  const ers=Math.round(powerUnit.energy/powerUnit.maxEnergy*100);
  if(ersPercent)ersPercent.textContent=ers;
  if(ersFill){ersFill.style.width=ers+'%';ersFill.classList.toggle('low',ers<=20);}
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
 updateRaceHUD();
 drawMiniMap();
 renderer.render(scene,camera);
}
animate();

addEventListener('resize',()=>{
  camera.aspect=innerWidth/innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(innerWidth,innerHeight);
});
