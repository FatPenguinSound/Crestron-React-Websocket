import { useRef } from 'react';
import './App.css'
import SocketConnection from './SocketConnection';
import ReconnectingWebSocket from 'reconnecting-websocket';
import { Slider, sliderClasses } from '@mui/base'

function App() {
  const socket = useRef(SocketConnection());

  const Buttons = () => {
    const b = [];
    for(let i = 1; i < 4; i++)
    {
      b.push(<Button key={i} onPress={(num) => { socket.current.send(`Button: ${num}`) }} id={i}>Button {i}</Button>);
    }
    return b;
  }

  return (
    <div className='Content'>
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
        onChange={(e, val) => { socket.current.send(`Slider: ${val}`); }}
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
  console.log(`Button ${btn} has been pressed.`);  
}

export default App;
