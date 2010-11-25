/**
 * OpenTSPS + Unity3d Extension
 * Created by James George on 11/24/2010
 * 
 * This example is distributed under The MIT License
 *
 * Copyright (c) 2010 James George
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 * 
 * This Receiver class is a port of the ofxTSPSReceiver combined with an example taken from the OSCuMote
 * http://forum.unity3d.com/threads/21273-OSCuMote-Wiimote-support-for-the-free-version
 *  
 */
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using OSC.NET;

using UnityEngine;
namespace TSPS
{
	public class OpenTSPSReceiver
	{
		private bool connected = false;
		private int port = 3333;
		private OSCReceiver receiver;
		private Thread thread;
		
		private Dictionary<int, OpenTSPSPerson> people = new Dictionary<int, OpenTSPSPerson>(32);
		private List<OpenTSPSListener> listeners = new List<OpenTSPSListener>();
		
		//thread safe Producer/Consumer Queue for incoming messages
		private List<OSCMessage> processQueue = new List<OSCMessage>();
		
		public OpenTSPSReceiver() {}

		public OpenTSPSReceiver(int port) {
			this.port = port;
		}
	
		public int getPort() {
			return port;
		}
				
		public void connect() {
			
			Debug.Log("starting connect ");

			try {
				receiver = new OSCReceiver(port);
				thread = new Thread(new ThreadStart(listen));
				thread.Start();
				connected = true;
			} catch (Exception e) {
				Console.WriteLine("failed to connect to port "+port);
				Console.WriteLine(e.Message);
			}
		}
		
		/**
		 * Call update every frame in order to dispatch all messages that have come
		 * in on the listener thread
		 */
		public void update()
		{
			//processMessages has to be called on the main thread
			//so we used a shared proccessQueue full of OSC Messages
			lock(processQueue){
				foreach( OSCMessage message in processQueue){
					processMessage(message);
				}
				processQueue.Clear();
			}
		}
		
		public void disconnect() {
       		if (receiver!=null) receiver.Close();
        	receiver = null;
			connected = false;
		}

		public bool isConnected() { return connected; }

		private void listen() {
			while(connected) {
				try {
					OSCPacket packet = receiver.Receive();
					if (packet!=null) {
						lock(processQueue){
							//Debug.Log( "adding  packets " + processQueue.Count );
							if (packet.IsBundle()) {
								ArrayList messages = packet.Values;
								for (int i=0; i<messages.Count; i++) {
									processQueue.Add( (OSCMessage)messages[i] );
								}
							} else{
								processQueue.Add( (OSCMessage)packet );
							}
						}
					} else Console.WriteLine("null packet");
				} catch (Exception e) { 
					Debug.Log( e.Message );
					Console.WriteLine(e.Message); 
				}
			}
		}

		private void processMessage(OSCMessage message) {
			
			string address = message.Address;
			ArrayList args = message.Values;
			
			if (address == "TSPS/personEntered/") {
				addPerson(args);
			}
			else if(address == "TSPS/personMoved/" || address == "TSPS/personUpdated/"){				
				
				
				int person_id = (int)args[0];
				OpenTSPSPerson person = null;
				if (!people.ContainsKey(person_id)) {
					person = addPerson(args);
				}
				else{
					person = people[person_id];
					updatePerson(person, args);
					for (int i = 0; i < listeners.Count; i++) {
						OpenTSPSListener listener = (OpenTSPSListener)listeners[i];
						if (listener!=null){
							if(address == "TSPS/personMoved/"){
								listener.personMoved(person);
							}
							else{
								listener.personUpdated(person);
							}
						}
					}					
				}
			}
			else if(address == "TSPS/personWillLeave/"){
				int person_id = (int)args[0];
				if (people.ContainsKey(person_id)) {
					OpenTSPSPerson personToRemove = people[person_id];				
					people.Remove(person_id);
					for (int i = 0; i < listeners.Count; i++) {
						OpenTSPSListener listener = (OpenTSPSListener)listeners[i];
						if (listener!=null){
							listener.personWillLeave(personToRemove);
						}
					}
				}
			}
			else if(address == "TSPS/scene/"){
				//TODO
				//create a scene object that can store global optical flow
				//and scene time parameters
			}
		}
		
		private OpenTSPSPerson addPerson(ArrayList args)
		{
			OpenTSPSPerson newPerson = new OpenTSPSPerson();
			updatePerson(newPerson, args);
			people.Add(newPerson.id, newPerson);	
			for (int i = 0; i < listeners.Count; i++) {
				OpenTSPSListener listener = (OpenTSPSListener)listeners[i];
				if (listener!=null){
					listener.personEntered(newPerson);
				}
			}
			return newPerson;
		}
		
		private void updatePerson(OpenTSPSPerson person, ArrayList args)
		{
			person.id = (int)args[0];
			person.age = (int)args[1];
			person.centroidX = (float)args[2];
			person.centroidY = (float)args[3];
			person.velocityX = (float)args[4];
			person.velocityY = (float)args[5];
			person.boundingRectOriginX = (float)args[6];
			person.boundingRectOriginY = (float)args[7];
			person.boundingRectSizeWidth = (float)args[8];
			person.boundingRectSizeHeight = (float)args[9];
			person.opticalFlowVelocityX = (float)args[10];
			person.opticalFlowVelocityY = (float)args[11];			
			
			//TODO Track contours and add them to the person object
			//if (m->getNumArgs() > 12){
			//	contour.clear();
			//	for (int i = 12; i < m->getNumArgs(); i += 2){
			//		contour.push_back(ofPoint(m->getArgAsFloat(i), m->getArgAsFloat(i+1)));
			//	}
			//}			
		}
		
		public void addPersonListener(OpenTSPSListener listener) {
			listeners.Add(listener);
		}
		
		public void removePersonListener(OpenTSPSListener listener) {	
			listeners.Remove(listener);
		}
				
	}
}