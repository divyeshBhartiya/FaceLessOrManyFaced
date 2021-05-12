# FaceLessOrManyFaced
Don't go over the name of this repo. I am a GOT fan !!!

This is an application which uses Face Detection API from OpenCV (OpenCVSharp4). 

The idea is that the user will upload a picture and the application will detect and crop various faces in the picture and send mail to the user. 

For this the application uses the concepts of Microservices, Rabbit MQ and Mass Transit for indirect communication between the services, gRPC for direct communication, Face Detection API for detecting faces, Order API to query the orders raised by the users in the system, Notification Service for sending mails with cropped faces, Signal R Core for Push notification to the client application so that it gets refreshed automatically in case of receiving any pushed notification. 

This repository uses the latest version of ASP.NET 5.0 for most of the code.

## Notes:
Faces.API: The FacesAzureController uses the Face Detection API provided by Azure. 
You would just require AzureSubscriptionKey and AzureEndPoint and you are good to go ahead. It is found that, with this setup and configuration for Face Detection, it is easier to Dockerize/Containerize the Faces.API.

Hence, for containerizing it would be wiser to go ahead with the FacesAzureController implementation as FacesController uses libraries whose images are either too heavy or not available. I didn't go ahead with containerization as I was feeling too lazy...lol. 

For containerization, please go ahead with docker-compose as it is easier to learn if you are doing it for the first time.

### References: 
https://www.udemy.com/

### Thanks a lot to:
F. Frank Ozz
(Software Architect, Author)

https://www.udemy.com/user/f-ozgul/
