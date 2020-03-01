using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;

namespace JMT
{
    class Program
    {
        // criar principal 
        // dat permissão no aas ao principal com o formato app:<aapID>@<tenantID>


        static void Main(string[] args)
        {
            string tenantID = "";
            string clientId = "";
            string clientSecret = "";
            string resourceID = "";
            string aasModel = "";
            string query = @"";
            if (args.Length == 6)
            {
                tenantID = args[0];
                clientId = args[1];
                clientSecret = args[2];
                resourceID = args[3];
                aasModel = args[4];
                query = args[5].Replace("\"", "");
            }

            string tk = "";
            bool getToken = true;
            if(File.Exists(@".\token"))
            {
                string[] lines = System.IO.File.ReadAllLines(@".\token");
                if(tenantID == lines[0] && clientId == lines[1])
                {
                    DateTime dttk = (new DateTime(long.Parse(lines[2]))) - (new TimeSpan(0, 0, 10));
                    if(dttk>DateTime.Now)
                    {
                        tk = lines[3];
                        getToken = false;
                    }
                }
            }
            if(getToken)
            {
                AuthenticationContext authContext = new AuthenticationContext("https://login.windows.net/" + tenantID);
                ClientCredential cc = new ClientCredential(clientId, clientSecret);
                AuthenticationResult token = authContext.AcquireTokenAsync("https://" + resourceID, cc).Result;
                tk = token.AccessToken;
                string[] nf = new string[4];
                nf[0] = tenantID;
                nf[1] = clientId;
                nf[2] = token.ExpiresOn.Ticks.ToString();
                nf[3] = tk;
                System.IO.File.WriteAllLines(@".\token",nf);
            }

            var connectionString = $"Provider=MSOLAP;Data Source=asazure://"+resourceID+"/"+aasModel+";Password=" + tk + ";Persist Security Info=True; Impersonation Level=Impersonate;";
            var ssasConnection = new AdomdConnection(connectionString);

            AdomdCommand ccc = new AdomdCommand(query, ssasConnection);
            AdomdDataAdapter objDataAdapter = new AdomdDataAdapter(ccc);
            System.Data.DataTable dt = new System.Data.DataTable();
            DateTime dtN = DateTime.Now;
            ssasConnection.Open();
            DateTime dtNq = DateTime.Now;
            objDataAdapter.Fill(dt);
            TimeSpan ts = DateTime.Now - dtN;
            string r = (DateTime.Now - dtN).Milliseconds.ToString() + " " + (DateTime.Now - dtNq).Milliseconds.ToString() + " " + dt.Rows.Count.ToString();
            ssasConnection.Close();
            Console.WriteLine(r);
        }
    }
}
