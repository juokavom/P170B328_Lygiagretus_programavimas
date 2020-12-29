'use strict';
const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();
//const delay = (time) => new Promise((res) => setTimeout(res, time));

const array = [...Array(7).keys()];
const data = require('./Data/IFF8-12_AkramasJ_L1_dat_1.json')['items'];

const balancer = spawnStateless(system, async (msg, ctx) => {
    if (msg.flag == 1) {
        //console.log('Balancer, value : ', msg.item, '. Sending to worker.');
        const index = array.shift();
        dispatch(worker[index], { item: msg.item });
        array.push(index);
    }
    if (msg.flag == 2) {
        //console.log('Received from worker-',msg.worker, ', value : ', msg.item);
        dispatch(repository, { item: msg.item, flag: 1 });
    }
    if (msg.flag == 3) {
        dispatch(repository, { flag: 2 });
    }
    if (msg.flag == 4) {
        dispatch(printer, { items: msg.items });
    }

}, 'balancer');

const worker = [];
array.forEach(i => {
    worker.push(
        spawnStateless(balancer, async (msg, ctx) => {
            //console.log(ctx.name, ', message: ', msg.item, ', working...');
            msg.item.result = ([...Buffer.from(msg.item.title)].reduce((sum, i) => sum + i, 0) ^ msg.item.quantity) * msg.item.price;
            if (msg.item.result % 1 > 0.5) {
                //console.log(ctx.name, ', sending back to parent(balancer)...');
                dispatch(ctx.parent, { item: msg.item, flag: 2, worker: i });
            }
        }, `worker-${i}`));
});

const repo = [];
const repository = spawnStateless(balancer, async (msg, ctx) => {
    if (msg.flag == 1) {
        repo.push(msg.item);
        repo.sort((a, b) => a.result - b.result);
    }
    if (msg.flag == 2) {
        dispatch(ctx.parent, { items: repo, flag: 4 });
    }
}, 'repository');

const printer = spawnStateless(balancer, async (msg, ctx) => {
    const output = '-------------------------------------------------\n|Title               |Quantity|Price  |Result   |' +
        '\n-------------------------------------------------\n'.concat(msg.items.reduce((output, i) => output.concat('|', i.title.padEnd(20), '|',
            i.quantity.toString().padEnd(8), '|', i.price.toString().padEnd(7), '|', i.result.toFixed(2).toString().padEnd(9), '|\n'), ''),
            '-------------------------------------------------');
    const fs = require('fs');
    fs.writeFile('./Data/IFF8-12_AkramasJ_Egzaminas_rez.txt', output, function (err) {
        if (err) throw err;
        console.log('Printed!');
    });
}, 'printer');

data.map(element => {
    dispatch(balancer, { item: element, flag: 1 });
});
dispatch(balancer, { flag: 3 });