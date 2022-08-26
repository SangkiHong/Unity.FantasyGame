using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace SK
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        [Header("UI")]
        public Text StatusText;
        public InputField RoomInput, NickNameInput;

        private void Awake() => Screen.SetResolution(960, 540, false);

        void Start() => PhotonNetwork.ConnectUsingSettings();

        public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

        public override void OnJoinedLobby() => PhotonNetwork.JoinOrCreateRoom("room", new RoomOptions { MaxPlayers = 4 }, null);

        private void Update()
        {
            StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        }

        public override void OnConnected()
        {
            base.OnConnected();
            print("�������ӿϷ�");
            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        }

        public void Disconnect() => PhotonNetwork.Disconnect();

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            print("�������");
        }

        public void JoinLobby() => PhotonNetwork.JoinLobby();
        public void CreateRoom() => PhotonNetwork.CreateRoom(RoomInput.text, new RoomOptions { MaxPlayers = 2 });

        public void JoinRoom() => PhotonNetwork.JoinRoom(RoomInput.text);

        public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(RoomInput.text, new RoomOptions { MaxPlayers = 2 }, null);

        public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

        public void LeaveRoom() => PhotonNetwork.LeaveRoom();

        public override void OnCreatedRoom() => print("�游���Ϸ�");

        public override void OnJoinedRoom() => print("�������Ϸ�");

        public override void OnCreateRoomFailed(short returnCode, string message) => print("�游������");

        public override void OnJoinRoomFailed(short returnCode, string message) => print("����������");

        public override void OnJoinRandomFailed(short returnCode, string message) => print("�淣����������");

    }
}
