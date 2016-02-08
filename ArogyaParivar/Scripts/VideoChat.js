var mediaStream = null;
(function () {
    var app = angular.module('videochat-app', []);

    app.controller('VideoChatController', function ($scope, $attrs) {
        // scope variables
        $scope.mediaStream = null;
        $scope.hub = $.connection.webRtcHub;
        $scope.userName ='';
        $scope.connectionID = '';
        $scope.mode = 'idle',
        $scope.userList = [],
        $scope.intiator = ($attrs.usertype == 'nurse') ? false : true,
        $scope.targetID = '',
        $scope.userID = '';
        
        _connect = function(username, onSuccess, onFailure) {
            // Set Up SignalR Signaler
            var hub = $.connection.webRtcHub;
            $.support.cors = true;
            $.connection.hub.url = '/signalr/hubs';
            // Setup client SignalR operations
            _setupHubCallbacks(hub);
            $.connection.hub.start()
                .done(function () {
                    console.log('connected to SignalR hub... connection id: ' + $scope.hub.connection.id);

                    // Tell the hub what our username is
                    hub.server.join(username,$scope.userID);
                    
                    if (onSuccess) {
                        onSuccess(hub);
                    }
                })
                .fail(function (event) {
                    if (onFailure) {
                        onFailure(event);
                    }
                });

            
            $scope.hub = hub;
        },

        _start = function (arogyaid, UserID, TargetUserID,UserName,UserType) {
            hub = $scope.hub;
            if (UserType == 'doctor') {
                $scope.intiator = true;
            }
            else
                $scope.intiator = false;

            $scope.userid = UserID;
            $scope.targetID = TargetUserID;
            $scope.arogyaID = arogyaid;
            // Show warning if WebRTC support is not detected
            if (webrtcDetectedBrowser == null) {
                console.log('Your browser doesnt appear to support WebRTC.');
                $('.browser-warning').show();
            }
            _startSession(UserName);

            // Then proceed to the next step, gathering username
            //_getUsername();
        },
        
        _getUsername = function() {
            alertify.prompt("What is your name?", function (e, username) {
                if (e == false || username == '') {
                    username = 'User ' + Math.floor((Math.random() * 10000) + 1);
                    alertify.success('You really need a username, so we will call you... ' + username);
                }

                // proceed to next step, get media access and start up our connection
                _startSession(username);
            }, '');
        },

        _startSession = function (username) {
            $scope.userName = username;
            //viewModel.Username(username); // Set the selected username in the UI
            //viewModel.Loading(true); // Turn on the loading indicator

            // Ask the user for permissions to access the webcam and mic
            getUserMedia(
                {
                    // Permissions to request
                    video: true,
                    audio: true
                },
                function (stream) { // succcess callback gives us a media stream
                    //$('.instructions').hide();
                    
                    // Now we have everything we need for interaction, so fire up SignalR
                    _connect(username, function (hub) {                        
                        // tell the viewmodel our conn id, so we can be treated like the special person we are.
                        $scope.connectionID = hub.connection.id;
                        //viewModel.MyConnectionId(hub.connection.id);
                        // Initialize our client signal manager, giving it a signaler (the SignalR hub) and some callbacks
                        console.log('initializing connection manager');
                        connectionManager.initialize(hub.server, _callbacks.onReadyForStream, _callbacks.onStreamAdded, _callbacks.onStreamRemoved);
                        
                        // Store off the stream reference so we can share it later
                        mediaStream = $scope.mediaStream = stream;

                        // Load the stream into a video element so it starts playing in the UI
                        console.log('playing my local video feed');
                        var videoElement = document.querySelector('.video.mine');
                        attachMediaStream(videoElement, $scope.mediaStream);

                        // Hook up the UI
                        _attachUiHandlers();
                        if($scope.intiator)
                            _placeCall();
                        //viewModel.Loading(false);
                    }, function (event) {
                        debugger;
                        alert('<h4>Failed SignalR Connection</h4> We were not able to connect you to the signaling server.<br/><br/>Error: ' + JSON.stringify(event));
                        //viewModel.Loading(false);
                    });  
                },
                function (error) { // error callback
                    alert('<h4>Failed to get hardware access!</h4> Do you have another browser type open and using your cam/mic?<br/><br/>Actual Error: ' + JSON.stringify(error));
                    //viewModel.Loading(false);
                }
            );
        },
        _placeCall = function () {
            // Find the target user's SignalR client id
            //var targetConnectionId = $(this).attr('data-cid');
            
            // Make sure we are in a state where we can make a call
            if ($scope.mode !== 'idle') {
                alertify.error('Sorry, you are already in a call.');
                return;
            }

            // Then make sure we aren't calling ourselves.
            if ($scope.targetUserID != $scope.userID) {
                // Initiate a call
                $scope.hub.server.callUser($scope.targetUserID, $scope.arogyaID);
                    
                // UI in calling mode
                $scope.mode = 'calling';
                $scope.$apply();
            } else {
                alertify.error("Cannot call yourself.");
            }
        },
        _attachUiHandlers = function() {
            // Add click handler to users in the "Users" pane
            $('.user').live('click', _placeCall);

            // Add handler for the hangup button
            $('.hangup').click(function () {
                // Only allow hangup if we are not idle
                if ($scope.mode != 'idle') {
                    $scope.hub.server.hangUp();
                    connectionManager.closeAllConnections();
                    $scope.mode = 'idle';
                    $scope.$apply();
                }
            });
        },
        
        _setupHubCallbacks = function (hub) {
            // Hub Callback: Incoming Call
            hub.client.incomingCall = function (callingUser,arogyaID) {
                console.log('incoming call from: ' + JSON.stringify(callingUser) + ' for Patient with ArogyaID : ' + JSON.stringify(arogyaID));

                // Ask if we want to talk
                alertify.confirm(callingUser.Username + ' is calling ' + ' for Patient with ArogyaID : ' + arogyaID + '.  Do you want to chat?', function (e) {
                    if (e) {
                        // I want to chat
                        hub.server.answerCall(true, callingUser.ConnectionId);
                        $('#IfCasesheet')[0].src = "/HealthEducator/VideoCasesheet?arogyaID=" + arogyaID;
                        // So lets go into call mode on the UI
                        $scope.mode = 'incall';
                        $scope.$apply();
                    } else {
                        // Go away, I don't want to chat with you
                        hub.server.answerCall(false, callingUser.ConnectionId);
                    }
                });
            };

            // Hub Callback: Call Accepted
            hub.client.callAccepted = function (acceptingUser) {
                console.log('call accepted from: ' + JSON.stringify(acceptingUser) + '.  Initiating WebRTC call and offering my stream up...');

                // Callee accepted our call, let's send them an offer with our video stream
                connectionManager.initiateOffer(acceptingUser.ConnectionId, $scope.mediaStream);
                
                // Set UI into call mode
                $scope.mode = 'incall';
                $scope.$apply();
            };

            // Hub Callback: Call Declined
            hub.client.callDeclined = function (decliningConnectionId, reason) {
                console.log('call declined from: ' + decliningConnectionId);

                // Let the user know that the callee declined to talk
                alertify.error(reason);

                // Back to an idle UI
                $scope.mode = 'idle';
                $scope.$apply();
            };

            // Hub Callback: Call Ended
            hub.client.callEnded = function (connectionId, reason) {
                console.log('call with ' + connectionId + ' has ended: ' + reason);

                // Let the user know why the server says the call is over
                alertify.error(reason);

                // Close the WebRTC connection
                connectionManager.closeConnection(connectionId);

                // Set the UI back into idle mode
                $scope.mode = 'idle';
                $scope.$apply();
            };

            // Hub Callback: Update User List
            hub.client.updateUserList = function (userList) {
                $scope.userList = userList;
                $scope.$apply();
            };

            // Hub Callback: WebRTC Signal Received
            hub.client.receiveSignal = function (callingUser, data) {
                connectionManager.newSignal(callingUser.ConnectionId, data);
            };
        },

        // Connection Manager Callbacks
        _callbacks = {
            onReadyForStream: function (connection) {
                // The connection manager needs our stream
                // todo: not sure I like this
                connection.addStream($scope.mediaStream);
            },
            onStreamAdded: function (connection, event) {
                console.log('binding remote stream to the partner window');

                // Bind the remote stream to the partner window
                var otherVideo = document.querySelector('.video.partner');
                attachMediaStream(otherVideo, event.stream); // from adapter.js
            },
            onStreamRemoved: function (connection, streamId) {
                // todo: proper stream removal.  right now we are only set up for one-on-one which is why this works.
                console.log('removing remote stream from partner window');
                
                // Clear out the partner window
                var otherVideo = document.querySelector('.video.partner');
                otherVideo.src = '';
            }
        };

        if (!$scope.intiator)
            _start('', 3, '', 'nurse', 'nurse');
        //_start($scope.hub);

})
}());