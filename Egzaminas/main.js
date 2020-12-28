const { start, dispatch, stop, spawnStateless } = require('nact');
const system = start();

const delay = (time) => new Promise((res) => setTimeout(res, time));

const ping = spawnStateless(system, async (msg, ctx) =>  {
  console.log(msg.value);
  // ping: Pong is a little slow. So I'm giving myself a little handicap :P
  await delay(500);
  dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
}, 'ping');

const pong = spawnStateless(system, (msg, ctx) =>  {
  console.log(msg.value);
  dispatch(msg.sender, { value: ctx.name, sender: ctx.self });
}, 'pong');

dispatch(ping, { value: 'begin', sender:pong });