using EmailService;
using MassTransit;
using Messaging.InterfacesConstants.Events;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace NotificationService.Consumers
{
    public class OrderProcessedEventConsumer : IConsumer<IOrderProcessedEvent>
    {
        private const string Content = @"FaceLessOrManyFaced has sent you the faces detected from the picture you uploaded. 
                  This email is sent from the Notification Microservice of Face(s) Detection App. 
                  Apart from the Notification Microservice, it has Orders Microservice, MVC Web App for User Interactions, 
                  Face Detection API powered by OpenCV and Rabbit MQ for communication between these services. 
                  Order Microservices and Rabbit MQ are acting as orchaestrators and SignalR is used for send Push Notifications 
                  to the Web App for status updates of the order. 
                  The idea is to detect and crop the faces from a uploaded image and mail them to the user. 
                  For more such cool POCs and Web Apps please contact the developer over the weekdays.

                  Cheers!!!";
        private readonly IEmailSender _emailSender;
        public OrderProcessedEventConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public async Task Consume(ConsumeContext<IOrderProcessedEvent> context)
        {
            var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            var result = context.Message;
            var facesData = result.Faces;
            if (facesData.Count < 1)
            {
                await Console.Out.WriteLineAsync($"No faces Detected");
            }
            else
            {
                int j = 0;
                foreach (var face in facesData)
                {
                    MemoryStream ms = new MemoryStream(face);
                    var image = Image.FromStream(ms);
                    image.Save(rootFolder + "/Images/face" + j + ".jpg", ImageFormat.Jpeg);
                    j++;
                }
            }
            // Here we will add the Email Sending code

            string[] mailAddress = { result.UserEmail };

            await _emailSender.SendEmailAsync(new Message(mailAddress, "Your Order: " + result.OrderId,
                  Content, facesData));
            await context.Publish<IOrderDispatchedEvent>(new
            {
                context.Message.OrderId,
                DispatchDateTime = DateTime.UtcNow
            });
        }
    }
}
