namespace MyHomewoNearloc.Opc.Kepserver.KepserverWriteClient.ConsoleSamplerk
{
    class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                var values = new Dictionary<string, ushort> { 
                    { "Channel1.Device1.Tag2", 15 }, 
                    { "Channel1.Device1.Tag30", 9 } 
                };

                var kepserverWriteClient = new KepserverWriteClient("nearloc.zoneUpdater", "10.6.4.195", "49320");

                kepserverWriteClient.WriteDict(values);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}