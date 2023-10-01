import React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import BaseService from '../CommunicationLayer'
import { Image, Container, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';

class Dashboard extends React.Component {

    constructor(props) {
        super(props);
        this.state = {

            devices: [],
            devicesData: []
        }
    }

    goToDevice = async(deviceId, isActive)=>{
        this.props.history.push({
          pathname: `/devicePanel`,
          state: { deviceId: deviceId, isActive: isActive}
        });       
      }

    printConnectedDevices = () => {
        let elements = [];

        this.state.devices.map((item, index) =>
            elements.push(<ListGroup.Item action variant="info">
            <div style={{display:'flex',flexDirection:'row',backgroundColor:'whitesmoke'}} >
               <div class="card bg-primary" style={{width: '100%'}}>
                    <div class="card-header">
                        <h3 class="card-title">Device ID: {item.deviceId}</h3>
                    </div>
                    <div class="card-body">
                        Is device active: {item.isActive.toString()}
                    </div>
                    <div class="card-footer">
                        Telemetry interval: {item.telemetryInterval}
                    </div>
                </div>
                <div style={{display:'flex', width: '100%'}}>
                    live data
                </div>
                <div style={{display:'flex',  width: '100%'}}>
                <Button onClick={()=>this.goToDevice(item.deviceId, item.isActive)} variant="link" className="ml-2"> 
                {item.deviceId} </Button>
                </div>
                </div>
            </ListGroup.Item>)
        );

        return elements;
    }




    componentDidMount = async () => {
        var data = await BaseService.getConnectedDevices();
        this.setState({ devices: data });
    }

    render() {

        return (
            <div class="celaStrana">
                 
                <div class="firstPart">
                    <div class="firstItem">
                        <div>Connected devices: </div>
                        <div>
                            <ListGroup>
                                {this.printConnectedDevices()}
                            </ListGroup>
                        </div>
                    </div>
                    <div class="firstItem">2 za live data dijagram</div>
                </div>
                <div class="secondPart">
                    3 za opste podatke
                    <div class="card bg-gradient-success">
                        <div class="card-header">
                            <h3 class="card-title">Success Card Example</h3>
                        </div>
                        <div class="card-body">
                            The body of the card
                        </div>
                        <div class="card-footer">
                            The footer of the card
                        </div>
                    </div>
                </div>
            </div>
        )
    }

}

export default Dashboard