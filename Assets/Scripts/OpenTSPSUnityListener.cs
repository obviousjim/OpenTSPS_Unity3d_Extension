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
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TSPS;

public class OpenTSPSUnityListener : MonoBehaviour  {
		
	//create some materials and apply a different one to each new person
	public Material	[] materials;
	public UnityOSCReceiver receiver;
	
	//a place to hold all the people
	private Dictionary<int, OpenTSPSPerson> people = new Dictionary<int, OpenTSPSPerson>(32);
	//a place to hold game objects that we attach to people, maps person ID => their object
	private Dictionary<int,GameObject> peopleCubes = new Dictionary<int,GameObject>();
	
	//game engine stuff for the example
	public GameObject boundingPlane; //put the people on this plane
	public GameObject personMarker; //used to represent people moving about in our example
	
	void OnGUI() {
		if( receiver.isConnected() ) {
			GUI.Label( new Rect( 10, 10, 500, 100), "Connected to TSPS on Port " + receiver.getPort() );
		}
	}
	
	void OSCMessageReceived(OSC.NET.OSCMessage message){	
		string address = message.Address;
		ArrayList args = message.Values;
		
		if (address == "/TSPS/personEntered/") {
			addPerson(args);
		}
		else if(address == "/TSPS/personUpdated/"){				
			int person_id = (int)args[0];
			OpenTSPSPerson person = null;
			if (!people.ContainsKey(person_id)) {
				person = addPerson(args);
			}
			else{
				person = people[person_id];
				updatePerson(person, args);
				personUpdated(person);
			}
		}
		else if(address == "/TSPS/personWillLeave/"){
			int person_id = (int)args[0];
			if (people.ContainsKey(person_id)) {
				OpenTSPSPerson personToRemove = people[person_id];				
				people.Remove(person_id);
				personWillLeave(personToRemove);
			}
		}
	}
		
	private OpenTSPSPerson addPerson(ArrayList args) {
		OpenTSPSPerson newPerson = new OpenTSPSPerson();
		updatePerson(newPerson, args);
		people.Add(newPerson.id, newPerson);	
		personEntered(newPerson);
		return newPerson;
	}
	
	private void updatePerson(OpenTSPSPerson person, ArrayList args) {
		person.id = (int)args[0];
		person.oid = (int)args[1];
		person.age = (int)args[2];
		person.centroidX = (float)args[3];
		person.centroidY = (float)args[4];
		person.velocityX = (float)args[5];
		person.velocityY = (float)args[6];
		person.boundingRectOriginX = (float)args[7];
		person.boundingRectOriginY = (float)args[8];
		person.boundingRectSizeWidth = (float)args[9];
		person.boundingRectSizeHeight = (float)args[10];
		person.opticalFlowVelocityX = (float)args[11];
		person.opticalFlowVelocityY = (float)args[12];			
		
		//TODO: Track contours and add them to the person object
		//if (m->getNumArgs() > 12){
		//	contour.clear();
		//	for (int i = 12; i < m->getNumArgs(); i += 2){
		//		contour.push_back(ofPoint(m->getArgAsFloat(i), m->getArgAsFloat(i+1)));
		//	}
		//}			
	}
	
	
	public void personEntered(OpenTSPSPerson person){
		Debug.Log(" person entered with ID " + person.id);
		GameObject personObject = (GameObject)Instantiate(personMarker, positionForPerson(person), Quaternion.identity);
		personObject.renderer.material = materials[person.id % materials.Length];
		peopleCubes.Add(person.id,personObject);

	}

	public void personUpdated(OpenTSPSPerson person) {
		//Debug.Log("Person updated with ID " + person.id);
		if(peopleCubes.ContainsKey(person.id)){
			GameObject cubeToMove = peopleCubes[person.id];
			cubeToMove.transform.position = positionForPerson(person);
		}
	}

	public void personWillLeave(OpenTSPSPerson person){
		Debug.Log("Person leaving with ID " + person.id);
		if(peopleCubes.ContainsKey(person.id)){
			Debug.Log("Destroying cube");
			GameObject cubeToRemove = peopleCubes[person.id];
			peopleCubes.Remove(person.id);
			//delete it from the scene	
			Destroy(cubeToRemove);
		}
	}
	
	//maps the OpenTSPS coordinate system into one that matches the size of the boundingPlane
	private Vector3 positionForPerson(OpenTSPSPerson person){
		Bounds meshBounds = boundingPlane.GetComponent<MeshFilter>().sharedMesh.bounds;
		return new Vector3( (float)(.5 - person.centroidX) * meshBounds.size.x, 0.25f, (float)(person.centroidY - .5) * meshBounds.size.z );
	}
	
}
