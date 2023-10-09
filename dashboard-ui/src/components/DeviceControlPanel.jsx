
import React from 'react';
import BaseService from '../CommunicationLayer'
import Switch from "react-switch";
import CanvasJSReact from '@canvasjs/react-charts';
import DataTable from 'react-data-table-component';
import { Image, Container, Button } from 'react-bootstrap';
import { useEffect, useState } from "react"
var CanvasJSChart = CanvasJSReact.CanvasJSChart;

class DevicePanel extends React.Component {

  constructor(props) {
    super(props);
    this.state = {
      device: this.props.location.state.device,
      isActive: this.props.location.state.device.isActive,
      telemetryInterval: this.props.location.state.device.telemetryInterval,
      telemetryIntervalInput: "",
      numberOfLanesInput: this.props.location.state.device.numberOfLanes,
      numberOfLanes: this.props.location.state.device.numberOfLanes,
      devicesData: [],
      messageInput: "",
      carAccidentReported:true,
      refinedData:[],
      delay: 2000,
      count:0
    }
    this.handleChange = this.handleChange.bind(this);
    this.handleChangeInput = this.handleChangeInput.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleChangeTextArea = this.handleChangeTextArea.bind(this);

    this.handleSubmitMessage = this.handleSubmitMessage.bind(this);

    this.handleChangeLaneInput = this.handleChangeLaneInput.bind(this);
    this.handleSubmitLanes = this.handleSubmitLanes.bind(this);

  }

  async handleChange(isActive) {
    if (isActive) {
      await BaseService.turnOnDevice(this.state.device.deviceId);
    } else {
      await BaseService.turnOffDevice(this.state.device.deviceId);
    }
    this.setState({ isActive });
  }

  handleChangeInput(event) {
    this.setState({ telemetryIntervalInput: event.target.value });
  }

  handleChangeTextArea(event) {
    this.setState({ messageInput: event.target.value });
  }

  async handleSubmit(event) {
    event.preventDefault();
    this.setState({ telemetryInterval: this.state.telemetryIntervalInput });
    await BaseService.updateTelemetryInterval(this.state.device.deviceId, this.state.telemetryIntervalInput)
  }

  async handleSubmitMessage(event) {
    event.preventDefault();
    await BaseService.sendMessageToDevice(this.state.device.deviceId, this.state.messageInput)
    this.setState({ messageInput: "" });
  }

  handleChangeLaneInput(event) {
    this.setState({ numberOfLanesInput: event.target.value });
  }

  async handleSubmitLanes(event) {
    event.preventDefault();
    this.setState({ numberOfLanes: this.state.numberOfLanesInput });
    await BaseService.updateNumOfLanes(this.state.device.deviceId, this.state.numberOfLanesInput)
  }

  componentDidMount = async () => {

    var devices = await BaseService.getConnectedDevices();
    var device = devices.filter((x)=> x.deviceId== this.state.device.deviceId);
    this.setState({isActive: device.isActive})

    var data = await BaseService.getDeviceData(this.state.device.deviceId);
    this.setState({ carAccidentReported: data[0].body.reportedAccident })
    this.setState({ devicesData: data })

    var refinedData= await BaseService.getRefinedData(this.state.device.deviceId);
    this.setState({refinedData: refinedData})

    this.interval = setInterval(this.tick, this.state.delay);

  }


  componentDidUpdate= async(prevProps, prevState)=> {
    if (prevState.delay !== this.state.delay) {
      clearInterval(this.interval);

      var devices = await BaseService.getConnectedDevices();
      var device = devices.filter((x)=> x.deviceId== this.state.device.deviceId);
      this.setState({isActive: device.isActive})
  
      var data = await BaseService.getDeviceData(this.state.device.deviceId);
      this.setState({ carAccidentReported: data[0].body.reportedAccident })
      this.setState({ devicesData: data })
  
      var refinedData= await BaseService.getRefinedData(this.state.device.deviceId);
      this.setState({refinedData: refinedData})
      this.interval = setInterval(this.tick, this.state.delay);
    }

  }

  tick = () => {
    this.setState({
      count: this.state.count + 1
    });
  }

