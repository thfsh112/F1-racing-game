import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js';

const root=document.querySelector('#game');
const speedEl=document.querySelector('#speed');
const lapEl=document.querySelector('#lap');
const timeEl=document.querySelector('#time');
const count=document.querySelector('#countdown');
const touch=document.querySelector('#touch');

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

const mats={
 body:new THREE.MeshStandardMaterial({color:0x9da4aa,metalness:.72,roughness:.25}),
 carbon:new THREE.MeshStandardMaterial({color:0x11151a,metalness:.35,roughness:.55}),
 tyre:new THREE.MeshStandardMaterial({color:0x101010,roughness:.96}),
 glass:new THREE.MeshStandardMaterial({color:0x07121b,metalness:.25,roughness:.12}),
 trim:new THREE.MeshStandardMaterial({color:0x4b535a,metalness:.7,roughness:.3}),
 light:new THREE.MeshStandardMaterial({color:0xdce8ef,metalness:.25,roughness:.2})
};

const car=new THREE.Group();
function add(mesh,name){
  mesh.name=name;car.add(mesh);return mesh;
}
function box(name,s,p,m,rot=0){
  const o=add(new THREE.Mesh(new THREE.BoxGeometry(...s),m),name);
  o.position.set(...p);o.rotation.y=rot;return o;
}
function sphere(name,s,p,m){
  const o=add(new THREE.Mesh(new THREE.SphereGeometry(1,24,12),m),name);
  o.scale.set(...s);o.position.set(...p);return o;
}
function beam(name,a,b,r,m){
  const d=new THREE.Vector3(...b).sub(new THREE.Vector3(...a));
  const o=add(new THREE.Mesh(new THREE.CylinderGeometry(r,r,d.length,10),m),name);
  o.position.copy(new THREE.Vector3(...a).add(new THREE.Vector3(...b)).multiplyScalar(.5));
  o.quaternion.setFromUnitVectors(new THREE.Vector3(0,1,0),d.normalize());
  return o;
}

// Original 2026-inspired single-seater: proportions are designed for this game,
// while keeping the body neutral so a livery system can be added later.
box('Floor',[1.72,.10,3.75],[0,.18,0],mats.carbon);
sphere('MainChassis',[.48,.15,1.22],[0,.40,.02],mats.body);
sphere('FrontMonocoque',[.39,.16,.78],[0,.44,.88],mats.body);
sphere('LongNose',[.22,.105,.80],[0,.42,1.63],mats.body);
box('NoseTip',[.15,.07,.32],[0,.36,2.31],mats.carbon);
box('NoseTop',[.34,.035,.58],[0,.52,1.66],mats.trim);

sphere('LeftSidepod',[.29,.17,.74],[-.56,.47,.05],mats.body);
sphere('RightSidepod',[.29,.17,.74],[.56,.47,.05],mats.body);
box('LeftInlet',[.12,.15,.48],[-.78,.53,.34],mats.carbon);
box('RightInlet',[.12,.15,.48],[.78,.53,.34],mats.carbon);
box('LeftFloorEdge',[.065,.055,1.30],[-.70,.27,-.10],mats.carbon);
box('RightFloorEdge',[.065,.055,1.30],[.70,.27,-.10],mats.carbon);

sphere('EngineCover',[.34,.19,.70],[0,.60,-.63],mats.body);
sphere('RearTaper',[.42,.15,.55],[0,.48,-1.16],mats.body);
box('Diffuser',[1.25,.12,.55],[0,.27,-1.48],mats.carbon);
box('RearSpine',[.18,.18,.68],[0,.73,-.86],mats.body);

box('CockpitOpening',[.47,.08,.76],[0,.62,.27],mats.carbon);
sphere('Seat',[.16,.09,.25],[0,.61,.03],mats.carbon);
box('CockpitRim',[.54,.035,.10],[0,.67,.27],mats.trim);

beam('HaloCenter',[0,.64,.43],[0,.94,.08],.045,mats.carbon);
beam('HaloLeft',[0,.94,.08],[-.30,.91,.17],.045,mats.carbon);
beam('HaloRight',[0,.94,.08],[.30,.91,.17],.045,mats.carbon);
beam('HaloTop',[-.30,.91,.17],[.30,.91,.17],.045,mats.carbon);
box('CameraPod',[.07,.07,.11],[0,1.00,.83],mats.carbon);

const frontWing=new THREE.Group();frontWing.position.set(0,.29,2.27);car.add(frontWing);
function wing(parent,name,w,y,z,depth,mat){
  const o=new THREE.Mesh(new THREE.BoxGeometry(w,.065,depth),mat);o.name=name;o.position.set(0,y,z);parent.add(o);return o;
}
wing(frontWing,'FrontWingMain',1.82,0,0,.36,mats.carbon);
wing(frontWing,'FrontWingUpper',1.62,.08,.03,.28,mats.body);
wing(frontWing,'FrontWingLower',1.94,-.07,.05,.22,mats.carbon);
box('FrontEndplateL',[.07,.27,.42],[-.92,.37,2.27],mats.carbon);
box('FrontEndplateR',[.07,.27,.42],[.92,.37,2.27],mats.carbon);

