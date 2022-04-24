using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace VrTeamTesting.ConnectionString
{
    public sealed class ConnnectionManager
    {
        public ConnnectionManager()
        {

        }
        private static ConnnectionManager instance = null;
        private static readonly object padlock = new object();

        public static ConnnectionManager Connection
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ConnnectionManager();
                    }
                    return instance;
                }
            }
        }

        private static DateTime LastConnectedOn;
        private static ServiceClient Service;
        public static ServiceClient EstablishConnection()
        {
            //string clientId = "315ba50d-600a-4db5-abc2-399b8ea646b7";
            //string clientSecret = "BWw8Q~lS3m9T.6goW~yIxpxkTjogjz6e3F4iBaJk";
            //string orgURL = "https://vedtest.crm8.dynamics.com/";
            string clientId = Environment.GetEnvironmentVariable("ClientId");
            string clientSecret = Environment.GetEnvironmentVariable("ClientSecret");
            string orgURL = Environment.GetEnvironmentVariable("OrganisationURL");
            string connectionString = $"AuthType=ClientSecret;url={orgURL};ClientId={clientId};ClientSecret={clientSecret}";
            Service = new ServiceClient(connectionString) ?? throw new Exception("Not Connected. Dataverse ServiceClient is null");
            LastConnectedOn = DateTime.Now;
            return Service;
        }


        public ServiceClient Get()
        {

            TimeSpan difference = DateTime.Now - LastConnectedOn;

            if (Service == null || difference.TotalMinutes > 9)
            {
                Service = EstablishConnection();
            }

            LastConnectedOn = DateTime.Now;

            return Service;
        }
    }
}
