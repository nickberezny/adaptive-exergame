# import the necessary packages
from collections import deque
from imutils.video import VideoStream
import numpy as np
import argparse
import cv2
import imutils
import time
import datetime
import os

import socket
import keyboard

def getCenter(blurred, lower, upper):
	hsv = cv2.cvtColor(blurred, cv2.COLOR_BGR2HSV)
	# construct a mask for the color "green", then perform
	# a series of dilations and erosions to remove any small
	# blobs left in the mask
	mask = cv2.inRange(hsv, lower, upper)
	mask = cv2.erode(mask, None, iterations=2)
	mask = cv2.dilate(mask, None, iterations=2)

	# find contours in the mask and initialize the current
	# (x, y) center of the ball
	cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL,
		cv2.CHAIN_APPROX_SIMPLE)
	cnts = imutils.grab_contours(cnts)
	center = None
	# only proceed if at least one contour was found
	if len(cnts) > 0:
		# find the largest contour in the mask, then use
		# it to compute the minimum enclosing circle and
		# centroid
		c = max(cnts, key=cv2.contourArea)
		((x, y), radius) = cv2.minEnclosingCircle(c)
		M = cv2.moments(c)
		center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))
		# only proceed if the radius meets a minimum size
		if radius > 10:
			# draw the circle and centroid on the frame,
			# then update the list of tracked points
			cv2.circle(frame, (int(x), int(y)), int(radius),
				(0, 255, 255), 2)
			cv2.circle(frame, center, 5, (0, 0, 255), -1)

	else:
		center = None

	return center

HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 8081       # The port used by the server
BUFFER_SIZE = 1024

clicked = 0;
bounds = ""


s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((HOST, PORT))
s.setblocking(0)
s.settimeout(0.0001)
s.send(b'T_AWAKE')



#f = open(args["filename"], "a")
fset = open("marker_setpoints.txt", "w")
fdata = open("tracker_data.txt", "w")
r = open("HSV.txt", "r")

hsv_data = r.readlines()

vid_src = int(hsv_data[0])

x = hsv_data[1]
temp = x.split(',')
greenLower = (int(temp[0]),int(temp[1]),int(temp[2]))
x = hsv_data[2]
temp = x.split(',')
greenUpper = (int(temp[0]),int(temp[1]),int(temp[2]))

x = hsv_data[3]
temp = x.split(',')
blueLower = (int(temp[0]),int(temp[1]),int(temp[2]))
x = hsv_data[4]
temp = x.split(',')
blueUpper = (int(temp[0]),int(temp[1]),int(temp[2]))


x = hsv_data[5]
temp = x.split(',')
yellowLower = (int(temp[0]),int(temp[1]),int(temp[2]))
x = hsv_data[6]
temp = x.split(',')
yellowUpper = (int(temp[0]),int(temp[1]),int(temp[2]))



pts = deque(maxlen=64)
# if a video path was not supplied, grab the reference
# to the webcam

vs = VideoStream(src=vid_src).start()

time.sleep(2.0)
seconds_in_day = 24 * 60 * 60
count = 0

curr_time = 0;
# keep looping
while True:

	try:
		data = s.recv(BUFFER_SIZE).decode("utf-8")
		print(data)
	except:
		data = None

	if clicked == 2:
		later_time = datetime.datetime.now()
		difference = later_time - first_time
		curr_time = curr_time  + float(difference.microseconds/1000.0) + float(difference.seconds*1000.0)
		first_time = later_time


	'''
	if clicked == 0:
		msg = bytes(str(1.0) + "," + str(2.0),"ascii")
		s.sendall(msg)
		clicked = 1
	'''

	# grab the current frame
	frame = vs.read()
	# if we are viewing a video and we did not grab a frame,
	# then we have reached the end of the video
	if frame is None:
		break
	# resize the frame, blur it, and convert it to the HSV
	# color space
	frame = imutils.resize(frame, width=600)
	blurred = cv2.GaussianBlur(frame, (11, 11), 0)
	

	centerGreen = getCenter(blurred, greenLower, greenUpper)
	centerYellow = getCenter(blurred, yellowLower, yellowUpper)
	centerBlue = getCenter(blurred, blueLower, blueUpper)
	#center = getCenter(blurred, orangeLower, orangeUpper)

	center = centerGreen #set which marker to use as game input
	AnkleCenter = centerBlue
	key = cv2.waitKey(1) & 0xFF

	if clicked == 2:
		count = count + 1
	 
		
	if clicked == 2:
		fdata.write(str(float(curr_time)/1000.0) + "," )

		#msg = bytes(str(0),"ascii")
		if centerGreen != None:
			fdata.write(str(centerGreen[0]) + "," + str(centerGreen[1]) + "\n")
			msg = bytes(str(center[1]),"ascii")
			s.sendall(msg)

		#if centerGreen == None:
		
		if data == "G_STOP":
			fdata.write(str(datetime.datetime.now()) + "\n")
			break

		
	else:
		
		if data == "G_RUN":
			first_time = datetime.datetime.now()
			fdata.write(str(first_time.hour) + "," + str(first_time.minute) + ","  + str(first_time.second) + ","  + str(first_time.microsecond)[:-3] + ","  "\n")
			fdata.write("Time (s), x, y \n")
			clicked = 2

		if clicked == 1 and data == "G_SET2":
			print('click')
			fset.write("Hip2: " + str(center[0]) + "," + str(center[1]) + "\n")
			
			#clicked = 2
			bounds = bounds + "," + str(center[1])
			msg = bytes(bounds,"ascii")
			s.sendall(msg)
			print(msg)
			

		if clicked == 0 and data == "G_SET1": 
			if AnkleCenter != None and center != None:
				print('click')
				fset.write("Ankle: " + str(AnkleCenter[0]) + "," + str(AnkleCenter[1]) + "\n")
				fset.write("Hip1: " + str(center[0]) + "," + str(center[1]) + "\n")
				bounds = str(center[1])
				clicked = 1
			else:
				print("Markers are not visible")
				
					
	# update the points queue
	pts.appendleft(center)

		# loop over the set of tracked points
	for i in range(1, len(pts)):
		# if either of the tracked points are None, ignore
		# them
		if pts[i - 1] is None or pts[i] is None:
			continue
		# otherwise, compute the thickness of the line and
		# draw the connecting lines
		thickness = int(np.sqrt(64 / float(i + 1)) * 2.5)
		cv2.line(frame, pts[i - 1], pts[i], (0, 0, 255), thickness)
	# show the frame to our screen
	cv2.imshow("Frame", frame)
	key = cv2.waitKey(1) & 0xFF
	# if the 'q' key is pressed, stop the loop
	if key == ord("q"):
		break

vs.stop()
s.close()
vs.release()
# close all windows
cv2.destroyAllWindows()

