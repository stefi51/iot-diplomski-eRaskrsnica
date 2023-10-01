import React from "react";
import './App.css';
import { BrowserRouter, Route, Switch } from "react-router-dom";
import Dashboard from "./components/Dashboard";
import DevicePanel from "./components/DeviceControlPanel";
import  MultipleAxisChart  from "./components/Chart";



function App() {
  return (
    <BrowserRouter>
      <Switch>
        <Route path="/" exact component={Dashboard} />        
        <Route path="/devicePanel" component={DevicePanel} />
        <Route path="/chart" component={MultipleAxisChart} />
      </Switch>
    </BrowserRouter>
  );
}
export default App;
