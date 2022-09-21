const mqttCreds = {
    hostName: 'localhost',
    port: 8080,
    useTls: false,
    clientId: 'webbrowser' + Date.now(),
    userName: 'user',
    password: 'password'
}

const gbid = id => document.getElementById(id)

const start = () => {
   let Telemetry
   let Properties
   protobuf.load('mqttdevice.proto')
    .then(function(root) {
        Telemetry = root.lookupType('Telemetry')
        Properties = root.lookupType('Properties')
    })

    const client = mqtt.connect(`${mqttCreds.useTls ? 'wss' : 'ws'}://${mqttCreds.hostName}:${mqttCreds.port}/mqtt`, {
                clientId: mqttCreds.clientId, username: mqttCreds.userName, password: mqttCreds.password })
                client.on('connect', () => {
                    client.subscribe('device/+/telemetry')
                    client.subscribe('device/+/props/#')
                })
                
    client.on('message', (topic, message) => {
        const segments = topic.split('/')
        const deviceId = segments[1]
        if (deviceId) {
            gbid('deviceId').innerText = deviceId
        }
        const what = segments[2]
        if (what === 'telemetry')
        {
            const tel = Telemetry.decode(message)
            const el = document.createElement('div')
            el.innerText = `${topic} ${tel.temperature}`
            gbid('app').appendChild(el)
        }

        if (what === 'props')
        {
            const prop = Properties.decode(message)
            if (prop.sdkInfo) {
                const el = document.createElement('div')
                el.innerText = prop.sdkInfo
                gbid('deviceProps').appendChild(el)
            }
        }
        
    })
}
window.onload = start
