import { WebXPanel, isActive } from "@crestron/ch5-webxpanel";
import { CrComLib } from "@crestron/ch5-crcomlib/build_bundles/cjs/cr-com-lib";

import {
  bridgeReceiveIntegerFromNative,
  bridgeReceiveBooleanFromNative,
  bridgeReceiveStringFromNative,
  bridgeReceiveObjectFromNative,
} from "@crestron/ch5-crcomlib/build_bundles/cjs/cr-com-lib";

import { useRef, useState, useEffect } from 'react';
import './App.css'
import SocketConnection from './SocketConnection';
import { Slider, sliderClasses } from '@mui/base'
import _ from 'lodash'

const configuration = { 
  host: 'window.location.host', // defaults to window.location.host 
  ipId: '0x0f', // string representing a hex value. Might contain "0x" or not. Defaults to "0x03" 
}; 

if (isActive){ 
  WebXPanel.initialize(configuration); 
} 


window.CrComLib = CrComLib;
window["bridgeReceiveIntegerFromNative"] = bridgeReceiveIntegerFromNative;
window["bridgeReceiveBooleanFromNative"] = bridgeReceiveBooleanFromNative;
window["bridgeReceiveStringFromNative"] = bridgeReceiveStringFromNative;
window["bridgeReceiveObjectFromNative"] = bridgeReceiveObjectFromNative;

let socket = null;

function App() {
  const [colour, setColour] = useState("rgb(100, 100, 100)");

  useEffect(() => {
    socket = SocketConnection();
    socket.addEventListener('message', (event) => {
      console.log("Message from server.", event.data)
      const msg = JSON.parse(event.data);
      const newColour = `rgb(${msg.Value})`
      console.log(`New colour: ${newColour}`);
      setColour(newColour);
    });

    socket.reconnect();

    return () => {
      socket.close();
    }


  }, [])


  const Buttons = () => {
    const b = [];
    for(let i = 1; i < 4; i++)
    {
      b.push(<Button key={i} onPress={_.debounce((num) => { 
       socket.send(JSON.stringify({Control: "Button", Value: num}))}, 150)
      } id={i}>Button {i}</Button>);
    }
    return b;
  }

  return (
    <div className='Content' style={{backgroundColor: colour}}>
      <h1>Crestron Websocket Test Framework</h1>
      <div className='Buttons'>
        {Buttons()}
      </div>
      <div className='Sliders'>
      <Slider
        slotProps={{
          root: { className: 'Slider' },
          rail: { className: 'Slider-rail' },
          track: { className: 'Slider-track' },
          thumb: { className: 'Slider-thumb' },
        }}
        defaultValue={50}
        onChange={(e, val) => { socket.send(JSON.stringify({Control: "Slider", Value: val})); }}
      />
      </div>
    </div>
  )
}

function Button({id, onPress, children}){
  const num = id;
  return (
    <button className='Button' onClick={() => {onButtonPress(num); onPress(num);}}>{children}</button>
  )
}

function onButtonPress(btn){
  //console.log(`Button ${btn} has been pressed.`);  
}

export default App;
