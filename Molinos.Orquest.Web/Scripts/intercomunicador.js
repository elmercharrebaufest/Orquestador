function ComunicadorViewModel(codigoDispositivo, idBtnActivar, idBtnMic, remotePlayer, audioSource, serverIp) {
    var self = this;
    self.pcConfig = { "iceServers": [{ "urls": ["stun:stun1.l.google.com:19302", "stun:stun2.l.google.com:19305"] }] };
    self.remotePlayer = remotePlayer;
    self.audioSource = audioSource;

    self.btnActivar = $('#' + idBtnActivar);
    self.btnMic = $('#' + idBtnMic);

    self.publishingPathListen = codigoDispositivo + '2web';
    self.publishingPathSpeak = 'web2' + codigoDispositivo;

    self.selectMic = ko.observable();
    self.retry = ko.observable(false);
    self.ipServer = ko.observable(serverIp);
    self.server = ko.observable();
    self.recordingUri = ko.observable();
    self.localStream = ko.observable();
    self.websocket = ko.observable(null);
    self.disconnectingListen = ko.observable(null);
    self.pingInterval = ko.observable(null);
    self.myListenId = ko.observable(-1);
    self.mySpeakId = ko.observable(-1);
    self.connectInterval = ko.observable(null);
    self.otherPeersListen = ko.observable({});
    self.otherPeersSpeak = ko.observable({});
    self.pcListen = ko.observable();
    self.pcSpeak = ko.observable();
    self.request = ko.observable(null);
    self.hangingGet = ko.observable(null);

    self.debugLog = ko.observableArray([]);

    self.desactivar = function () {
        self.btnActivar.prop('checked', false);
        self.btnMic.prop('disabled', true);
    }

    self.trace = function (message) {
        console.log(message);
        self.debugLog.push(message);
    }

    self.seleccionarMic = function () {
        $("#" + self.audioSource).modal('show', {
            keyboard: false
        });
    }

    self.inicializar = function () {
        self.btnActivar.off();
        self.btnMic.prop('disabled', !self.btnActivar.prop('checked'));
        self.btnActivar.on('change', () => {
            self.btnMic.prop('disabled', !self.btnActivar.prop('checked'));
            self.retry(self.btnActivar.prop('checked'));

            if (self.btnActivar.prop('checked')) {
                self.debugLog([]);

                self.server(`${self.ipServer()}:8888/rtc`);
                self.recordingUri(`http://${self.server()}`);

                self.trace('Listo para recibir audio');
                self.signInSpeak();
                self.connectListen();
            } else {
                self.trace('Fin de recepcion');

                self.disconnectListen();
                self.disconnectSpeak();
            }
        });

        self.btnMic.on('mousedown', () => {
            self.trace('**************** Inicio de envio de audio **************');
            self.localStream().getTracks().forEach((track) => track.enabled = true);
        });

        self.btnMic.on('mouseup', () => {
            self.trace('******************* Fin de emision de audio *******************');
            self.localStream().getTracks().forEach((track) => track.enabled = false);
        });

        //self.enumerateDevices();

        $(window).on('unload', function () {
            self.disconnectListen();
        });
    }

    self.connectListen = function () {
        self.signInListen(`ws://${self.server()}`, self.publishingPathListen);
        self.trace("Listen on " + self.publishingPathListen);
    }

    self.signInListen = function (server, localName) {
        try {
            if (self.websocket()) {
                self.websocket().close();
                self.websocket(null);
            }

            self.websocket(new WebSocket(server + "/sign_in?channel=" + localName));
            self.websocket().onopen = function (e) {
                self.trace("Signalling server connected");
            };
            self.websocket().onclose = function (e) {
                
                self.trace("Signalling server disconnected. Code: " + e.code + ", reason: " + e);
                if (!self.disconnectingListen()) {
                    self.disconnectListen();
                }
            };
            self.websocket().onmessage = function (e) {
                self.handlePeerMessageListen(e.data);
            };
            self.websocket().onerror = function (e) {
                self.trace("Error ws: " + e.data || e);
            };
        } catch (e) {
            self.trace("error catch signInListen: " + e);
        }
    }

    self.disconnectListen = function() {
        self.disconnectingListen(true);

        if (self.websocket()) {
            self.websocket().close();
            self.websocket(null);
        }

        if (self.pingInterval() != null) {
            clearInterval(self.pingInterval());
            self.pingInterval(null);
        }

        self.myListenId(-1);

        self.disconnectingListen(false);

        if (self.retry()) {
            window.setTimeout(self.connectListen, 1000);
        }
    }

    self.handlePeerMessageListen = function(data) {
        var dataJson = JSON.parse(data);
        if (dataJson == null) {
            return;
        }

        var peer_id = parseInt(dataJson.from);
        var str = "Message from '" + self.otherPeersListen()[peer_id] + "': " + data;

        if (dataJson["data-type"] == "peer-list") {
            self.myListenId(parseInt(dataJson.data[0].id));
            self.trace("My id: " + self.myListenId());
            for (var i = 1; i < dataJson.data.length; i++) {
                self.trace("Peer " + i + ": id " + dataJson.data[i].id + ", name: " + dataJson.data[i].name);
                self.otherPeersListen()[parseInt(dataJson.data[i].id)] = dataJson.data[i].name;
            }

            self.pingInterval(setInterval(() => {
                self.sendToPeerListen("ping", 1, null);
            }, 10000));
        }
        else if (dataJson["data-type"] == "message") {
            dataJson = dataJson.data;
            if (dataJson != null && dataJson.type != null && dataJson.type == "offer") {
                self.trace(str);
                self.trace("Received SDP offer, preparing answer...");
                self.createPeerConnectionListen(peer_id);
                self.pcListen().setRemoteDescription(new RTCSessionDescription(dataJson), self.onRemoteSdpSuccessListen, self.onRemoteSdpErrorListen);
                self.pcListen().createAnswer(function (sessionDescription) {
                    if (sessionDescription == null) {
                        self.trace("Sending SDP answer (callback): undefined SDP");
                    }
                    else {
                        self.pcListen().setLocalDescription(sessionDescription, self.onSetLocalSdpSuccessListen, self.onSetLocalSdpErrorListen);
                        var data = JSON.stringify(sessionDescription);
                        self.trace("Prepared SDP answer (callback): " + data);
                        self.sendToPeerListen("message", peer_id, data);
                    }
                }, function (error) { // error
                    self.trace("Create answer error: " + error);
                }).then(function (sessionDescription) {
                    if (sessionDescription == null) {
                        self.trace("Sending SDP answer (then): undefined SDP");
                    }
                    else {
                        self.pcListen().setLocalDescription(sessionDescription, self.onSetLocalSdpSuccessListen, self.onSetLocalSdpErrorListen);
                        var data = JSON.stringify(sessionDescription);
                        self.trace("Prepared SDP answer (then): " + data);
                        self.sendToPeerListen("message", peer_id, data);
                    }
                }).catch((error) => {
                    self.trace("Create SDP answer error: " + error);
                });
            }
            else if (dataJson != null && dataJson.candidate != null) {
                self.trace(str);
                self.trace("Adding ICE candidate " + dataJson.candidate);
                var candidate = new RTCIceCandidate({ sdpMLineIndex: dataJson.sdpMLineIndex, candidate: dataJson.candidate });
                self.pcListen().addIceCandidate(candidate, self.onAddIceCandidateSuccessListen, self.onAddIceCandidateErrorListen);
            }
            else {
                self.trace(str);
                self.trace("Unsupported message!");
            }
        }
        else {
            self.trace(str);
            self.trace("Unsupported message!");
        }
    }

    self.sendToPeerListen = function(dataType, peer_id, data) {
        try {
            var dataJson = '{"data-type":"' + dataType + '","from":' + self.myListenId() + ',"to":' + peer_id;
            if (data != null) {
                dataJson += ',"data":' + data;
            }
            dataJson += '}';
            self.trace("Sending: " + dataJson);
            self.websocket().send(dataJson);
        } catch (e) {
            self.trace("send to peer error: " + e.description);
        }
    }

    self.createPeerConnectionListen = function(peer_id) {
        try {
            self.pcListen(new RTCPeerConnection(self.pcConfig));
            self.pcListen().onicecandidate = function (event) {
                if (event.candidate) {
                    var candidate = {
                        sdpMLineIndex: event.candidate.sdpMLineIndex,
                        sdpMid: event.candidate.sdpMid,
                        candidate: event.candidate.candidate
                    };
                    self.sendToPeerListen("message", peer_id, JSON.stringify(candidate));
                } else {
                    self.trace("End of candidates.");
                }
            };
            self.pcListen().onconnecting = function (message) {
                self.trace("Session connecting.");
            };
            self.pcListen().onopen = function (message) {
                self.trace("Session opened.");
            };
            self.pcListen().ontrack = self.onRemoteStreamAddedListen;
            self.pcListen().onremovestream = function (event) {
                self.trace("Remote stream removed.");
            };
            self.pcListen().onidpvalidationerror = function (ev) {
                self.trace("onidpvalidationerror");
            };
            self.pcListen().onidpassertionerror = function (ev) {
                self.trace("onidpassertionerror");
            };
            self.pcListen().onnegotiationneeded = function () {
                self.trace("onnegotiationneeded");
            };
            self.pcListen().onconnectionstatechange = function (event) {
                self.trace("Connection state changed to " + pcListen.connectionState);
            };
            self.pcListen().oniceconnectionstatechange = function (event) {
                self.trace("ICE connection state changed to " + pcListen.iceConnectionState);
            };
            self.pcListen().onicegatheringstatechange = function () {
                self.trace("ICE gathering state changed to " + pcListen.iceGatheringState);
            };
            self.pcListen().onsignalingstatechange = function (event) {
                self.trace("Signaling state changed to " + pcListen.signalingState);
            };
            self.trace("Created RTCPeerConnection with config: " + JSON.stringify(pcConfig));
        }
        catch (e) {
            self.trace("Failed to create PeerConnection, exception: " + e);
        }
    }

    self.onRemoteSdpSuccessListen = function () {
        self.trace('onRemoteSdpSuccess');
    }

    self.onRemoteSdpErrorListen = function (event) {
        self.trace("onRemoteSdpError: event.name: " + event.name + ", event.message: " + event.messag);
    }

    self.onSetLocalSdpSuccessListen = function() {
        self.trace("setLocalDescription success");
    }

    self.onSetLocalSdpErrorListen = function() {
        self.trace("setLocalDescription failed");
    }

    self.onAddIceCandidateSuccessListen = function() {
        self.trace("addIceCandidate success");
    }

    self.onAddIceCandidateErrorListen = function() {
        self.trace("addIceCandidate failed");
    }

    self.onRemoteStreamAddedListen = function (event) {
        self.trace("Got remote stream");
        try {
            var remoteVideoElement = document.getElementById(self.remotePlayer);
            if (event.track.kind == 'video') {
                self.trace("Can play H.264: " + remoteVideoElement.canPlayType('video/mp4; codecs="avc1.42E01E, mp4a.40.2"'));
            }
            remoteVideoElement.srcObject = event.streams[0];
            self.trace("Remote stream added: " + event.track.kind + ", active: " + event.streams[0].active);
        } catch (e) {
            self.trace("Remote stream added exception: " + e);
        }
    }

    self.signInSpeakCallback = function() {
        try {
            if (self.request().readyState == 4) {
                if (self.request().status == 200) {
                    self.trace("sign in successful");
                    var peers = self.request().responseText.split("\n");
                    self.mySpeakId(parseInt(peers[0].split(',')[1]));
                    self.trace("My id: " + self.mySpeakId());
                    for (var i = 1; i < peers.length; ++i) {
                        if (peers[i].length > 0) {
                            self.trace("Peer " + i + ": " + peers[i]);
                            var parsed = peers[i].split(',');
                            self.otherPeersSpeak()[parseInt(parsed[1])] = parsed[0];
                        }
                    }
                    self.startHangingGet();
                    self.createPeerConnectionSpeak(1); // server id is always 1
                    self.trace("Estado localstream: " + self.localStream());
                    self.localStream().getTracks().forEach((track) => {
                        track.enabled = false;
                        self.pcSpeak().addTrack(track, self.localStream());
                    });
                    self.request(null);
                }
            }
        } catch (e) {
            self.trace("error signInSpeakCallback: " + e.description || e);
        }
    }

    self.disconnectSpeak = function() {
        try {
            if (self.request()) {
                self.request().abort();
                self.request(null);
            }

            if (self.hangingGet()) {
                self.hangingGet().abort();
                self.hangingGet(null);
            }

            if (self.mySpeakId() != -1) {
                self.request(new XMLHttpRequest());
                self.request().open("GET", self.recordingUri() + "/sign_out?peer_id=" + self.mySpeakId(), false);
                self.request().send();
                self.request(null);
                self.mySpeakId(-1);
            }

            if (self.localStream() != null) {
                self.localStream().getTracks().forEach(function (track) {
                    track.stop();
                });
                self.localStream(null);
            }
        } catch (e) {
            self.trace("disconnect error: " + e.description);
        }
    }

    self.startHangingGet = function() {
        try {
            //trace("startHangingGet");
            self.hangingGet(new XMLHttpRequest());
            self.hangingGet().onreadystatechange = self.hangingGetCallback;
            self.hangingGet().ontimeout = self.onHangingGetTimeout;
            self.hangingGet().open("GET", self.recordingUri() + "/wait?peer_id=" + self.mySpeakId(), true);
            self.hangingGet().send();
        } catch (e) {
            self.trace("error" + e.description);
        }
    }

    self.hangingGetCallback = function() {
        try {
            if (self.hangingGet().readyState != 4) {
                //trace("readyState: " + hangingGet.readyState);
                return;
            }
            if (self.hangingGet().status != 200) {
                self.trace("server error: " + self.hangingGet().status + " " + self.hangingGet().statusText);
                self.disconnectSpeak();
            } else {
                var peer_id = self.parseIntHeader(self.hangingGet(), "Pragma");
                if (peer_id == self.mySpeakId()) {
                    self.handleServerNotification(self.hangingGet().responseText);
                } else {
                    self.handlePeerMessageSpeak(peer_id, self.hangingGet().responseText);
                }
            }
            if (self.hangingGet()) {
                self.hangingGet().abort();
                self.hangingGet(null);
            }
        } catch (e) {
            self.trace("Hanging get receive error: " + e);
        }
        if (self.mySpeakId() != -1) {
            try {
                window.setTimeout(self.startHangingGet, 0);
            } catch (e) {
                self.trace("Hanging get send error: " + e);
            }
        }
    }

    self.onHangingGetTimeout = function() {
        self.trace("hanging get timeout. issuing again.");
        self.hangingGet().abort();
        self.hangingGet(null);
        if (self.mySpeakId() != -1)
            window.setTimeout(self.startHangingGet, 0);
    }

    self.handleServerNotification = function(data) {
        self.trace("Message from: " + self.mySpeakId() + ':' + data);
        var parsed = data.split(',');
        if (parseInt(parsed[2]) != 0)
            self.otherPeersSpeak[parseInt(parsed[1])] = parsed[0];
    }

    self.handlePeerMessageSpeak = function(peer_id, data) {
        var str = "Message from '" + self.otherPeersSpeak()[peer_id] + "':" + data;

        var dataJson = JSON.parse(data);
        if (dataJson != null && dataJson.type != null && dataJson.type == "offer") {
            self.trace(str);
            self.trace("Received SDP offer, preparing answer...");
            self.pcSpeak().setRemoteDescription(new RTCSessionDescription(dataJson), onRemoteSdpSuccessSpeak, onRemoteSdpErrorSpeak);
            self.pcSpeak().createAnswer(function (sessionDescription) {
                if (sessionDescription == null) {
                    self.trace("Sending SDP answer (callback): undefined SDP");
                }
                else {
                    self.pcSpeak().setLocalDescription(sessionDescription, self.onSetLocalSdpSuccessSpeak, self.onSetLocalSdpFailureSpeak);
                    var data = JSON.stringify(sessionDescription);
                    self.trace("Prepared SDP answer (callback): " + data);
                    self.sendToPeerSpeak(peer_id, data);
                }
            }, function (error) { // error
                self.trace("Create answer error: " + error);
            }).then(function (sessionDescription) {
                if (sessionDescription == null) {
                    self.trace("Sending SDP answer (then): undefined SDP");
                }
                else {
                    self.pcSpeak().setLocalDescription(sessionDescription, self.onSetLocalSdpSuccessSpeak, self.onSetLocalSdpFailureSpeak);
                    var data = JSON.stringify(sessionDescription);
                    self.trace("Prepared SDP answer (then): " + data);
                    self.sendToPeerSpeak(peer_id, data);
                }
            }).catch((error) => {
                self.trace("Create SDP answer error: " + error);
            });
        }
        else if (dataJson != null && dataJson.type != null && dataJson.type == "answer") {
            self.trace(str);
            self.trace("Received SDP answer");
            self.pcSpeak().setRemoteDescription(new RTCSessionDescription(dataJson), self.onRemoteSdpSuccessSpeak, self.onRemoteSdpErrorSpeak);
        }
        else if (dataJson != null && dataJson.candidate != null) {
            self.trace(str);
            self.trace("Adding ICE candidate " + dataJson.candidate);
            var candidate = new RTCIceCandidate({ sdpMLineIndex: dataJson.sdpMLineIndex, candidate: dataJson.candidate });
            self.pcSpeak().addIceCandidate(candidate, self.onIceCandidateSuccessSpeak, self.onIceCandidateFailureSpeak);
        }
        else if (dataJson != null && dataJson.action != null && dataJson.action == "none") {
            // hanging request expired
        }
        else {
            self.trace(str);
            self.trace("Unsupported message!");
        }
    }

    self.sendToPeerSpeak = function(peer_id, data) {
        try {
            self.trace(peer_id + " Send " + data);
            if (self.mySpeakId() == -1) {
                self.trace("Not connected");
                return;
            }
            if (peer_id == self.mySpeakId()) {
                self.trace("Can't send a message to oneself :)");
                return;
            }
            var r = new XMLHttpRequest();
            let uriMsg = self.recordingUri() + "/message?peer_id=" + self.mySpeakId() + "&to=" + peer_id;
            self.trace(uriMsg);
            r.open("POST", uriMsg, true);
            r.setRequestHeader("Content-Type", "text/plain");
            r.send(data);
        } catch (e) {
            self.trace("send to peer error: " + e, 'S');
        }
    }

    self.createPeerConnectionSpeak = function(peer_id) {
        try {
            self.pcSpeak(new RTCPeerConnection(self.pcConfig));
            self.pcSpeak().onicecandidate = function (event) {
                if (event.candidate) {
                    var candidate = {
                        sdpMLineIndex: event.candidate.sdpMLineIndex,
                        sdpMid: event.candidate.sdpMid,
                        candidate: event.candidate.candidate
                    };
                    self.sendToPeerSpeak(peer_id, JSON.stringify(candidate));
                } else {
                    self.trace("End of candidates.");
                }
            };
            self.pcSpeak().onconnecting = function (message) {
                self.trace("Session connecting.");
            };
            self.pcSpeak().onopen = function (message) {
                self.trace("Session opened.");
            };
            self.pcSpeak().onremovestream = function (event) {
                self.trace("Remote stream removed.");
            };
            self.pcSpeak().onidpvalidationerror = function (ev) {
                self.trace("onidpvalidationerror");
            };
            self.pcSpeak().onidpassertionerror = function (ev) {
                self.trace("onidpassertionerror");
            };
            self.pcSpeak().onnegotiationneeded = self.onNegotiationNeeded;
            self.pcSpeak().onconnectionstatechange = function (event) {
                self.trace("Connection state changed to " + self.pcSpeak().connectionState);
            };
            self.pcSpeak().oniceconnectionstatechange = function (event) {
                self.trace("ICE connection state changed to " + self.pcSpeak().iceConnectionState);
            };
            self.pcSpeak().onicegatheringstatechange = function () {
                self.trace("ICE gathering state changed to " + self.pcSpeak().iceGatheringState);
            };
            self.pcSpeak().onsignalingstatechange = function (event) {
                self.trace("Signaling state changed to " + self.pcSpeak().signalingState);
            };

            self.trace("Created RTCPeerConnection with config: " + JSON.stringify(self.pcConfig));
        }
        catch (e) {
            self.trace("Failed to create PeerConnection, exception: " + e);
        }
    }

    self.onNegotiationNeeded = async function() {
        try {
            self.trace("onnegotiationneeded");
            self.trace("pcSpeak: " + JSON.stringify(self.pcSpeak().localDescription));
            await self.pcSpeak().setLocalDescription(await self.pcSpeak().createOffer());
            var data = JSON.stringify(self.pcSpeak().localDescription);
            self.trace("Prepared SDP offer: " + data);
            self.sendToPeerSpeak(1, data);
        } catch (e) {
            self.trace("Failed to negotiate, exception: " + e);
        }
    }

    self.onSetLocalSdpSuccessSpeak = function() {
        self.trace("setLocalDescription success");
    }

    self.onSetLocalSdpFailureSpeak = function() {
        self.trace("setLocalDescription failed");
    }

    self.onIceCandidateSuccessSpeak = function() {
        self.trace("addIceCandidate success");
    }

    self.onIceCandidateFailureSpeak = function() {
        self.trace("addIceCandidate failed");
    }

    self.onRemoteSdpSuccessSpeak = function() {
        self.trace('onRemoteSdpSucces');
    }

    self.onRemoteSdpErrorSpeak = function(event) {
        self.trace("onRemoteSdpError: event.name: " + event.name + ", event.message: " + event.message);
    }

    self.parseIntHeader = function(r, name) {
        var val = r.getResponseHeader(name);
        return val != null && val.length ? parseInt(val) : -1;
    }

    self.signInSpeak = async function() {
        try {
            await self.permisosMic();
            self.request(new XMLHttpRequest());
            self.request().onreadystatechange = self.signInSpeakCallback;
            var uri = self.recordingUri() + "/sign_in?channel=" + self.publishingPathSpeak + "&publish=true";
            self.trace("Connect to " + uri);
            self.request().open("GET", uri, true);
            self.request().send();
        } catch (e) {
            self.trace("error catch signInSpeak: " + e.description);
        }
    }

    self.permisosMic = async function () {
        var audioInputSelect = document.getElementById(self.audioSource);
        const audioSource = audioInputSelect.value;

        if (audioSource) {
            const constraints = {
                audio: { deviceId: audioSource ? { exact: audioSource } : undefined },
            };

            let stream = await navigator.mediaDevices.getUserMedia(constraints);
            self.localStream(stream);
            self.trace('Permisos mic concedidos con localstream: ' + self.localStream());
        }
    }

    self.inicializar();
}

async function geMicDevices(selectId) {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ audio: true });

        let devices = await navigator.mediaDevices.enumerateDevices();
        gotMicDevices(devices, selectId);
        //if (localStream != null) {
        //    localStream.getTracks().forEach(function (track) {
        //        track.stop();
        //    });
        //    localStream = null;
        //}
    } catch (e) {
        console.log("enumerate error: " + e.description);
    }
}

function gotMicDevices(deviceInfos, selectId) {
    var audioInputSelect = document.getElementById(selectId);

    for (let i = 0; i !== deviceInfos.length; ++i) {
        const deviceInfo = deviceInfos[i];
        const option = document.createElement('option');
        option.value = deviceInfo.deviceId;
        option.selected = i == 0;
        if (deviceInfo.kind === 'audioinput') {
            option.text = deviceInfo.label || `Device ${audioInputSelect.length + 1}`;
            audioInputSelect.appendChild(option);
        }
    }
}
