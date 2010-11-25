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


using System;
namespace TSPS
{

/**
 * The TuioListener interface provides a simple callback infrastructure which is used by the {@link TuioClient} class 
 * to dispatch TUIO events to all registered instances of classes that implement the TuioListener interface defined here.<P> 
 * Any class that implements the TuioListener interface is required to implement all of the callback methods defined here.
 * The {@link TuioClient} makes use of these interface methods in order to dispatch TUIO events to all registered TuioListener implementations.<P>
 * <code>
 * public class MyTuioListener implements TuioListener<br/>
 * ...</code><p><code>
 * MyTuioListener listener = new MyTuioListener();<br/>
 * TuioClient client = new TuioClient();<br/>
 * client.addTuioListener(listener);<br/>
 * client.start();<br/>
 * </code>
 *
 * @author Martin Kaltenbrunner
 * @version 1.4
 */
	public interface OpenTSPSListener
	{
		/**
		 * This callback method is invoked by the TuioClient when a new TuioObject is added to the session.   
		 *
		 * @param  tobj  the TuioObject reference associated to the addTuioObject event
		 */
		void personEntered(OpenTSPSPerson person);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioObject is updated during the session.   
		 *
		 * @param  tobj  the TuioObject reference associated to the updateTuioObject event
		 */
		void personUpdated(OpenTSPSPerson person);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioObject is removed from the session.   
		 *
		 * @param  tobj  the TuioObject reference associated to the removeTuioObject event
		 */
		void personMoved(OpenTSPSPerson person);

		/**
		 * This callback method is invoked by the TuioClient when a new TuioCursor is added to the session.   
		 *
		 * @param  tcur  the TuioCursor reference associated to the addTuioCursor event
		 */
		void personWillLeave(OpenTSPSPerson person);
	}
}
