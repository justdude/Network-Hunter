using UnityEngine;
using System.Collections;

public class LoginGUI : MonoBehaviour {
	
	public int NextLevelNumber = 1;
	
	public string serverName = "127.0.0.1";
	public string serverPort = "9933";
	
	public string username = "iam";
	public string password = "123";
	
	private bool enabled = false;
	private bool isLoading = false;
	
	void Start () {
		enabled = true;
	}
	
	void OnGUI() {
		if(!enabled) return; 
		
	    //GUI.skin.font = myFont;                 //задаем шрифт
	    GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, 
	      Screen.height / 2 - 150, 300, 300), "", "box");   //рисуем область отображения
		
		    GUILayout.BeginVertical();               //начинаем вертикальную группу
		
			    GUILayout.Label("Адрес сервера");            //далее идут пары Label+TextField для считывания данных
			    serverName = GUILayout.TextField(serverName);
			
			    GUILayout.Label("Порт сервера");
			    serverPort = (GUILayout.TextField(serverPort));
			
			    GUILayout.Label("Имя пользователя");
			    username = GUILayout.TextField(username, 24);
			
			   /* GUILayout.Label("Пароль пользователя");
			    password = GUILayout.PasswordField(password, "*"[0], 24);*/
			
			    //проверяем заполненность всех данных, если true - отображаем кнопку соединения
			
			    if (serverName != "" && serverPort != "") {
			
			      if (GUILayout.Button("Соединиться")) {
					string error = SmartFoxHandler.Instance.Connect(serverName,
																	int.Parse(serverPort),
																	username
																	);
					Debug.Log(error);
			      }
				if (SmartFoxHandler.Instance.IsJoinned && !isLoading) 
				{
					isLoading = true;
					print("loading");
					Application.LoadLevel(NextLevelNumber);
				}
			
			    }
			
			    GUILayout.Label((true==SmartFoxHandler.Instance.IsLoggedIn)?" connected":"...");
		    GUILayout.EndVertical();
	    GUILayout.EndArea();
	
	  }
	
	private void Update()
	{
		//Test chat
		if (SmartFoxHandler.Instance.IsJoinned) 
			SmartFoxHandler.Instance.SendAll("Hello guys");
		
		SmartFoxHandler.Instance.ProcessEvents();
	}
	
	private void OnDestroy()
	{
		SmartFoxHandler.Instance.Disconnect();
	}
}
