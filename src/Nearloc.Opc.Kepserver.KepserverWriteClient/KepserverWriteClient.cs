namespace Neraloc.Opc.Kepserver
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using global::Opc.Ua;
    using global::Opc.Ua.Client;
    using global::Opc.Ua.Configuration;

    public class KepserverWriteClient
    {
        EndpointDescription _endpointDesc;
        ApplicationConfiguration _config;

        public KepserverWriteClient(string applicationName, string serverIp, string serverPort)
        {
            Initialize(applicationName, serverIp, serverPort);
        }
        public void WriteDict(Dictionary<string, ushort> valueDict)
        {
            using (var session = Session.Create(_config, new ConfiguredEndpoint(null, _endpointDesc, 
                EndpointConfiguration.Create(_config)), false, "", 60000, null, null).GetAwaiter().GetResult())
            {
                StatusCodeCollection results = null;
                DiagnosticInfoCollection infos = null;

                session.Write(null,
                    GetWriteValueCollection(valueDict),
                    out results, out infos);


                if (results.Any(sc => StatusCode.IsNotGood(sc)))
                {
                    var messageBuilder = new StringBuilder();

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (StatusCode.IsNotGood(results[i]))
                        {
                            messageBuilder.AppendLine($"{valueDict.ElementAt(i).Key}: {StatusCodes.GetBrowseName(results[i].Code)}");
                        }                        
                    }

                    throw new KepserverWriteException(messageBuilder.ToString());
                }
            }
        }

        private void Initialize(string applicationName, string serverIp, string serverPort)
        {
            _config = new ApplicationConfiguration()
            {
                ApplicationName = applicationName,
                ApplicationUri = Utils.Format($@"urn:{0}:{applicationName}", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = applicationName },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };

            _config.Validate(ApplicationType.Client).GetAwaiter().GetResult();

            if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                _config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            var application = new ApplicationInstance
            {
                ApplicationName = applicationName,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = _config
            };

            application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();

            _endpointDesc = CoreClientUtils.SelectEndpoint($"opc.tcp://{serverIp}:{serverPort}", useSecurity: false);
        }



        private WriteValueCollection GetWriteValueCollection(Dictionary<string, ushort> valueDict)
        {
            var valueCollection = new WriteValueCollection(valueDict.Count);

            foreach (var item in valueDict)
            {
                var writevalue = new WriteValue();
                writevalue.NodeId = new NodeId($"ns=2;{item.Key}");
                writevalue.Value = new DataValue(new Variant(item.Value));
                writevalue.AttributeId = 13;
                valueCollection.Add(writevalue);
            }

            return valueCollection;
        }
        
    }
}
