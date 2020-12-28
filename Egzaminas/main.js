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
            console.log(ctx.name, ', message: ', msg.item, ', working...');

            // var myBuffer = [];
            // var buffer = new Buffer(msg.item.title, 'utf16le');
            // for (var i = 0; i < buffer.length; i++) {
            //     myBuffer.push(buffer[i]);
            // }     
            await delay(500);
            console.log(ctx.name, ', sending back to balancer...');
            dispatch(balancer, { item: msg.item, flag: 2, index: i });
          }, `worker-${i}`));
});

const bottleneck = [];
const balancer = spawnStateless(system, async (msg, ctx) =>  {
    if(msg.flag == 1){
        send(msg.item);
    }
    if(msg.flag == 2){
        console.log('Received from worker-',msg.index, ', value : ', msg.item);
    }

    function send(item){
        console.log('Balancer, value : ', item, '. Sending to worker.');
        const index = array.shift();
        dispatch(worker[index], { item: item});
        array.push(index);
    }

}, 'balancer');


data['items'].forEach(element => {
    dispatch(balancer, { item: element, flag: 1 });
});
