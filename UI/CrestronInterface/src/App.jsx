import './App.css'
import SocketConnection from './SocketConnection';

function App() {
  const socket = SocketConnection();

  const Buttons = () => {
    const b = [];
    for(let i = 1; i < 4; i++)
    {
      b.push(<Button key={i} id={i}>Button {i}</Button>);
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
        <input type='range' min={1} max={100} defaultValue={50} className='Slider'/>
      </div>
    </div>
  )
}

function Button({id, children}){
  const num = id;
  return (
    <button className='Button' onClick={() => onButtonPress(num)}>{children}</button>
  )
}

function onButtonPress(btn){
  console.log(`Button ${btn} has been pressed.`);
}

export default App;
