﻿using UnityEngine;
using UnityEngine.Networking;

using System.Collections;

public class networkactions : NetworkBehaviour{







	void Start(){



		//CmdSendVar ();



		if (GameObject.Find ("QuestDatabase") != null) {


			GameObject.Find ("QuestDatabase").SendMessage("setNetworkIdentity",this);


		}


	}









	[Command]
public void	CmdSendVar(int id,string deviceid, string var, string value){

		SendVariable v = new SendVariable ();
		v.id = id;
		v.messagetype = "setVar";
		v.deviceid = deviceid;
		v.var = var;
		v.value = value;

		GameObject.Find("MediaServer").SendMessage("addSendVariableToQueue",v);
		RpcMessageSuccesful (id);

	}



	
	[Command]
	public void	CmdSendFile(int id,string deviceid, string var, string filetype, byte[] file){
		
		SendVariable v = new SendVariable ();
		v.id = id;
		v.messagetype = "setFile";
		v.deviceid = deviceid;
		v.var = var;

		v.filetype = filetype;
		v.bytes = file;
		
		GameObject.Find("MediaServer").SendMessage("addSendVariableToQueue",v);
		RpcMessageSuccesful (id);

	}






	
	[Command]
	public void	CmdAddToFile(int id, string deviceid, string var, string filetype, byte[] file){
		
		SendVariable v = new SendVariable ();
		v.id = id;
		v.messagetype = "addToFile";
		v.deviceid = deviceid;
		v.var = var;
		
		v.filetype = filetype;
		v.bytes = file;
		
		GameObject.Find("MediaServer").SendMessage("addSendVariableToQueue",v);
		RpcMessageSuccesful (id);

	}



	
	[Command]
	public void	CmdFinishFile(int id, string deviceid, string var, string filetype){
		
		SendVariable v = new SendVariable ();
		v.id = id;
		v.messagetype = "finishFile";
		v.deviceid = deviceid;
		v.var = var;
		v.filetype = filetype;
		GameObject.Find("MediaServer").SendMessage("addSendVariableToQueue",v);

		RpcMessageSuccesful (id);
	}





	[ClientRpc]
	public void RpcMessageSuccesful(int id) {

		GameObject.Find ("QuestDatabase").SendMessage ("messageReceived", id);

	}



	[Command]
	public void	CmdAskForNextExpectedMessage(string deviceid){

		int expectedmessage = 0;

		if (PlayerPrefs.HasKey ("nextmessage_" + deviceid)) {


			expectedmessage = PlayerPrefs.GetInt("nextmessage_"+deviceid);

		}

		RpcReturnNextExpectedMessage (expectedmessage);

	}


	[ClientRpc]
	public void RpcReturnNextExpectedMessage(int nextexpectedmessage){

		GameObject.Find ("QuestDatabase").SendMessage ("setExpectedNextMessage", nextexpectedmessage);


	}


}

[System.Serializable]
public class SendVariable{

	public int id;
	public string messagetype;


	public string deviceid;
	public string var;
	public string value;

	public string filepath;
	public byte[] bytes;
	public string filetype;

}