//Jokūbas Akramas IFF-8/12
//Egzamino užduotis
//2020-12-29
//P170B328 Lygiagretusis programavimas

'use strict';
const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();
//const delay = (time) => new Promise((res) => setTimeout(res, time));

//Darbininkų indeksų masyvas
const array = [...Array(7).keys()];
//Duomenų failas
const data = require('./Data/IFF8-12_AkramasJ_L1_dat_1.json')['items'];

//Aktorius skirstytuvas, system vaikas
const balancer = spawnStateless(system, async (msg, ctx) => {
    //Gaunamas įrašas iš pradinio scenarijaus ir persiunčiamas darbininkui
    //Darbininkas išrenkamas iš indeksų masyvo ir dedamas į galą (vėliavėlė = 1)
    if (msg.flag == 1) {
        //console.log('Balancer, value : ', msg.item, '. Sending to worker.');
        const index = array.shift();
        //Įrašas siunčiamas išrinktam darbininkui į mailboxą
        dispatch(worker[index], { item: msg.item });
        array.push(index);
    }
    //Iš darbininko gaunamas įrašas, kuris atitiko kriterijų
    //ir persiunčiamas rezultatų kaupikliui (vėliavėlė = 2)
    if (msg.flag == 2) {
        //console.log('Received from worker-',msg.worker, ', value : ', msg.item);
        dispatch(repository, { item: msg.item, flag: 1 });
    }
    //Iš rezultatų kaupiklio prašoma persiųsti rezultatų masyvą (vėliavėlė = 3)
    if (msg.flag == 3) {
        dispatch(repository, { flag: 2 });
    }
    //Rezultatų kaupiklis persiuntė rezultatus, jie siunčiami spausdintojui (vėliavėlė = 4)
    if (msg.flag == 4) {
        dispatch(printer, { items: msg.items });
    }
}, 'balancer');

const worker = [];
//Kiekvienam darbininkų indeksui sukuriamas darbininkas
array.forEach(i => {
    worker.push(
        //Aktorius darbininkas, skirstytuvo(balancer) vaikas
        spawnStateless(balancer, async (msg, ctx) => {
            //console.log(ctx.name, ', message: ', msg.item, ', working...');
            //Skaičiuojamas rezultatas
            msg.item.result = ([...Buffer.from(msg.item.title)].reduce((sum, i) => sum + i, 0) ^ msg.item.quantity) * msg.item.price;
            //Rezultatui atitikus kriterijų šis siunčiamas skirstytuvui (vėliavėlė = 2)
            if (msg.item.result % 1 > 0.5) {
                //console.log(ctx.name, ', sending back to parent(balancer)...');
                dispatch(ctx.parent, { item: msg.item, flag: 2, worker: i });
            }
        }, `worker-${i}`));
});

//Atfiltruotų rezultatų masyvas
const repo = [];
//Aktorius rezultatų kaupiklis, skirstytuvo(balancer) vaikas
const repository = spawnStateless(balancer, async (msg, ctx) => {
    //Po vieną gaunamas įrašas, kaupiamas surikiuotame masyve (vėliavėlė = 1)
    if (msg.flag == 1) {
        repo.push(msg.item);
        repo.sort((a, b) => a.result - b.result);
    }
    //Rezultatų masyvas persiunčiamas skirstytuvui (vėliavėlė = 2)
    if (msg.flag == 2) {
        dispatch(ctx.parent, { items: repo, flag: 4 });
    }
}, 'repository');

//Aktorius spausdintojas, skirstytuvo(balancer) vaikas
const printer = spawnStateless(balancer, async (msg, ctx) => {
    //Formuojamas tekstinis rezultatas iš gauto masyvo elementų
    const output = '-------------------------------------------------\n|Title               |Quantity|Price  |Result   |' +
        '\n-------------------------------------------------\n'.concat(msg.items.reduce((output, i) => output.concat('|', i.title.padEnd(20), '|',
            i.quantity.toString().padEnd(8), '|', i.price.toString().padEnd(7), '|', i.result.toFixed(2).toString().padEnd(9), '|\n'), ''),
            '-------------------------------------------------');
    //Rezultatas išvedamas į failą
    require('fs').writeFile('./Data/IFF8-12_AkramasJ_Egzaminas_rez.txt', output, function (err) {
        if (err) throw err;
        console.log('Printed!');
    });
}, 'printer');

data.map(element => {
    dispatch(balancer, { item: element, flag: 1 });
});
dispatch(balancer, { flag: 3 });