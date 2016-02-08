using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ArogyaParivar.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly List<UserCall> UserCalls = new List<UserCall>();
        private static readonly List<CallOffer> CallOffers = new List<CallOffer>(); 

        public void SendMessage(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public override Task OnConnected()
        {
            var newUsers = OnlineUser.userObj.Where(item => item.newStatus == true).Select(item => item.userId).ToList();
            UserModal user = OnlineUser.userObj.Where(item => item.userName == Context.QueryString["UserName"].ToString()).SingleOrDefault();
            user.connectionId = Context.ConnectionId;
            return Clients.All.joined(Context.ConnectionId, newUsers);
        }

        public void GetAllOnlineStatus()
        {
            Clients.Caller.OnlineStatus(Context.ConnectionId, OnlineUser.userObj.Select(item => item.userId).ToList());
        }

        public void CreateGroup(string currentUserId, string toConnectTo)
        {
            string strGroupName = GetUniqueGroupName(currentUserId, toConnectTo);
            string connectionId_To = OnlineUser.userObj.Where(item => item.userId == toConnectTo).Select(item => item.connectionId).SingleOrDefault();
            if (!string.IsNullOrEmpty(connectionId_To))
            {
                Groups.Add(Context.ConnectionId, strGroupName);
                Groups.Add(connectionId_To, strGroupName);
                Clients.Caller.setChatWindow(strGroupName, toConnectTo);
            }
        }

        private string GetUniqueGroupName(string currentUserId, string toConnectTo)
        {
            return (currentUserId.GetHashCode() ^ toConnectTo.GetHashCode()).ToString();
        }

        public void Send(string message, string groupName)
        {
            if (Clients != null)
            {
                Clients.Group(groupName).addMessage(message, groupName);
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
           
            if (OnlineUser.userObj.Any(x => x.connectionId == Context.ConnectionId))
            {
                HangUp();
                UserModal user = OnlineUser.userObj.First(x => x.connectionId == Context.ConnectionId);
                OnlineUser.userObj.Remove(user);
                GetAllOnlineStatus();
            }
        
            return base.OnDisconnected(stopCalled);
        }

        public void CallUser(string targetConnectionId)
        {
            var callingUser = OnlineUser.userObj.SingleOrDefault(u => u.connectionId == Context.ConnectionId);
            var targetUser = OnlineUser.userObj.SingleOrDefault(u => u.connectionId == targetConnectionId);

            // Make sure the person we are trying to call is still here
            if (targetUser == null)
            {
                // If not, let the caller know
                Clients.Caller.callDeclined(targetConnectionId, "The user you called has left.");
                return;
            }

            // And that they aren't already in a call
            if (GetUserCall(targetUser.connectionId) != null)
            {
                Clients.Caller.callDeclined(targetConnectionId, string.Format("{0} is already in a call.", targetUser.userName));
                return;
            }

            // They are here, so tell them someone wants to talk
            Clients.Client(targetConnectionId).incomingCall(callingUser);

            // Create an offer
            CallOffers.Add(new CallOffer
            {
                Caller = callingUser,
                Callee = targetUser
            });
        }

        public void AnswerCall(bool acceptCall, string targetConnectionId)
        {
            var callingUser = OnlineUser.userObj.SingleOrDefault(u => u.connectionId == Context.ConnectionId);
            var targetUser = OnlineUser.userObj.SingleOrDefault(u => u.connectionId == targetConnectionId);

            // This can only happen if the server-side came down and clients were cleared, while the user
            // still held their browser session.
            if (callingUser == null)
            {
                return;
            }

            // Make sure the original caller has not left the page yet
            if (targetUser == null)
            {
                Clients.Caller.callEnded(targetConnectionId, "The other user in your call has left.");
                return;
            }

            // Send a decline message if the callee said no
            if (acceptCall == false)
            {
                Clients.Client(targetConnectionId).callDeclined(callingUser, string.Format("{0} did not accept your call.", callingUser.userName));
                return;
            }

            // Make sure there is still an active offer.  If there isn't, then the other use hung up before the Callee answered.
            var offerCount = CallOffers.RemoveAll(c => c.Callee.connectionId == callingUser.connectionId
                                                  && c.Caller.connectionId == targetUser.connectionId);
            if (offerCount < 1)
            {
                Clients.Caller.callEnded(targetConnectionId, string.Format("{0} has already hung up.", targetUser.userName));
                return;
            }

            // And finally... make sure the user hasn't accepted another call already
            if (GetUserCall(targetUser.connectionId) != null)
            {
                // And that they aren't already in a call
                Clients.Caller.callDeclined(targetConnectionId, string.Format("{0} chose to accept someone elses call instead of yours :(", targetUser.userName));
                return;
            }

            // Remove all the other offers for the call initiator, in case they have multiple calls out
            CallOffers.RemoveAll(c => c.Caller.connectionId == targetUser.connectionId);

            // Create a new call to match these folks up
            UserCalls.Add(new UserCall
            {
                Users = new List<UserModal> { callingUser, targetUser }
            });

            // Tell the original caller that the call was accepted
            Clients.Client(targetConnectionId).callAccepted(callingUser);

            // Update the user list, since thes two are now in a call
            SendUserListUpdate();
        }

        public void HangUp()
        {
            var callingUser = OnlineUser.userObj.SingleOrDefault(u => u.connectionId == Context.ConnectionId);

            if (callingUser == null)
            {
                return;
            }

            var currentCall = GetUserCall(callingUser.connectionId);

            // Send a hang up message to each user in the call, if there is one
            if (currentCall != null)
            {
                foreach (var user in currentCall.Users.Where(u => u.connectionId != callingUser.connectionId))
                {
                    Clients.Client(user.connectionId).callEnded(callingUser.connectionId, string.Format("{0} has hung up.", callingUser.userName));
                }

                // Remove the call from the list if there is only one (or none) person left.  This should
                // always trigger now, but will be useful when we implement conferencing.
                currentCall.Users.RemoveAll(u => u.connectionId == callingUser.connectionId);
                if (currentCall.Users.Count < 2)
                {
                    UserCalls.Remove(currentCall);
                }
            }

            // Remove all offers initiating from the caller
            CallOffers.RemoveAll(c => c.Caller.connectionId == callingUser.connectionId);

            SendUserListUpdate();
        }

        #region Private Helpers

        private void SendUserListUpdate()
        {
            OnlineUser.userObj.ForEach(u => u.InCall = (GetUserCall(u.connectionId) != null));
            Clients.All.updateUserList(Users);
        }

        private UserCall GetUserCall(string connectionId)
        {
            var matchingCall =
                UserCalls.SingleOrDefault(uc => uc.Users.SingleOrDefault(u => u.connectionId == connectionId) != null);
            return matchingCall;
        }

        #endregion
    }

    public class UserModal
    {
        public string connectionId = string.Empty;
        public string userName = string.Empty;
        public string userId = string.Empty;
        public bool newStatus = false;
        public string sessionId = string.Empty;
        public bool InCall = false;
    }

    public static class OnlineUser
    {
        public static List<UserModal> userObj = new List<UserModal>();

        public static UserModal AddOnlineUser(string strConnectionId, string strUserName, string strUserId, string strSessionId)
        {
            UserModal user = new UserModal();
            user.connectionId = strConnectionId;
            user.userName = strUserName;
            user.userId = strUserId;
            user.newStatus = true;
            user.sessionId = strSessionId;
            userObj.Add(user);
            return user;
        }

        public static void RemoveOnlineUser(string strConnectionId, string strUserId)
        {
            var userRemove = (UserModal)userObj.Where(item => item.connectionId == strConnectionId);
            userObj.Remove(userRemove);
        }
    }

    public class UserCall
    {
        public List<UserModal> Users;
    }

    public class CallOffer
    {
        public UserModal Caller;
        public UserModal Callee;
    }
}