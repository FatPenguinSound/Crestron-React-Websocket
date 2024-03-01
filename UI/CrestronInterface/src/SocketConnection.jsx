import ReconnectingWebSocket from 'reconnecting-websocket';

function SocketConnection(){
    let address= "169.254.245.163"
    let url = `ws://${address}:5000/app`
    const rws = new ReconnectingWebSocket(url, [], {debug: false, startClosed: true});

    rws.addEventListener('open', () => {
    console.log("Connected to server.");
    //rws.send("Hello server.");
    });

    rws.addEventListener('close', () => {
    console.log("Connection to server closed.");
    });

    rws.addEventListener('error', () => {
    console.log("Error on socket.");
    });

    rws.reconnect();

    return rws;
}

export default SocketConnection;
