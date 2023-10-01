import axios from 'axios';


class BaseService {
    constructor() {
        this.IoTHubBaseUrl = "https://localhost:7229/";

    }

    getConnectedDevices = async () => {
        let res = await axios.get(this.IoTHubBaseUrl + 'devices');
        let data = await res.data;
        return data;
    }

    getDeviceData = async (deviceId) => {
        let res = await axios.get(this.IoTHubBaseUrl + 'devices/' + deviceId + 'device-data');
        let data = await res.data;
        return data;
    }

    sendMessageToDevice = async (deviceId, message) => {
        let res = await axios.post(this.IoTHubBaseUrl + 'devices/' + deviceId + 'messages', { message });
        let data = await res.data;
        return data;
    }

    turnOnDevice = async (deviceId) => {
        let res = await axios.put(this.IoTHubBaseUrl + 'devices/' + deviceId + '/turn-on');
        let data = await res.data;
        return data;
    }

    turnOffDevice = async (deviceId) => {
        let res = await axios.put(this.IoTHubBaseUrl + 'devices/' + deviceId + '/turn-off');
        let data = await res.data;
        return data;
    }

    updateTelemetryInterval = async (deviceId) => {
        let res = await axios.put(this.IoTHubBaseUrl + 'devices/' + deviceId + '/telemetry-interval');
        let data = await res.data;
        return data;
    }

}

export default (new BaseService());