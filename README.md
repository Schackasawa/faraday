<h1>Project Faraday: Circuit Simulation in Virtual Reality</h1>

Project Faraday is an experiment in virtual reality education, allowing anyone with a VR headset to explore basic circuit design and develop an intuition for the way electricity behaves in the real world. Beginners of all ages can benefit from the ability to combine any number of wires, motors, bulbs and switches to construct arbitrarily complex experiments. Unlike traditional classroom education, virtual reality allows the student to actually see electron flow and its effects on various components in an accessible and fun environment, hopefully encouraging natural exploration and hands-on learning.

<h2>Technology</h2>

This project uses the Unity game engine, Unity's XR integration framework, and <a href="https://spicesharp.github.io/SpiceSharp/index.html">SpiceSharp</a>, a freely available open source circuit simulation library. The code has been developed and tested extensively on the Meta(Oculus) Rift headset and Touch controllers, but because it uses Unity's XR framework rather than proprietary Meta APIs, it can be easily adapted to other headsets and controllers as well.

<h2>Features</h2>

<ul>
  <li>Unlimited supply of circuit components, including batteries, switches, bulbs, motors, and wires</li>
  <li>Components snap to grid when dropped for fool-proof circuit creation 
  <li>Audio feedback and current flow visualization whenever a valid circuit is completed</li>
  <li>Short circuit detection with visual and auditory feedback, indicating the exact components involved</li>
  <li>Adaptive components - motors change speed and bulbs change intensity based on level of current</li>
  <li>Interactive components - bulbs change color and switches open/close when pinched</li>
  <li>Label lever activates current, resistance, and voltage drop labels on active circuits</li>
  <li>Reset lever sends all components back to their dispensers for easy cleanup</li>
  <li>Table height can be easily adjusted by grabbing front bar for seated or standing play</li>
  <li>Teleport locomotion with controller button as well as smooth locomotion and snap-turning with thumbstick</li>
  <li>Relaxing mountain meadow environment makes for a serene learning experience</li>
</ul>
  
<h2>Author</h2>

All code outside of the SpiceSharp library designed, written, and tested by <a href="https://www.linkedin.com/in/dschack/">Darren Schack</a>, a Seattle-based full stack software engineer with a passion for technology and a particular interest in virtual reality.

