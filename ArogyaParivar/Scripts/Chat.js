(function () {
    var app = angular.module('chat-app', []).directive('myPostRepeatDirective', function() {
        return function(scope, element, attrs) {
            if (scope.$last){
                // iteration is complete, do whatever post-processing
                // is necessary
                element.parent().css('border', '1px solid black');
            }
        }});

    app.controller('ChatController', function ($scope) {
        // scope variables
        $scope.name = 'Guest'; // holds the user's name
        $scope.message = ''; // holds the new message
        $scope.messages = []; // collection of messages coming from server
        $scope.chatHub = null; // holds the reference to hub
        $scope.onlineUsers = []; // collection of online users
        $scope.onlineUsers[0] = userID;
        $scope.chatHub = $.connection.chatHub; // initializes hub
        $.connection.hub.qs = { "UserName": userName };
        $.connection.hub.start().done(function () {
            $scope.chatHub.server.getAllOnlineStatus();
        }); // starts hub

        
        // register a client method on hub to be invoked by the server
        $scope.chatHub.client.broadcastMessage = function (name, message) {
            var newMessage = name + ' says: ' + message;

            // push the newly coming message to the collection of messages
            $scope.messages.push(newMessage);
            $scope.$apply();
        };

        $scope.newMessage = function () {
            // sends a new message to the server
            $scope.chatHub.server.sendMessage($scope.name, $scope.message);

            $scope.message = '';
        }

        $scope.chatHub.client.joined = function (connectionId, userList) {
            $scope.onlineUsers = userList;
        };

        $scope.chatHub.client.OnlineStatus = function (connectionId, userList) {
            $scope.onlineUsers = userList;
            $scope.$apply();
        };

        $scope.UserItemClick = function (val) {
            //if ($(this).hasClass('online')) {
                $scope.chatHub.server.createGroup(userID,val);
                var chatWindow = $("#divChatWindow").clone(true);
                $(chatWindow).attr('chatToId', val);
                $("#chatContainer").append(chatWindow);
            //}
            return false;
        };

        $scope.chatHub.client.setChatWindow = function (groupName, toConnectTo) {
            var chatWindow = $('[chattoid="'+toConnectTo+'"]');
            chatWindow.attr("groupname", groupName);
            chatWindow.show();
        };

        // submit button click event
        $scope.ChatSend = function ($event) {
            strChatText = $('.ChatText', $event.currentTarget.parentElement).val();
            if (strChatText != '') {
                var strGroupName = $($event.currentTarget.parentElement).attr("groupname");
                if (typeof strGroupName !== 'undefined' && strGroupName !== false)
                    $scope.chatHub.server.send(userName + ' : ' + strChatText, strGroupName);
                $('.ChatText', $(this).parent()).find('ul').append(strChatText);
            }
            return false;
        };

        $scope.chatHub.client.addMessage = function (message, groupName) {
            if ($('div[groupname=' + groupName + ']').length == 0) {
                var chatWindow = $("#divChatWindow").clone(true);
                $(chatWindow).css('display', 'block');
                $(chatWindow).attr('groupname', groupName);
                $("#chatContainer").append(chatWindow);
            }
            $('div[groupname=' + groupName + ']').find('ul').append('<LI>' + message + '');
        };
    })
}());
