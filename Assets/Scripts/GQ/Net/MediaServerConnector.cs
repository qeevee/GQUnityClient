﻿using UnityEngine;
using System.Collections;

public class MediaServerConnector : IMediaServerConnector {

	private float _timeout = 10.0f;
	private networkactions _networkActionsObject;

	public MediaServerConnector (float timeout, networkactions networkActionsObject) {
		_timeout = timeout;
		_networkActionsObject = networkActionsObject;
	}

	public void send (SendQueueEntry message) {
		message.timeout = _timeout;

		if ( message.mode == MediaConnectorClient.MODE_VALUE ) {
			_networkActionsObject.CmdSendVar(message.id, SystemInfo.deviceUniqueIdentifier, message.var, message.value, message.resetid);
		}
		else
		if ( message.mode == MediaConnectorClient.MODE_FILE_START ) {

			_networkActionsObject.CmdSendFile(message.id, SystemInfo.deviceUniqueIdentifier, message.var, message.filetype, message.file, message.resetid);

		}
		else
		if ( message.mode == MediaConnectorClient.MODE_FILE_MID ) {

			_networkActionsObject.CmdAddToFile(message.id, SystemInfo.deviceUniqueIdentifier, message.var, message.filetype, message.file, message.resetid);

		}
		else
		if ( message.mode == MediaConnectorClient.MODE_FILE_FINISH ) {

			_networkActionsObject.CmdFinishFile(message.id, SystemInfo.deviceUniqueIdentifier, message.var, message.filetype, message.resetid);

		}
	}
}
