﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GQ.Client.Net {

	public class SendQueue : ISendQueue {

		public List<SendQueueEntry> queue;

		private int idCounter;


		public SendQueue () {
			queue = new List<SendQueueEntry>();
			idCounter = 0;
		}

		public void addTextMessage (string ip, string var, string text, int questId) {

			SendQueueEntry sqe = new SendQueueEntry();

			if ( queue == null || queue.Count == 0 ) {

				idCounter = 0;
				sqe.resetid = true;

			}

			sqe.id = idCounter;
			idCounter++;
			sqe.questid = questId;

			if ( idCounter == int.MaxValue ) {
				idCounter = 0;
			}

			PlayerPrefs.SetInt("nextmessage_" + sqe.ip, idCounter);

			sqe.mode = SendQueueEntry.MODE_VALUE;
			sqe.timeout = 0f;

			sqe.ip = ip;
			sqe.var = var;
			sqe.value = text;

			queue.Add(sqe);

//			serialize(sqe);

		}



	}



}