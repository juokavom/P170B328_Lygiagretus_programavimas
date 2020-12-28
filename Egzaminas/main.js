const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();

const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(3).keys()];
const names = ['First', 'Second', 'Third', 'Fourth', 'Fifth'];


const worker = [];

array.forEach(i => {
    worker.push(
        spawnStateless(system, async (msg, ctx) =>  {
            console.log(msg.name);
            await delay(500);
          }));
  });

console.log(worker.length);

dispatch(worker[2], { name: 'begin1' });

/*
const balancer = spawnStateless(system, async (msg, ctx) =>  {
    console.log('Balancer, value : ', msg.name);
    console.log(array);
    array.forEach(el => {if(el < 15){
        dispatch(worker[el], { name: element });

    }});
    // ping: Pong is a little slow. So I'm giving myself a little handicap :P
    await delay(500);
    //dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
    //dispatch(msg.sender, { value: msg.value, sender: ctx.self });
  }, 'balancer');




  names.forEach(element => {
    console.log(element);
    dispatch(balancer, { name: element });
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
