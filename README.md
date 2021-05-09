# FaceLessOrManyFaced
Don't go over the name of this repo. I am a GOT fan !!!

This is an application which uses Face Detection API from OpenCV (OpenCVSharp4). 

The idea is that the user will upload a picture and the application will detect and crop various faces in the picture and send mail to the user. 

For this the application uses the concepts of Microservices, Rabbit MQ and Mass Transit for indirect communication between the services, gRPC for direct communication, Face Detection API for detecting faces, PictureOrder API to query the orders raised by the users in the system, Notification Service for sending mails with cropped faces, Signal R Core for Push notification to the client application so that it gets refreshed automatically in case of receiving any pushed notification. 
