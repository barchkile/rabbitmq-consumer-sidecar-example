const express = require('express');
const app = express();
const http = require('http').Server(app);
const io = require('socket.io')(http);
const port = process.env.PORT || 3000;
app.use(express.json())

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

io.on('connection', (socket) => {
  console.log("new connection");
  socket.on('message', message => {
    console.log("socket message", message);
    sendMessage(message);
  });
});

app.get('/is-alive', (req,res) => {
  console.log('is-alive');
  res.sendStatus(200);
});

app.post('/message', (req,res) => {
  sendMessage(JSON.stringify(req.body));
  res.send({Success: true});
});

const sendMessage = message => {
  console.log('message', message);
  io.emit('message', message);
};

http.listen(port, () => {
  console.log(`MyApp running at http://localhost:${port}/`);
});
