using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Logging;
using Sfs2X.Requests;
using Sfs2X.Entities;


public class SmartFoxHandler  {
	
	private static SmartFoxHandler _instance;
	public static SmartFoxHandler Instance
	{
		get{ 
			if (_instance ==null)
			{
					_instance = new SmartFoxHandler();
			}
			return _instance;
		}
	}
	
	private SmartFox sfs;
	private string _exeptionMessage;
	public string lastErrorMessage
	{
		get
		{
			return _exeptionMessage;
		}
	}
	
	private string serverAdress = "127.0.0.1";
	private string serverPort   = "9933";
	
	public string UserName = "";
	public string ZoneName = "BasicExamples";
	public string RoomName = "The Lobby";
	
	private bool isLoggedIn = false;
	private bool isJoining  = false;
	private bool isJoined   = false;
	
	public SmartFoxHandler()
	{
		sfs = new SmartFox();
		sfs.ThreadSafeMode = false;
	}
	
	 ~SmartFoxHandler()
	{
		if (sfs!=null && sfs.IsConnected)
		this.Disconnect();
	}
	
	private void InitListiners()
	{
		sfs.RemoveAllEventListeners();
		
		//присоединяемся к серверу
	 	sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);        
	
	     //потеря соединения с сервером
	    sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);   
	
		//авторизовались на сервере
	    sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);             
	
	     //ошибка авторизации с сервером
	    sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);       
	
	   //пришел запрос что произведен выход
	    sfs.AddEventListener(SFSEvent.LOGOUT, OnLogout);            
	
	    //udp инициализирован  
	    sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);          
	
	    //присоединились к комнате
	    sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);         
	
	    //ошибка создания комнаты
	    sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnCreateRoomError); 
	
	    //ошибка конекта комнаты
	    sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);    
		
		//прием массового сообщения
		sfs.AddEventListener(SFSEvent.PUBLIC_MESSAGE,OnPublicMessage);
	}
	
	public void ProcessEvents () {
		sfs.ProcessEvents();
	}
	
	public string ServerAdress
	{
		get{ return serverAdress;}
	}
	
	public string ServerPort
	{
		get{ return serverPort;}
	}
	
	public bool IsConnected
	{
		get { return sfs.IsConnected;}
	}
	
	public bool IsLoggedIn
	{
		get {return isLoggedIn;}
	}
	
	public bool IsJoining
	{
		get {return isJoining;}
	}
	
	public bool IsJoinned
	{
		get {return isJoined;}
	}
	
	public string Connect(string ip,int port,string userName)
	{
		string debugMessage = String.Empty;
		try
		{
			this.serverAdress = ip;
			this.serverPort = port.ToString();
			this.UserName = userName;
			
			sfs.Connect(this.serverAdress,
						int.Parse(this.serverPort));
			
			InitListiners();
			ResetStates();
		}
		catch(Exception ex)
		{
			debugMessage = ex.Message;
		}
		return debugMessage;
	}
	
	
	
	public void Disconnect()
	{
		this.ClearListiners();
		sfs.Disconnect();
	}
	
    private void ClearListiners() {
    	sfs.RemoveAllEventListeners();
   }
	
	public RoomSettings CreateRoomSettings(string roomName,short numMaxUsers = 100)
	{
	      //записываем все настройки нашей новой комнаты 
	      RoomSettings settings = new RoomSettings(roomName);
	
		  settings.MaxUsers = numMaxUsers;
	      settings.GroupId = "game";
	      settings.IsGame = true;
	      settings.MaxUsers = (short)numMaxUsers;
	      settings.MaxSpectators = 0;
		return settings;
	}
	
	public string[] GetRoomNames()
	{
		//получаем от smartfox список всех комнат и перебираем их
	    List<Sfs2X.Entities.Room> allRooms = sfs.RoomManager.GetRoomList();
		
		string[] roomsName = new string[allRooms.Count];
		
	    for (int i =0; i<allRooms.Count; i++) 
		{
	      roomsName[i] = allRooms[i].Name;
	    }
		return roomsName;
	}
	
	private Sfs2X.Entities.Room FindRoomByName(string name)
	{
		List<Sfs2X.Entities.Room> allRooms = sfs.RoomManager.GetRoomList();
	    for (int i =0; i<allRooms.Count; i++) 
		{
	       if (allRooms[i].Name==name)
				return allRooms[i];
	    }
		return null;
	}
	
	public void ConnectToRoom(string roomName) 
	{
	
	    //получаем от smartfox список всех комнат и перебираем их
	    List<Sfs2X.Entities.Room> allRooms = sfs.RoomManager.GetRoomList();
	
		Room room = null;
		room = FindRoomByName(roomName);
	    //если игровых комнат не найдено - создадим свою
	    if (room==null) {
			roomName = sfs.MySelf.Name + " game";
			RoomSettings settings = CreateRoomSettings(roomName);
	      	sfs.Send(new CreateRoomRequest(settings, true, sfs.LastJoinedRoom));
	    }
	    else {
	       this.isJoining = true;
	       sfs.Send(new JoinRoomRequest(roomName));
	    }
	
	  }
	
	public void SendAll(string text)
	{
		sfs.Send(new PublicMessageRequest(text));
	}

	///EVENTS
	
	private void OnConnection(BaseEvent evt) 
	{
	    bool success = (bool)evt.Params["success"];
	    if (success) {
		  //Debug.Log("OnConnection "+"succesed");
			
	      //отправляем наши имя, пустой пароль и название зоны для присоединения
	      sfs.Send(new LoginRequest(UserName, "", ZoneName));  
	    }
	    else _exeptionMessage = "On Connection callback got: " 
								+ success + " (error : <" 
								+ (string)evt.Params["errorMessage"] + ">)";
	
	  }
	
	private void ResetStates()
	{
		this.isJoined =  false;
		this.isJoining = false;
		this.isLoggedIn =false;
	}
	
	private void OnConnectionLost(BaseEvent evt) 
	{
	    _exeptionMessage = "OnConnectionLost";
	    ClearListiners();
	  }

  	private void OnLogin(BaseEvent evt) 
	{
    try {
      if (evt.Params.Contains("success") && !(bool)evt.Params["success"]) {
        _exeptionMessage = (string)evt.Params["errorMessage"];
      }
      else {
		this.isLoggedIn = true;
        sfs.InitUDP(serverAdress, int.Parse(serverPort));
      }
    }

    catch (Exception ex) {
      _exeptionMessage = "Exception handling login request: " + ex.Message + " " + ex.StackTrace;
    }

  }

	private void OnLoginError(BaseEvent evt) 
	{
	    _exeptionMessage = "Login error: " + (string)evt.Params["errorMessage"];
	}

  	private void OnLogout(BaseEvent evt) {
   		_exeptionMessage = "Logout";
    	sfs.Disconnect();
		ResetStates();
  	}

  	private void OnUdpInit(BaseEvent evt) 
	{

	    if (evt.Params.Contains("success") && !(bool)evt.Params["success"]) {
	      _exeptionMessage = (string)evt.Params["errorMessage"];
	    }
	    else {
			ConnectToRoom(this.RoomName);//берем список комнат
	    }
			
	}

  	private void OnJoinRoom(BaseEvent evt) 
	{
		Debug.Log("!!!!!joined!!!!!!!");
    	Room room = (Room)evt.Params["room"];
	    //if (room.IsGame)
			this.isJoined = true;
	      //ClearListiners();
	      _exeptionMessage = "Joined game room " + room.Name;
		  sfs.Send(new PublicMessageRequest("User "+this.UserName+" entered to room "));
  	}

    private void OnCreateRoomError(BaseEvent evt) 
	{
    _exeptionMessage = "Room creation error; the following error occurred: " 
						+ (string)evt.Params["errorMessage"];
  	}

    private void OnJoinRoomError(BaseEvent evt) 
	{

    string error = (string)evt.Params["errorMessage"];
    _exeptionMessage = "Room join error; the following error occurred: " 
						+ error;
  	}
	
	private void OnPublicMessage(BaseEvent ev)
	{
		string message = (string)ev.Params["message"];
	    User sender = (User)ev.Params["sender"];
	    Debug.Log("User " + sender.Name + " said: " + message);
	}

}
