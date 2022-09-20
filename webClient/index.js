import {Telemetry} from './mqttdevice.js'
const mqttCreds = {
    hostName: 'localhost',
    port: 8080,
    useTls: false,
    clientId: 'webbrowser' + Date.now(),
    userName: 'user',
    password: 'password'
}
const start = () => {
    const appDiv = document.getElementById('app')
    const client = mqtt.connect(`${mqttCreds.useTls ? 'wss' : 'ws'}://${mqttCreds.hostName}:${mqttCreds.port}/mqtt`, {
                clientId: mqttCreds.clientId, username: mqttCreds.userName, password: mqttCreds.password })
                client.on('connect', () => {
                    client.subscribe('device/+/telemetry')
                })
                
    client.on('message', (topic, message) => {
        const el = document.createElement('div')
        el.innerText = topic + new String(message)
        appDiv.appendChild(el)
        const t = Telemetry.toObject(message)
        console.log(message)
    })
}
window.onload = start
