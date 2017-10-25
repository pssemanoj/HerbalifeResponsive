using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public  class OLCDataProvider
    {
        public  decimal CaseRate { get; set; }
        public  decimal Weight { get; set; }
        public  decimal ActualFreight { get; set; }
        public  decimal BeforeWeight { get; set; }
        public  decimal VolumeWeight { get; set; } 
        public  decimal PhysicalWeight { get; set; }
        public  decimal Insurance { get; set; }
        public  decimal InsuranceRate { get; set; }

        public List<Message> Getorderparams(List<Message> messages)
        {
            var msg1 = new Message()
            {
                MessageType = "CaseRate",
                MessageValue = CaseRate.ToString()
            };

            messages.Add(msg1);

            var msg2 = new Message()
            {
                MessageType = "Weight",
                MessageValue = Weight.ToString()
            };

            messages.Add(msg2);

            var msg3 = new Message()
            {
                MessageType = "ActualFreight",
                MessageValue = ActualFreight.ToString()
            };

            messages.Add(msg3);

            var msg4 = new Message()
            {
                MessageType = "BeforeWeight",
                MessageValue = BeforeWeight.ToString()
            };

            messages.Add(msg4);

            var msg5 = new Message()
            {
                MessageType = "VolumeWeight",
                MessageValue = VolumeWeight.ToString()
            };

            messages.Add(msg5);

            var msg6 = new Message()
            {
                MessageType = "PhysicalWeight",
                MessageValue = PhysicalWeight.ToString()
            };

            messages.Add(msg6);

            var msg7 = new Message()
            {
                MessageType = "Insurance",
                MessageValue = Insurance.ToString()
            };

            messages.Add(msg7);

            var msg8 = new Message()
            {
                MessageType = "InsuranceRate",
                MessageValue = InsuranceRate.ToString()
            };
            messages.Add(msg8);
            return messages;
        }
    }
}
