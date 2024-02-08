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
                    intervalType: "minutes",
                    valueFormatString: "hh TT K",                   
                    interval: 30
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
            },
            optionsAir: {
                animationEnabled: true,
                exportEnabled: true,
                title: {
                    text: "Air quality"
                },
                subtitles: [{
                    text: "Lower is better."
                }],
                axisY: {
                    title: "Air index quality (AQI)"
                },
                toolTip: {
                    shared: true
                },
                legend: {
                    cursor: "pointer",
                    itemclick: this.toggleDataSeries2
                },
                data: []
            }

        }

    }

    goToDevice = async (device) => {
       // clearInterval(this.intervalId);
        this.props.history.push({
            pathname: `/devicePanel`,
            state: { device }
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
        await this.loadData2();
    }

    componentWillUnmount() {
        clearInterval(this.intervalId);
    }

    loadData2 = async () => {
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

        let graphElements = [];
        Object.entries(groupedData).map(([group, data]) =>
            graphElements.push({
                type: "spline",
                name: group,
                showInLegend: true,
                xValueType: "dateTime",
                intervalType: "minutes",
                valueFormatString: "hh TT K",                   
                interval: 30,
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

        var airData = await BaseService.getAirQualityData("intersection-1");
        var intersection1graph = [];

        airData.map((el) => {
            intersection1graph.push({ x: new Date(el.rawDate), y: el.airQualityIndex })
        });

        var airint2Data = await BaseService.getAirQualityData("raskrsnica-3");
        var intersection2graph = [];

        airint2Data.map((el) => {
            intersection2graph.push({ x: new Date(el.rawDate), y: el.airQualityIndex })
        });

        var airData3 = await BaseService.getAirQualityData("Vozda-Karadjordja-1");
        var intersection3graph = [];

        airData3.map((el) => {
            intersection3graph.push({ x: new Date(el.rawDate), y: el.airQualityIndex })
        });



        let graphElements2 = [];

        graphElements2.push({
            type: "spline",
            name: "intersection-1",
            showInLegend: true,
            xValueType: "dateTime",
            dataPoints: intersection1graph
        });
        graphElements2.push({
            type: "spline",
            name: "raskrsnica-3",
            showInLegend: true,
            xValueType: "dateTime",
            dataPoints: intersection2graph
        });
        graphElements2.push({
            type: "spline",
            name: "Vozda-Karadjordja-1",
            showInLegend: true,
            xValueType: "dateTime",
            dataPoints: intersection3graph
        });
        var airProperty = this.state.optionsAir;
        airProperty.data = graphElements2;
        this.setState({ optionsAir: airProperty });
        this.chart2.render();
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

    toggleDataSeries2 = (e) => {
        if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
            e.dataSeries.visible = false;
        }
        else {
            e.dataSeries.visible = true;
        }
        this.chart2.render();
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
                    <div class="secondItem" style={{ padding: '15px' }}>
                        <CanvasJSChart options={this.state.optionsAir}
                                                    onRef={ref => this.chart2 = ref}
                        />
                    </div>
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