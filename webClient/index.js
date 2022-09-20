const mqttCreds = {
    hostName: 'localhost',
    port: 8080,
    useTls: false,
    clientId: 'webbrowser' + Date.now(),
    userName: 'user',
    password: 'password'
}
const start = () => {
   let Telemetry
   protobuf.load('mqttdevice.proto')
    .then(function(root) {
        Telemetry = root.lookupType('Telemetry')
    })

    const appDiv = document.getElementById('app')
    const client = mqtt.connect(`${mqttCreds.useTls ? 'wss' : 'ws'}://${mqttCreds.hostName}:${mqttCreds.port}/mqtt`, {
                clientId: mqttCreds.clientId, username: mqttCreds.userName, password: mqttCreds.password })
                client.on('connect', () => {
                    client.subscribe('device/+/telemetry')
                })
                
    client.on('message', (topic, message) => {
        const tel = Telemetry.decode(message)
        const el = document.createElement('div')
        el.innerText = `${topic} ${tel.temperature}`
        appDiv.appendChild(el)
        console.log(tel)
    })
}
window.onload = start
