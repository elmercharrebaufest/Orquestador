function IntercomunicadorDispositivoVM(config) {
    this.containerId = config.containerId;
    this.generalIds = config.generalIds;
    this.vmData = config.vmData;
    this.labels = config.labels;
    this.messages = config.messages;
    this.constants = config.constants;
    this.models = config.models;
    this.generalUrls = config.generalUrls;
    this.aucDataSourceUrls = config.aucDataSourceUrls;
}


IntercomunicadorDispositivoVM.prototype = {
    onReady: function () {
        let self = this;
        self.vm = {
            mainModule: {
                selectors: {
                    btnActivar: $('#' + self.generalIds.btnActivar),
                    btnMicrofono: $('#' + self.generalIds.btnMicrofono),
                    remotePlayer: $('#' + self.generalIds.remotePlayer),
                    audioSource: $('#' + self.generalIds.audioSource),
                    pressedAudioButton: null,
                },
                states: {
                    microfonoActivado: false
                },
                actions: {},
                methods: {
                    connectListen: function () {
                        self.vm.mainModule.methods.signInListen(self.vmData.ICWSServerUrl, self.vmData.PublishingPathListen);
                        self.trace("Listen on " + self.vmData.PublishingPathListen);
                    },
                    signInListen: function (server, localName) {
                        try {
                            if (self.vm.mainModule.models.configuration.websocket) {
                                self.vm.mainModule.models.configuration.websocket.close();
                                self.vm.mainModule.models.configuration.websocket = null;
                            }
                            self.vm.mainModule.models.configuration.websocket = new WebSocket(server + "/sign_in?channel=" + localName);
                            self.vm.mainModule.models.configuration.websocket.onopen = function (e) {
                                self.trace("Signalling server connected");
                            };
                            self.vm.mainModule.models.configuration.websocket.onclose = function (e) {
                                self.trace("Signalling server disconnected. Code: " + e.code + ", reason: " + e);
                                if (!self.vm.mainModule.models.configuration.disconnectingListen) {
                                    self.vm.mainModule.methods.disconnectListen();
                                }
                            };
                            self.vm.mainModule.models.configuration.websocket.onmessage = function (e) {
                                self.vm.mainModule.methods.handlePeerMessageListen(e.data);
                            };
                            self.vm.mainModule.models.configuration.websocket.onerror = function (e) {
                                self.trace("Error ws: " + e.data || e);
                            };
                        } catch (e) {
                            self.trace("error catch signInListen: " + e);
                        }
                    },
                    disconnectListen: function () {
                        self.vm.mainModule.models.configuration.disconnectingListen = true;

                        if (self.vm.mainModule.models.configuration.websocket) {
                            self.vm.mainModule.models.configuration.websocket.close();
                            self.vm.mainModule.models.configuration.websocket = null;
                        }

                        if (self.vm.mainModule.models.configuration.pingInterval != null) {
                            clearInterval(self.vm.mainModule.models.configuration.pingInterval);
                            self.vm.mainModule.models.configuration.pingInterval = null;
                        }

                        self.vm.mainModule.models.configuration.myListenId = -1;

                        self.vm.mainModule.models.configuration.disconnectingListen = false;

                        if (self.vm.mainModule.models.configuration.retry) {
                            window.setTimeout(self.vm.mainModule.methods.connectListen, 1000);
                        }
                    },
                    handlePeerMessageListen: function (data) {
                        var dataJson = JSON.parse(data);
                        if (dataJson == null) {
                            return;
                        }

                        var peer_id = parseInt(dataJson.from);
                        var str = "Message from '" + self.vm.mainModule.models.configuration.otherPeersListen[peer_id] + "': " + data;

                        if (dataJson["data-type"] == "peer-list") {
                            self.vm.mainModule.models.configuration.myListenId = parseInt(dataJson.data[0].id);
                            self.trace("My id: " + self.vm.mainModule.models.configuration.myListenId);
                            for (var i = 1; i < dataJson.data.length; i++) {
                                self.trace("Peer " + i + ": id " + dataJson.data[i].id + ", name: " + dataJson.data[i].name);
                                self.vm.mainModule.models.configuration.otherPeersListen[parseInt(dataJson.data[i].id)] = dataJson.data[i].name;
                            }

                            self.vm.mainModule.models.configuration.pingInterval = setInterval(() => {
                                self.vm.mainModule.methods.sendToPeerListen("ping", 1, null);
                            }, 10000);
                        }
                        else if (dataJson["data-type"] == "message") {
                            dataJson = dataJson.data;
                            if (dataJson != null && dataJson.type != null && dataJson.type == "offer") {
                                self.trace(str);
                                self.trace("Received SDP offer, preparing answer...");
                                self.vm.mainModule.methods.createPeerConnectionListen(peer_id);
                                self.vm.mainModule.models.configuration.pcListen.setRemoteDescription(new RTCSessionDescription(dataJson), self.vm.mainModule.methods.onRemoteSdpSuccessListen, self.vm.mainModule.methods.onRemoteSdpErrorListen);
                                self.vm.mainModule.models.configuration.pcListen.createAnswer(function (sessionDescription) {
                                    if (sessionDescription == null) {
                                        self.trace("Sending SDP answer (callback): undefined SDP");
                                    }
                                    else {
                                        self.vm.mainModule.models.configuration.pcListen.setLocalDescription(sessionDescription, self.vm.mainModule.methods.onSetLocalSdpSuccessListen, self.vm.mainModule.methods.onSetLocalSdpErrorListen);
                                        var data = JSON.stringify(sessionDescription);
                                        self.trace("Prepared SDP answer (callback): " + data);
                                        self.vm.mainModule.methods.sendToPeerListen("message", peer_id, data);
                                    }
                                }, function (error) {
                                    self.trace("Create answer error: " + error);
                                }).then(function (sessionDescription) {
                                    if (sessionDescription == null) {
                                        self.trace("Sending SDP answer (then): undefined SDP");
                                    }
                                    else {
                                        self.vm.mainModule.models.configuration.pcListen.setLocalDescription(sessionDescription, self.vm.mainModule.methods.onSetLocalSdpSuccessListen, self.vm.mainModule.methods.onSetLocalSdpErrorListen);
                                        var data = JSON.stringify(sessionDescription);
                                        self.trace("Prepared SDP answer (then): " + data);
                                        self.vm.mainModule.methods.sendToPeerListen("message", peer_id, data);
                                    }
                                }).catch((error) => {
                                    self.trace("Create SDP answer error: " + error);
                                });
                            }
                            else if (dataJson != null && dataJson.candidate != null) {
                                self.trace(str);
                                self.trace("Adding ICE candidate " + dataJson.candidate);
                                var candidate = new RTCIceCandidate({ sdpMLineIndex: dataJson.sdpMLineIndex, candidate: dataJson.candidate });
                                self.vm.mainModule.models.configuration.pcListen.addIceCandidate(candidate, self.vm.mainModule.methods.onAddIceCandidateSuccessListen, self.vm.mainModule.methods.onAddIceCandidateErrorListen);
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
                    },
                    sendToPeerListen: function (dataType, peer_id, data) {
                        try {
                            var dataJson = '{"data-type":"' + dataType + '","from":' + self.vm.mainModule.models.configuration.myListenId + ',"to":' + peer_id;
                            if (data != null) {
                                dataJson += ',"data":' + data;
                            }
                            dataJson += '}';
                            self.trace("Sending: " + dataJson);
                            self.vm.mainModule.models.configuration.websocket.send(dataJson);
                        } catch (e) {
                            self.trace("send to peer error: " + e.description);
                        }
                    },
                    createPeerConnectionListen: function (peer_id) {
                        try {
                            self.vm.mainModule.models.configuration.pcListen = new RTCPeerConnection(self.vm.mainModule.models.configuration.pcConfig);
                            self.vm.mainModule.models.configuration.pcListen.onicecandidate = function (event) {
                                if (event.candidate) {
                                    var candidate = {
                                        sdpMLineIndex: event.candidate.sdpMLineIndex,
                                        sdpMid: event.candidate.sdpMid,
                                        candidate: event.candidate.candidate
                                    };
                                    self.vm.mainModule.methods.sendToPeerListen("message", peer_id, JSON.stringify(candidate));
                                } else {
                                    self.trace("End of candidates.");
                                }
                            };
                            self.vm.mainModule.models.configuration.pcListen.onconnecting = function (message) {
                                self.trace("Session connecting.");
                            };
                            self.vm.mainModule.models.configuration.pcListen.onopen = function (message) {
                                self.trace("Session opened.");
                            };
                            self.vm.mainModule.models.configuration.pcListen.ontrack = self.vm.mainModule.methods.onRemoteStreamAddedListen;
                            self.vm.mainModule.models.configuration.pcListen.onremovestream = function (event) {
                                self.trace("Remote stream removed.");
                            };
                            self.vm.mainModule.models.configuration.pcListen.onidpvalidationerror = function (ev) {
                                self.trace("onidpvalidationerror");
                            };
                            self.vm.mainModule.models.configuration.pcListen.onidpassertionerror = function (ev) {
                                self.trace("onidpassertionerror");
                            };
                            self.vm.mainModule.models.configuration.pcListen.onnegotiationneeded = function () {
                                self.trace("onnegotiationneeded");
                            };
                            self.vm.mainModule.models.configuration.pcListen.onconnectionstatechange = function (event) {
                                self.trace("Connection state changed to " + self.vm.mainModule.models.configuration.pcListen.connectionState);
                            };
                            self.vm.mainModule.models.configuration.pcListen.oniceconnectionstatechange = function (event) {
                                self.trace("ICE connection state changed to " + self.vm.mainModule.models.configuration.pcListen.iceConnectionState);
                            };
                            self.vm.mainModule.models.configuration.pcListen.onicegatheringstatechange = function () {
                                self.trace("ICE gathering state changed to " + self.vm.mainModule.models.configuration.pcListen.iceGatheringState);
                            };
                            self.vm.mainModule.models.configuration.pcListen.onsignalingstatechange = function (event) {
                                self.trace("Signaling state changed to " + self.vm.mainModule.models.configuration.pcListen.signalingState);
                            };
                            self.trace("Created RTCPeerConnection with config: " + JSON.stringify(self.vm.mainModule.models.configuration.pcConfig));
                        }
                        catch (e) {
                            self.trace("Failed to create PeerConnection, exception: " + e);
                        }
                    },
                    onRemoteSdpSuccessListen: function () {
                        self.trace('onRemoteSdpSuccess');
                    },
                    onRemoteSdpErrorListen: function (event) {
                        self.trace("onRemoteSdpError: event.name: " + event.name + ", event.message: " + event.messag);
                    },
                    onSetLocalSdpSuccessListen: function () {
                        self.trace("setLocalDescription success");
                    },
                    onSetLocalSdpErrorListen: function () {
                        self.trace("setLocalDescription failed");
                    },
                    onAddIceCandidateSuccessListen: function () {
                        self.trace("addIceCandidate success");
                    },
                    onAddIceCandidateErrorListen: function () {
                        self.trace("addIceCandidate failed");
                    },
                    onRemoteStreamAddedListen: function (event) {
                        self.trace("Got remote stream");
                        try {
                            var remoteVideoElement = self.vm.mainModule.selectors.remotePlayer;
                            if (event.track.kind == 'video') {
                                self.trace("Can play H.264: " + remoteVideoElement[0].canPlayType('video/mp4; codecs="avc1.42E01E, mp4a.40.2"'));
                            }
                            remoteVideoElement[0].srcObject = event.streams[0];
                            self.trace("Remote stream added: " + event.track.kind + ", active: " + event.streams[0].active);
                        } catch (e) {
                            self.trace("Remote stream added exception: " + e);
                        }
                    },
                    signInSpeakCallback: function () {
                        try {
                            if (self.vm.mainModule.models.configuration.request.readyState == 4) {
                                if (self.vm.mainModule.models.configuration.request.status == 200) {
                                    self.trace("sign in successful");
                                    var peers = self.vm.mainModule.models.configuration.request.responseText.split("\n");
                                    self.vm.mainModule.models.configuration.mySpeakId = parseInt(peers[0].split(',')[1]);
                                    self.trace("My id: " + self.vm.mainModule.models.configuration.mySpeakId);
                                    for (var i = 1; i < peers.length; ++i) {
                                        if (peers[i].length > 0) {
                                            self.trace("Peer " + i + ": " + peers[i]);
                                            var parsed = peers[i].split(',');
                                            self.vm.mainModule.models.configuration.otherPeersSpeak[parseInt(parsed[1])] = parsed[0];
                                        }
                                    }
                                    self.vm.mainModule.methods.startHangingGet();
                                    self.vm.mainModule.methods.createPeerConnectionSpeak(1); // server id is always 1
                                    self.trace("Estado localstream: " + self.vm.mainModule.models.configuration.localStream);
                                    self.vm.mainModule.models.configuration.localStream.getTracks().forEach((track) => {
                                        track.enabled = false;
                                        self.vm.mainModule.models.configuration.pcSpeak.addTrack(track, self.vm.mainModule.models.configuration.localStream);
                                    });
                                    self.vm.mainModule.models.configuration.request = null;
                                } else if (self.vm.mainModule.models.configuration.request.status == 409) {
                                    self.vm.mainModule.selectors.btnActivar.prop('checked', false);
                                    self.vm.mainModule.selectors.btnMicrofono.prop('disabled', true);
                                    alert("Ya existe una conexión creada para el dispositivo seleccionado")
                                }
                            }
                        } catch (e) {
                            self.trace("error signInSpeakCallback: " + e.description || e);
                        }
                    },
                    disconnectSpeak: function () {
                        try {
                            if (self.vm.mainModule.models.configuration.request) {
                                self.vm.mainModule.models.configuration.request.abort();
                                self.vm.mainModule.models.configuration.request = null;
                            }

                            if (self.vm.mainModule.models.configuration.hangingGet) {
                                self.vm.mainModule.models.configuration.hangingGet.abort();
                                self.vm.mainModule.models.configuration.hangingGet = null;
                            }

                            if (self.vm.mainModule.models.configuration.mySpeakId != -1) {
                                self.vm.mainModule.models.configuration.request = new XMLHttpRequest();
                                self.vm.mainModule.models.configuration.request.open("GET", self.vmData.ICWebServerUrl + "/sign_out?peer_id=" + self.vm.mainModule.models.configuration.mySpeakId, false);
                                self.vm.mainModule.models.configuration.request.send();
                                self.vm.mainModule.models.configuration.request = null;
                                self.vm.mainModule.models.configuration.mySpeakId = -1;
                            }

                            if (self.vm.mainModule.models.configuration.localStream != null) {
                                self.vm.mainModule.models.configuration.localStream.getTracks().forEach(function (track) {
                                    track.enabled = false;
                                });
                                /*               self.vm.mainModule.models.configuration.localStream = null;*/
                            }
                        } catch (e) {
                            self.trace("disconnect error: " + e.description);
                        }
                    },
                    startHangingGet: function () {
                        try {
                            self.vm.mainModule.models.configuration.hangingGet = new XMLHttpRequest();
                            self.vm.mainModule.models.configuration.hangingGet.onreadystatechange = self.vm.mainModule.methods.hangingGetCallback;
                            self.vm.mainModule.models.configuration.hangingGet.ontimeout = self.vm.mainModule.methods.onHangingGetTimeout;
                            self.vm.mainModule.models.configuration.hangingGet.open("GET", self.vmData.ICWebServerUrl + "/wait?peer_id=" + self.vm.mainModule.models.configuration.mySpeakId, true);
                            self.vm.mainModule.models.configuration.hangingGet.send();
                        } catch (e) {
                            self.trace("error" + e.description);
                        }
                    },
                    hangingGetCallback: function () {
                        try {
                            if (self.vm.mainModule.models.configuration.hangingGet.readyState != 4) {
                                return;
                            }
                            if (self.vm.mainModule.models.configuration.hangingGet.status != 200) {
                                self.trace("server error: " + self.vm.mainModule.models.configuration.hangingGet.status + " " + self.vm.mainModule.models.configuration.hangingGet.statusText);
                                self.vm.mainModule.methods.disconnectSpeak();
                            } else {
                                var peer_id = self.vm.mainModule.methods.parseIntHeader(self.vm.mainModule.models.configuration.hangingGet, "Pragma");
                                if (peer_id == self.vm.mainModule.models.configuration.mySpeakId) {
                                    self.vm.mainModule.methods.handleServerNotification(self.vm.mainModule.models.configuration.hangingGet.responseText);
                                } else {
                                    self.vm.mainModule.methods.handlePeerMessageSpeak(peer_id, self.vm.mainModule.models.configuration.hangingGet.responseText);
                                }
                            }
                            if (self.vm.mainModule.models.configuration.hangingGet) {
                                self.vm.mainModule.models.configuration.hangingGet.abort();
                                self.vm.mainModule.models.configuration.hangingGet = null;
                            }
                        } catch (e) {
                            self.trace("Hanging get receive error: " + e);
                        }
                        if (self.vm.mainModule.models.configuration.mySpeakId != -1) {
                            try {
                                window.setTimeout(self.vm.mainModule.methods.startHangingGet, 0);
                            } catch (e) {
                                self.trace("Hanging get send error: " + e);
                            }
                        }
                    },
                    onHangingGetTimeout: function () {
                        self.trace("hanging get timeout. issuing again.");
                        self.vm.mainModule.models.configuration.hangingGet.abort();
                        self.vm.mainModule.models.configuration.hangingGet = null;
                        if (self.vm.mainModule.models.configuration.mySpeakId != -1)
                            window.setTimeout(self.vm.mainModule.methods.startHangingGet, 0);
                    },
                    handleServerNotification: function (data) {
                        self.trace("Message from: " + self.vm.mainModule.models.configuration.mySpeakId + ':' + data);
                        var parsed = data.split(',');
                        if (parseInt(parsed[2]) != 0)
                            self.vm.mainModule.models.configuration.otherPeersSpeak[parseInt(parsed[1])] = parsed[0];
                    },
                    handlePeerMessageSpeak: function (peer_id, data) {
                        var str = "Message from '" + self.vm.mainModule.models.configuration.otherPeersSpeak[peer_id] + "':" + data;

                        var dataJson = JSON.parse(data);
                        if (dataJson != null && dataJson.type != null && dataJson.type == "offer") {
                            self.trace(str);
                            self.trace("Received SDP offer, preparing answer...");
                            self.vm.mainModule.models.configuration.pcSpeak.setRemoteDescription(new RTCSessionDescription(dataJson), onRemoteSdpSuccessSpeak, onRemoteSdpErrorSpeak);
                            self.vm.mainModule.models.configuration.pcSpeak.createAnswer(function (sessionDescription) {
                                if (sessionDescription == null) {
                                    self.trace("Sending SDP answer (callback): undefined SDP");
                                }
                                else {
                                    self.vm.mainModule.models.configuration.pcSpeak.setLocalDescription(sessionDescription, self.vm.mainModule.methods.onSetLocalSdpSuccessSpeak, self.vm.mainModule.methods.onSetLocalSdpFailureSpeak);
                                    var data = JSON.stringify(sessionDescription);
                                    self.trace("Prepared SDP answer (callback): " + data);
                                    self.vm.mainModule.methods.sendToPeerSpeak(peer_id, data);
                                }
                            }, function (error) {
                                self.trace("Create answer error: " + error);
                            }).then(function (sessionDescription) {
                                if (sessionDescription == null) {
                                    self.trace("Sending SDP answer (then): undefined SDP");
                                }
                                else {
                                    self.vm.mainModule.models.configuration.pcSpeak.setLocalDescription(sessionDescription, self.vm.mainModule.methods.onSetLocalSdpSuccessSpeak, self.vm.mainModule.methods.onSetLocalSdpFailureSpeak);
                                    var data = JSON.stringify(sessionDescription);
                                    self.trace("Prepared SDP answer (then): " + data);
                                    self.vm.mainModule.methods.sendToPeerSpeak(peer_id, data);
                                }
                            }).catch((error) => {
                                self.trace("Create SDP answer error: " + error);
                            });
                        }
                        else if (dataJson != null && dataJson.type != null && dataJson.type == "answer") {
                            self.trace(str);
                            self.trace("Received SDP answer");
                            self.vm.mainModule.models.configuration.pcSpeak.setRemoteDescription(new RTCSessionDescription(dataJson), self.vm.mainModule.methods.onRemoteSdpSuccessSpeak, self.vm.mainModule.methods.onRemoteSdpErrorSpeak);
                        }
                        else if (dataJson != null && dataJson.candidate != null) {
                            self.trace(str);
                            self.trace("Adding ICE candidate " + dataJson.candidate);
                            var candidate = new RTCIceCandidate({ sdpMLineIndex: dataJson.sdpMLineIndex, candidate: dataJson.candidate });
                            self.vm.mainModule.models.configuration.pcSpeak.addIceCandidate(candidate, self.vm.mainModule.methods.onIceCandidateSuccessSpeak, self.vm.mainModule.methods.onIceCandidateFailureSpeak);
                        }
                        else if (dataJson != null && dataJson.action != null && dataJson.action == "none") {
                            // hanging request expired
                        }
                        else {
                            self.trace(str);
                            self.trace("Unsupported message!");
                        }
                    },
                    sendToPeerSpeak: function (peer_id, data) {
                        try {
                            self.trace(peer_id + " Send " + data);
                            if (self.vm.mainModule.models.configuration.mySpeakId == -1) {
                                self.trace("Not connected");
                                return;
                            }
                            if (peer_id == self.vm.mainModule.models.configuration.mySpeakId) {
                                self.trace("Can't send a message to oneself :)");
                                return;
                            }
                            var r = new XMLHttpRequest();
                            let uriMsg = self.vmData.ICWebServerUrl + "/message?peer_id=" + self.vm.mainModule.models.configuration.mySpeakId + "&to=" + peer_id;
                            self.trace(uriMsg);
                            r.open("POST", uriMsg, true);
                            r.setRequestHeader("Content-Type", "text/plain");
                            r.send(data);
                        } catch (e) {
                            self.trace("send to peer error: " + e, 'S');
                        }
                    },
                    createPeerConnectionSpeak: function (peer_id) {
                        try {
                            self.vm.mainModule.models.configuration.pcSpeak = new RTCPeerConnection(self.vm.mainModule.models.configuration.pcConfig);
                            self.vm.mainModule.models.configuration.pcSpeak.onicecandidate = function (event) {
                                if (event.candidate) {
                                    var candidate = {
                                        sdpMLineIndex: event.candidate.sdpMLineIndex,
                                        sdpMid: event.candidate.sdpMid,
                                        candidate: event.candidate.candidate
                                    };
                                    self.vm.mainModule.methods.sendToPeerSpeak(peer_id, JSON.stringify(candidate));
                                } else {
                                    self.trace("End of candidates.");
                                }
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onconnecting = function (message) {
                                self.trace("Session connecting.");
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onopen = function (message) {
                                self.trace("Session opened.");
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onremovestream = function (event) {
                                self.trace("Remote stream removed.");
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onidpvalidationerror = function (ev) {
                                self.trace("onidpvalidationerror");
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onidpassertionerror = function (ev) {
                                self.trace("onidpassertionerror");
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onnegotiationneeded = self.vm.mainModule.methods.onNegotiationNeeded;
                            self.vm.mainModule.models.configuration.pcSpeak.onconnectionstatechange = function (event) {
                                self.trace("Connection state changed to " + self.vm.mainModule.models.configuration.pcSpeak.connectionState);
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.oniceconnectionstatechange = function (event) {
                                self.trace("ICE connection state changed to " + self.vm.mainModule.models.configuration.pcSpeak.iceConnectionState);
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onicegatheringstatechange = function () {
                                self.trace("ICE gathering state changed to " + self.vm.mainModule.models.configuration.pcSpeak.iceGatheringState);
                            };
                            self.vm.mainModule.models.configuration.pcSpeak.onsignalingstatechange = function (event) {
                                self.trace("Signaling state changed to " + self.vm.mainModule.models.configuration.pcSpeak.signalingState);
                            };

                            self.trace("Created RTCPeerConnection with config: " + JSON.stringify(self.vm.mainModule.models.configuration.pcConfig));
                        }
                        catch (e) {
                            self.trace("Failed to create PeerConnection, exception: " + e);
                        }
                    },
                    onNegotiationNeeded: async function () {
                        try {
                            self.trace("onnegotiationneeded");
                            self.trace("pcSpeak: " + JSON.stringify(self.vm.mainModule.models.configuration.pcSpeak.localDescription));
                            await self.vm.mainModule.models.configuration.pcSpeak.setLocalDescription(await self.vm.mainModule.models.configuration.pcSpeak.createOffer());
                            var data = JSON.stringify(self.vm.mainModule.models.configuration.pcSpeak.localDescription);
                            self.trace("Prepared SDP offer: " + data);
                            self.vm.mainModule.methods.sendToPeerSpeak(1, data);
                        } catch (e) {
                            self.trace("Failed to negotiate, exception: " + e);
                        }
                    },
                    onSetLocalSdpSuccessSpeak: function () {
                        self.trace("setLocalDescription success");
                    },
                    onSetLocalSdpFailureSpeak: function () {
                        self.trace("setLocalDescription failed");
                    },
                    onIceCandidateSuccessSpeak: function () {
                        self.trace("addIceCandidate success");
                    },
                    onIceCandidateFailureSpeak: function () {
                        self.trace("addIceCandidate failed");
                    },
                    onRemoteSdpSuccessSpeak: function () {
                        self.trace('onRemoteSdpSucces');
                    },
                    onRemoteSdpErrorSpeak: function (event) {
                        self.trace("onRemoteSdpError: event.name: " + event.name + ", event.message: " + event.message);
                    },
                    parseIntHeader: function (r, name) {
                        var val = r.getResponseHeader(name);
                        return val != null && val.length ? parseInt(val) : -1;
                    },
                    signInSpeak: async function () {
                        try {
                            self.vm.mainModule.models.configuration.request = new XMLHttpRequest();
                            self.vm.mainModule.models.configuration.request.onreadystatechange = self.vm.mainModule.methods.signInSpeakCallback;
                            var uri = self.vmData.ICWebServerUrl + "/sign_in?channel=" + self.vmData.PublishingPathSpeak + '-' + self.vmData.AudioPort + "&publish=true";
                            self.trace("Connect to " + uri);
                            self.vm.mainModule.models.configuration.request.open("GET", uri, true);
                            self.vm.mainModule.models.configuration.request.send();
                        } catch (e) {
                            self.trace("error catch signInSpeak: " + e.description);
                        }
                    },
                    permisosMic: async function () {
                        var audioInputSelect = self.vm.mainModule.selectors.audioSource;
                        const audioSource = audioInputSelect.val();

                        if (!audioSource) {
                            const constraints = {
                                audio: { deviceId: audioSource ? { exact: audioSource } : undefined },
                            };

                            let stream = await navigator.mediaDevices.getUserMedia(constraints);
                            self.vm.mainModule.models.configuration.localStream = stream;
                            self.trace('Permisos mic concedidos con localstream: ' + self.vm.mainModule.models.configuration.localStream);
                        }
                    },
                    getMicDevices: async function () {
                        try {
                            await self.vm.mainModule.methods.permisosMic();
                            if (self.vm.mainModule.models.configuration.devices.length == 0) {
                                self.vm.mainModule.models.configuration.devices = await navigator.mediaDevices.enumerateDevices();
                                self.vm.mainModule.methods.gotMicDevices();
                            }
                        } catch (e) {
                            console.log("enumerate error: " + e.description);
                        }
                    },
                    gotMicDevices: function () {
                        var audioInputSelect = self.vm.mainModule.selectors.audioSource;
                        if (audioInputSelect.find('option').length == 0) {
                            for (let i = 0; i <= self.vm.mainModule.models.configuration.devices.length; ++i) {
                                const deviceInfo = self.vm.mainModule.models.configuration.devices[i];
                                const option = document.createElement('option');
                                option.value = deviceInfo.deviceId;
                                option.selected = i == 0;
                                if (deviceInfo.kind === 'audioinput') {
                                    option.text = deviceInfo.label || `Device ${audioInputSelect.length + 1}`;
                                    audioInputSelect[0].appendChild(option);
                                }
                            }
                        }
                    },
                    deviceActivation: function (enable) {
                        $.ajax({
                            url: self.vmData.DeviceActivationUrl,
                            dataType: "json",
                            type: 'POST',
                            data: {
                                codigoDispositivo: self.vmData.Codigo,
                                activar: enable,
                            },
                            success: function (data) {
                            },
                            error: function (error) {
                            },
                        }).always(function () {
                        });
                    }
                },
                models: {
                    configuration: {
                        pcConfig: JSON.parse(self.vmData.ICPCConfig),
                        myListenId: -1,
                        mySpeakId: -1,
                        retry: false,
                        websocket: null,
                        disconnectingListen: null,
                        pingInterval: null,
                        request: null,
                        hangingGet: null,
                        pcListen: null,
                        pcSpeak: null,
                        localStream: null,
                        otherPeersListen: {},
                        otherPeersSpeak: {},
                        devices: [],
                        debugLog: [],
                    }
                },
                events: {},
                validations: {},
            },
        };

        self.inicializar();
    },
    trace: function (message) {
        console.log(message);
        let self = this;
        self.vm.mainModule.models.configuration.debugLog.push(message);
    },
    inicializar: function () {
        let self = this;
        self.vm.mainModule.methods.getMicDevices();
        self.vm.mainModule.selectors.btnActivar.off();
        self.vm.mainModule.selectors.btnMicrofono.prop('disabled', !self.vm.mainModule.selectors.btnActivar.prop('checked'));
        self.vm.mainModule.selectors.btnActivar.on('change', () => {
            self.vm.mainModule.selectors.btnMicrofono.prop('disabled', !self.vm.mainModule.selectors.btnActivar.prop('checked'));
            self.vm.mainModule.models.configuration.retry = self.vm.mainModule.selectors.btnActivar.prop('checked');
            if (self.vm.mainModule.selectors.btnActivar.prop('checked')) {
                self.vm.mainModule.models.configuration.debugLog = [];
                self.trace('Listo para recibir audio');
                self.vm.mainModule.methods.signInSpeak();
                self.vm.mainModule.methods.connectListen();
                self.vm.mainModule.methods.deviceActivation(true);
            } else {
                self.trace('Fin de recepcion');
                self.vm.mainModule.methods.disconnectListen();
                self.vm.mainModule.methods.disconnectSpeak();
                self.vm.mainModule.methods.deviceActivation(false);
            }
        });

        self.vm.mainModule.selectors.btnMicrofono.on('mousedown', (e) => {
            self.trace('**************** Inicio de envio de audio **************');
            self.vm.mainModule.models.configuration.localStream.getTracks().forEach((track) => track.enabled = true);
            self.vm.mainModule.selectors.remotePlayer.prop('muted', true);
            self.vm.mainModule.states.microfonoActivado = true;
            self.vm.mainModule.selectors.pressedAudioButton = $(e.currentTarget);
            self.vm.mainModule.selectors.pressedAudioButton.find("i").switchClass("fa-microphone-slash", "fa-microphone", 0);
        });

        $(document).on('mouseup', () => {
            if (self.vm.mainModule.states.microfonoActivado) {
                self.trace('******************* Fin de emision de audio *******************');
                self.vm.mainModule.models.configuration.localStream.getTracks().forEach((track) => track.enabled = false);
                self.vm.mainModule.selectors.remotePlayer.prop('muted', false);
                self.vm.mainModule.states.microfonoActivado = false;
                if (self.vm.mainModule.selectors.pressedAudioButton) {
                    self.vm.mainModule.selectors.pressedAudioButton.find("i").switchClass("fa-microphone", "fa-microphone-slash", 0);
                    self.vm.mainModule.selectors.pressedAudioButton = null;
                }
            }
        });

        $(window).on('unload', function () {
            self.vm.mainModule.methods.disconnectListen();
        });
    }
}