   onResolveClick = async () => {
    await BaseService.resolveAccident(this.state.device.deviceId);
    this.setState({carAccidentReported: false})
  };
  render() {

    const columns = [
      {
        name: 'Vehicle per hour',
        selector: row => row.body.vehiclePerHour,
      },
      {
        name: 'Average speed',
        selector: row => row.body.averageSpeedPerLane,
      },
      {
        name: 'Temperature',
        selector: row => row.body.temperature,
      },
      {
        name: 'Air quality index',
        selector: row => row.body.airQualityIndex,
      },
      {
        name: 'Active lanes',
        selector: row => row.body.numberOfLanes,
      },
      {
        name: 'Date',
        selector: row => row.body.timeStamp,
      }

    ];

    const columnsRefined = [
      {
        name: 'State',
        selector: row => row.intersectionState,
      },
      {
        name: 'Average speed',
        selector: row => row.rawData.averageSpeedPerLane,
      },
      {
        name: 'Air quality',
        selector: row => row.airQuality,
      },
      {
        name: 'Air quality index',
        selector: row => row.rawData.airQualityIndex,
      },
      {
        name: 'Was accident',
        selector: row => row.rawData.reportedAccident.toString(),
      }

    ];
    const ExpandedComponent = ({ data }) => <pre>{JSON.stringify(data, null, 2)}

      {data.Date}
    </pre>;

    return (
      <div class="celaStrana" >
        <div style={{ display: 'flex', flexDirection: 'row', width: '100%', height: '100%', flex: 1 }} >
          <div style={{ display: 'flex', flexDirection: "column", width: '100%', height: '100%', flex: 1 }}>
            <div>
              <h4>Device info:</h4>
            </div>
            <div style={{ width: '100%' }}>
              <div class="card-header">
                <h3 class="card-title">Device ID: {this.state.device.deviceId}</h3>
              </div>
              <div class="card-body">
                Telemetry interval: {this.state.telemetryInterval} s
              </div>
              <div class="card-body">
                Number of lanes: {this.state.numberOfLanes}
              </div>
              <div class="card-body">
                Reported car accident: {this.state.carAccidentReported.toString()}
                <div style={{ display: 'flex', width: '100%', justifyContent: 'center', flexDirection: 'column', paddingRight: '15px' }}>
                  <Button style={{ paddingRight: '15px' }} disabled={this.state.carAccidentReported? false: true} 
                  onClick={()=> this.onResolveClick()} > Resolve accident. </Button>
                </div>
              </div>
            </div>
            <div>
              Turn on/ turn off. <Switch onChange={this.handleChange} checked={this.state.isActive} />

            </div>

            <div>
              Update telemetry interval:
              <form onSubmit={this.handleSubmit}>
                <label>
                  <input
                    type="number"
                    min="1"
                    value={this.state.telemetryIntervalInput}
                    onChange={this.handleChangeInput}
                  />
                </label>
                <input type="submit" value='Submit' />
              </form>
            </div>
            <div>
              Send message to device.
              <form onSubmit={this.handleSubmitMessage}>
                <label>
                  <textarea
                    value={this.state.messageInput}
                    onChange={this.handleChangeTextArea}
                  />
                </label>
                <input type="submit" value='Submit' />
              </form>
            </div>
            <div>
              Change number of lanes:
              <form onSubmit={this.handleSubmitLanes}>
                <label>
                  <input
                    type="number"
                    min="1"
                    value={this.state.numberOfLanesInput}
                    onChange={this.handleChangeLaneInput}
                  />
                </label>
                <input type="submit" value='Change' />
              </form>

            </div>
          </div>

          <div style={{ flex: 1 }}> Refined data:

          <DataTable
            columns={columnsRefined}
            data={this.state.refinedData}
            expandableRows
            expandableRowsComponent={ExpandedComponent}
            pagination
          />
          
          </div>
        </div>
        <div style={{ width: '100%', height: '100%', flex: 1, paddingRight: '15px', paddingLeft: '15px' }}>
          Raw data:
          <DataTable
            columns={columns}
            data={this.state.devicesData}
            expandableRows
            expandableRowsComponent={ExpandedComponent}
            pagination
          />
        </div>
      </div>

    );
  }
}

export default DevicePanel