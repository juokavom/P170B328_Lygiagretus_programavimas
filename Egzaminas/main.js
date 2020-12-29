'use strict';
const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();
const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(7).keys()];
const data = require('./Data/IFF8-12_AkramasJ_L1_dat_3.json');

const worker = [];
array.forEach(i => {
    worker.push(
        spawnStateless(system, async (msg, ctx) =>  {
            //console.log(ctx.name, ', message: ', msg.item, ', working...');
            const bytes = [...Buffer.from(msg.item.title)];
            const result = (bytes.reduce((sum, i) => sum + i, 0) ^ msg.item.quantity) * msg.item.price;
            //console.log(ctx.name, ', sending back to balancer...');
            if(result %1 > 0.5) dispatch(balancer, { item: msg.item, flag: 2, worker: i, result: result });
          }, `worker-${i}`));
});

const balancer = spawnStateless(system, async (msg, ctx) =>  {
    if(msg.flag == 1){
        send(msg.item);
    }
    if(msg.flag == 2){
        console.log('Received from worker-',msg.worker, ', value : ', msg.item, ', result: ', msg.result);
    }

    function send(item){
        //console.log('Balancer, value : ', item, '. Sending to worker.');
        const index = array.shift();
        dispatch(worker[index], { item: item});
        array.push(index);
    }

}, 'balancer');


data['items'].forEach(element => {
    dispatch(balancer, { item: element, flag: 1 });
});
