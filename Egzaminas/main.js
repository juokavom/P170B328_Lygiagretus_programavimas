const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();

const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(3).keys()];
const names = ['First', 'Second', 'Third', 'Fourth', 'Fifth'];
//const names = ['First'];


const worker = [];
const bottleneck = [];


array.forEach(i => {
    worker.push(
        spawnStateless(system, async (msg, ctx) =>  {
            //console.log(ctx.name, ', message: ', msg.name, ', working...');
            await delay(500);
            //console.log(ctx.name, ', sending back to balancer...');
            dispatch(balancer, { name: msg.name, flag: 2, index: i });
          }, `worker-${i}`));
  });

console.log(worker.length);


const balancer = spawnStateless(system, async (msg, ctx) =>  {

    if(msg.flag == 1){
        if(array.length < 1){
            //console.log('No free actor workers, adding to stack for later ', msg.name);
            bottleneck.push(msg.name);     
        } else {
            send(msg.name);
        }
    }
    if(msg.flag == 2){
        console.log('Received from worker-', msg.index, ', value : ', msg.name);
        array.push(msg.index);
        if(bottleneck.length > 0){    
            send(bottleneck.pop());
        }
    }

    function send(name){
        //console.log('Balancer, value : ', name, '. Sending to worker.');
        dispatch(worker[array.pop()], { name: name});
    }

  }, 'balancer');




  names.forEach(element => {
    //console.log(element);
    dispatch(balancer, { name: element, flag: 1 });
});

/*
const ping = spawnStateless(system, async (msg, ctx) =>  {
  //console.log(msg.value);
  // ping: Pong is a little slow. So I'm giving myself a little handicap :P
  await delay(500);
  //dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
  dispatch(msg.sender, { value: msg.value, sender: ctx.self });
}, 'ping');

const pong = spawnStateless(system, (msg, ctx) =>  {
  console.log(msg.value);
  //dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
}, 'pong');


const map1 = array.map(el => el = el * 2);

map1.forEach(element => {
    console.log(element);
});


dispatch(ping, { value: 'begin1', sender:pong });
dispatch(ping, { value: 'begin2', sender:pong });
dispatch(ping, { value: 'begin3', sender:pong });
dispatch(ping, { value: 'begin4', sender:pong });
*/
