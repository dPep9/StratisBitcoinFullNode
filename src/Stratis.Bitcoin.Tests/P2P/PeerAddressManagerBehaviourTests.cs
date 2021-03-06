﻿using System;
using System.IO;
using System.Net;
using Moq;
using NBitcoin;
using Stratis.Bitcoin.Configuration.Logging;
using Stratis.Bitcoin.P2P;
using Stratis.Bitcoin.P2P.Peer;
using Stratis.Bitcoin.P2P.Protocol;
using Stratis.Bitcoin.P2P.Protocol.Payloads;
using Stratis.Bitcoin.Utilities;
using Xunit;

namespace Stratis.Bitcoin.Tests.P2P
{
    public sealed class PeerAddressManagerBehaviourTests : TestBase
    {
        private readonly ExtendedLoggerFactory extendedLoggerFactory;
        private readonly Network network;
        private readonly INetworkPeerFactory networkPeerFactory;

        public PeerAddressManagerBehaviourTests()
        {
            this.extendedLoggerFactory = new ExtendedLoggerFactory();
            this.extendedLoggerFactory.AddConsoleWithFilters();

            this.network = Network.Main;
            this.networkPeerFactory = new NetworkPeerFactory(this.network, DateTimeProvider.Default, this.extendedLoggerFactory);
        }

        [Fact]
        public void PeerAddressManagerBehaviour_ReceivedPing_UpdateLastSeen()
        {
            var ipAddress = IPAddress.Parse("::ffff:192.168.0.1");
            var endpoint = new IPEndPoint(ipAddress, 80);

            var peerFolder = AssureEmptyDirAsDataFolder(Path.Combine(AppContext.BaseDirectory, "PeerAddressManagerBehaviourTests"));
            var addressManager = new PeerAddressManager(DateTimeProvider.Default, peerFolder, this.loggerFactory);
            addressManager.AddPeer(endpoint, IPAddress.Loopback);

            var networkPeer = new Mock<INetworkPeer>();
            networkPeer.SetupGet(n => n.PeerEndPoint).Returns(endpoint);
            networkPeer.SetupGet(n => n.State).Returns(NetworkPeerState.HandShaked);

            var messageReceived = new AsyncExecutionEvent<INetworkPeer, IncomingMessage>();
            networkPeer.SetupGet(n => n.MessageReceived).Returns(messageReceived);

            var stateChanged = new AsyncExecutionEvent<INetworkPeer, NetworkPeerState>();
            networkPeer.SetupGet(n => n.StateChanged).Returns(stateChanged); 

            var behaviour = new PeerAddressManagerBehaviour(DateTimeProvider.Default, addressManager) { Mode = PeerAddressManagerBehaviourMode.AdvertiseDiscover };
            behaviour.Attach(networkPeer.Object);

            var message = new IncomingMessage(new PingPayload(), this.network);

            //Trigger the event handler
            networkPeer.Object.MessageReceived.ExecuteCallbacksAsync(networkPeer.Object, message).GetAwaiter().GetResult();

            var peer = addressManager.FindPeer(endpoint);
            Assert.Equal(DateTimeProvider.Default.GetUtcNow().Date, peer.LastSeen.Value.Date);
        }

        [Fact]
        public void PeerAddressManagerBehaviour_ReceivedPong_UpdateLastSeen()
        {
            var ipAddress = IPAddress.Parse("::ffff:192.168.0.1");
            var endpoint = new IPEndPoint(ipAddress, 80);

            var peerFolder = AssureEmptyDirAsDataFolder(Path.Combine(AppContext.BaseDirectory, "PeerAddressManagerBehaviourTests"));
            var addressManager = new PeerAddressManager(DateTimeProvider.Default, peerFolder, this.loggerFactory);
            addressManager.AddPeer(endpoint, IPAddress.Loopback);

            var networkPeer = new Mock<INetworkPeer>();
            networkPeer.SetupGet(n => n.PeerEndPoint).Returns(endpoint);
            networkPeer.SetupGet(n => n.State).Returns(NetworkPeerState.HandShaked);

            var messageReceived = new AsyncExecutionEvent<INetworkPeer, IncomingMessage>();
            networkPeer.SetupGet(n => n.MessageReceived).Returns(messageReceived);

            var stateChanged = new AsyncExecutionEvent<INetworkPeer, NetworkPeerState>();
            networkPeer.SetupGet(n => n.StateChanged).Returns(stateChanged);

            var behaviour = new PeerAddressManagerBehaviour(DateTimeProvider.Default, addressManager) { Mode = PeerAddressManagerBehaviourMode.AdvertiseDiscover };
            behaviour.Attach(networkPeer.Object);

            var message = new IncomingMessage(new PongPayload(), this.network);

            //Trigger the event handler
            networkPeer.Object.MessageReceived.ExecuteCallbacksAsync(networkPeer.Object, message).GetAwaiter().GetResult();

            var peer = addressManager.FindPeer(endpoint);
            Assert.Equal(DateTimeProvider.Default.GetUtcNow().Date, peer.LastSeen.Value.Date);
        }
    }
}