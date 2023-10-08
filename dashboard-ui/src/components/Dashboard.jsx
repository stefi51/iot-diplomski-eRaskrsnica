import React from 'react';
import ListGroup from 'react-bootstrap/ListGroup'
import BaseService from '../CommunicationLayer'
import { Image, Container, Button } from 'react-bootstrap';
import CanvasJSReact from '@canvasjs/react-charts';
var CanvasJSChart = CanvasJSReact.CanvasJSChart;
class Dashboard extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            devices: [],
            devicesData: [],
            dataPoints: [],
            options: {
                theme: "light2",
                animationEnabled: true,
                exportEnabled: true,
                title: {
                    text: "Average speed"
                },
                axisY: {
                    title: "Speed (km/h)"
                },
                axisX: {
                    title: "Timeline",
                    intervalType: "hour",
                    valueFormatString: "YYYY-MM-DD HH:mm:ss",
                    labelMaxWidth: 1000,
                },
                toolTip: {
                    shared: true
                },
                legend: {
                    verticalAlign: "center",
                    horizontalAlign: "right",
                    reversed: true,
                    cursor: "pointer",
                    itemclick: this.toggleDataSeries
                },
                data: []
            }
        }

    }

    goToDevice = async (device) => {
        this.props.history.push({
            pathname: `/devicePanel`,
            state: {device }
        });
    }

    printConnectedDevices = () => {
        let elements = [];
        this.state.devices.map((item, index) =>
            elements.push(<ListGroup.Item action variant="primary">
                <div style={{ display: 'flex', flexDirection: 'row', backgroundColor: 'whitesmoke' }} >
                    <div style={{ width: '100%' }}>
                        <div class="card-header">
                            <h3 class="card-title">Device ID: {item.deviceId}</h3>
                        </div>
                        <div class="card-body">
                            Is device active: {item.isActive.toString()}
                        </div>
                        <div class="card-body">
                            Telemetry interval: {item.telemetryInterval} s
                        </div>
                    </div>
                    <div style={{ display: 'flex', width: '100%', justifyContent: 'center', flexDirection: 'column', paddingRight: '15px' }}>

                        <Button style={{ paddingRight: '15px' }} onClick={() => this.goToDevice(item)}> Device commands </Button>
                    </div>
                </div>
            </ListGroup.Item>)
        );

        return elements;
    }
    componentDidMount = async () => {
        var data = await BaseService.getConnectedDevices();

        var deviceAllData = await BaseService.getAllData();

        const groupedData = deviceAllData.reduce((groups, item) => {
            var category = item.deviceId;
            if (!groups[category]) {
                groups[category] = [];
            }
            groups[category].push(item.body);
            return groups;
        }, {});

        console.log(groupedData);

        let graphElements = [];
        Object.entries(groupedData).map(([group, data]) =>
            graphElements.push({
                type: "spline",
                name: group,
                showInLegend: true,
                xValueType: "dateTime",
                dataPoints: data.reduce((res, d) => {

                    res.push({ x: new Date(d.timeStamp), y: d.averageSpeedPerLane })
                    return res;
                }, [])
            }
            )
        )
        console.log(graphElements);
        var someProperty = this.state.options;
        someProperty.data = graphElements;
        this.setState({ options: someProperty });
        this.chart.render();
        this.setState({ devices: data });
    }

    toggleDataSeries = (e) => {
        if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
            e.dataSeries.visible = false;
        }
        else {
            e.dataSeries.visible = true;
        }
        this.chart.render();
    }

    render() {

        return (
            <div class="celaStrana">
                <div class="firstPart">
                    <div class="firstItem">
                        <div>Connected(Available) devices: </div>
                        <div>
                            <ListGroup>
                                {this.printConnectedDevices()}
                            </ListGroup>
                        </div>
                    </div>
                    <div class="secondItem">2 za live data dijagram</div>
                </div>
                <div class="secondPart" style={{ paddingRight: '15px' }}>
                    <CanvasJSChart options={this.state.options}
                        onRef={ref => this.chart = ref}
                    />
                </div>
            </div>
        )
    }

}

export default Dashboard