'use strict';
const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();
const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(7).keys()];
const data = require('./Data/IFF8-12_AkramasJ_L1_dat_1.json');

const worker = [];
array.forEach(i => {
    worker.push(
        spawnStateless(system, async (msg, ctx) =>  {
            //console.log(ctx.name, ', message: ', msg.item, ', working...');
            const bytes = [...Buffer.from(msg.item.title)];
            const result = (bytes.reduce((sum, i) => sum + i, 0) ^ msg.item.quantity) * msg.item.price;
            if(result %1 > 0.5){            
                //console.log(ctx.name, ', sending back to balancer...');
                dispatch(balancer, { item: msg.item, flag: 2, worker: i, result: result });
            }
          }, `worker-${i}`));
});

const repo = [];
const repository = spawnStateless(system, async (msg, ctx) =>  {
    if(msg.flag == 1){
        repo.push(msg.item);
    }
    if(msg.flag == 2){
        dispatch(balancer, { items: repo, flag: 4 });
    }
}, 'repository');

const printer = spawnStateless(system, async (msg, ctx) =>  {
    console.log(msg.items);
}, 'printer');

const balancer = spawnStateless(system, async (msg, ctx) =>  {
    if(msg.flag == 1){
        //console.log('Balancer, value : ', msg.item, '. Sending to worker.');
        const index = array.shift();
        dispatch(worker[index], { item: msg.item});
        array.push(index);
    }
    if(msg.flag == 2){
        //console.log('Received from worker-',msg.worker, ', value : ', msg.item, ', result: ', msg.result);
        dispatch(repository, {item: msg.item, result: msg.result, flag: 1});
    }
    if(msg.flag == 3){
        dispatch(repository, {flag: 2});
    }
    if(msg.flag == 4){
        dispatch(printer, {items: msg.items});
    }

}, 'balancer');


data['items'].forEach(element => {
    dispatch(balancer, { item: element, flag: 1 });
});
dispatch(balancer, { flag: 3 });