
using System.IO;
using System.Xml.Serialization;
using Kolera_MTSK.Login;
namespace Kolera_Mtsk.Services
{
    public class XmlConfigService
    {
        public ServerAyarModel GetServerAyar(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Baglanti.xml bulunamadı.");

            XmlSerializer ser =
                new XmlSerializer(typeof(ServerAyarModel));

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return (ServerAyarModel)ser.Deserialize(fs);
            }
        }
    }
}
