﻿using System;
using System.Threading;
using JustEat.StatsD;

namespace ShoeHub.DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("This utility program will populate randomly generated data to your Graphite");
            Console.WriteLine("The generated Graphite buckets will represent an imaginary shoe store called Shoe Hub\n");
            
            getIpAddress:

            Console.Write("Enter the IP address of your StatsD server: ");
            var ipAddress = Console.ReadLine();

            if (string.IsNullOrEmpty(ipAddress))
            {
                Console.WriteLine("The IP address cannot be empty. Please try again. Ctrl-C to stop!");
                goto getIpAddress;
            }


            getNumberOfDataPoints:

            Console.Write($"Please enter the number of data points from 1 to ({(int.MaxValue - 1).ToString("###,###")}): ");
            var dataPointsCount = 0;
            var dataPointsCountStr = Console.ReadLine();
            if (!int.TryParse(dataPointsCountStr, out dataPointsCount))
            {
                Console.WriteLine("The value you entered is not valid. Please try again");
                goto getNumberOfDataPoints;
            }

            const short Refund = 0;
            var countryCodes = new[]{"AU","US","IN"};
            var paymentMethods = new[] {"Card","Cash","Paypal"};
            var shoeTypes = new[] {"Loafers","Boots","HighHeels"};

            var publisher = new StatsDPublisher(new StatsDConfiguration
            {
                Host = ipAddress,
                OnError = f=> false
            });

            var randomGenerator = new Random(DateTime.Now.Millisecond);

            for (int i = 1; i <= dataPointsCount; i++)
            {
                Console.WriteLine("Sending metrics to server...");
                var shoeType = shoeTypes[randomGenerator.Next(shoeTypes.Length)];

                var salesBucketName = $"shoehub.sales.{shoeType}";
                var soldValue = randomGenerator.Next(10,20);
                publisher.Increment(soldValue, salesBucketName);
                publisher.Increment(soldValue, $"{salesBucketName}.min");
                publisher.Increment(soldValue, $"{salesBucketName}.max");

                Console.WriteLine($"{salesBucketName}:{soldValue}");

                var countryCode = countryCodes[randomGenerator.Next(countryCodes.Length)];
                var paymentOrRefund = randomGenerator.Next(2);

                if (paymentOrRefund == Refund)
                {
                    var refundBucketName = $"shoehub.{countryCode}.refunds";
                    var refundValue = randomGenerator.Next(1000);
                    publisher.Gauge(refundValue, refundBucketName);
                }
                else
                {
                    var paymentMethod = paymentMethods[randomGenerator.Next(paymentMethods.Length)];
                    var paymentMethodBucketName = $"shoehub.{countryCode}.payments.{paymentMethod}";
                    var paymentValue = randomGenerator.Next(1000);
                    publisher.Gauge(paymentValue, paymentMethodBucketName);
                }
                Thread.Sleep(new TimeSpan(0,0,0,0, randomGenerator.Next(500)));
            }

            Console.WriteLine("All datapoints were sent to StatsD. Press any keys...");
            Console.ReadKey();
        }
    }
}
