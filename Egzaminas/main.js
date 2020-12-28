const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();

const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(3).keys()];
const data = ['First', 'Second', 'Third', 'Fourth', 'Fifth'];


const worker = [];


array.forEach(i => {
    worker.push(
        spawnStateless(system, async (msg, ctx) =>  {
            //console.log(ctx.name, ', message: ', msg.item, ', working...');
            await delay(500);
            //console.log(ctx.name, ', sending back to balancer...');
            dispatch(balancer, { item: msg.item, flag: 2, index: i });
          }, `worker-${i}`));
  });

console.log(worker.length);


const bottleneck = [];
const balancer = spawnStateless(system, async (msg, ctx) =>  {

    //Send to worker
    if(msg.flag == 1){
        if(array.length < 1){
            //console.log('No free actor workers, adding to queue for later ', msg.name);
            bottleneck.push(msg.item);     
        } else {
            send(msg.item);
        }
    }
    //Receive from worker
    if(msg.flag == 2){
        console.log('Received from worker-', msg.index, ', value : ', msg.item);
        array.push(msg.index);
        if(bottleneck.length > 0){    
            send(bottleneck.shift());
        }
    }

    function send(item){
        //console.log('Balancer, value : ', name, '. Sending to worker.');
        dispatch(worker[array.shift()], { item: item});
    }

}, 'balancer');


data.forEach(element => {
    //console.log(element);
    dispatch(balancer, { item: element, flag: 1 });
});