const rearWing=new THREE.Group();rearWing.position.set(0,1.02,-1.58);car.add(rearWing);
wing(rearWing,'RearWingMain',1.62,0,0,.24,mats.carbon);
wing(rearWing,'RearWingUpper',1.50,.17,.02,.20,mats.body);
wing(rearWing,'RearWingLower',1.55,-.16,.03,.17,mats.carbon);
box('RearEndplateL',[.07,.70,.12],[-.80,0,0],mats.carbon);
box('RearEndplateR',[.07,.70,.12],[.80,0,0],mats.carbon);
beam('RearSupportL',[-.20,.56,-1.42],[-.20,1.02,-1.58],.035,mats.carbon);
beam('RearSupportR',[.20,.56,-1.42],[.20,1.02,-1.58],.035,mats.carbon);

const wheels=[];
function wheel(side,z,front){
  const x=side*.84;
  const g=new THREE.Group();g.position.set(x,.36,z);car.add(g);
  const tire=new THREE.Mesh(new THREE.CylinderGeometry(.37,.37,.19,24),mats.tyre);
  tire.rotation.z=Math.PI/2;g.add(tire);
  const hub=new THREE.Mesh(new THREE.CylinderGeometry(.13,.13,.205,16),mats.trim);
  hub.rotation.z=Math.PI/2;g.add(hub);
  const disc=new THREE.Mesh(new THREE.CylinderGeometry(.22,.22,.21,20),mats.carbon);
  disc.rotation.z=Math.PI/2;g.add(disc);
  beam('UpperSuspension',[side*.37,.50,z+(front?-.15:.15)],[x,.45,z],.025,mats.carbon);
  beam('LowerSuspension',[side*.32,.30,z+(front?.15:-.15)],[x,.34,z],.025,mats.carbon);
  wheels.push(g);
}
wheel(-1,1.18,true);wheel(1,1.18,true);wheel(-1,-1.18,false);wheel(1,-1.18,false);
scene.add(car);

const key={left:false,right:false,throttle:false,brake:false};
addEventListener('keydown',e=>{
  if(e.key==='ArrowLeft'||e.key==='a')key.left=true;
  if(e.key==='ArrowRight'||e.key==='d')key.right=true;
  if(e.key==='ArrowUp'||e.key==='w')key.throttle=true;
  if(e.key==='ArrowDown'||e.key==='s')key.brake=true;
});
addEventListener('keyup',e=>{
  if(e.key==='ArrowLeft'||e.key==='a')key.left=false;
  if(e.key==='ArrowRight'||e.key==='d')key.right=false;
  if(e.key==='ArrowUp'||e.key==='w')key.throttle=false;
  if(e.key==='ArrowDown'||e.key==='s')key.brake=false;
});
touch.querySelectorAll('button').forEach(b=>{
  const k=b.dataset.key;
  b.onpointerdown=e=>{e.preventDefault();key[k]=true};
  b.onpointerup=b.onpointercancel=b.onpointerleave=()=>key[k]=false;
});

let run=false,v=0,offset=0,progress=0,last=.99,laps=0,start=0;
function fmt(ms){
  const m=Math.floor(ms/60000),sec=Math.floor(ms/1000)%60,mm=Math.floor(ms)%1000;
  return String(m).padStart(2,'0')+':'+String(sec).padStart(2,'0')+'.'+String(mm).padStart(3,'0');
}
function place(){
  const p=curve.getPointAt(progress),d=curve.getTangentAt(progress);
  const r=new THREE.Vector3(d.z,0,-d.x);
  p.addScaledVector(r,offset);car.position.copy(p);car.position.y=.15;
  car.rotation.y=Math.atan2(d.x,d.z);
}
place();

async function startRace(){
  run=false;v=0;offset=0;progress=0;last=.99;laps=0;place();
  for(const n of ['3','2','1']){count.textContent=n;await new Promise(x=>setTimeout(x,600));}
  count.textContent='GO!';run=true;start=performance.now();
  setTimeout(()=>count.textContent='',500);
}
document.querySelector('#restart').onclick=startRace;
startRace();

const clock=new THREE.Clock();
function animate(){
  requestAnimationFrame(animate);
  const dt=Math.min(clock.getDelta(),.04),now=performance.now();
  if(run){
    const target=key.throttle?42:0;
    v+=(target-v*.32-(key.brake?58:0))*dt;
    v=Math.max(0,Math.min(82,v));
    const steer=(key.right?1:0)-(key.left?1:0);
    offset+=steer*v*.070*dt;
    offset=Math.max(-W/2-3,Math.min(W/2+3,offset));
    if(Math.abs(offset)>W/2)v*=.985;
    const previous=progress;
    progress=(progress+v*dt/L)%1;
    if(previous>.8&&progress<.2){laps++;if(laps>=3){run=false;count.textContent='FINISH';}}
    speedEl.textContent=Math.round(v*3.6)+' KM/H';
    lapEl.textContent=Math.min(laps+1,3)+' / 3 LAP';
    timeEl.textContent=fmt(now-start);
    place();
  }
  const f=new THREE.Vector3(Math.sin(car.rotation.y),0,Math.cos(car.rotation.y));
  camera.position.lerp(car.position.clone().addScaledVector(f,-11).setY(5.8),.10);
  camera.lookAt(car.position.x,car.position.y+.55,car.position.z+1);
  renderer.render(scene,camera);
}
animate();

addEventListener('resize',()=>{
  camera.aspect=innerWidth/innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(innerWidth,innerHeight);
});
