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
                 "FaceLessOrManyFaced has sent you the faces detected from the picture you uploaded. For more such cool POCs and Web Apps please contact the developer over the weekdays." +
                 " Cheers!!!", facesData));
            await context.Publish<IOrderDispatchedEvent>(new
            {
                context.Message.OrderId,
                DispatchDateTime = DateTime.UtcNow
            });
        }
    }
}
