import React from "react";
import './App.css';
import { BrowserRouter, Route, Switch } from "react-router-dom";
import Dashboard from "./components/Dashboard";
import DevicePanel from "./components/DeviceControlPanel";



function App() {
  return (
    <BrowserRouter>
      <Switch>
        <Route path="/" exact component={Dashboard} />        
        <Route path="/devicePanel" component={DevicePanel} />
      </Switch>
    </BrowserRouter>
  );
}
export default App;
